using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

namespace JigLibSDX_CSE
{
    public class ObservableMesh : Mesh, INotifyPropertyChanged
    {
        public ObservableMesh(Device device, int faceCount, int vertexCount, MeshFlags options, VertexElement[] vertexDeclaration)
            : base(device, faceCount, vertexCount, options, vertexDeclaration) { }

        public ObservableMesh(Device device, int faceCount, int vertexCount, MeshFlags options, VertexFormat fvf)
            : base(device, faceCount, vertexCount, options, fvf) { }
        
        public ObservableMesh(IntPtr pointer) : base(pointer) { }

        public event PropertyChangedEventHandler PropertyChanged;

        

        // Create the OnPropertyChanged method to raise the event
        protected void NotifyPropertyChanged(string name)
        {            
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
