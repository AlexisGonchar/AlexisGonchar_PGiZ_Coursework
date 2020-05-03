using SharpDX;
using System;
using System.Collections.Generic;

namespace Template
{
    public class Zombie : IModel
    {
        public Dictionary<String, MeshObject> MeshObjects;
        public Zombie(Dictionary<String, MeshObject> meshes)
        {
            MeshObjects = meshes;
            ZombieUp();
        }

        private void ZombieUp()
        {
            foreach (MeshObject meshObject in MeshObjects.Values)
            {
                meshObject.MoveBy(0, 2.6f, 0);
            }
        }

        public void RotateToPlayer(Character c)
        {
            MeshObject zombie = MeshObjects["head"];
            Vector2 zv = new Vector2(zombie.Position.Z, zombie.Position.X);
            Vector2 cv = new Vector2(c.Position.Z, c.Position.X);
            Vector2 D = cv - zv;
            double angle = Math.PI - Math.Atan2(D.Y, -D.X);

            Vector4 tV;
            

            Vector4 a = c.Position - zombie.Position;
            float magnitude = (float)Math.Sqrt(a.X * a.X + a.Z * a.Z);
            if (magnitude <= 6f)
            {
                tV = zombie.Position;
            }
            else
            {
                tV = zombie.Position + a / magnitude * 0.15f;
            }
            foreach (MeshObject meshObject in MeshObjects.Values)
            {
                meshObject.Yaw = (float)angle;
                meshObject.MoveTo(tV.X, meshObject.Position.Y, tV.Z);
            }
        }

        public MeshObject GetLeftLeg()
        {
            return MeshObjects["legLeft"];
        }

        public MeshObject GetRightLeg()
        {
            return MeshObjects["legRight"];
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
            foreach (MeshObject meshObject in MeshObjects.Values)
            {
                meshObject.Render(renderer, viewMatrix, projectionMatrix);
            }
        }

        public void MoveBy(float dX, float dY, float dZ)
        {
            foreach (MeshObject meshObject in MeshObjects.Values)
            {
                meshObject.MoveBy(dX, dY, dZ);
            }
        }
    }
}
