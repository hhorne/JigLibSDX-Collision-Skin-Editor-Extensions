using System;
using SlimDX;
using JigLibSDX.Utils;

namespace JigLibSDX.Math
{
    public static class MathHelper
    {
        public const float TwoPi = (float)(System.Math.PI * 2.0);
        public const float Pi = (float)(System.Math.PI);
        public const float PiOver2 = (float)(System.Math.PI / 2.0);
        public const float PiOver4 = (float)(System.Math.PI / 4.0);

        public const float DegreesToRadiansRatio = (float)(180.0 / System.Math.PI);
        public const float RadiansToDegreesRatio = (float)(1.0 / (180.0 / System.Math.PI));

        #region Temp Variables
        private static Vector3 _tempVector3;
        private static float _tempFloat;
        #endregion

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static float Min(float value1, float value2)
        {
            if (value1 <= value2)
            {
                return value1;
            }
            else
            {
                return value2;
            }
        }

        public static float Max(float value1, float value2)
        {
            if (value1 >= value2)
            {
                return value1;
            }
            else
            {
                return value2;
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value <= max)
            {
                _tempFloat = value; 
            }
            else
            {
                _tempFloat = max;
            }

            if (_tempFloat >= min)
            {
                return _tempFloat;
            }
            else
            {
                return min;
            }
        }

        public static float Distance(float value1, float value2)
        {
            _tempFloat = value1 - value2;

            if (_tempFloat >= 0)
            {
                return _tempFloat;
            }
            else
            {
                return -_tempFloat;
            }
        }

        public static float ToRadians(float degrees)
        {
            return degrees * RadiansToDegreesRatio;
        }

        public static float ToDegrees(float radians)
        {
            return radians * DegreesToRadiansRatio;
        }

        public static float VectorToRadians(Vector2 vector)
        {
            return (float)System.Math.Atan2(vector.X, -(double)vector.Y);
        }

        public static void VectorToRadians(Vector3 vector, out float upDownRadians, out float leftRightRadians)
        {
            upDownRadians = (float)System.Math.Acos(vector.Y / vector.Length());
            leftRightRadians = (float)System.Math.Atan2(vector.Z, vector.X);
        }

        public static Vector2 RadiansToVector(float radians)
        {
            return new Vector2((float)System.Math.Sin(radians), -(float)System.Math.Cos(radians));
        }

        public static void RadiansToVector(float radians, ref Vector2 vector)
        {
            vector.X = (float)System.Math.Sin(radians);
            vector.Y = -(float)System.Math.Cos(radians);
        }

        public static Vector3 RadiansToVector(float upDownRadians, float leftRightRadians)
        {
            _tempVector3.X = (float)System.Math.Cos((double)leftRightRadians) * (float)System.Math.Sin((double)upDownRadians);
            _tempVector3.Y = (float)System.Math.Cos((double)upDownRadians);
            _tempVector3.Z = (float)System.Math.Sin((double)leftRightRadians) * (float)System.Math.Sin((double)upDownRadians); 

            return _tempVector3;
        }

        public static void RotateVector(ref Vector2 vector, float radians)
        {
            float length = vector.Length();
            float newRadians = (float)System.Math.Atan2(vector.X, -(double)vector.Y) + radians;

            vector.X = (float)System.Math.Sin(newRadians) * length;
            vector.Y = -(float)System.Math.Cos(newRadians) * length;
        }

        public static Vector3 QuaternionToEulerAngle(Quaternion rotation)
        {
            Matrix rotationMatrix = Matrix.RotationQuaternion(rotation);

            Vector3 rotationAxes = new Vector3();
            Vector3 forward = Vector3.TransformNormal(Vector3Helper.Forward, rotationMatrix);
            Vector3 up = Vector3.TransformNormal(Vector3Helper.Up, rotationMatrix);

            rotationAxes.X = (float)System.Math.Asin(forward.Y);
            rotationAxes.Y = (float)System.Math.Atan2((double)-forward.X, (double)-forward.Z);

            if (rotationAxes.X == MathHelper.PiOver2)
            {
                rotationAxes.Y = (float)System.Math.Atan2((double)up.X, (double)up.Z);
                rotationAxes.Z = 0;
            }
            else if (rotationAxes.X == -MathHelper.PiOver2)
            {
                rotationAxes.Y = (float)System.Math.Atan2((double)-up.X, (double)-up.Z);
                rotationAxes.Z = 0;
            }
            else
            {
                up = Vector3.TransformNormal(up, Matrix.RotationY(-rotationAxes.Y));
                up = Vector3.TransformNormal(up, Matrix.RotationX(-rotationAxes.X));

                rotationAxes.Z = (float)System.Math.Atan2((double)-up.X, (double)up.Y);
            }

            return rotationAxes;
        }

        public static void QuaternionToEulerAngle(Quaternion rotation, out float yaw, out float pitch, out float roll)
        {            
            Matrix rotationMatrix = Matrix.RotationQuaternion(rotation);

            Vector3 forward = Vector3.TransformNormal(Vector3Helper.Forward, rotationMatrix);
            Vector3 up = Vector3.TransformNormal(Vector3Helper.Up, rotationMatrix);

            yaw = (float)System.Math.Asin(forward.Y);
            pitch = (float)System.Math.Atan2((double)-forward.X, (double)-forward.Z);

            if (yaw == MathHelper.PiOver2)
            {
                pitch = (float)System.Math.Atan2((double)up.X, (double)up.Z);
                roll = 0;
            }
            else if (yaw == -MathHelper.PiOver2)
            {
                pitch = (float)System.Math.Atan2((double)-up.X, (double)-up.Z);
                roll = 0;
            }
            else
            {
                up = Vector3.TransformNormal(up, Matrix.RotationY(-pitch));
                up = Vector3.TransformNormal(up, Matrix.RotationX(-yaw));

                roll = (float)System.Math.Atan2((double)-up.X, (double)up.Y);
            }
        }

        //Yeah this is probably not the best solution, but usualy values are just on step over max or min.
        public static float Loop(float value, float min, float max)
        {
            if (max < min)
            {
                float temp = max;
                max = min;
                min = temp;
            }
            else if (max == min)
            {
                return max;
            }

            float range = max - min;

            while (value > max)
            {
                value -= range;
            }

            while (value < min)
            {
                value += range;
            }

            return value;
        }
    }
}
