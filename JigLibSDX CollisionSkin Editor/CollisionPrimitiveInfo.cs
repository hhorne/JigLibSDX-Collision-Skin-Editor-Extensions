using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using SlimDX;
using JigLibSDX.Collision;
using JigLibSDX.Geometry;
using JigLibSDX.Math;

namespace JigLibSDX_CSE
{
    [XmlRoot("CollisionPrimitiveInfo")]
    public class CollisionPrimitiveInfo
    {
        private Primitive _primitive;
        private MaterialProperties _materialProperties;
        private bool _hidden;
        private Color4 _color;

        [XmlElement("Primitive")]
        public Primitive Primitive
        {
            get { return _primitive; }
            set { _primitive = value; }
        }

        [XmlIgnore]
        public Vector3 Scale
        {
            get 
            {
                switch ((PrimitiveType)_primitive.Type)
                { 
                    case PrimitiveType.AABox:
                        AABox aaBox = (AABox)_primitive;
                        return aaBox.GetSideLengths();

                    case PrimitiveType.Box:
                        Box box = (Box)_primitive;
                        return box.SideLengths;                        

                    case PrimitiveType.Sphere:
                        Sphere sphere = (Sphere)_primitive;
                        return new Vector3(sphere.Radius);

                    case PrimitiveType.Capsule:
                        Capsule capsule = (Capsule)_primitive;
                        return new Vector3(capsule.Radius, capsule.Radius, capsule.Length);

                    default:
                        return Vector3.Zero;
                }
            }

            set 
            {
                if (value.X <= 0f)
                {
                    value.X = 0.001f;
                }

                if (value.Y <= 0f)
                {
                    value.Y = 0.001f;
                }

                if (value.Z <= 0f)
                {
                    value.Z = 0.001f;
                }

                switch ((PrimitiveType)_primitive.Type)
                {
                    case PrimitiveType.AABox:
                        AABox aaBox = (AABox)_primitive;
                        aaBox.MinPos = -value / 2f;
                        aaBox.MaxPos = value / 2f;
                        break;

                    case PrimitiveType.Box:
                        Box box = (Box)_primitive;
                        box.SideLengths = value;
                        break;

                    case PrimitiveType.Sphere:
                        Sphere sphere = (Sphere)_primitive;
                        if (value.X != sphere.Radius)
                        {
                            sphere.Radius = value.X;
                            return;
                        }
                        
                        if (value.Y != sphere.Radius)
                        {
                            sphere.Radius = value.Y;
                            return;
                        }
                        
                        if (value.Z != sphere.Radius)
                        {
                            sphere.Radius = value.Z;
                            return;
                        }
                        break;

                    case PrimitiveType.Capsule:
                        Capsule capsule = (Capsule)_primitive;

                        capsule.Length = value.Z;

                        if (value.Y != capsule.Radius && value.X != capsule.Radius)
                        {
                            capsule.Radius = (value.X + value.Y) / 2f;
                        }
                        else
                        {
                            if (value.Y != capsule.Radius)
                            {
                                capsule.Radius = value.Y;
                                return;
                            }

                            if (value.X != capsule.Radius)
                            {
                                capsule.Radius = value.X;
                                return;
                            }
                        }
                        break;
                }
            }
        }

        [XmlIgnore]
        public Vector3 Position
        {
            get { return _primitive.Transform.Position; }
            set { _primitive.Transform = new JigLibSDX.Math.Transform(value, _primitive.Transform.Orientation); }
        }

        [XmlIgnore]
        public Matrix Orientation
        {
            get { return _primitive.Transform.Orientation; }
            set
            {
                _primitive.Transform = new Transform(_primitive.Transform.Position, value);
            }
        }

        [XmlIgnore]
        public Vector3 Rotations
        {
            get 
            {
                Vector3 scale;
                Vector3 translation;
                Quaternion orientation;

                _primitive.Transform.Orientation.Decompose(out scale, out orientation, out translation);

                return MathHelper.QuaternionToEulerAngle(orientation); 
            }
            set
            {
                _primitive.Transform = new Transform(_primitive.Transform.Position, Matrix.RotationYawPitchRoll(value.Y, value.X, value.Z));
            }
        }

        [XmlElement("MaterialProperties")]
        public MaterialProperties MaterialProperties
        {
            get { return _materialProperties; }
            set { _materialProperties = value; }
        }

        [XmlElement("Hidden")]
        public bool Hidden
        {
            get { return _hidden; }
            set { _hidden = value; }
        }

        [XmlElement("Color")]
        public Color4 Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public CollisionPrimitiveInfo()
        {
            Initialize(PrimitiveType.Box, new MaterialProperties(), new Color4());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Only AABox, Box, Capsule & Sphere will be accepted.</param>
        public CollisionPrimitiveInfo(PrimitiveType type, MaterialProperties materialProperties)
        {
            Random random = new Random();

            Initialize(type, materialProperties, new Color4((float)random.NextDouble() * 0.5f + 0.5f, (float)random.NextDouble() * 0.5f + 0.5f, (float)random.NextDouble() * 0.5f + 0.5f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Only AABox, Box, Capsule & Sphere will be accepted.</param>
        /// <param name="color"></param>
        public CollisionPrimitiveInfo(PrimitiveType type, MaterialProperties materialProperties, Color4 color)
        {
            Initialize(type, materialProperties, color);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Only AABox, Box, Capsule & Sphere will be accepted.</param>
        /// <param name="color"></param>
        private void Initialize(PrimitiveType type, MaterialProperties materialProperties, Color4 color)
        {
            switch(type)
            {
                case PrimitiveType.AABox:
                    _primitive = new AABox(new Vector3(-0.5f), new Vector3(0.5f));
                    break;

                case PrimitiveType.Box:
                    _primitive = new Box(Vector3.Zero, Matrix.Identity, new Vector3(1f));
                    break;

                case PrimitiveType.Capsule:
                    _primitive = new Capsule(Vector3.Zero, Matrix.Identity, 1f, 1f);
                    break;

                case PrimitiveType.Sphere:
                    _primitive = new Sphere(Vector3.Zero, 1f);
                    break;

                default:
                    goto case PrimitiveType.Box;
            }

            _materialProperties = materialProperties;
            _color = color;
        }
    }
}