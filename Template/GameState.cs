using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    public enum GameStateEnum{
        Win, 
        GameOver,
        Exit
    }

    public class GameState
    {
        public GameStateEnum state = GameStateEnum.Exit;
    }
}
