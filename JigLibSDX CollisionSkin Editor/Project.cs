using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace JigLibSDX_CSE
{
    [XmlRoot("JigLibSDXProject")]
    public class Project : IDisposable
    {
        private string _filePath;
        private string _meshFilePath;
        private SlimDX.Direct3D9.Cull _cullMode;

        private JigLibSDX.Geometry.PrimitiveProperties _primitiveProperties;

        private int _selectedPrimitive;
        private List<CollisionPrimitiveInfo> _collisionPrimitiveInfos;

        [XmlIgnore]
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        [XmlElement("MeshFilePath")]
        public string MeshFilePath
        {
            get { return _meshFilePath; }
            set { _meshFilePath = value; }
        }

        [XmlElement("MeshCullMode")]
        public int MeshCullMode
        {
            get { return (int)_cullMode; }
            set { _cullMode = (SlimDX.Direct3D9.Cull)value; }
        }

        [XmlElement("PrimitiveProperties")]
        public JigLibSDX.Geometry.PrimitiveProperties PrimitiveProperties
        {
            get { return _primitiveProperties; }
            set { _primitiveProperties = value; }
        }

        [XmlElement("SelectedCollisionPrimitiveInfo")]
        public int SelectedCollisionPrimitiveInfo
        {
            get 
            {
                if (_collisionPrimitiveInfos == null || _selectedPrimitive < 0 
                    || _selectedPrimitive >= _collisionPrimitiveInfos.Count || _collisionPrimitiveInfos.Count == 0)
                {
                    _selectedPrimitive = -1;
                }

                return _selectedPrimitive; 
            }

            set 
            {
                if (_collisionPrimitiveInfos == null || value < 0 
                    || value > _collisionPrimitiveInfos.Count || _collisionPrimitiveInfos.Count == 0)
                {
                    _selectedPrimitive = -1;
                }
                else
                {
                    _selectedPrimitive = value;
                }
            }
        }

        [XmlElement("CollisionPrimitiveInfos")]
        public List<CollisionPrimitiveInfo> CollisionPrimitiveInfos
        {
            get { return _collisionPrimitiveInfos; }
            set { _collisionPrimitiveInfos = value; }
        }

        public Project()
        {
            _selectedPrimitive = -1;
            _collisionPrimitiveInfos = new List<CollisionPrimitiveInfo>();

            _primitiveProperties = new JigLibSDX.Geometry.PrimitiveProperties(JigLibSDX.Geometry.PrimitiveProperties.MassDistributionEnum.Solid,
                 JigLibSDX.Geometry.PrimitiveProperties.MassTypeEnum.Mass,
                 0.001f);
        }

        public void Dispose()
        {
            _collisionPrimitiveInfos = null;
        }
    }
}
