using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SlimDX;

using JigLibSDX.Geometry;
using JigLibSDX.Collision;
using JigLibSDX.Math;
using JigLibSDX.Utils;

namespace JigLibSDX_CSE
{
    public partial class Code : Form
    {
        public Code(ref Project project, bool jiglibX, bool primitivesOnly)
        {
            InitializeComponent();

            codeBox.Text = GetCode(ref project, jiglibX, primitivesOnly);
        }

        private string GetCode(ref Project project, bool jiglibX, bool primitivesOnly)
        {
            Box box = null;
            Sphere sphere = null;
            Capsule capsule = null;
            MaterialProperties materialProperties;

            Quaternion orientation;
            Vector3 scale;
            Vector3 translation;

            string header = "";
            string headerPrimitiveVars = "";
            string primitiveProperties = "";
            string primitives = "";
            string primitive = "";
            string footer = "";

            string jiglibGeometryNamespace = "";
            string jiglibCollisionNamespace = "";
            string jiglibPhysicsNamespace = "";
            string jiglibMathNamespace = "";

            string positionString = "";
            string orientationString = "";
            string sideLengthString = "";
            string radiusString = "";
            string lengthString = "";

            Vector3 mainEdge = Vector3.Zero;

            bool boxPresent;
            bool spherePresent;
            bool capsulePresent;

            if (project != null && project.CollisionPrimitiveInfos != null && project.CollisionPrimitiveInfos.Count > 0)
            {
                if (!jiglibX)
                {
                    jiglibGeometryNamespace = "JigLibSDX.Geometry.";
                    jiglibCollisionNamespace = "JigLibSDX.Collision.";
                    jiglibPhysicsNamespace = "JigLibSDX.Physics.";
                    jiglibMathNamespace = "JigLibSDX.Math.";
                }
                else
                {
                    jiglibGeometryNamespace = "JigLibX.Geometry.";
                    jiglibCollisionNamespace = "JigLibX.Collision.";
                    jiglibPhysicsNamespace = "JigLibX.Physics.";
                    jiglibMathNamespace = "JigLibX.Math.";
                }

                #region Header
                GetProjectPrimitives(ref project, out boxPresent, out spherePresent, out capsulePresent);

                if (boxPresent)
                {
                    headerPrimitiveVars = "\t" + jiglibGeometryNamespace + "Box box;\r\n";
                }

                if (spherePresent)
                {
                    headerPrimitiveVars += "\t" + jiglibGeometryNamespace + "Sphere sphere;\r\n";
                }

                if (capsulePresent)
                {
                    headerPrimitiveVars += "\t" + jiglibGeometryNamespace + "Capsule capsule;\r\n";
                }

                if (!primitivesOnly)
                {
                    header = "/// <summary>\r\n";

                    if (!jiglibX)
                    {
                        header += "/// Creates all needed object information for the JigLibSDX physics simulator.\r\n";
                    }
                    else
                    {
                        header += "/// Creates all needed object information for the JigLibX physics simulator.\r\n";
                    }

                    header += "/// </summary>\r\n" +
                        "/// <param name=\"body\">Returns Body.</param>\r\n" +
                        "/// <param name=\"skin\">Returns CollisionSkin.</param>\r\n" +
                        "/// <param name=\"centerOfMass\">Returns the center of mass wich is usefull for drawing objects.</param>\r\n" +
                        "private void GetPhysicsInformation(out " + jiglibPhysicsNamespace + "Body body, out " + jiglibCollisionNamespace + "CollisionSkin skin, out Vector3 centerOfMass)\r\n" +
                        "{\r\n" +
                        "\t#region Header\r\n" +
                        "\t// Variables\r\n" +
                        "\t" + jiglibCollisionNamespace + "MaterialProperties materialProperties;\r\n" +
                        "\t" + jiglibGeometryNamespace + "PrimitiveProperties primitiveProperties;\r\n" +
                        headerPrimitiveVars +
                        "\t\r\n" +
                        "\t#region Mass Variables\r\n" +
                        "\tfloat mass;\r\n" +
                        "\tMatrix inertiaTensor;\r\n" +
                        "\tMatrix inertiaTensorCoM;\r\n" +
                        "\t#endregion\r\n" +
                        "\t\r\n" +
                        "\t// Create Skin & Body\r\n" +
                        "\tbody = new " + jiglibPhysicsNamespace + "Body();\r\n" +
                        "\tskin = new " + jiglibCollisionNamespace + "CollisionSkin(body);\r\n" +
                        "\tbody.CollisionSkin = skin;\r\n" +
                        "\t#endregion\r\n" +
                        "\t\r\n";
                }
                else
                {
                    header = "\t// Variables\r\n" +
                        "\t" + jiglibCollisionNamespace + "CollisionSkin skin = new " + jiglibCollisionNamespace + "CollisionSkin(null);\r\n" +
                        "\t" + jiglibCollisionNamespace + "MaterialProperties materialProperties;\r\n" +
                        "\t" + jiglibGeometryNamespace + "PrimitiveProperties primitiveProperties;\r\n" +
                        headerPrimitiveVars +
                        "\t\r\n";
                }
                #endregion

                #region Primitive Properties
                primitiveProperties = "\t#region Primitive Properties\r\n" +
                    "\tprimitiveProperties = new " + jiglibGeometryNamespace + "PrimitiveProperties();\r\n";

                if (project.PrimitiveProperties.MassDistribution == PrimitiveProperties.MassDistributionEnum.Solid)
                {
                    primitiveProperties += "\tprimitiveProperties.MassDistribution = " + jiglibGeometryNamespace + "PrimitiveProperties.MassDistributionEnum.Solid;\r\n";
                }
                else
                {
                    primitiveProperties += "\tprimitiveProperties.MassDistribution = " + jiglibGeometryNamespace + "PrimitiveProperties.MassDistributionEnum.Shell;\r\n";
                }

                if (project.PrimitiveProperties.MassType == PrimitiveProperties.MassTypeEnum.Mass)
                {
                    primitiveProperties += "\tprimitiveProperties.MassType = " + jiglibGeometryNamespace + "PrimitiveProperties.MassTypeEnum.Mass;\r\n";
                }
                else
                {
                    primitiveProperties += "\tprimitiveProperties.MassType = " + jiglibGeometryNamespace + "PrimitiveProperties.MassTypeEnum.Density;\r\n";
                }

                primitiveProperties += "\tprimitiveProperties.MassOrDensity = " + project.PrimitiveProperties.MassOrDensity.ToString().Replace(",", ".") + "f;\r\n" +
                    "\t#endregion\r\n" +
                    "\t\r\n";
                #endregion

                #region Primitives
                for (int i = 0; i < project.CollisionPrimitiveInfos.Count; i++)
                {
                    materialProperties = project.CollisionPrimitiveInfos[i].MaterialProperties;

                    // Material Properties
                    primitive = "\t#region Primitive " + i.ToString() + "\r\n" +
                        "\t// MaterialProperties:\r\n" +
                        "\tmaterialProperties = new " + jiglibCollisionNamespace + "MaterialProperties();\r\n" +
                        "\tmaterialProperties.StaticRoughness = " + materialProperties.StaticRoughness.ToString().Replace(",", ".") + "f;\r\n" +
                        "\tmaterialProperties.DynamicRoughness = " + materialProperties.DynamicRoughness.ToString().Replace(",", ".") + "f;\r\n" +
                        "\tmaterialProperties.Elasticity = " + materialProperties.Elasticity.ToString().Replace(",", ".") + "f;\r\n" +
                        "\t\r\n";

                    switch ((PrimitiveType)project.CollisionPrimitiveInfos[i].Primitive.Type)
                    {
                        #region PrimitiveType.Box
                        case PrimitiveType.Box:
                            box = (Box)project.CollisionPrimitiveInfos[i].Primitive;
                            box.Orientation.Decompose(out scale, out orientation, out translation);

                            // Create Primitive
                            // Since the Box isn't centered I we must center it first, rotate it and move it to the right position.
                            mainEdge = box.SideLengths * -0.5f;
                            mainEdge = Vector3.TransformCoordinate(mainEdge, Matrix.RotationQuaternion(orientation));
                            mainEdge += box.Position;

                            positionString = "new Vector3(" + mainEdge.X.ToString().Replace(",", ".") + "f, " + mainEdge.Y.ToString().Replace(",", ".") + "f, " + mainEdge.Z.ToString().Replace(",", ".") + "f)";
                            sideLengthString = "new Vector3(" + box.SideLengths.X.ToString().Replace(",", ".") + "f, " + box.SideLengths.Y.ToString().Replace(",", ".") + "f, " + box.SideLengths.Z.ToString().Replace(",", ".") + "f)";

                            if (!jiglibX)
                            {
                                orientationString = "Matrix.RotationQuaternion(new Quaternion(" + orientation.X.ToString().Replace(",", ".") + "f, " + orientation.Y.ToString().Replace(",", ".") + "f, " + orientation.Z.ToString().Replace(",", ".") + "f, " + orientation.W.ToString().Replace(",", ".") + "f))";
                            }
                            else
                            {
                                orientationString = "Matrix.CreateFromQuaternion(new Quaternion(" + orientation.X.ToString().Replace(",", ".") + "f, " + orientation.Y.ToString().Replace(",", ".") + "f, " + orientation.Z.ToString().Replace(",", ".") + "f, " + orientation.W.ToString().Replace(",", ".") + "f))";
                            }

                            primitive += "\t// Primitive:\r\n" +
                                    "\tbox = new " + jiglibGeometryNamespace + "Box(" + positionString + ",\r\n\t\t" + orientationString + ",\r\n\t\t" + sideLengthString + ");\r\n" +
                                    "\t\r\n";

                            // Add Primitive To Skin
                            primitive += "\tskin.AddPrimitive(box, materialProperties);\r\n" +
                                "\t#endregion\r\n" +
                                "\t\r\n";

                            break;
                        #endregion

                        #region PrimitiveType.Sphere
                        case PrimitiveType.Sphere:
                            sphere = (Sphere)project.CollisionPrimitiveInfos[i].Primitive;

                            // Create Primitive
                            primitive += "\t// Primitive:\r\n" +
                                "\tsphere = new " + jiglibGeometryNamespace + "Sphere(new Vector3(" + sphere.Position.X.ToString().Replace(",", ".") + "f, " + sphere.Position.Y.ToString().Replace(",", ".") + "f, " + sphere.Position.Z.ToString().Replace(",", ".") +
                                "f), " + sphere.Radius.ToString().Replace(",", ".") + "f);\r\n";

                            // Add Primitive To Skin
                            primitive += "\tskin.AddPrimitive(sphere, materialProperties);\r\n" +
                                "\t#endregion\r\n" +
                                "\t\r\n";

                            break;
                        #endregion

                        #region PrimitiveType.Capsule
                        case PrimitiveType.Capsule:
                            capsule = (Capsule)project.CollisionPrimitiveInfos[i].Primitive;
                            capsule.Orientation.Decompose(out scale, out orientation, out translation);

                            // Create Primitive 
                            // Since the Capsule isn't centered I we must center it first, rotate it and move it to the right position.
                            mainEdge = new Vector3(0f, 0f, capsule.Length * -0.5f);
                            mainEdge = Vector3.TransformCoordinate(mainEdge, Matrix.RotationQuaternion(orientation));
                            mainEdge += capsule.Position;

                            positionString = "new Vector3(" + mainEdge.X.ToString().Replace(",", ".") + "f, " + mainEdge.Y.ToString().Replace(",", ".") + "f, " + mainEdge.Z.ToString().Replace(",", ".") + "f)";
                            radiusString = capsule.Radius.ToString().Replace(",", ".") + "f";
                            lengthString = capsule.Length.ToString().Replace(",", ".") + "f";

                            if (!jiglibX)
                            {
                                orientationString = "Matrix.RotationQuaternion(new Quaternion(" + orientation.X.ToString().Replace(",", ".") + "f, " + orientation.Y.ToString().Replace(",", ".") + "f, " + orientation.Z.ToString().Replace(",", ".") + "f, " + orientation.W.ToString().Replace(",", ".") + "f))";
                            }
                            else
                            {
                                orientationString = "Matrix.CreateFromQuaternion(new Quaternion(" + orientation.X.ToString().Replace(",", ".") + "f, " + orientation.Y.ToString().Replace(",", ".") + "f, " + orientation.Z.ToString().Replace(",", ".") + "f, " + orientation.W.ToString().Replace(",", ".") + "f))";
                            }

                            primitive += "\t// Primitive:\r\n" +
                                    "\tcapsule = new " + jiglibGeometryNamespace + "Capsule(" + positionString + ",\r\n\t\t" + orientationString + ",\r\n\t\t" + radiusString  + ",\r\n\t\t" + lengthString + ");\r\n" +
                                    "\t\r\n";

                            // Add Primitive To Skin
                            primitive += "\tskin.AddPrimitive(capsule, materialProperties);\r\n" +
                                "\t#endregion\r\n" +
                                "\t\r\n";

                            break;
                        #endregion
                    }

                    primitives += primitive;

                }
                #endregion

                if (!primitivesOnly)
                {
                    #region Footer
                    footer = "\t#region Footer\r\n" +
                        "\t// Extract Mass Properties\r\n" +
                        "\tskin.GetMassProperties(primitiveProperties, out mass, out centerOfMass, out inertiaTensor, out inertiaTensorCoM);\r\n" +
                        "\t\r\n" +
                        "\t// Set Mass Properties\r\n" +
                        "\tbody.BodyInertia = inertiaTensorCoM;\r\n" +
                        "\tbody.Mass = mass;\r\n" +
                        "\t\r\n" +
                        "\t// Sync Body & Skin\r\n" +
                        "\tbody.MoveTo(Vector3.Zero, Matrix.Identity);\r\n" +
                        "\tskin.ApplyLocalTransform(new " + jiglibMathNamespace + "Transform(-centerOfMass, Matrix.Identity));\r\n" +
                        "\t\r\n" +
                        "\t// Enable Body\r\n" +
                        "\tbody.EnableBody();\r\n" +
                        "\t#endregion\r\n" +
                        "}";
                    #endregion
                }

                return header + primitiveProperties + primitives + footer;
            }
            else
            {
                return "C'mon -_-! You need at least one primitive.";
            }
        }

        private void GetProjectPrimitives(ref Project project, out bool boxPresent, out bool spherePresent, out bool capsulePresent)
        {
            boxPresent = false;
            spherePresent = false;
            capsulePresent = false;

            for (int i = 0; i < project.CollisionPrimitiveInfos.Count; i++)
            {
                switch ((PrimitiveType)project.CollisionPrimitiveInfos[i].Primitive.Type)
                {
                    case PrimitiveType.Box:
                        boxPresent = true;
                        break;

                    case PrimitiveType.Sphere:
                        spherePresent = true;
                        break;

                    case PrimitiveType.Capsule:
                        capsulePresent = true;
                        break;
                }
            }
        }
    }
}
