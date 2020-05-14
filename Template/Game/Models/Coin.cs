using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    class Coin : IModel
    {
        public MeshObject mesh;
        public Coin(MeshObject mesh)
        {
            this.mesh = mesh;
        }

        public bool CharacterCollision(Character c)
        {
            throw new NotImplementedException();
        }

        public void Render(Renderer renderer, Matrix viewMatrix, Matrix projectionMatrix)
        {
            mesh.Render(renderer, viewMatrix, projectionMatrix);
        }
    }
}
