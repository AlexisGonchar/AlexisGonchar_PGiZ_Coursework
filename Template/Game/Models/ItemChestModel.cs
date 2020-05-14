using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    public class ItemChestModel
    {
        public bool AnimateChest;
        public MeshObject mesh;
        public ItemChestModel(MeshObject mesh)
        {
            this.mesh = mesh;
            AnimateChest = false;
        }

        public void Render(Renderer renderer, Matrix viewMatrix, Matrix projectionMatrix)
        {
            mesh.Render(renderer, viewMatrix, projectionMatrix);
        }

        public void Animate()
        {
            mesh.Position = new Vector4(mesh.Position.X, mesh.Position.Y + 0.1f, mesh.Position.Z, 0);
            if (mesh.Position.Y > 4)
                AnimateChest = false;
        }
    }
}
