using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Graphics
{
    public class MeshObjects : IDisposable
    {
        private List<MeshObject> _objects;
        public int Count { get => _objects.Count; }
        public MeshObject this[int index] { get { return _objects[index]; } }
        public MeshObject this[string name]
        {
            get
            {
                foreach (MeshObject meshObject in _objects)
                {
                    if (meshObject.Name == name) return meshObject;
                }
                return null;
            }
        }

        public MeshObjects()
        {
            _objects = new List<MeshObject>(2000);
        }

        public void Add(MeshObject meshObject)
        {
            _objects.Add(meshObject);
            meshObject.Index = _objects.Count - 1;
        }

        public void Dispose()
        {
            for (int i = _objects.Count - 1; i >= 0; --i)
            {
                MeshObject meshObject = _objects[i];
                _objects.RemoveAt(i);
                meshObject.Dispose();
            }
        }
    }
}
