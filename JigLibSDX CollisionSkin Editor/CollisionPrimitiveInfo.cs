using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MonitoredUndo;
using SlimDX;
using JigLibSDX.Collision;
using JigLibSDX.Geometry;
using JigLibSDX.Math;

namespace JigLibSDX_CSE
{
    [XmlRoot("CollisionPrimitiveInfo")]
    public class CollisionPrimitiveInfo : INotifyPropertyChanged, ISupportsUndo
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
        public PrimitiveType PrimitiveType
        {
            get { return (PrimitiveType)_primitive.Type; }
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
            set
            {
                var updatedTransform = new JigLibSDX.Math.Transform(value, _primitive.Transform.Orientation);
                UndoablePropertyChanging("Position", _primitive.Transform, updatedTransform);
                _primitive.Transform = updatedTransform;
                OnPropertyChanged("Position");
            }
        }

        [XmlIgnore]
        public Matrix Orientation
        {
            get { return _primitive.Transform.Orientation; }
            set
            {
                var updatedTransform = new Transform(_primitive.Transform.Position, value);
                UndoablePropertyChanging("Orientation", _primitive.Transform.Orientation, updatedTransform);
                _primitive.Transform = updatedTransform;
                OnPropertyChanged("Orientation");
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
                var transform = new Transform(_primitive.Transform.Position, Matrix.RotationYawPitchRoll(value.Y, value.X, value.Z));
                UndoablePropertyChanging("Rotations", _primitive.Transform, transform);
                _primitive.Transform = transform;
                OnPropertyChanged("Rotations");
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the PropertyChanged event for the 
        /// specified property.
        /// </summary>
        /// <param name="propertyName">
        /// A string representing the name of 
        /// the property that changed.</param>
        /// <remarks>
        /// Only raise the event if the value of the property 
        /// has changed from its previous value</remarks>
        protected void OnPropertyChanged(string propertyName)
        {
            // Validate the property name in debug builds
            VerifyProperty(propertyName);

            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Verifies whether the current class provides a property with a given
        /// name. This method is only invoked in debug builds, and results in
        /// a runtime exception if the <see cref="OnPropertyChanged"/> method
        /// is being invoked with an invalid property name. This may happen if
        /// a property's name was changed but not the parameter of the property's
        /// invocation of <see cref="OnPropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            Type type = this.GetType();

            // Look for a *public* property with the specified name
            System.Reflection.PropertyInfo pi = type.GetProperty(propertyName);
            if (pi == null)
            {
                // There is no matching property - notify the developer
                string msg = "OnPropertyChanged was invoked with invalid " +
                                "property name {0}. {0} is not a public " +
                                "property of {1}.";
                msg = String.Format(msg, propertyName, type.FullName);
                System.Diagnostics.Debug.Fail(msg);
            }
        }

        #endregion

        #region Undo

        /// <summary>
        /// Call this when a property changes that should be tracked for undo.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="oldValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        protected void UndoablePropertyChanging(string propertyName, object oldValue, object newValue)
        {
            DefaultChangeFactory.OnChanging(this, propertyName, oldValue, newValue);
        }

        /// <summary>
        /// Call this when the ViewModel has an ObservableCollection that changed.
        /// The method will attempt to create an undo change item for it.
        /// </summary>
        /// <param name="propertyName">The name of the property that holds the collection.</param>
        /// <param name="collection">The collection instance.</param>
        /// <param name="e">The event args raised by the INotifyCollectionChanged.CollectionChanged event.</param>
        protected void UndoableCollectionChanged(string propertyName, object collection, NotifyCollectionChangedEventArgs e)
        {
            DefaultChangeFactory.OnCollectionChanged(this, propertyName, collection, e);
        }

        public object GetUndoRoot()
        {
            return this;
        }

        #endregion
    }
}