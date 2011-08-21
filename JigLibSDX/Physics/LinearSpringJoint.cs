///                                                              ///
/// THIS FILE CONTAINS ADJUSTED FARSEER PHYSICS 2.1 (Ms-PL) CODE ///
///                          http://farseerphysics.codeplex.com/ ///
///                                                              ///

using System;
using SlimDX;

namespace JigLibSDX.Physics
{
    public sealed class LinearSpringJoint : SpringJointBase
    {
        #region Attributes
        private Body _body0;
        private Body _body1;
        private Vector3 _attachPoint0;
        private Vector3 _attachPoint1;
        private float _restLength;
        #endregion

        #region Properties
        public Body Body0
        {
            get { return _body0; }
            set { _body0 = value; }
        }

        public Body Body1
        {
            get { return _body1; }
            set { _body1 = value; }
        }

        public Vector3 AttachPoint0
        {
            get { return _attachPoint0; }
            set { _attachPoint0 = value; }
        }

        public Vector3 AttachPoint1
        {
            get { return _attachPoint1; }
            set { _attachPoint1 = value; }
        }
        #endregion

        #region Constructors
        public LinearSpringJoint(Body body0, Vector3 attachPoint0, Body body1, Vector3 attachPoint1, float springConstant, float dampingConstant)
            : base(springConstant, dampingConstant)
        {
            Initialize(body0, attachPoint0, body1, attachPoint1);
        }

        public LinearSpringJoint(Body body0, Vector3 attachPoint0, Body body1, Vector3 attachPoint1, float springConstant, float dampingConstant, float breakPoint)
            : base(springConstant, dampingConstant, breakPoint)
        {
            Initialize(body0, attachPoint0, body1, attachPoint1);
        }

        private void Initialize(Body body0, Vector3 attachPoint0, Body body1, Vector3 attachPoint1)
        {
            _body0 = body0;
            _body1 = body1;
            _attachPoint0 = attachPoint0;
            _attachPoint1 = attachPoint1;

            _difference = body1.GetWorldPosition(attachPoint1) - body0.GetWorldPosition(attachPoint0);
            _restLength = _difference.Length();
        }
        #endregion

        public override void UpdateController(float dt)
        {
            // Tha base does some checks which must be executed first.
            base.UpdateController(dt);

            if (this.IsControllerEnabled)
            {
                //If both bodies can't move. Don't apply forces to them.
                if (_body0.Immovable && _body1.Immovable)
                    return;

                if (!_body0.IsBodyEnabled && !_body1.IsBodyEnabled)
                    return;

                //F = -{s(L-r) + d[(v1-v2).L]/l}L/l   : s=spring const, d = dampning const, L=difference vector (p1-p2), l = difference magnitude, r = rest length,
                _body0.GetWorldPosition(ref _attachPoint0, out _worldPoint0);
                _body1.GetWorldPosition(ref _attachPoint1, out _worldPoint1);

                //Get the difference between the two attachpoints
                Vector3.Subtract(ref _worldPoint0, ref _worldPoint1, out _difference);
                float differenceMagnitude = _difference.Length();

                //If already close to rest length then return
                if (differenceMagnitude < _epsilon)
                {
                    return;
                }

                //Calculate spring force
                SpringError = differenceMagnitude - _restLength;
                Vector3.Normalize(ref _difference, out _differenceNormalized);
                _springForce = SpringConstant * SpringError; //kX

                //Calculate relative velocity
                _body0.GetVelocity(ref _attachPoint0, out _velocityAtPoint0);
                _body1.GetVelocity(ref _attachPoint1, out _velocityAtPoint1);
                Vector3.Subtract(ref _velocityAtPoint0, ref _velocityAtPoint1, out _relativeVelocity);

                //Calculate dampning force
                _temp = Vector3.Dot(_relativeVelocity, _difference);
                _dampningForce = DampingConstant * _temp / differenceMagnitude; //bV     

                //Calculate final force (spring + dampning)
                Vector3.Multiply(ref _differenceNormalized, -(_springForce + _dampningForce), out _force);

                if (_force != Vector3.Zero)
                {
                    if (!_body0.Immovable)
                    {
                        _body0.AddWorldForce(_force, _worldPoint0);
                        //_body0.AddBodyForce(_force, _attachPoint0);
                    }

                    if (!_body1.Immovable)
                    {
                        Vector3.Multiply(ref _force, -1, out _force);
                        _body1.AddWorldForce(_force, _worldPoint1);
                        //_body1.AddBodyForce(_force, _attachPoint1);
                    }
                }
            }
        }

        #region ApplyForce variables
        private const float _epsilon = JigLibSDX.Math.JiggleMath.Epsilon;
        private float _dampningForce;
        private Vector3 _differenceNormalized;
        private Vector3 _force;
        private float _springForce;
        private float _temp;
        private Vector3 _difference = Vector3.Zero;
        private Vector3 _relativeVelocity = Vector3.Zero;
        private Vector3 _velocityAtPoint0 = Vector3.Zero;
        private Vector3 _velocityAtPoint1 = Vector3.Zero;
        private Vector3 _worldPoint0 = Vector3.Zero;
        private Vector3 _worldPoint1 = Vector3.Zero;
        #endregion
    }
}
