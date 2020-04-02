using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    public class Chest : IModel
    {
        public Dictionary<String, MeshObject> meshObjects;

        public Chest(Dictionary<String, MeshObject> meshes)
        {
            meshObjects = meshes;
            ChestUp();
        }

        private void ChestUp()
        {
            foreach (MeshObject meshObject in meshObjects.Values)
            {
                meshObject.MoveBy(0, 1.31f, 0);
            }
        }

        public bool CharacterCollision(Character c)
        {
            throw new NotImplementedException();
        }

        public void Render(Renderer renderer, Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach (MeshObject meshObject in meshObjects.Values)
            {
                meshObject.Render(renderer, viewMatrix, projectionMatrix);
            }
        }
    }
}
