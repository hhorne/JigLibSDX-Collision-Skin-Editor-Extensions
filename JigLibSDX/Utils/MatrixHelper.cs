using System;
using SlimDX;

namespace JigLibSDX.Utils
{
    public static class MatrixHelper
    {
        #region Get values
        #region Left
        public static Vector3 GetLeft(Matrix matrix)
        {
            Vector3 vector;

            vector.X = -matrix.M11;
            vector.Y = -matrix.M12;
            vector.Z = -matrix.M13;
            vector.Normalize();

            return vector;
        }

        public static Vector3 GetLeft(ref Matrix matrix)
        {
            Vector3 vector;

            vector.X = -matrix.M11;
            vector.Y = -matrix.M12;
            vector.Z = -matrix.M13;
            vector.Normalize();

            return vector;
        }

        public static void GetLeft(ref Matrix matrix, out Vector3 vector)
        {
            vector.X = -matrix.M11;
            vector.Y = -matrix.M12;
            vector.Z = -matrix.M13;
            vector.Normalize();
        }
        #endregion

        #region Right
        public static Vector3 GetRight(Matrix matrix)
        {
            Vector3 vector;

            vector.X = matrix.M11;
            vector.Y = matrix.M12;
            vector.Z = matrix.M13;
            vector.Normalize();

            return vector;
        }

        public static Vector3 GetRight(ref Matrix matrix)
        {
            Vector3 vector;

            vector.X = matrix.M11;
            vector.Y = matrix.M12;
            vector.Z = matrix.M13;
            vector.Normalize();

            return vector;
        }

        public static void GetRight(ref Matrix matrix, out Vector3 vector)
        {
            vector.X = matrix.M11;
            vector.Y = matrix.M12;
            vector.Z = matrix.M13;
            vector.Normalize();
        }
        #endregion

        #region Up
        public static Vector3 GetUp(Matrix matrix)
        {
            Vector3 vector;

            vector.X = matrix.M21;
            vector.Y = matrix.M22;
            vector.Z = matrix.M23;
            vector.Normalize();

            return vector;
        }

        public static Vector3 GetUp(ref Matrix matrix)
        {
            Vector3 vector;

            vector.X = matrix.M21;
            vector.Y = matrix.M22;
            vector.Z = matrix.M23;
            vector.Normalize();

            return vector;
        }

        public static void GetUp(ref Matrix matrix, out Vector3 vector)
        {
            vector.X = matrix.M21;
            vector.Y = matrix.M22;
            vector.Z = matrix.M23;
            vector.Normalize();
        }
        #endregion

        #region Down
        public static Vector3 GetDown(Matrix matrix)
        {
            Vector3 vector;

            vector.X = -matrix.M21;
            vector.Y = -matrix.M22;
            vector.Z = -matrix.M23;
            vector.Normalize();

            return vector;
        }

        public static Vector3 GetDown(ref Matrix matrix)
        {
            Vector3 vector;

            vector.X = -matrix.M21;
            vector.Y = -matrix.M22;
            vector.Z = -matrix.M23;
            vector.Normalize();

            return vector;
        }

        public static void GetDown(ref Matrix matrix, out Vector3 vector)
        {
            vector.X = -matrix.M21;
            vector.Y = -matrix.M22;
            vector.Z = -matrix.M23;
            vector.Normalize();
        }
        #endregion

        #region Forward
        public static Vector3 GetForward(Matrix matrix)
        {
            Vector3 vector;

            vector.X = -matrix.M31;
            vector.Y = -matrix.M32;
            vector.Z = -matrix.M33;
            vector.Normalize();

            return vector;
        }

        public static Vector3 GetForward(ref Matrix matrix)
        {
            Vector3 vector;

            vector.X = -matrix.M31;
            vector.Y = -matrix.M32;
            vector.Z = -matrix.M33;
            vector.Normalize();

            return vector;
        }

        public static void GetForward(ref Matrix matrix, out Vector3 vector)
        {
            vector.X = -matrix.M31;
            vector.Y = -matrix.M32;
            vector.Z = -matrix.M33;
            vector.Normalize();
        }
        #endregion

        #region Backward
        public static Vector3 GetBackward(Matrix matrix)
        {
            Vector3 vector;

            vector.X = matrix.M31;
            vector.Y = matrix.M32;
            vector.Z = matrix.M33;
            vector.Normalize();

            return vector;
        }

        public static Vector3 GetBackward(ref Matrix matrix)
        {
            Vector3 vector;

            vector.X = matrix.M31;
            vector.Y = matrix.M32;
            vector.Z = matrix.M33;
            vector.Normalize();

            return vector;
        }

        public static void GetBackward(ref Matrix matrix, out Vector3 vector)
        {
            vector.X = matrix.M31;
            vector.Y = matrix.M32;
            vector.Z = matrix.M33;
            vector.Normalize();
        }
        #endregion

        #region Translation
        public static Vector3 GetTranslation(Matrix matrix)
        {
            Vector3 vector;

            vector.X = matrix.M41;
            vector.Y = matrix.M42;
            vector.Z = matrix.M43;
            vector.Normalize();

            return vector;
        }

        public static Vector3 GetTranslation(ref Matrix matrix)
        {
            Vector3 vector;

            vector.X = matrix.M41;
            vector.Y = matrix.M42;
            vector.Z = matrix.M43;
            vector.Normalize();

            return vector;
        }

        public static void GetTranslation(ref Matrix matrix, out Vector3 vector)
        {
            vector.X = matrix.M41;
            vector.Y = matrix.M42;
            vector.Z = matrix.M43;
            vector.Normalize();
        }
        #endregion
        #endregion

        #region Set values
        #region LeftRight
        public static void SetLeftRight(ref Matrix matrix, ref Vector3 leftRight)
        {
            matrix.M11 = leftRight.X;
            matrix.M12 = leftRight.Y;
            matrix.M13 = leftRight.Z;
        }

        public static void SetLeftRight(ref Matrix matrix, Vector3 leftRight)
        {
            matrix.M11 = leftRight.X;
            matrix.M12 = leftRight.Y;
            matrix.M13 = leftRight.Z;
        }

        public static Matrix SetLeftRight(Matrix matrix, ref Vector3 leftRight)
        {
            matrix.M11 = leftRight.X;
            matrix.M12 = leftRight.Y;
            matrix.M13 = leftRight.Z;
            return matrix;
        }

        public static Matrix SetLeftRight(Matrix matrix, Vector3 leftRight)
        {
            matrix.M11 = leftRight.X;
            matrix.M12 = leftRight.Y;
            matrix.M13 = leftRight.Z;
            return matrix;
        }
        #endregion

        #region UpDown
        public static void SetUpDown(ref Matrix matrix, ref Vector3 upDown)
        {
            matrix.M21 = upDown.X;
            matrix.M22 = upDown.Y;
            matrix.M23 = upDown.Z;
        }

        public static void SetUpDown(ref Matrix matrix, Vector3 upDown)
        {
            matrix.M21 = upDown.X;
            matrix.M22 = upDown.Y;
            matrix.M23 = upDown.Z;
        }

        public static Matrix SetUpDown(Matrix matrix, ref Vector3 upDown)
        {
            matrix.M21 = upDown.X;
            matrix.M22 = upDown.Y;
            matrix.M23 = upDown.Z;
            return matrix;
        }

        public static Matrix SetUpDown(Matrix matrix, Vector3 upDown)
        {
            matrix.M21 = upDown.X;
            matrix.M22 = upDown.Y;
            matrix.M23 = upDown.Z;
            return matrix;
        }
        #endregion

        #region ForwardBackward
        public static void SetForwardBackward(ref Matrix matrix, ref Vector3 forwardBackward)
        {
            matrix.M31 = forwardBackward.X;
            matrix.M32 = forwardBackward.Y;
            matrix.M33 = forwardBackward.Z;
        }

        public static void SetForwardBackward(ref Matrix matrix, Vector3 forwardBackward)
        {
            matrix.M31 = forwardBackward.X;
            matrix.M32 = forwardBackward.Y;
            matrix.M33 = forwardBackward.Z;
        }

        public static Matrix SetForwardBackward(Matrix matrix, ref Vector3 forwardBackward)
        {
            matrix.M31 = forwardBackward.X;
            matrix.M32 = forwardBackward.Y;
            matrix.M33 = forwardBackward.Z;
            return matrix;
        }

        public static Matrix SetForwardBackward(Matrix matrix, Vector3 forwardBackward)
        {
            matrix.M31 = forwardBackward.X;
            matrix.M32 = forwardBackward.Y;
            matrix.M33 = forwardBackward.Z;
            return matrix;
        }
        #endregion

        #region Translation
        public static void SetTranslation(ref Matrix matrix, ref Vector3 vector)
        {
            matrix.M41 = vector.X;
            matrix.M42 = vector.Y;
            matrix.M43 = vector.Z;
        }

        public static void SetTranslation(ref Matrix matrix, Vector3 vector)
        {
            matrix.M41 = vector.X;
            matrix.M42 = vector.Y;
            matrix.M43 = vector.Z;
        }

        public static Matrix SetTranslation(Matrix matrix, ref Vector3 vector)
        {
            matrix.M41 = vector.X;
            matrix.M42 = vector.Y;
            matrix.M43 = vector.Z;
            return matrix;
        }

        public static Matrix SetTranslation(Matrix matrix, Vector3 vector)
        {
            matrix.M41 = vector.X;
            matrix.M42 = vector.Y;
            matrix.M43 = vector.Z;
            return matrix;
        }
        #endregion
        #endregion
    }
}
