using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template
{
    public class Wall : IModel
    {
        public MeshObject Mesh;
        public BoundingBox BoxCollider;

        public Wall (MeshObject mesh, BoundingBox box)
        {
            Mesh = mesh;
            BoxCollider = box;
        }

        public bool CharacterCollision(Character c)
        {
            bool collision = Collision.BoxIntersectsBox(ref BoxCollider, ref c.BoxCollider);
            return collision;
        }

        public void Render(Renderer renderer, Matrix viewMatrix, Matrix projectionMatrix)
        {
            Mesh.Render(renderer, viewMatrix, projectionMatrix);
        }
    }
}
