#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using JigLibSDX.Geometry;
using JigLibSDX.Math;
using JigLibSDX.Utils;
#endregion

namespace JigLibSDX.Collision
{

    /// <summary>
    /// DetectFunctor for CapsuleCapsule collison detection.
    /// </summary>
    public class CollDetectCylinderCylinder : DetectFunctor
    {
        private Random random = new Random();

        public CollDetectCylinderCylinder()
            : base("CylinderCylinder", (int)PrimitiveType.Cylinder, (int)PrimitiveType.Cylinder)
        {
        }

        public override void CollDetect(CollDetectInfo info, float collTolerance, CollisionFunctor collisionFunctor)
        {
            Vector3 body0Pos = (info.Skin0.Owner != null) ? info.Skin0.Owner.OldPosition : Vector3.Zero;
            Vector3 body1Pos = (info.Skin1.Owner != null) ? info.Skin1.Owner.OldPosition : Vector3.Zero;

            // todo - proper swept test
            #region Get Cylinders
            Cylinder oldCylinder0 = (Cylinder)info.Skin0.GetPrimitiveOldWorld(info.IndexPrim0);
            Cylinder newCylinder0 = (Cylinder)info.Skin0.GetPrimitiveNewWorld(info.IndexPrim0);
            Cylinder oldCylinder1 = (Cylinder)info.Skin1.GetPrimitiveOldWorld(info.IndexPrim1);
            Cylinder newCylinder1 = (Cylinder)info.Skin1.GetPrimitiveNewWorld(info.IndexPrim1);
            #endregion

            Segment oldSeg0 = new Segment(oldCylinder0.Position, oldCylinder0.Length * MatrixHelper.GetBackward(oldCylinder0.Orientation));
            Segment newSeg0 = new Segment(newCylinder0.Position, newCylinder0.Length * MatrixHelper.GetBackward(newCylinder0.Orientation));
            Segment oldSeg1 = new Segment(oldCylinder1.Position, oldCylinder1.Length * MatrixHelper.GetBackward(oldCylinder1.Orientation));
            Segment newSeg1 = new Segment(newCylinder1.Position, newCylinder1.Length * MatrixHelper.GetBackward(newCylinder1.Orientation));

            float radSum = newCylinder0.Radius + newCylinder1.Radius;

            float oldt0, oldt1;
            float newt0, newt1;
            float oldDistSq = Distance.SegmentSegmentDistanceSq(out oldt0, out oldt1, oldSeg0, oldSeg1);
            float newDistSq = Distance.SegmentSegmentDistanceSq(out newt0, out newt1, newSeg0, newSeg1);

            if (System.Math.Min(oldDistSq, newDistSq) < ((radSum + collTolerance) * (radSum + collTolerance)))
            {
                Vector3 pos0 = oldSeg0.GetPoint(oldt0);
                Vector3 pos1 = oldSeg1.GetPoint(oldt1);

                Vector3 delta = pos0 - pos1;

                float dist = (float)System.Math.Sqrt((float)oldDistSq);
                float depth = radSum - dist;

                if (dist > JiggleMath.Epsilon)
                {
                    delta /= dist;
                }
                else
                {
                    // todo - make this not random
                    delta = Vector3.TransformCoordinate(Vector3Helper.Backward, Matrix.RotationAxis(Vector3Helper.Up, MathHelper.ToRadians(random.Next(360))));
                }

                Vector3 worldPos = pos1 +
                    (oldCylinder1.Radius - 0.5f * depth) * delta;

                unsafe
                {
                    SmallCollPointInfo collInfo = new SmallCollPointInfo(worldPos - body0Pos, worldPos - body1Pos, depth);
                    collisionFunctor.CollisionNotify(ref info, ref delta, &collInfo, 1);
                }

            }


        }
    }
}
