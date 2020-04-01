using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template
{
    public class GameField
    {
        private List<Wall> _walls;

        public GameField()
        {
            _walls = new List<Wall>();
        }

        public void AddWall(Wall wall)
        {
            _walls.Add(wall);
        }

        public Wall GetWall(int index)
        {
            return _walls[index];
        }

        public int GetWallsCount()
        {
            return _walls.Count();
        }

        public bool CharachterWallCollision(Character character, int wallIndex)
        {
            bool collision = Collision.BoxIntersectsBox(ref _walls[wallIndex].BoxCollider, ref character.BoxCollider);
            return collision;
        }
    }
}
