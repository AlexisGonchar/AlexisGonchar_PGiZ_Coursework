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
        public List<Zombie> zombies;
        public List<Chest> chests;

        public GameField()
        {
            _walls = new List<Wall>();
            zombies = new List<Zombie>();
            chests = new List<Chest>();
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
    }
}
