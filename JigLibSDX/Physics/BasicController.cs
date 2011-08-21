using System;
using SlimDX;

namespace JigLibSDX.Physics
{
    public sealed class BasicController : Controller
    {
        #region Attributes
        private Body _body;

        private Vector3 _constantForce = Vector3.Zero;
        private Vector3 _constantForcePosition = Vector3.Zero;

        private Vector3 _constantTorque = Vector3.Zero;


        private Vector3 _singleForce = Vector3.Zero;
        private Vector3 _singleForcePosition = Vector3.Zero;

        private Vector3 _singleTorque = Vector3.Zero;
        #endregion

        #region Properties
        public Body Body
        {
            get { return _body; }
            private set { _body = value; }
        }

        public Vector3 ConstantForce
        {
            get { return _constantForce; }
            set { _constantForce = value; }
        }

        public Vector3 ConstantForcePosition
        {
            get { return _constantForcePosition; }
            set { _constantForcePosition = value; }
        }

        public Vector3 ConstantTorque
        {
            get { return _constantTorque; }
            set { _constantTorque = value; }
        }

        public Vector3 SingleForce
        {
            get { return _singleForce; }
            set { _singleForce = value; }
        }

        public Vector3 SingleForcePosition
        {
            get { return _singleForcePosition; }
            set { _singleForcePosition = value; }
        }

        public Vector3 SingleTorque
        {
            get { return _singleTorque; }
            set { _singleTorque = value; }
        }
        #endregion

        #region Constructor
        public BasicController(Body body)
        {
            _body = body;
        }
        #endregion

        public override void UpdateController(float dt)
        {
            if (_body != null)
            {
                #region Constant Force
                if (_constantForce != Vector3.Zero)
                {
                    if (!_body.IsActive)
                    {
                        _body.SetActive();
                    }

                    if (_constantForcePosition == Vector3.Zero)
                    {
                        _body.AddBodyForce(_constantForce);
                    }
                    else
                    {
                        _body.AddBodyForce(_constantForce, _constantForcePosition);
                    }
                }
                #endregion

                #region Constant Torque
                if (_constantTorque != Vector3.Zero)
                {
                    if (!_body.IsActive)
                    {
                        _body.SetActive();
                    }

                    _body.AddBodyTorque(_constantTorque);
                }
                #endregion

                #region Single Force
                if (_singleForce != Vector3.Zero)
                {
                    if (!_body.IsActive)
                    {
                        _body.SetActive();
                    }

                    if (_singleForcePosition == Vector3.Zero)
                    {
                        _body.AddBodyForce(_constantForce);
                    }
                    else
                    {
                        _body.AddBodyForce(_constantForce, _constantForcePosition);
                        _singleForcePosition = Vector3.Zero;
                    }

                    _singleForce = Vector3.Zero;
                }
                #endregion

                #region Single Torque
                if (_singleTorque != Vector3.Zero)
                {
                    if (!_body.IsActive)
                    {
                        _body.SetActive();
                    }

                    _body.AddBodyTorque(_singleTorque);
                    _singleTorque = Vector3.Zero;
                }
                #endregion
            }
        }
    }
}