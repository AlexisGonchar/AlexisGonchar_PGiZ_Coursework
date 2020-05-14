using SharpDX;
using SharpHelper.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Graphics;

namespace Template
{

    public enum ChestState
    {
        Close,
        Openning,
        Open
    }

    public enum Items
    {
        Coin,
        Food,
        Shield,
        Key,
        None
    }

    public class Chest : IModel
    {
        public Dictionary<String, MeshObject> MeshObjects;
        public BoundingBox BoxCollider;
        public ChestState State;
        public Items Item;
        public SharpAudioVoice chestOpenSound;

        public Chest(Dictionary<String, MeshObject> meshes, BoundingBox box, SharpAudioDevice device)
        {
            MeshObjects = meshes;
            BoxCollider = box;
            ChestUp();
            State = ChestState.Close;
            chestOpenSound = new SharpAudioVoice(device, "Resources\\Audio\\chest.wav");
        }

        private void ChestUp()
        {
            foreach (MeshObject meshObject in MeshObjects.Values)
            {
                meshObject.MoveBy(0.0f, 0.48f, 0.0f);
            }
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

        public void RotateChestToWall(System.Drawing.Bitmap bmp, int x, int y)
        {
            float yaw = 0;
            float dY = 0;
            float dX = 0;
            if (ColorDetect.Detect(bmp.GetPixel(x - 1, y)) == ColorBMP.Black)
            {
                yaw = (float)Math.PI;
                dX = -0.964f;
            }
            else if (ColorDetect.Detect(bmp.GetPixel(x + 1, y)) == ColorBMP.Black)
            {
                dX = 0.964f;
            }
            if (ColorDetect.Detect(bmp.GetPixel(x, y + 1)) == ColorBMP.Black)
            {
                yaw = (float)Math.PI / 2;
                dY = 0.964f;
            }else if (ColorDetect.Detect(bmp.GetPixel(x, y - 1)) == ColorBMP.Black)
            {
                yaw = (float)-Math.PI / 2;
                dY = -0.964f;
            }
            foreach (MeshObject meshObject in MeshObjects.Values)
            {
                meshObject.Yaw = yaw;
                meshObject.MoveBy(dY, 0, dX);
            }
        }

        public void SetOver(Material material)
        {
            foreach (MeshObject meshObject in MeshObjects.Values)
            {
                meshObject._material = material;
            }
        }
        
        public void Open()
        {
            State = Animations.OpenChest(this);
        }
    }
}
