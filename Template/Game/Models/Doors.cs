using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    public class Doors : IModel
    {
        public MeshObject door1, door2;
        public BoundingBox BoxCollider;

        public Doors(MeshObject mesh1, MeshObject mesh2)
        {
            door1 = mesh1;
            door2 = mesh2;
            door2.Yaw = (float)Math.PI;
            BoxCollider = new BoundingBox();
        }

        public bool CharacterCollision(Character c)
        {
            bool collision = Collision.BoxIntersectsBox(ref BoxCollider, ref c.BoxCollider);
            return collision;
        }

        public void MoveBy(float dX, float dY, float dZ)
        {
            door1.MoveBy(dX + 2, dY, dZ);
            door2.MoveBy(dX + 8, dY, dZ);
        }

        public void Render(Renderer renderer, Matrix viewMatrix, Matrix projectionMatrix)
        {
            door1.Render(renderer, viewMatrix, projectionMatrix);
            door2.Render(renderer, viewMatrix, projectionMatrix);
        }
    }
}
