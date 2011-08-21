#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using JigLibSDX.Math;
using JigLibSDX.Physics;
using JigLibSDX.Geometry;
using JigLibSDX.Collision;
using JigLibSDX.Utils;
#endregion

namespace JigLibSDX.Objects
{
    public class Wheel
    {
        #region Attributes
        private Car _car;

        /// local mount position
        private Vector3 _position;
        private Vector3 _axisUp;
        private float _spring;
        private float _travel;
        private float _inertia;
        private float _radius;
        private float _sideFriction;
        private float _forwardFriction;
        private float _damping;
        private int _numberOfRays;

        // things that change 
        private float _angularVelocity;
        private float _steerAngle;
        private float _torque;
        private float _driveTorque;
        private float _axisAngle;
        private float _displacement; // = mTravel when fully compressed
        private float _upSpeed; // speed relative to the car
        private bool _locked;
        
        // last frame stuff
        private float _lastDisplacement;
        private bool _lastOnFloor;

        /// used to estimate the friction
        private float _angularVelocityForGrip;
        #endregion

        #region Properties
        /// <summary>
        /// get steering angle in degrees
        /// </summary>
        public float SteerAngle
        {
            get { return _steerAngle; }
            set { _steerAngle = value; }
        }

        /// <summary>
        /// lock/unlock the wheel
        /// </summary>
        public bool Lock
        {
            get { return _locked; }
            set { _locked = value; }
        }

        /// <summary>
        /// the basic origin position
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
        }

        /// <summary>
        /// the suspension axis in the car's frame
        /// </summary>
        public Vector3 LocalAxisUp
        {
            get { return _axisUp; }
        }

        /// <summary>
        /// wheel radius
        /// </summary>
        public float Radius
        {
            get { return _radius; }
        }

        /// <summary>
        /// the displacement along our up axis
        /// </summary>
        public float Displacement
        {
            get { return _displacement; }
        }

        public float AxisAngle
        {
            get { return _axisAngle; }
        }

        public bool OnFloor
        {
            get { return _lastOnFloor; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a wheel with a dumping spring.
        /// </summary>
        /// <param name="car"></param>
        /// <param name="position">Position relative to car, in car's space.</param>
        /// <param name="axisUp">Up axis of the wheel, in car's space.</param>
        /// <param name="spring">Force per suspension offset.</param>
        /// <param name="travel">Suspension travel upwards.</param>
        /// <param name="inertia">Inertia about the axe.</param>
        /// <param name="radius">Wheel radius.</param>
        /// <param name="sideFriction">Slide wheel friction.</param>
        /// <param name="forwardFriction">Brake / accelerate wheel friction.</param>
        /// <param name="damping"></param>
        /// <param name="numRays"></param>
        public void Setup(Car car, Vector3 position, Vector3 axisUp, float spring, float travel, float inertia,
            float radius, float sideFriction, float forwardFriction, float damping, int numRays)
        {
            _car = car;
            _position = position;
            _axisUp = axisUp;
            _spring = spring;
            _travel = travel;
            _inertia = inertia;
            _radius = radius;
            _sideFriction = sideFriction;
            _forwardFriction = forwardFriction;
            _damping = damping;
            _numberOfRays = numRays;

            _predicate = new WheelPredicate(car.Chassis.Body.CollisionSkin);

            Reset();
        }
        #endregion

        #region Reset
        /// <summary>
        /// sets everything that varies to a default
        /// </summary>
        public void Reset()
        {
            _angularVelocity = 0.0f;
            _steerAngle = 0.0f;
            _torque = 0.0f;
            _driveTorque = 0.0f;
            _axisAngle = 0.0f;
            _displacement = 0.0f;
            _upSpeed = 0.0f;
            _locked = false;
            _lastDisplacement = 0.0f;
            _lastOnFloor = false;
            _angularVelocityForGrip = 0.0f;
        }
        #endregion

        #region AddForcesToCar Attributes
        // do a number of rays, and choose the deepest penetration
        private const int _MAX_RAYS = 32;
        private float[] _fracs = new float[_MAX_RAYS];
        private CollisionSkin[] _otherSkins = new CollisionSkin[_MAX_RAYS];
        private Vector3[] _groundPositions = new Vector3[_MAX_RAYS];
        private Vector3[] _groundNormals = new Vector3[_MAX_RAYS];
        private Segment[] _segments = new Segment[_MAX_RAYS];
        private WheelPredicate _predicate;
        #endregion

        /// <summary>
        /// Adds forces from to the parent body.
        /// </summary>
        /// <param name="dt">Time step in seconds.</param>
        /// <returns>True if the wheel is on the ground.</returns>
        public bool AddForcesToCar(float dt)
        {
            Vector3 force = Vector3.Zero;
            _lastDisplacement = _displacement;
            _displacement = 0.0f;

            Body parentBody = _car.Chassis.Body;

            // Get wheel position and spring up axis, relative to car.
            Vector3 worldPosition = parentBody.Position + Vector3.TransformCoordinate(_position, parentBody.Orientation);
            Vector3 worldAxisUp = Vector3.TransformCoordinate(_axisUp, parentBody.Orientation);

            // Forward normal of the wheel(?), considering the steer angle. (Isn't this getting the rotation axis of the wheel?)
            Vector3 wheelForward = Vector3.TransformCoordinate(MatrixHelper.GetRight(parentBody.Orientation), JiggleMath.RotationMatrix(_steerAngle, worldAxisUp));

            // Left normal of the wheel.
            Vector3 wheelLeft = Vector3.Cross(worldAxisUp, wheelForward);
            wheelLeft.Normalize();

            // Up normal of the wheel.
            Vector3 wheelUp = Vector3.Cross(worldAxisUp, wheelLeft);


            // start of ray
            float rayLen = 2.0f * _radius + _travel;
            Vector3 wheelRayEnd = worldPosition - _radius * worldAxisUp;
            Segment wheelRay = new Segment(wheelRayEnd + rayLen * worldAxisUp, -rayLen * worldAxisUp);

            int numberOfRaysToUse = System.Math.Min(_numberOfRays, _MAX_RAYS);

            // adjust the start position of the ray - divide the wheel into numRays+2 
            // rays, but don't use the first/last.
            float deltaFwd = (2.0f * _radius) / (numberOfRaysToUse + 1);
            float deltaFwdStart = deltaFwd;


            _lastOnFloor = false;
            int bestIRay = 0;
            int iRay;

            CollisionSystem collisionSystem = PhysicsSystem.CurrentPhysicsSystem.CollisionSystem;

            for (iRay = 0; iRay < numberOfRaysToUse; ++iRay)
            {
                _fracs[iRay] = float.MaxValue; //SCALAR_HUGE;
                // work out the offset relative to the middle ray
                float distFwd = (deltaFwdStart + iRay * deltaFwd) - _radius;
                //float zOffset = mRadius * (1.0f - CosDeg(90.0f * (distFwd / mRadius)));
                float zOffset = _radius * (1.0f - (float)System.Math.Cos( MathHelper.ToRadians( 90.0f * (distFwd / _radius))));

                _segments[iRay] = wheelRay;
                _segments[iRay].Origin += distFwd * wheelForward + zOffset * wheelUp;

                if (collisionSystem.SegmentIntersect(out _fracs[iRay], out _otherSkins[iRay],
                                                 out _groundPositions[iRay], out _groundNormals[iRay], _segments[iRay], _predicate))
                {
                    _lastOnFloor = true;

                    if (_fracs[iRay] < _fracs[bestIRay])
                        bestIRay = iRay;
                }
            }

            
            if (!_lastOnFloor)
                return false;

            // use the best one
            Vector3 groundPos = _groundPositions[bestIRay];
            float frac = _fracs[bestIRay];
            CollisionSkin otherSkin = _otherSkins[bestIRay];

            Vector3 groundNormal = worldAxisUp;

            if (numberOfRaysToUse > 1)
            {
                for (iRay = 0; iRay < numberOfRaysToUse; ++iRay)
                {
                    if (_fracs[iRay] <= 1.0f)
                    {
                        groundNormal += (1.0f - _fracs[iRay]) * (worldPosition - _segments[iRay].GetEnd());
                    }
                }

                JiggleMath.NormalizeSafe(ref groundNormal);
            }
            else
            {
                groundNormal = _groundNormals[bestIRay];
            }

            
            Body worldBody = otherSkin.Owner;

            _displacement = rayLen * (1.0f - frac);
            _displacement = MathHelper.Clamp(_displacement, 0, _travel);

            float displacementForceMag = _displacement * _spring;

            // reduce force when suspension is par to ground
            displacementForceMag *= Vector3.Dot(_groundNormals[bestIRay], worldAxisUp);

            // apply damping
            float dampingForceMag = _upSpeed * _damping;

            float totalForceMag = displacementForceMag + dampingForceMag;

            if (totalForceMag < 0.0f) totalForceMag = 0.0f;

            Vector3 extraForce = totalForceMag * worldAxisUp;

            force += extraForce;

            // side-slip friction and drive force. Work out wheel- and floor-relative coordinate frame
            Vector3 groundUp = groundNormal;
            Vector3 groundLeft = Vector3.Cross(groundNormal, wheelForward);
            JiggleMath.NormalizeSafe(ref groundLeft);

            Vector3 groundFwd = Vector3.Cross(groundLeft, groundUp);

            Vector3 wheelPointVel = parentBody.Velocity +
                                    Vector3.Cross(parentBody.AngularVelocity, Vector3.TransformCoordinate( _position, parentBody.Orientation));// * mPos);

            Vector3 rimVel = _angularVelocity * Vector3.Cross(wheelLeft, groundPos - worldPosition);
            wheelPointVel += rimVel;

            // if sitting on another body then adjust for its velocity.
            if (worldBody != null)
            {
                Vector3 worldVel = worldBody.Velocity +
                 Vector3.Cross(worldBody.AngularVelocity, groundPos - worldBody.Position);

                wheelPointVel -= worldVel;
            }

            // sideways forces
            float noslipVel = 0.2f;
            float slipVel = 0.4f;
            float slipFactor = 0.7f;

            float smallVel = 3;
            float friction = _sideFriction;

            float sideVel = Vector3.Dot(wheelPointVel, groundLeft);

            if ((sideVel > slipVel) || (sideVel < -slipVel))
                friction *= slipFactor;
            else
                if ((sideVel > noslipVel) || (sideVel < -noslipVel))
                    friction *= 1.0f - (1.0f - slipFactor) * (System.Math.Abs(sideVel) - noslipVel) / (slipVel - noslipVel);

            if (sideVel < 0.0f)
                friction *= -1.0f;

            if (System.Math.Abs(sideVel) < smallVel)
                friction *= System.Math.Abs(sideVel) / smallVel;

            float sideForce = -friction * totalForceMag;

            extraForce = sideForce * groundLeft;
            force += extraForce;

            // fwd/back forces
            friction = _forwardFriction;
            float fwdVel = Vector3.Dot(wheelPointVel, groundFwd);

            if ((fwdVel > slipVel) || (fwdVel < -slipVel))
                friction *= slipFactor;
            else
                if ((fwdVel > noslipVel) || (fwdVel < -noslipVel))
                    friction *= 1.0f - (1.0f - slipFactor) * (System.Math.Abs(fwdVel) - noslipVel) / (slipVel - noslipVel);

            if (fwdVel < 0.0f)
                friction *= -1.0f;

            if (System.Math.Abs(fwdVel) < smallVel)
                friction *= System.Math.Abs(fwdVel) / smallVel;

            float fwdForce = -friction * totalForceMag;

            extraForce = fwdForce * groundFwd;
            force += extraForce;

            // fwd force also spins the wheel
            Vector3 wheelCentreVel = parentBody.Velocity +
                                     Vector3.Cross(parentBody.AngularVelocity, Vector3.TransformCoordinate(_position, parentBody.Orientation));// * mPos);

            _angularVelocityForGrip = Vector3.Dot(wheelCentreVel, groundFwd) / _radius;
            _torque += -fwdForce * _radius;

            // add force to car
            parentBody.AddWorldForce(force, groundPos);

            // add force to the world
            if (worldBody != null && !worldBody.Immovable)
            {
                // todo get the position in the right place...
                // also limit the velocity that this force can produce by looking at the 
                // mass/inertia of the other object
                float maxOtherBodyAcc = 500.0f;
                float maxOtherBodyForce = maxOtherBodyAcc * worldBody.Mass;

                if (force.LengthSquared() > (maxOtherBodyForce * maxOtherBodyForce))
                    force *= maxOtherBodyForce / force.Length();

                worldBody.AddWorldForce(-force, groundPos);
            }

            return true;
        }

        /// <summary>
        /// Updates the rotational state etc
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt)
        {
            if (dt <= 0.0f)
                return;

            float origAngVel = _angularVelocity;
            _upSpeed = (_displacement - _lastDisplacement) / System.Math.Max(dt, JiggleMath.Epsilon);

            if (_locked)
            {
                _angularVelocity = 0;
                _torque = 0;
            }
            else
            {
                _angularVelocity += _torque * dt / _inertia;
                _torque = 0;

                // prevent friction from reversing dir - todo do this better
                // by limiting the torque
                if (((origAngVel > _angularVelocityForGrip) && (_angularVelocity < _angularVelocityForGrip)) ||
                     ((origAngVel < _angularVelocityForGrip) && (_angularVelocity > _angularVelocityForGrip)))
                    _angularVelocity = _angularVelocityForGrip;

                _angularVelocity += _driveTorque * dt / _inertia;
                _driveTorque = 0;

                float maxAngVel = 200;
                _angularVelocity = MathHelper.Clamp(_angularVelocity, -maxAngVel, maxAngVel);

                _axisAngle += MathHelper.ToDegrees(dt * _angularVelocity);
            }
        }

        /// <summary>
        /// power
        /// </summary>
        /// <param name="torque"></param>
        public void AddTorque(float torque)
        {
            _driveTorque += torque;
        }
    }

    /// Predicate for the wheel->world intersection test
    class WheelPredicate : CollisionSkinPredicate1
    {
        CollisionSkin mSkin;

        public  WheelPredicate(CollisionSkin carSkin)
        {
            mSkin = carSkin;
        }

        public override bool ConsiderSkin(CollisionSkin skin)
        {
            return (skin.ID != mSkin.ID);
        }
    }

}
