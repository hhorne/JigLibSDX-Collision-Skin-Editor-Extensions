using System;

using SlimDX;

using JigLibSDX;
using JigLibSDX.Collision;
using JigLibSDX.Geometry;
using JigLibSDX.Math;
using JigLibSDX.Physics;
using JigLibSDX.Utils;
using JigLibSDX.Objects;

namespace JigLibSDX.Objects
{
    public class BasicObject : IDisposable
    {
        #region Attributes
        private Body _body;
        private CollisionSkin _skin;

        private Primitive _primitive;
        private MaterialProperties _materialProperties;
        private PrimitiveProperties _primitiveProperties;
        #endregion

        #region Properties
        public Body Body
        {
            get { return _body; }
        }

        public CollisionSkin Skin
        {
            get { return _skin; }
        }

        public Vector3 Position
        {
            get { return _body.Position; }
            set { _body.MoveTo(value, Matrix.Identity); }
        }

        public Matrix PositionAndOrientation
        {
            get { return _skin.GetPrimitiveLocal(0).Transform.Orientation * _body.Orientation * Matrix.Translation(_body.Position); }
        }
        #endregion

        /// <summary>
        /// Initializes a new BasicObject. Body is enabled by default.
        /// </summary>
        /// <param name="primitive">Primitive that will define the CollisionSkin.</param>
        /// <param name="materialProperties"></param>
        /// <param name="primitiveProperties"></param>
        public BasicObject(Primitive primitive, MaterialProperties materialProperties, PrimitiveProperties primitiveProperties)
        {
            Initialize(primitive, materialProperties, primitiveProperties, true);
        }

        /// <summary>
        /// Initializes a new BasicObject.
        /// </summary>
        /// <param name="primitive">Primitive that will define the CollisionSkin.</param>
        /// <param name="materialProperties"></param>
        /// <param name="primitiveProperties"></param>
        /// <param name="enableBody"></param>
        public BasicObject(Primitive primitive, MaterialProperties materialProperties, PrimitiveProperties primitiveProperties, bool enableBody)
        {
            Initialize(primitive, materialProperties, primitiveProperties, enableBody);
        }

        private void Initialize(Primitive primitive, MaterialProperties materialProperties, PrimitiveProperties primitiveProperties, bool enableBody)
        {
            float mass;
            Vector3 centerOfMass;
            Matrix inertiaTensor;
            Matrix inertiaTensorCoM;

            // Set variables ...
            _primitive = primitive;
            _primitiveProperties = primitiveProperties;
            _materialProperties = materialProperties;

            // Create and link Body and CollisionSkin.
            _body = new Body();
            _skin = new CollisionSkin(_body);
            _body.CollisionSkin = _skin;

            // Add primitive to CollisionSkin.
            _skin.AddPrimitive(primitive, materialProperties);

            // Set body properties.
            _skin.GetMassProperties(primitiveProperties, out mass, out centerOfMass, out inertiaTensor, out inertiaTensorCoM);

            _body.BodyInertia = inertiaTensorCoM;
            _body.Mass = mass;

            // Sync CollisionSkin and Body.
            _body.MoveTo(Vector3.Zero, Matrix.Identity);
            _skin.ApplyLocalTransform(new Transform(-centerOfMass, Matrix.Identity));

            // Enable Body.
            if (enableBody)
            {
                _body.EnableBody();
            }
            else
            {
                _body.DisableBody();
            }
        }

        #region IDisposable Member
        public void Dispose()
        {
            _primitive = null;
            _body = null;
            _skin = null;
        }
        #endregion
    }
}
