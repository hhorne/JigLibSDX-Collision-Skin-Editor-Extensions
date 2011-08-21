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
    public abstract class Figment : IDisposable
    {
        private Part _mainPart;
        private Hashtable _parts;

        #region Properties
        public Part MainPart
        {
            get { return _mainPart; }
        }
        #endregion

        public Figment()
        {
            _parts = new Hashtable();
        }

        #region RelatedParts Controlling
        public Part GetPart(string key)
        {
            if (_parts.ContainsKey(key))
            {
                return (Part)_parts[key];
            }
            else
            {
                return null;
            }
        }

        public void AddPart(string key, Part part, bool asMainPart)
        {
            if (_parts.ContainsKey(key))
            {
                Part existingPart = (Part)_parts[key];

                if (existingPart == part)
                {
                    part = (Part)_parts[key];
                }
                else
                {
                    existingPart.DisableRelatedControllers();
                    _parts[key] = part;
                }
            }
            else
            {
                _parts.Add(key, part);
            }

            if (asMainPart)
            {
                _mainPart = part;
            }
        }

        public void RemovePart(string key)
        {
            if (_parts.ContainsKey(key))
            {
                Part part = (Part)_parts[key];
                part.DisableRelatedControllers();

                _parts.Remove(key);
            }
        }

        public void RemovePart(Part part)
        {
            if (_parts.ContainsValue(part))
            {
                part.DisableRelatedControllers();

                object key = GetRelatedPartKey(part);
                if (key != null)
                {
                    _parts.Remove(key);
                }
            }
        }

        private object GetRelatedPartKey(Part part)
        {
            if (_parts.ContainsValue(part))
            {
                IDictionaryEnumerator enumerator = _parts.GetEnumerator();
                Part tempPart;

                while (enumerator.MoveNext())
                {
                    tempPart = (Part)enumerator.Value;

                    if (tempPart == part)
                    {
                        return enumerator.Key;
                    }
                }
            }

            return null;
        }

        public void EnableRelatedParts()
        {
            IDictionaryEnumerator enumerator = _parts.GetEnumerator();
            Part part;

            while (enumerator.MoveNext())
            {
                part = (Part)enumerator.Value;
                part.EnableRelatedControllers();
            }
        }

        public void DisableRelatedParts()
        {
            IDictionaryEnumerator enumerator = _parts.GetEnumerator();
            Part part;

            while (enumerator.MoveNext())
            {
                part = (Part)enumerator.Value;
                part.DisableRelatedControllers();
            }
        }
        #endregion

        #region IDisposable Member
        public virtual void Dispose()
        {
            if (_parts != null)
            {
                _parts.Clear();
                _parts = null;
            }
        }
        #endregion
    }
}
