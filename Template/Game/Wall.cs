using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template
{
    public class Wall
    {
        public MeshObject Mesh;
        public BoundingBox BoxCollider;

        public Wall (MeshObject mesh, BoundingBox box)
        {
            Mesh = mesh;
            BoxCollider = box;
        }
    }
}
