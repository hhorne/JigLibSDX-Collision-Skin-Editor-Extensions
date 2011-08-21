using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;

using JigLibSDX;
using JigLibSDX.Physics;
using JLG = JigLibSDX.Geometry; //Has conflicts with SlimDX.
using JigLibSDX.Collision;
using JigLibSDX.Math;
using JigLibSDX.Utils;

using SlimDX;
using SlimDX.Direct3D9;

namespace JigLibSDX.Physics
{
    public abstract class Part : Body, IDisposable
    {
        // Physics
        private JLG.PrimitiveProperties _primitiveProperties;
        private MassProperties _massProperties;
        private BasicController _mainController;

        // Controllers
        private Hashtable _relatedControllers;

        #region Properties
        public JLG.PrimitiveProperties PrimitiveProperties
        {
            get { return _primitiveProperties; }
            protected set { _primitiveProperties = value; }
        }

        public MassProperties MassProperties
        {
            get { return _massProperties; }
            protected set { _massProperties = value; }
        }

        public BasicController MainController
        {
            get { return _mainController; }
            protected set { _mainController = value; }
        }

        public Matrix WorldMatrix
        {
            get { return transform.Orientation * Matrix.Translation(transform.Position - _massProperties.CenterOfMass); }
        }
        #endregion

        /// <summary>
        /// Creates a part.
        /// </summary>
        /// <param name="primitiveProperties">Properties of the body's shape.</param>
        /// <param name="massProperties">Properties of the body's mass.</param>
        /// <param name="withMainController">Adds a main BasicController for this part.</param>
        public Part(JLG.PrimitiveProperties primitiveProperties, MassProperties massProperties, bool withMainController) : base()
        {
            _primitiveProperties = primitiveProperties;
            _massProperties = massProperties;

            if (withMainController)
            {
                // Create Main Controller
                _mainController = new BasicController(this);

                if (PhysicsSystem.CurrentPhysicsSystem != null)
                {
                    PhysicsSystem.CurrentPhysicsSystem.AddController(_mainController);
                }
            }

            _relatedControllers = new Hashtable();
        }

        #region RelatedControllers Controlling
        public Controller GetController(string key)
        {
            if (_relatedControllers.ContainsKey(key))
            {
                return (Controller)_relatedControllers[key];
            }
            else
            {
                return null;
            }
        }

        public void AddController(string key, Controller controller, bool enableController)
        {
            if (_relatedControllers.ContainsKey(key))
            {
                Controller existingController = (Controller)_relatedControllers[key];

                if (existingController == controller)
                {
                    controller = (Controller)_relatedControllers[key];
                }
                else
                {
                    existingController.DisableController();
                    _relatedControllers[key] = controller;
                }
            }
            else
            {
                _relatedControllers.Add(key, controller);
            }

            if (enableController)
            {
                controller.EnableController();
            }
            else
            {
                controller.DisableController();
            }
        }

        public void RemoveController(string key)
        {
            if (_relatedControllers.ContainsKey(key))
            {
                Controller controller = (Controller)_relatedControllers[key];
                controller.DisableController();

                _relatedControllers.Remove(key);
            }
        }

        public void RemoveController(Controller controller)
        {
            if (_relatedControllers.ContainsValue(controller))
            {
                controller.DisableController();

                object key = GetRelatedControllerKey(controller);
                if (key != null)
                {
                    _relatedControllers.Remove(key);
                }
            }
        }

        private object GetRelatedControllerKey(Controller controller)
        {
            if (_relatedControllers.ContainsValue(controller))
            {
                IDictionaryEnumerator enumerator = _relatedControllers.GetEnumerator();
                Controller tempController;

                while (enumerator.MoveNext())
                {
                    tempController = (Controller)enumerator.Value;

                    if (tempController == controller)
                    {
                        return enumerator.Key;
                    }
                }
            }

            return null;
        }

        public void EnableRelatedControllers()
        {
            IDictionaryEnumerator enumerator = _relatedControllers.GetEnumerator();
            Controller controller;

            while (enumerator.MoveNext())
            {
                controller = (Controller)enumerator.Value;
                controller.EnableController();
            }
        }

        public void DisableRelatedControllers()
        {
            IDictionaryEnumerator enumerator = _relatedControllers.GetEnumerator();
            Controller controller;

            while (enumerator.MoveNext())
            {
                controller = (Controller)enumerator.Value;
                controller.DisableController();
            }
        }
        #endregion

        #region IDisposable Member
        public virtual void Dispose()
        {
            _mainController.DisableController();
            _mainController = null;

            IDictionaryEnumerator enumerator = _relatedControllers.GetEnumerator();
            Controller controller;

            while (enumerator.MoveNext())
            {
                controller = (Controller)enumerator.Value;
                controller.DisableController();
            }

            if (_relatedControllers != null)
            {
                _relatedControllers.Clear();
                _relatedControllers = null;
            }
        }
        #endregion
    }
}
