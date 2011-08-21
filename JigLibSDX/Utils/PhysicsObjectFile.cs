using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using SlimDX;

using JigLibSDX.Physics;
using JigLibSDX.Geometry;
using JigLibSDX.Collision;
using JigLibSDX.Math;

namespace JigLibSDX.Utils
{
    #region MaterialPrimitivePair
    [XmlRoot("MaterialPrimitivePair")]
    public struct MaterialPrimitivePair
    {
        [XmlElement("MaterialID")]
        public int MaterialID;

        [XmlElement("MaterialProperties")]
        public MaterialProperties MaterialProperties;

        [XmlElement("Primitive")]
        public Primitive Primitive;

        public static MaterialPrimitivePair CreatePair(Primitive primitive, int materialID, MaterialProperties materialProperties)
        {
            return CreatePair(ref primitive, ref materialID, ref materialProperties);
        }

        public static MaterialPrimitivePair CreatePair(Primitive primitive, int materialID, ref MaterialProperties materialProperties)
        {
            return CreatePair(ref primitive, ref materialID, ref materialProperties);
        }

        public static MaterialPrimitivePair CreatePair(ref Primitive primitive, ref int materialID, ref MaterialProperties materialProperties)
        {
            MaterialPrimitivePair newPair = new MaterialPrimitivePair();
            newPair.Primitive = primitive;
            newPair.MaterialProperties = materialProperties;

            return newPair;
        }
    }
    #endregion

    #region MassProperties
    [XmlRoot("MassProperties")]
    public struct MassProperties
    {
        [XmlElement("CenterOfMass")]
        public Vector3 CenterOfMass;

        [XmlElement("Mass")]
        public float Mass;

        [XmlElement("InertiaTensor")]
        public Matrix InertiaTensor;

        [XmlElement("InertiaTensorCoM")]
        public Matrix InertiaTensorCoM;

        [XmlIgnore]
        public static MassProperties Zero
        {
            get { return MassProperties.CreateProperties(0.001f, Vector3.Zero, Matrix.Identity, Matrix.Identity); }
        }

        public static MassProperties CreateProperties(float mass, Vector3 centerOfMass, Matrix inertiaTensor, Matrix inertiaTensorCoM)
        {
            MassProperties newProps = new MassProperties();
            newProps.Mass = mass;
            newProps.CenterOfMass = centerOfMass;
            newProps.InertiaTensor = inertiaTensor;
            newProps.InertiaTensorCoM = inertiaTensorCoM;

            return newProps;
        }
    }
    #endregion

    #region PhysicsObjectCreationData
    [XmlRoot("PhysicsObjectCreationData")]
    public class PhysicsObjectData
    {
        private PrimitiveProperties _primitiveProperties;
        private List<MaterialPrimitivePair> _materialPrimitivePairs;

        private MassProperties _massProperties;

        [XmlElement("PrimitiveProperties")]
        public PrimitiveProperties PrimitiveProperties
        {
            get { return _primitiveProperties; }
            set { _primitiveProperties = value; }
        }

        [XmlElement("MaterialPrimitivePairs")]
        public List<MaterialPrimitivePair> MaterialPrimitivePairs
        {
            get { return _materialPrimitivePairs; }
            set { _materialPrimitivePairs = value; }
        }

        [XmlElement("MassProperties")]
        public MassProperties MassProperties
        {
            get { return _massProperties; }
            set { _massProperties = value; }
        }

        public PhysicsObjectData()
        {
            _materialPrimitivePairs = new List<MaterialPrimitivePair>();
        }

        public void SetPrimitiveProperties(PrimitiveProperties primitiveProperties)
        {
            _primitiveProperties = primitiveProperties;
        }

        public void Add(Primitive primitive, MaterialProperties materialProperties)
        {
            _materialPrimitivePairs.Add(MaterialPrimitivePair.CreatePair(primitive, (int)MaterialTable.MaterialID.UserDefined, ref materialProperties));
        }

        public void Add(Primitive primitive, int materialID, MaterialProperties materialProperties)
        {
            _materialPrimitivePairs.Add(MaterialPrimitivePair.CreatePair(primitive, materialID, ref materialProperties));
        }

        public void CalculateMassProperties()
        {
            CollisionSkin skin = new CollisionSkin();

            #region Mass Variables
            Vector3 centerOfMass;
            float mass;
            Matrix inertiaTensor;
            Matrix inertiaTensorCoM;
            #endregion

            for (int i = 0; i < MaterialPrimitivePairs.Count; i++)
            {
                if (this.MaterialPrimitivePairs[i].MaterialID == (int)MaterialTable.MaterialID.UserDefined)
                {
                    skin.AddPrimitive(this.MaterialPrimitivePairs[i].Primitive, this.MaterialPrimitivePairs[i].MaterialID);
                }
                else
                {
                    skin.AddPrimitive(this.MaterialPrimitivePairs[i].Primitive, this.MaterialPrimitivePairs[i].MaterialProperties);
                }
            }

            skin.GetMassProperties(PrimitiveProperties, out mass, out centerOfMass, out inertiaTensor, out inertiaTensorCoM);
            skin.RemoveAllPrimitives();

            _massProperties.Mass = mass;
            _massProperties.CenterOfMass = centerOfMass;
            _massProperties.InertiaTensor = inertiaTensor;
            _massProperties.InertiaTensorCoM = inertiaTensorCoM;
        }
    }
    #endregion

    public static class PhysicsObjectFile
    {
        /// <summary>
        /// Loads a physics world object.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="skin"></param>
        /// <param name="body"></param>
        /// <param name="massProperties"></param>
        /// <returns></returns>
        public static bool Load(string filePath, out CollisionSkin skin, out Body body, out PrimitiveProperties primitiveProperties, out MassProperties massProperties)
        {
            skin = null;
            body = null;
            primitiveProperties = new PrimitiveProperties(PrimitiveProperties.MassDistributionEnum.Solid, PrimitiveProperties.MassTypeEnum.Mass, 0.001f);
            massProperties = MassProperties.Zero;

            if (File.Exists(filePath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PhysicsObjectData));
                TextReader textReader = new StreamReader(filePath);

                PhysicsObjectData data = (PhysicsObjectData)xmlSerializer.Deserialize(textReader);
                textReader.Close();

                if (data != null && data.MaterialPrimitivePairs != null && data.MaterialPrimitivePairs.Count > 0)
                {
                    body = new JigLibSDX.Physics.Body();
                    skin = new JigLibSDX.Collision.CollisionSkin(body);
                    body.CollisionSkin = skin;

                    primitiveProperties = data.PrimitiveProperties;

                    for (int i = 0; i < data.MaterialPrimitivePairs.Count; i++)
                    {
                        if (data.MaterialPrimitivePairs[i].MaterialID == (int)MaterialTable.MaterialID.UserDefined)
                        {
                            skin.AddPrimitive(data.MaterialPrimitivePairs[i].Primitive, data.MaterialPrimitivePairs[i].MaterialID);
                        }
                        else
                        {
                            skin.AddPrimitive(data.MaterialPrimitivePairs[i].Primitive, data.MaterialPrimitivePairs[i].MaterialProperties);
                        }
                    }

                    massProperties = data.MassProperties;

                    body.BodyInertia = massProperties.InertiaTensorCoM;
                    body.Mass = massProperties.Mass;

                    body.MoveTo(Vector3.Zero, Matrix.Identity);
                    skin.ApplyLocalTransform(new Transform(-massProperties.CenterOfMass, Matrix.Identity));

                    body.EnableBody();
                }

                return true;
            }
            else
            {
                MessageBox.Show("File \"" + filePath + "\" not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Saves all needed information to recreate an physics world object.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="overwrite"></param>
        /// <param name="skin"></param>
        /// <param name="primitiveProperties"></param>
        /// <param name="massProperties">"Null" will cause re-calculation of the mass properties. If you are saving loaded data: Re-calc might cause data loss!</param>
        /// <returns></returns>
        public static bool Save(string filePath, bool overwrite, CollisionSkin skin, PrimitiveProperties primitiveProperties, MassProperties? massProperties)
        {
            if (!File.Exists(filePath) || (File.Exists(filePath) && overwrite))
            {
                PhysicsObjectData data = new PhysicsObjectData();
                data.SetPrimitiveProperties(primitiveProperties);

                for (int i = 0; i < skin.NumPrimitives; i++)
                {
                    data.Add(skin.GetPrimitiveLocal(i), skin.GetMaterialID(i), skin.GetMaterialProperties(i));
                }

                if (massProperties.HasValue)
                {
                    data.MassProperties = massProperties.Value;
                }
                else
                {
                    data.CalculateMassProperties();
                }

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PhysicsObjectData));
                TextWriter textWriter = new StreamWriter(filePath);

                xmlSerializer.Serialize(textWriter, data);
                textWriter.Close();

                return true;
            }
            else
            {
                MessageBox.Show("Can't write to file \"" + filePath + "\".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
