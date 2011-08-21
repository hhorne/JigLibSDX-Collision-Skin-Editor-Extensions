///                                                              ///
/// THIS FILE CONTAINS ADJUSTED FARSEER PHYSICS 2.1 (Ms-PL) CODE ///
///                          http://farseerphysics.codeplex.com/ ///
///                                                              ///

using System;
using System.Collections.Generic;

namespace JigLibSDX.Physics
{
    public abstract class SpringJointBase : Joint
    {
        #region Events
        public event EventHandler<EventArgs> Broke;
        #endregion

        #region Attributes
        private float _springConstant;
        private float _dampingConstant;
        private float _breakPoint = float.MaxValue;
        private float _springError;
        #endregion

        #region Properties
        public float SpringConstant
        {
            get { return _springConstant; }
            set
            {
                if (value >= 0f)
                {
                    _springConstant = value;
                }
                else
                {
                    _springConstant = 0f;
                }
            }
        }

        public float DampingConstant
        {
            get { return _dampingConstant; }
            set
            {
                if (value >= 0f)
                {
                    _dampingConstant = value;
                }
                else
                {
                    _dampingConstant = 0f;
                }
            }
        }

        public float BreakPoint
        {
            get { return _breakPoint; }
            set 
            {
                if (value >= 0f)
                {
                    _breakPoint = value;
                }
                else
                {
                    _breakPoint = 0f;
                }
            }
        }

        public float SpringError
        {
            get { return _springError; }
            protected set { _springError = value; }
        }
        #endregion

        public SpringJointBase(float springConstant, float dampingConstant)
        {
            SpringConstant = springConstant;
            DampingConstant = dampingConstant;
        }

        public SpringJointBase(float springConstant, float dampingConstant, float breakPoint)
        {
            SpringConstant = springConstant;
            DampingConstant = dampingConstant;
            BreakPoint = breakPoint;
        }

        public override void UpdateController(float dt)
        {
            // Leave if there is no reason to update.
            if (dt == 0f) return;

            // Raise "Broke" ?
            if ((float)System.Math.Abs(_springError) > _breakPoint)
            {
                if (Broke != null)
                {
                    Broke(this, EventArgs.Empty);
                }

                this.DisableController();
            }
        }
    }
}
