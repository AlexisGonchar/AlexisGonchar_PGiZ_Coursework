using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    public class HUDModel
    {
        public MeshObject mesh;

        public HUDModel(MeshObject mesh)
        {
            this.mesh = mesh;
        }

        public void Render(Renderer renderer, Matrix viewMatrix, Matrix projectionMatrix)
        {
            mesh.Render(renderer, viewMatrix, projectionMatrix);
        }
    }
}
