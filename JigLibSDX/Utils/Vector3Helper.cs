using System;
using SlimDX;

namespace JigLibSDX.Utils
{
    public static class Vector3Helper
    {
        public static readonly Vector3 Left = -Vector3.UnitX;      //new Vector3(-1f, 0f, 0f);
        public static readonly Vector3 Right = Vector3.UnitX;      //new Vector3(1f, 0f, 0f);
        public static readonly Vector3 Up = Vector3.UnitY;         //new Vector3(0f, 1f, 0f);
        public static readonly Vector3 Down = -Vector3.UnitY;      //new Vector3(0f, -1f, 0f);
        public static readonly Vector3 Forward = -Vector3.UnitZ;   //new Vector3(0f, 0f, -1f);
        public static readonly Vector3 Backward = Vector3.UnitZ;   //new Vector3(0f, 0f, 1f);

        #region Set
        public static void SetLeft(ref Vector3 vector)
        {
            vector.X = Left.X;
            vector.Y = Left.Y;
            vector.Z = Left.Z;
        }
        
        public static void SetRight(ref Vector3 vector)
        {
            vector.X = Right.X;
            vector.Y = Right.Y;
            vector.Z = Right.Z;
        }
        
        public static void SetUp(ref Vector3 vector)
        {
            vector.X = Up.X;
            vector.Y = Up.Y;
            vector.Z = Up.Z;
        }
        
        public static void SetDown(ref Vector3 vector)
        {
            vector.X = Down.X;
            vector.Y = Down.Y;
            vector.Z = Down.Z;
        }
        
        public static void SetForward(ref Vector3 vector)
        {
            vector.X = Forward.X;
            vector.Y = Forward.Y;
            vector.Z = Forward.Z;
        }
        
        public static void SetBackward(ref Vector3 vector)
        {
            vector.X = Backward.X;
            vector.Y = Backward.Y;
            vector.Z = Backward.Z;
        }
        #endregion
    }
}
