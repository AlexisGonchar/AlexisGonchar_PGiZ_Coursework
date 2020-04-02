using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    public class Zombie : IModel
    {
        public Dictionary<String, MeshObject> meshObjects;

        public Zombie(Dictionary<String, MeshObject> meshes)
        {
            meshObjects = meshes;
            ZombieUp();
        }

        private void ZombieUp()
        {
            foreach (MeshObject meshObject in meshObjects.Values)
            {
                meshObject.MoveBy(0, 2.6f, 0);
            }
        }

        public void RotateToPlayer(Camera c)
        {
            //float meshX = meshObjects["head"].Position.X == 0 ? meshObjects["head"].Position.X + 10 : meshObjects["head"].Position.X;
            //float meshY = meshObjects["head"].Position.Z == 0 ? meshObjects["head"].Position.Z + 10 : meshObjects["head"].Position.Z;
            //float plX = c.Position.X == 0 ? c.Position.X + 10 : c.Position.X;
            //float plY = c.Position.Z == 0 ? c.Position.Z + 10 : c.Position.Z;
            //float angle = (float)Math.Acos((-plX * meshX) / Math.Sqrt(Math.Pow(plX, 2) * (Math.Pow(meshX, 2) + Math.Pow(meshY, 2))));
            foreach (MeshObject meshObject in meshObjects.Values)
            {
                meshObject.Yaw = c.Yaw;
            }
        }

        public MeshObject GetLeftLeg()
        {
            return meshObjects["legLeft"];
        }

        public MeshObject GetRightLeg()
        {
            return meshObjects["legRight"];
        }

        public void Walk()
        {
            Animations.ZombieWalk(GetLeftLeg(), GetRightLeg());
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
