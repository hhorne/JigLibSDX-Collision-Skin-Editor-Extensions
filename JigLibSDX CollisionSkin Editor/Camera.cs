using System;
using System.Collections.Generic;

using JigLibSDX.Math;
using JigLibSDX.Utils;

using SlimDX;

namespace JigLibSDX_CSE
{
    class Camera
    {
        #region Attributes
        private Matrix _view;
        private Vector3 _position;
        private Vector3 _target;
        private Vector3 _upNormal;

        private Vector3 _distanceToCamera; // = _position - _target
        private float _vertical;   // upDown
        private float _horizontal; // leftRight

        private Matrix _projection;
        private int _width;
        private int _height;
        private float _fov; 
        private float _znear;
        private float _zfar;

        private Matrix _viewProjection;

        private Plane[] _viewFrustum;
        private Plane _targetPlane;

        private float _minDistanceToTarget;
        private float _maxDistanceToTarget;

        private bool _orthogonal;
        #endregion

        #region Temp Variables
        private Vector3 _tempVector3;
        private float _tempFloat;
        #endregion

        #region Consts
        private const float _piHalf = (float)Math.PI / 2f;
        private const float _pi = (float)Math.PI;
        #endregion

        #region View Properties
        public Matrix View
        {
            get { return _view; }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {

                // TODO: Max / Min Distance Check
                _position = value;

                #region Inline: _distanceToCamera = _position - _target;
                _distanceToCamera.X = _position.X - _target.X;
                _distanceToCamera.Y = _position.Y - _target.Y;
                _distanceToCamera.Z = _position.Z - _target.Z;
                #endregion

                MathHelper.VectorToRadians(_distanceToCamera, out _vertical, out _horizontal);
                UpdateVertical();
                UpdateHorizontal();

                UpdateView();
                UpdateViewFrustum();
            }
        }

        public Vector3 Target
        {
            get { return _target; }
            set 
            { 
                _target = value;

                // TODO: Max / Min Distance Check
                #region Inline: _distanceToCamera = _position - _target;
                _distanceToCamera.X = _position.X - _target.X;
                _distanceToCamera.Y = _position.Y - _target.Y;
                _distanceToCamera.Z = _position.Z - _target.Z;
                #endregion

                MathHelper.VectorToRadians(_distanceToCamera, out _vertical, out _horizontal);
                UpdateVertical();
                UpdateHorizontal();

                UpdateView();
                UpdateViewFrustum();
            }
        }

        public Vector3 UpNormal
        {
            get { return _upNormal; }
            set 
            { 
                _upNormal = value;
                UpdateView();
                UpdateViewFrustum();
            }
        }

        public Vector3 DistanceToCamera
        {
            get { return _distanceToCamera; }
            set 
            { 
                _tempFloat = value.Length();

                if (_tempFloat < _minDistanceToTarget || _tempFloat > _maxDistanceToTarget)
                {
                    if (_tempFloat < _minDistanceToTarget)
                    {
                        _distanceToCamera.Normalize();

                        #region Inline: _distanceToCamera = _distanceToCamera * _minDistanceToTarget;
                        _distanceToCamera.X = _distanceToCamera.X * _minDistanceToTarget;
                        _distanceToCamera.Y = _distanceToCamera.Y * _minDistanceToTarget;
                        _distanceToCamera.Z = _distanceToCamera.Z * _minDistanceToTarget;
                        #endregion
                    }

                    if (_tempFloat > _maxDistanceToTarget)
                    {
                        _distanceToCamera.Normalize();

                        #region Inline: _distanceToCamera = _distanceToCamera * _maxDistanceToTarget;
                        _distanceToCamera.X = _distanceToCamera.X * _maxDistanceToTarget;
                        _distanceToCamera.Y = _distanceToCamera.Y * _maxDistanceToTarget;
                        _distanceToCamera.Z = _distanceToCamera.Z * _maxDistanceToTarget;
                        #endregion
                    }
                }
                else
                {
                    _distanceToCamera = value;
                }

                #region Inline: _position = _distanceToCamera + _target;
                _position.X = _distanceToCamera.X + _target.X;
                _position.Y = _distanceToCamera.Y + _target.Y;
                _position.Z = _distanceToCamera.Z + _target.Z;
                #endregion

                MathHelper.VectorToRadians(_distanceToCamera, out _vertical, out _horizontal);
                UpdateVertical();
                UpdateHorizontal();

                UpdateView();
                UpdateViewFrustum();
                UpdateProjection();
            }
        }

        public float Vertical
        {
            get 
            { 
                return _vertical;
            }
            set
            {
                _vertical = value;
                UpdateVertical();

                #region Prevent vertical looping.
                if (_vertical >= (float)Math.PI)
                {
                    _vertical = (float)Math.PI - 0.0001f;
                }
                else if (_vertical <= 0f)
                {
                    _vertical = 0.0001f;
                }
                #endregion

                _tempFloat = _distanceToCamera.Length();
                _distanceToCamera = MathHelper.RadiansToVector(_vertical, _horizontal);

                #region Inline: _distanceToCamera *= _tempFloat;
                _distanceToCamera.X *= _tempFloat;
                _distanceToCamera.Y *= _tempFloat;
                _distanceToCamera.Z *= _tempFloat;
                #endregion

                #region Inline: _position = _distanceToCamera + _target;
                _position.X = _distanceToCamera.X + _target.X;
                _position.Y = _distanceToCamera.Y + _target.Y;
                _position.Z = _distanceToCamera.Z + _target.Z;
                #endregion

                UpdateView();
                UpdateViewFrustum();
            }
        }

        public float Horizontal
        {
            get { return _horizontal; }
            set
            {
                _horizontal = value;
                UpdateHorizontal();

                _tempFloat = _distanceToCamera.Length();
                _distanceToCamera = MathHelper.RadiansToVector(_vertical, _horizontal);

                #region Inline: _distanceToCamera *= _tempFloat;
                _distanceToCamera.X *= _tempFloat;
                _distanceToCamera.Y *= _tempFloat;
                _distanceToCamera.Z *= _tempFloat;
                #endregion

                #region Inline: _position = _distanceToCamera + _target;
                _position.X = _distanceToCamera.X + _target.X;
                _position.Y = _distanceToCamera.Y + _target.Y;
                _position.Z = _distanceToCamera.Z + _target.Z;
                #endregion

                UpdateView();
                UpdateViewFrustum();
            }
        }
        #endregion

        #region Projection Properties
        public Matrix Projection
        {
            get { return _projection; }
        }

        public int Width
        {
            get { return _width; }
            set 
            { 
                _width = value;
                UpdateProjection();
                UpdateViewFrustum();
            }
        }

        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                UpdateProjection();
                UpdateViewFrustum();
            }
        }

        public float Fov
        {
            get { return _fov; }
            set
            {
                _fov = value;
                UpdateProjection();
                UpdateViewFrustum();
            }
        }

        public float Znear
        {
            get { return _znear; }
            set
            {
                _znear = value;
                UpdateProjection();
                UpdateViewFrustum();
            }
        }

        public float Zfar
        {
            get { return _zfar; }
            set
            {
                _zfar = value;
                UpdateProjection();
                UpdateViewFrustum();
            }
        }

        public bool Orthogonal
        {
            get { return _orthogonal; }
            set 
            { 
                _orthogonal = value;
                UpdateView();
                UpdateProjection();
                UpdateViewFrustum();
            }
        }
        #endregion

        #region View Frustum Properties
        public Plane ViewFrustumLeft
        {
            get { return _viewFrustum[0]; }
        }

        public Plane ViewFrustumRight
        {
            get { return _viewFrustum[1]; }
        }

        public Plane ViewFrustumTop
        {
            get { return _viewFrustum[2]; }
        }

        public Plane ViewFrustumBottom
        {
            get { return _viewFrustum[3]; }
        }

        public Plane ViewFrustumNear
        {
            get { return _viewFrustum[4]; }
        }

        public Plane ViewFrustumFar
        {
            get { return _viewFrustum[5]; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Pre-calculated: View * Projection.
        /// </summary>
        public Matrix ViewProjection
        {
            get { return _viewProjection; }
        }

        public float MinDistanceToTarget
        {
            get { return _minDistanceToTarget; }
            set
            {
                if (value > _maxDistanceToTarget || value < 0.1f)
                {
                    if (value > _maxDistanceToTarget)
                    {
                        _minDistanceToTarget = _maxDistanceToTarget;
                    }

                    if (value < 0.1f)
                    {
                        _minDistanceToTarget = 0.1f;
                    }
                }
                else
                {
                    _minDistanceToTarget = value;
                }
            }
        }

        public float MaxDistanceToTarget
        {
            get { return _maxDistanceToTarget; }
            set
            {
                if (value < _minDistanceToTarget)
                {
                    _maxDistanceToTarget = _minDistanceToTarget;
                }
                else
                {
                    _maxDistanceToTarget = value;
                }
            }
        }
        #endregion

        #region Constructor
        public Camera(Vector3 position, Vector3 target, Vector3 up, int width, int height, float fov, float znear, float zfar)
        {
            _view = Matrix.Identity;
            _position = position;
            _target = target;
            _upNormal = up;

            _projection = Matrix.Identity;
            _width = width;
            _height = height;
            _fov = fov;
            _znear = znear;
            _zfar = zfar;

            _viewFrustum = new Plane[6];

            MinDistanceToTarget = 0f;
            MaxDistanceToTarget = 99f;

            //TODO: Max / Min Distance Check
            #region Inline: _distanceToCamera = _position - _target;
            _distanceToCamera.X = _position.X - _target.X;
            _distanceToCamera.Y = _position.Y - _target.Y;
            _distanceToCamera.Z = _position.Z - _target.Z;
            #endregion

            MathHelper.VectorToRadians(_distanceToCamera, out _vertical, out _horizontal);
            UpdateVertical();
            UpdateHorizontal();

            UpdateView();
            UpdateProjection();
            UpdateViewFrustum();
        }
        #endregion

        #region Updates
        private void UpdateVertical()
        {
            #region Prevent vertical looping.
            if (_vertical >= (float)Math.PI)
            {
                _vertical = (float)Math.PI - 0.0001f;
            }
            else if (_vertical <= 0f)
            {
                _vertical = 0.0001f;
            }
            #endregion
        }

        private void UpdateHorizontal()
        {
            _horizontal = MathHelper.Loop(_horizontal, -_pi, _pi);
        }

        private void UpdateView()
        {
            Matrix.LookAtLH(ref _position, ref _target, ref _upNormal, out _view);

            // Update ViewProjection
            Matrix.Multiply(ref _view, ref _projection, out _viewProjection);
        }

        private void UpdateProjection()
        {
            if (_orthogonal)
            {
                _tempFloat = _distanceToCamera.Length() * 0.001f;
                Matrix.OrthoLH((float)_width * _tempFloat, (float)_height * _tempFloat, -_zfar / 5f, _zfar, out _projection);
            }
            else
            {
                Matrix.PerspectiveFovLH(_fov, (float)_width / (float)_height, _znear, _zfar, out _projection);
            }

            // Update ViewProjection
            Matrix.Multiply(ref _view, ref _projection, out _viewProjection);
        }

        private void UpdateViewFrustum()
        {
            //Left
            _viewFrustum[0] = new Plane(_viewProjection.M14 + _viewProjection.M11, 
                                        _viewProjection.M24 + _viewProjection.M21,
                                        _viewProjection.M34 + _viewProjection.M31,
                                        _viewProjection.M44 + _viewProjection.M41);
            _viewFrustum[0].Normalize();

            //Right
            _viewFrustum[1] = new Plane(_viewProjection.M14 - _viewProjection.M11,
                                        _viewProjection.M24 - _viewProjection.M21, 
                                        _viewProjection.M34 - _viewProjection.M31,
                                        _viewProjection.M44 - _viewProjection.M41);
            _viewFrustum[1].Normalize();

            //Top
            _viewFrustum[2] = new Plane(_viewProjection.M14 - _viewProjection.M12,
                                        _viewProjection.M24 - _viewProjection.M22,
                                        _viewProjection.M34 - _viewProjection.M32,
                                        _viewProjection.M44 - _viewProjection.M42);
            _viewFrustum[2].Normalize();

            //Bottom
            _viewFrustum[3] = new Plane(_viewProjection.M14 + _viewProjection.M12,
                                        _viewProjection.M24 + _viewProjection.M22,
                                        _viewProjection.M34 + _viewProjection.M32,
                                        _viewProjection.M44 + _viewProjection.M42);
            _viewFrustum[3].Normalize();

            //Near
            _viewFrustum[4] = new Plane(_viewProjection.M13, 
                                        _viewProjection.M23, 
                                        _viewProjection.M33, 
                                        _viewProjection.M43);
            _viewFrustum[4].Normalize();

            //Far
            _viewFrustum[5] = new Plane(_viewProjection.M14 - _viewProjection.M13, 
                                        _viewProjection.M24 - _viewProjection.M23, 
                                        _viewProjection.M34 - _viewProjection.M33, 
                                        _viewProjection.M44 - _viewProjection.M43);
            _viewFrustum[5].Normalize();

            _targetPlane = new Plane(_target, GetBack());
        }
        #endregion

        public void Reset(int width, int height)
        {
            _width = width;
            _height = height;

            UpdateProjection();
            UpdateViewFrustum();
        }

        public void Move(Vector3 delta)
        {
            delta = Vector3.TransformCoordinate(delta, Matrix.RotationYawPitchRoll(-_horizontal - _piHalf, -_vertical - _piHalf, 0f));

            // Since we move both at the same time there is no point in updating _horizontal, _vertical and distance.
            #region Inline: _position += delta;
            _position.X += delta.X;
            _position.Y += delta.Y;
            _position.Z += delta.Z;
            #endregion

            #region Inline: _target = _position - _distanceToCamera;
            _target.X = _position.X - _distanceToCamera.X;
            _target.Y = _position.Y - _distanceToCamera.Y;
            _target.Z = _position.Z - _distanceToCamera.Z;
            #endregion

            UpdateView();
            UpdateViewFrustum();
        }

        public void MoveTo(Vector3 position)
        {
            // Since we move both at the same time there is no point in updating _horizontal, _vertical and distance.
            #region Inline: _position = position;
            _position.X = position.X;
            _position.Y = position.Y;
            _position.Z = position.Z;
            #endregion

            #region Inline: _target = _position - _distanceToCamera;
            _target.X = _position.X - _distanceToCamera.X;
            _target.Y = _position.Y - _distanceToCamera.Y;
            _target.Z = _position.Z - _distanceToCamera.Z;
            #endregion

            UpdateView();
            UpdateViewFrustum();
        }

        public void RotateAroundTarget(float verticalDelta, float horizontalDelta)
        {
            _vertical += verticalDelta;
            _horizontal += horizontalDelta;
            UpdateVertical();
            UpdateHorizontal();

            _tempFloat = _distanceToCamera.Length();
            _distanceToCamera = MathHelper.RadiansToVector(_vertical, _horizontal);

            #region Inline: _distanceToCamera *= _tempFloat;
            _distanceToCamera.X *= _tempFloat;
            _distanceToCamera.Y *= _tempFloat;
            _distanceToCamera.Z *= _tempFloat;
            #endregion

            #region Inline: _position = _distanceToCamera + _target;
            _position.X = _distanceToCamera.X + _target.X;
            _position.Y = _distanceToCamera.Y + _target.Y;
            _position.Z = _distanceToCamera.Z + _target.Z;
            #endregion

            UpdateView();
            UpdateViewFrustum();
        }

        #region Spherical Coordinates
        public void SetSphericalCoordinates(float vertical, float horizontal)
        {
            _vertical = vertical;
            _horizontal = horizontal;
            UpdateVertical();
            UpdateHorizontal();

            _tempFloat = _distanceToCamera.Length();
            _distanceToCamera = MathHelper.RadiansToVector(_vertical, _horizontal);

            #region Inline: _distanceToCamera *= _tempFloat;
            _distanceToCamera.X *= _tempFloat;
            _distanceToCamera.Y *= _tempFloat;
            _distanceToCamera.Z *= _tempFloat;
            #endregion

            #region Inline: _position = _distanceToCamera + _target;
            _position.X = _distanceToCamera.X + _target.X;
            _position.Y = _distanceToCamera.Y + _target.Y;
            _position.Z = _distanceToCamera.Z + _target.Z;
            #endregion

            UpdateView();
            UpdateViewFrustum();
        }

        public void SetSphericalCoordinates(float vertical, float horizontal, float distance)
        {
            _vertical = vertical;
            _horizontal = horizontal;
            UpdateVertical();
            UpdateHorizontal();

            _distanceToCamera = MathHelper.RadiansToVector(_vertical, _horizontal);

            // TODO: Max / Min Distance Check
            #region Inline: _distanceToCamera *= distance;
            _distanceToCamera.X *= distance;
            _distanceToCamera.Y *= distance;
            _distanceToCamera.Z *= distance;
            #endregion

            #region Inline: _position = _distanceToCamera + _target;
            _position.X = _distanceToCamera.X + _target.X;
            _position.Y = _distanceToCamera.Y + _target.Y;
            _position.Z = _distanceToCamera.Z + _target.Z;
            #endregion

            UpdateView();
            UpdateViewFrustum();
        }

        public void GetSphericalCoordinates(out float vertical, out float horizontal)
        {
            vertical = _vertical;
            horizontal = _horizontal;
            UpdateVertical();
            UpdateHorizontal();
        }

        public void GetSphericalCoordinates(out float vertical, out float horizontal, out float distance)
        {
            vertical = _vertical;
            horizontal = _horizontal;
            UpdateVertical();
            UpdateHorizontal();

            distance = _distanceToCamera.Length();
        }

        public void GetSphericalCoordinates(out float vertical, out float horizontal, out Vector3 distance)
        {
            vertical = _vertical;
            horizontal = _horizontal;
            UpdateVertical();
            UpdateHorizontal();

            distance = _distanceToCamera;
        }
        #endregion

        //TODO: Re-Check this ... seems wrong.
        public Matrix FaceCamera()
        {
            return Matrix.RotationYawPitchRoll(-_horizontal - _piHalf, - _vertical + _piHalf, 0f);
        }

        #region Directions
        public Vector3 GetUp()
        {
            return MathHelper.RadiansToVector(_vertical - _piHalf, _horizontal);
        }

        public Vector3 GetDown()
        {
            return MathHelper.RadiansToVector(_vertical + _piHalf, _horizontal);
        }

        public Vector3 GetLeft()
        {
            return MathHelper.RadiansToVector(_piHalf, _horizontal - _piHalf);
        }

        public Vector3 GetRight()
        {
            return MathHelper.RadiansToVector(_piHalf, _horizontal + _piHalf);
        }

        public Vector3 GetFront()
        {
            return -MathHelper.RadiansToVector(_vertical, _horizontal);
        }

        public Vector3 GetBack()
        {
            return MathHelper.RadiansToVector(_vertical, _horizontal);
        }

        //ToDo: Does vertical change the value of the axis you are looking at the most ?
        public Vector3 GetLookAtTargetAxis()
        {
            float x = 0f;
            float y = 0f;
            float z = 0f;

            if (_vertical >= (float)Math.PI / 4f && _vertical <= (float)Math.PI / 4f * 3f)
            {
                y = 1f;

                // < -pi/4; > -3*pi/4 == Looking at Y and +X
                if (_horizontal <= -_pi / 4f && _horizontal > -_pi / 4f * 3f)
                {
                    x = 1f;
                }

                // < -3*pi/4 && > -pi; > 3*pi/4 && < pi == Looking at Y and -Z
                if (_horizontal <= -_pi / 4f * 3f && _horizontal >= -_pi || _horizontal > _pi / 4f * 3f && _horizontal <= _pi)
                {
                    z = -1f;
                }

                // > pi/4; < 3*pi/4 == Looking at Y and -X
                if (_horizontal > _pi / 4f && _horizontal <= _pi / 4f * 3f)
                {
                    x = -1f;
                }

                // > -pi/4; < pi/4 == Looking at Y and +Z
                if (_horizontal > -_pi / 4f && _horizontal <= _pi / 4f)
                {
                    z = 1f;
                }
            }
            else
            {
                if (_vertical <= (float)Math.PI / 4f)
                {
                    // < -pi/4; > -3*pi/4 == Looking at +Z and +X
                    if (_horizontal <= -_pi / 4f && _horizontal > -_pi / 4f * 3f)
                    {
                        x = 1f;
                        z = 1f;
                    }

                    // < -3*pi/4 && > -pi; > 3*pi/4 && < pi == Looking at +X and -Z
                    if (_horizontal <= -_pi / 4f * 3f && _horizontal >= -_pi || _horizontal > _pi / 4f * 3f && _horizontal <= _pi)
                    {
                        x = 1f;
                        z = -1f;
                    }

                    // > pi/4; < 3*pi/4 == Looking at -Z and -X
                    if (_horizontal > _pi / 4f && _horizontal <= _pi / 4f * 3f)
                    {
                        x = -1f;
                        z = -1f;
                    }

                    // > -pi/4; < pi/4 == Looking at -X and +Z
                    if (_horizontal > -_pi / 4f && _horizontal <= _pi / 4f)
                    {
                        x = -1f;
                        z = 1f;
                    }
                }
                else
                { 
                    // < -pi/4; > -3*pi/4 == Looking at +Z and +X
                    if (_horizontal <= -_pi / 4f && _horizontal > -_pi / 4f * 3f)
                    {
                        x = 1f;
                        z = -1f;
                    }

                    // < -3*pi/4 && > -pi; > 3*pi/4 && < pi == Looking at +X and -Z
                    if (_horizontal <= -_pi / 4f * 3f && _horizontal >= -_pi || _horizontal > _pi / 4f * 3f && _horizontal <= _pi)
                    {
                        x = -1f;
                        z = -1f;
                    }

                    // > pi/4; < 3*pi/4 == Looking at -Z and -X
                    if (_horizontal > _pi / 4f && _horizontal <= _pi / 4f * 3f)
                    {
                        x = -1f;
                        z = 1f;
                    }

                    // > -pi/4; < pi/4 == Looking at -X and +Z
                    if (_horizontal > -_pi / 4f && _horizontal <= _pi / 4f)
                    {
                        x = 1f;
                        z = 1f;
                    }
                }
            }

            return new Vector3(x, y, z);
        }
        #endregion

        public Vector3 ScreenToView(float x, float y)
        {
            Viewport viewport = new Viewport(0, 0, _width, _height, _znear, _zfar);

            Vector3 near = new Vector3(x, y, _znear);
            Vector3 far = new Vector3(x, y, _zfar);

            near = Vector3.Unproject(near, viewport, _projection, _view, Matrix.Identity);
            far = Vector3.Unproject(far, viewport, _projection, _view, Matrix.Identity);

            Vector3 position;
            Plane.Intersects(_targetPlane, near, far, out position);

            ViewToScreen(position.X, position.Y, position.Z);

            return position;
        }

        public Vector2 ViewToScreen(float x, float y, float z)
        { 
            Viewport viewport = new Viewport(0, 0, _width, _height, _znear, _zfar);

            Vector3 position3d = Vector3.Project(new Vector3(x, y, z), viewport, _projection, _view, Matrix.Identity);
            Vector2 position2d = new Vector2((float)Math.Round((double)position3d.X, 0),
                                             (float)Math.Round((double)position3d.Y, 0));
            return position2d;
        }

        public void Zoom(float delta)
        {
            _tempVector3 = _distanceToCamera;
            _tempVector3.Normalize();

            _tempFloat = _distanceToCamera.Length() + delta;

            #region Inline: _tempVector3 *= _tempFloat;
            _tempVector3.X *= _tempFloat;
            _tempVector3.Y *= _tempFloat;
            _tempVector3.Z *= _tempFloat;
            #endregion

            DistanceToCamera = _tempVector3;
        }

        /// <summary>
        /// Tests if a point is in the view frustum.
        /// </summary>
        /// <param name="position">Position of the point.</param>
        /// <returns>True if in view frustum.</returns>
        public bool InViewFrustum(Vector3 position)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Plane.DotCoordinate(_viewFrustum[i], position) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tests if a sphere is in the view frustum.
        /// </summary>
        /// <param name="position">Positionof the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        /// <returns>True if in view frustum.</returns>
        public bool InViewFrustum(Vector3 position, float radius)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Plane.DotCoordinate(_viewFrustum[i], position) + radius < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
