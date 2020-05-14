using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Template
{
    public partial class Launcher : Form
    {
        GameState gameState;
        public Launcher()
        {
            InitializeComponent();
            gameState = new GameState();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Game game = null;
            game = new Game(gameState);
            game.Run();
            game.Dispose();
            if(gameState.state == GameStateEnum.GameOver)
            {
                pictureBox1.BackgroundImage = System.Drawing.Image.FromFile("Resources\\Pictures\\bggo.png");
                pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            }else if(gameState.state == GameStateEnum.Win)
            {
                pictureBox1.BackgroundImage = System.Drawing.Image.FromFile("Resources\\Pictures\\bgwin.png");
                pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            }else if(gameState.state == GameStateEnum.Exit)
            {
                pictureBox1.BackgroundImage = System.Drawing.Image.FromFile("Resources\\Pictures\\bg.png");
                pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            }
        }
    }
}
