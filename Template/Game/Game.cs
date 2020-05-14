using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using Template.Properties; // For work with resources of project Assembly.Properties. Then we can access to all inside Resorces.resx.
using Template.Graphics;
using SharpHelper.Audio;

namespace Template
{
    public class Game : IDisposable
    {
        // TODO: Realize game state logic.
        ///// <summary>Game states.</summary>
        ///// <remarks>
        ///// <list type="bullet">
        ///// <listheader>Correct state transitions:</listheader>
        ///// <item>BeforeStart -> Play,</item>
        ///// <item>BeforeStart -> Exit,</item>
        ///// <item>Play <-> Pause,</item>
        ///// <item>Play -> Finish,</item>
        ///// <item>Play -> Die,</item>
        ///// <item>Finish -> BeforeStart,</item>
        ///// <item>Die -> BeforeStart,</item>
        ///// <item>BeforeStart -> Exit,</item>
        ///// <item>Pause -> Exit,</item>
        ///// <item>Finish -> Exit,</item>
        ///// <item>Die -> Exit</item>
        ///// </list>
        ///// </remarks>
        //public enum GameState
        //{
        //    BeforeStart,
        //    Play,
        //    Pause,
        //    Finish,
        //    Die,
        //    Exit
        //}

        ///// <summary>Game states.</summary>
        //private GameState _gameState;
        ///// <summary>Game states.</summary>
        //public GameState State { get => _gameState; }

        // TODO: HUD to separate class.
        public struct HUDResources
        {
            public int textFPSTextFormatIndex;
            public int textFPSBrushIndex;
            public int armorIconIndex;
        }

        /// <summary>Main form of application.</summary>
        private RenderForm _renderForm;

        /// <summary>Flag if render form resized by user.</summary>
        //private bool _renderFormUserResized;

        /// <summary>DirectX 3D graphics objects.</summary>
        private DirectX3DGraphics _directX3DGraphics;

        /// <summary>Renderer.</summary>
        private Renderer _renderer;

        /// <summary>DirectX 2D graphic object.</summary>
        private DirectX2DGraphics _directX2DGraphics;

        private SamplerStates _samplerStates;
        private Textures _textures;
        private Materials _materials;
        private Illumination _illumination;
        private MeshObject _floor;
        private MeshObject _ceiling;
        private List<Vector4> lightPositions;
        /// <summary>List of objects with meshes.</summary>
        private MeshObjects _meshObjects;
        private System.Drawing.Bitmap bmp;
        private ColorBMP[,] levelMap;
        /// <summary>HUD resources.</summary>
        private HUDResources _HUDResources;

        /// <summary>Flag for display help.</summary>
        private bool _displayHelp;
        private string _helpString;

        private GameField gameField;

        private int countMeshes;

        /// <summary>Character.</summary>
        private Character _character;
        private Vector4 prePos;

        private MeshObject _sword;
        private bool swordAnim = false;

        private Chest targetChest;
        
        /// <summary>Camera object.</summary>
        private Camera _camera;

        HUDModel[] hearts;
        HUDModel[] keys;
        HUDModel[] shields;
        MeshObject target;
        Dictionary<Items, ItemChestModel> items;
        ItemChestModel item;
        MeshObject door, door2;
        List<Wall> nearestWalls;

        /// <summary>Projection matrix.</summary>
        private Matrix _projectionMatrix;

        /// <summary>View matrix.</summary>
        private Matrix _viewMatrix;

        /// <summary>Camera angular ratation step for moving mouse by 1 pixel.</summary>
        private float _angularCameraRotationStep;

        /// <summary>Input controller.</summary>
        private InputController _inputController;

        /// <summary>Time helper object for current time and delta time measurements.</summary>
        private TimeHelper _timeHelper;

        SharpAudioDevice device = new SharpAudioDevice();
        SharpAudioVoice voice, walkingSound, hitZombieSound, swordMissSound;

        bool walk = false;
        bool walkIsPlayed = false;

        Random random;

        int coinCount = 0;
        int keyCount = 0;
        int shieldCount = 0;


        /// <summary>First run flag for create DirectX buffers before render in first time.</summary>
        private bool _firstRun = true;

        /// <summary>Init HUD resources.</summary>
        /// <remarks>Create text format, text brush and armor icon.</remarks>
        private void InitHUDResources()
        {
            _HUDResources.textFPSTextFormatIndex = _directX2DGraphics.NewTextFormat("Input", SharpDX.DirectWrite.FontWeight.Normal,
                SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, 12,
                SharpDX.DirectWrite.TextAlignment.Leading, SharpDX.DirectWrite.ParagraphAlignment.Near);
            _HUDResources.textFPSBrushIndex = _directX2DGraphics.NewSolidColorBrush(new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 0.0f, 1.0f));
            _HUDResources.armorIconIndex = _directX2DGraphics.LoadBitmapFromFile("Resources\\Levels\\level_1_map.bmp");  // Don't use before Resizing. Bitmaps loaded, but not created.
        }

        /// <summary>
        /// Constructor. Initialize all objects.
        /// </summary>
        public Game()
        {
            random = new Random();
            //_gameState = GameState.BeforeStart;
            _helpString = Resources.HelpString;

            // Initialization order:
            // 1. Render form.
            _renderForm = new RenderForm("SharpDX");
            _renderForm.UserResized += RenderFormResizedCallback;
            _renderForm.Activated += RenderFormActivatedCallback;
            _renderForm.Deactivate += RenderFormDeactivateCallback;
            // 2. DirectX 3D.
            _directX3DGraphics = new DirectX3DGraphics(_renderForm);
            // 3. Renderer.
            _renderer = new Renderer(_directX3DGraphics);
            _renderer.CreateConstantBuffers();
            // 4. DirectX 2D.
            _directX2DGraphics = new DirectX2DGraphics(_directX3DGraphics);
            // 5. Load materials
            Loader loader = new Loader(_directX3DGraphics, _directX2DGraphics, _renderer, _directX2DGraphics.ImagingFactory);
            LoaderFromObj loaderFromObj = new LoaderFromObj(_directX3DGraphics, _directX2DGraphics, _renderer, _directX2DGraphics.ImagingFactory);
            _samplerStates = new SamplerStates(_directX3DGraphics);
            _textures = new Textures();
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\floor.png", true, _samplerStates.Textured));
            _renderer.SetWhiteTexture(_textures["floor.png"]);
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\wall.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\torch.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\chest.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\sword.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\ceiling.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\zombie.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\chest_over.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\heart.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\key.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\coin.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\food.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\target.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\door.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\shield.png", true, _samplerStates.Textured));
            _materials = loader.LoadMaterials("Resources\\Models\\materials.txt", _textures);
            gameField = new GameField();
            nearestWalls = new List<Wall>();
            // 6. Load meshes.
            _meshObjects = new MeshObjects();

            //_zombie = new Zombie(loaderFromObj.LoadMeshesFromObject("Resources\\Models\\zombie.obj", _materials[7]));
            _floor = loader.LoadMeshObject("Resources\\Models\\floor.txt", _materials);
            _ceiling = loader.LoadMeshObject("Resources\\Models\\ceiling.txt", _materials);
            _ceiling.MoveBy(0, 14, 0);
            AddCubes();
            ChestPuttingItems();
            _sword = loaderFromObj.LoadMeshesFromObject("Resources\\Models\\sword.obj", _materials[5])["swrod"];

            hearts = new HUDModel[5];
            for(int i = 0; i < 5; i++)
            {
                
                hearts[i] = new HUDModel(loaderFromObj.LoadMeshFromObject("Resources\\Models\\heart.obj", _materials[9]));
            }
            keys = new HUDModel[5];
            for (int i = 0; i < 5; i++)
            {
                
                keys[i] = new HUDModel(loaderFromObj.LoadMeshFromObject("Resources\\Models\\key.obj", _materials[10]));
            }
            shields = new HUDModel[3];
            for (int i = 0; i < 3; i++)
            {

                shields[i] = new HUDModel(loaderFromObj.LoadMeshFromObject("Resources\\Models\\shieldhud.obj", _materials[15]));
            }
            AddItemsModelInDictionary(loaderFromObj);
            door = loaderFromObj.LoadMeshFromObject("Resources\\Models\\door.obj", _materials[14]);
            door.MoveBy(0, 6f, 0);
            door2 = loaderFromObj.LoadMeshFromObject("Resources\\Models\\door.obj", _materials[14]);
            door2.MoveBy(6, 6f, 0);
            door2.Yaw = (float)Math.PI;
            target = loaderFromObj.LoadMeshFromObject("Resources\\Models\\target.obj", _materials[13]);
            // 6. Load HUD resources into DirectX 2D object.
            InitHUDResources();

            loader = null;

            // Character and camera. X0Z - ground, 0Y - to up.
            _character = new Character(new Vector4(0.0f, 6.0f, -12.0f, 1.0f), Game3DObject._PI, 0.0f, 0.0f, 12.0f); //********
            _camera = new Camera(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            _camera.AttachToObject(_character);

            // Input controller and time helper.
            _inputController = new InputController(_renderForm);
            _timeHelper = new TimeHelper();

            voice = new SharpAudioVoice(device, "Resources\\Audio\\bg.wav");
            walkingSound = new SharpAudioVoice(device, "Resources\\Audio\\walking.wav");
            hitZombieSound = new SharpAudioVoice(device, "Resources\\Audio\\hitZombie.wav");
            swordMissSound = new SharpAudioVoice(device, "Resources\\Audio\\miss.wav");
            walkingSound.Stopped += OnStop;
            voice.Voice.SetVolume(0.3f);
            voice.Stopped += OnStop;

            voice.Play();
            
        }

        private void AddItemsModelInDictionary(LoaderFromObj loaderFromObj)
        {
            items = new Dictionary<Items, ItemChestModel>();
            item = new ItemChestModel(loaderFromObj.LoadMeshFromObject("Resources\\Models\\coin.obj", _materials[11]));
            items.Add(Items.Coin, item);
            item = new ItemChestModel(loaderFromObj.LoadMeshFromObject("Resources\\Models\\food.obj", _materials[12]));
            items.Add(Items.Food, item);
            item = new ItemChestModel(loaderFromObj.LoadMeshFromObject("Resources\\Models\\keyItem.obj", _materials[10]));
            items.Add(Items.Key, item);
            item = new ItemChestModel(loaderFromObj.LoadMeshFromObject("Resources\\Models\\shield.obj", _materials[15]));
            items.Add(Items.Shield, item);
        }

        private void OnStop(SharpAudioVoice voice)
        {
            voice.Play();
        }

        public void initLight()
        {
            lightPositions = lightPositions.OrderBy(p => Math.Sqrt(Math.Pow(_character.Position.X - p.X,2) + Math.Pow(_character.Position.Z - p.Z, 2))).ToList();

            LightSource[] l = new LightSource[4];
            for(int i = 0; i < 4; i++)
            {
                l[i] = generatePointLight();
                l[i].Position = lightPositions[i];
            }

            _illumination = new Illumination(Vector4.Zero, new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new LightSource[]
            {
                l[0], l[1], l[2], l[3]
            });
        }

        public LightSource generatePointLight()
        {
            return new LightSource(LightSource.LightType.PointLight,
                    Vector4.Zero,
                    Vector4.Zero,
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    0.0f,
                    0.8f,
                    0.002f,
                    0.000f,
                    1);
        }

        public Wall AddWalls(Loader loader, ref int x, int y, int iter) 
        {
            Wall wall = null;
            if (ColorDetect.Detect(bmp.GetPixel(x + iter, y)) == ColorBMP.Black && iter < 10)
            {
                wall = AddWalls(loader, ref x, y, iter + 1);
            }
            else
            {
                float wallX = (y - 100) * 2 + 1;
                float wallZ = (x - 100) * 2 + iter;
                MeshObject m = loader.LoadMeshObject("Resources\\Models\\Walls\\wall_x" + iter + ".txt", _materials);
                m.MoveBy(wallX, 7, wallZ);
                BoundingBox b = new BoundingBox(new Vector3(wallX - 1, 0, wallZ - iter), new Vector3(wallX + 1, 10, wallZ + iter));
                wall = new Wall(m, b);
                x += iter - 1;
            }
            return wall;
        }

        void ChestPuttingItems()
        {
            foreach(Chest chest in gameField.chests)
            {
                chest.Item = Items.None;
            }
            int chestCount = gameField.chests.Count;
            for (int i = 0; i < 5; i++)
            {
                bool finded = true;
                Chest chest = null;
                while (finded)
                {
                    chest = gameField.chests[random.Next(0, chestCount)];
                    if (chest.Item != Items.Key)
                        finded = false;
                }
                chest.Item = Items.Key;
            }
            foreach (Chest chest in gameField.chests)
            {
                int k = 0;
                if (chest.Item != Items.Key)
                {
                    k = random.Next(0, 101);
                    Items item;
                    if(k < 35)
                    {
                        item = Items.Shield;
                    }else if(k < 70)
                    {
                        item = Items.Coin;
                    }
                    else
                    {
                        item = Items.Food;
                    }
                    chest.Item = item;
                }
                    
            }
        }

        public void AddCubes()
        {
            Loader loader = new Loader(_directX3DGraphics, _directX2DGraphics, _renderer, _directX2DGraphics.ImagingFactory);
            LoaderFromObj loaderFromObj = new LoaderFromObj(_directX3DGraphics, _directX2DGraphics, _renderer, _directX2DGraphics.ImagingFactory);
            string filePath = @"Resources\Levels\level_1.bmp";
            bmp = new System.Drawing.Bitmap(filePath);
            lightPositions = new List<Vector4>();
            levelMap = new ColorBMP[bmp.Height, bmp.Width];
            ColorBMP color;
            Wall wall;
            MeshObject m;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    m = null;
                    color = ColorDetect.Detect(bmp.GetPixel(x, y));
                    switch (color){
                        case ColorBMP.Black:
                            wall = AddWalls(loader, ref x, y, 1);
                            gameField.AddWall(wall);
                            break;
                        case ColorBMP.Yellow:
                            m = loader.LoadMeshObject("Resources\\Models\\torch.txt", _materials);
                            m.MoveBy((y - 100) * 2 + 1, 8, (x - 100) * 2 + 1);
                            RotateTorch(m, bmp, x, y);
                            lightPositions.Add(new Vector4((y - 100) * 2, 8, (x - 100) * 2, 1));
                            break;
                        case ColorBMP.Grey:
                            Vector3 min = new Vector3((y - 100) * 2, 0.45f, (x - 100) * 2);
                            Vector3 max = new Vector3((y - 100) * 2 + 2, 2.45f, (x - 100) * 2 + 2);
                            BoundingBox box = new BoundingBox(min, max);
                            Chest chest = new Chest(loaderFromObj.LoadMeshesFromObject("Resources\\Models\\chest.obj", _materials[6]), box, device);
                            chest.MoveBy((y - 100) * 2 + 1, 1, (x - 100) * 2 + 1);
                            chest.RotateChestToWall(bmp, x, y);
                            gameField.chests.Add(chest);
                            break;
                        case ColorBMP.Green:
                            m = loader.LoadMeshObject("Resources\\Models\\Walls\\wall_block.txt", _materials);
                            m.MoveBy((y - 100) * 2 + 1, 13, (x - 100) * 2 + 1);
                            break;
                        case ColorBMP.Red:
                            min = new Vector3((y - 100) * 2, 0.0f, (x - 100) * 2);
                            max = new Vector3((y - 100) * 2 + 2, 8f, (x - 100) * 2 + 2);
                            Zombie zombie = new Zombie(loaderFromObj.LoadMeshesFromObject("Resources\\Models\\zombie.obj", _materials[7]), device);
                            zombie.MoveBy((y - 100) * 2 + 1, 0, (x - 100) * 2 + 1);
                            zombie.BoxCollider.Maximum = max;
                            zombie.BoxCollider.Minimum = min;
                            gameField.zombies.Add(zombie);
                            break;
                        default:
                            break;
                    }
                    levelMap[x, y] = color;
                    if (m != null)
                    {
                        _meshObjects.Add(m);
                    }
                }
            }
        }

        public void RotateTorch(MeshObject m, System.Drawing.Bitmap bmp, int x, int y)
        {
            float move = 0.75f;
            if (ColorDetect.Detect(bmp.GetPixel(x + 1, y)) == ColorBMP.Black) 
            {
                m.Pitch = (float)-Math.PI / 6;
                m.MoveBy(0, 0, move);
            }
            if (ColorDetect.Detect(bmp.GetPixel(x - 1, y)) == ColorBMP.Black)
            {
                m.Pitch = (float)Math.PI / 6;
                m.MoveBy(0, 0, -move);
            }
            if (ColorDetect.Detect(bmp.GetPixel(x, y + 1)) == ColorBMP.Black)
            {
                m.Roll = (float)Math.PI / 6;
                m.MoveBy(move, 0, 0);
            }
            if (ColorDetect.Detect(bmp.GetPixel(x, y - 1)) == ColorBMP.Black)
            {
                m.Roll = (float)-Math.PI / 6;
                m.MoveBy(-move, 0, 0);
            }
        }

        public bool CheckCollision()
        {
            int px = (int) (_character.Position.Z + 200) / 2;
            int py = (int) (_character.Position.X + 200) / 2;
            if (levelMap[px, py] == ColorBMP.Black ||
                levelMap[px, py] == ColorBMP.Grey)
            {
                return true;
            }
            
            return false;
        }

        /// <summary>Render form activated callback. Hide cursor.</summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="args">Arguments.</param>
        public void RenderFormActivatedCallback(object sender, EventArgs args)
        {
            Cursor.Hide();
        }

        /// <summary>Render form deactivate event callback. Show cursor.</summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="args">Arguments.</param>
        public void RenderFormDeactivateCallback(object sender, EventArgs args)
        {
            Cursor.Show();
        }

        /// <summary>Render form user resized callback. Perform resizing of DirectX 3D object and renew camera rotation step and projection matrix.</summary>
        /// <param name="sender">Sender of event.</param>
        /// <param name="args">Arguments.</param>
        public void RenderFormResizedCallback(object sender, EventArgs args)
        {
            _directX3DGraphics.Resize();
            _camera.Aspect = _renderForm.ClientSize.Width / (float)_renderForm.ClientSize.Height;
            _angularCameraRotationStep = _camera.FOVY / _renderForm.ClientSize.Height;
            _projectionMatrix = _camera.GetProjectionMatrix();
        }

        /// <summary>Callback for RenderLoop.Run. Handle input and render scene.</summary>
        public void RenderLoopCallback()
        {
            walk = false;
            if (_firstRun)
            {
                RenderFormResizedCallback(this, new EventArgs());
                _firstRun = false;
            }

            _timeHelper.Update();
            _inputController.UpdateKeyboardState();
            _inputController.UpdateMouseState();
            if (_inputController.MouseUpdated) // TODO: Move handle input to separate thread.
            {
                _character.PitchBy(-_inputController.MouseRelativePositionY * _angularCameraRotationStep); //********
                _character.YawBy(_inputController.MouseRelativePositionX * _angularCameraRotationStep); //**********
            }
            if (_inputController.KeyboardUpdated)
            {
                prePos = _character.Position;
                _character.Speed = 24;
                if (_inputController.ShiftPressed) _character.Speed *= 3;
                _character.Crouch(6);
                if (_inputController.ControlPressed) _character.Crouch(3);     
                if (_inputController.WPressed) _character.MoveForwardBy(_timeHelper.DeltaT * _character.Speed);
                if (_inputController.SPressed) _character.MoveForwardBy(-_timeHelper.DeltaT * _character.Speed);
                if (_inputController.DPressed) _character.MoveRightBy(_timeHelper.DeltaT * _character.Speed);
                if (_inputController.APressed) _character.MoveRightBy(-_timeHelper.DeltaT * _character.Speed);
                if(_inputController.WPressed || _inputController.SPressed || _inputController.DPressed || _inputController.APressed)
                {
                    walk = true;
                }
                _character.BoundingsMove();
                CollisionWalls();
                CollisionChests();
                if (targetChest != null && _inputController.EPressed)
                {
                    if (targetChest.State == ChestState.Close)
                    {
                        targetChest.chestOpenSound.Play();
                        targetChest.Open();
                        Vector4 v = (Vector4)((targetChest.BoxCollider.Maximum + targetChest.BoxCollider.Minimum) / 2.0f);
                        float yawAngle = targetChest.MeshObjects["Cover"].Yaw;
                        item = items[targetChest.Item];
                        item.AnimateChest = true;
                        item.mesh.Position = v;
                        item.mesh.Yaw = yawAngle;
                        switch (targetChest.Item)
                        {
                            case Items.Coin:
                                _character.SpeedAttack = 500;
                                coinCount++;
                                break;
                            case Items.Key:
                                keyCount++;
                                break;
                            case Items.Food:
                                if (_character.Health == 5)
                                    _character.SpeedAttack = 500;
                                else
                                    _character.Health++;
                                break;
                            case Items.Shield:
                                shieldCount = shieldCount == 3 ? 3 : shieldCount + 1;
                                break;
                        }
                    }
                }
                if (_inputController.Esc) 
                {
                    _renderForm.Close();
                    voice.Stop();
                }
                // Toggle help by F1.
                if (_inputController.Func[0]) _displayHelp = !_displayHelp;
                // Switch solid and wireframe modes by F2, F3.
                if (_inputController.Func[1]) _directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Solid;
                if (_inputController.Func[2]) _directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Wireframe;
                // Toggle fullscreen mode by F4, F5.
                if (_inputController.Func[3]) _directX3DGraphics.IsFullScreen = false;
                if (_inputController.Func[4]) _directX3DGraphics.IsFullScreen = true;
                if (_inputController.MouseButtons[0])
                {
                    if (!swordAnim)
                    {
                        float distance;
                        bool miss = true;
                        Zombie zombieToRemove = null;
                        foreach (Zombie zombie in gameField.zombies)
                        {
                            zombie.BoxCollider.Intersects(ref _character.ray, out distance);
                            if (distance < 20 && distance != 0)
                            {
                                zombie.Health--;
                                zombieToRemove = zombie.Health <= 0 ? zombie : null;
                                hitZombieSound.Play();
                                miss = false;
                            }
                        }
                        if (miss)
                        {
                            swordMissSound.Play();
                        }
                        gameField.zombies.Remove(zombieToRemove);
                    }
                    swordAnim = true;
                    
                }
            }
            
            _viewMatrix = _camera.GetViewMatrix();
            countMeshes = 0;
            _renderer.BeginRender();
            initLight();
            RenderWalls();
            RenderChests();
            _illumination.EyePosition = _camera.Position;
            _renderer.UpdateIlluminationProperties(_illumination);
            _renderer.SetPerObjectConstants(_timeHelper.Time, 0);
            
            
            for (int i = 0; i <= _meshObjects.Count - 1; i++)
            {
                if (i > 0)
                {
                    _renderer.SetPerObjectConstants(_timeHelper.Time, 0);
                }
                MeshObject meshObject = _meshObjects[i];
                if (Math.Sqrt(Math.Pow(_character.Position.X - meshObject.Position.X, 2) + Math.Pow(_character.Position.Z - meshObject.Position.Z, 2)) < 100)
                {
                    countMeshes++;
                    meshObject.Render(_renderer, _viewMatrix, _projectionMatrix);
                }
            }
            float time = _timeHelper.Time;
            _renderer.SetPerObjectConstants(time, 0); //1);
            
            _floor.Render(_renderer, _viewMatrix, _projectionMatrix);
            _ceiling.Render(_renderer, _viewMatrix, _projectionMatrix);

            //Меч
            //Смещение меча вправо (+), влево (-)
            float yaw = _camera.Yaw + 0.4f;
            //Смещение меча вверх (+), вниз (-)
            float pitch = _camera.Pitch - 0.4f;
            if (!swordAnim)
                pitch += Animations.SwordIdle() / 20.0f;
            //Радиус сферы
            float radius = 4;

            float x = (float)(_character.Position.Z - radius * Math.Cos(pitch) * Math.Cos(yaw));
            float y = (float)(_character.Position.X - radius * Math.Sin(yaw) * Math.Cos(pitch));
            float z = (float)(_character.Position.Y + radius * Math.Sin(pitch));

            //
            _sword.Pitch = _camera.Pitch + 1.0f;
            //
            _sword.Yaw = _camera.Yaw - 0.3f;
            //
            _sword.Roll = _camera.Roll - 1.5f;
            _sword.MoveTo(y, z, x);
            if (swordAnim)
            {
                if(_character.SpeedAttack > 0)
                {
                    swordAnim = Animations.ImpactBySword(_sword, 2);
                    _character.SpeedAttack--;
                }
                else
                {
                    swordAnim = Animations.ImpactBySword(_sword, 1);
                }
            }
            _sword.Render(_renderer, _viewMatrix, _projectionMatrix);

            float angle = Animations.RotateHearts();
            DrawHearts(angle);
            DrawKeys(angle);
            DrawShields(angle);
            if (item.AnimateChest)
            {
                item.Animate();
                item.Render(_renderer, _viewMatrix, _projectionMatrix);
            }

            door.Render(_renderer, _viewMatrix, _projectionMatrix);
            door2.Render(_renderer, _viewMatrix, _projectionMatrix);
            RenderZombies();
            RenderTarget();

            RenderHUD();

            _renderer.EndRender();

            if (walk && !walkIsPlayed)
            {
                walkingSound.Play();
                walkIsPlayed = true;
            }else if (!walk && walkIsPlayed)
            {
                walkingSound.Stop();
                walkIsPlayed = false;
            }
        }

        private void RenderZombies()
        {
            float distance = 0;
            Vector4 v;
            foreach (Zombie zombie in gameField.zombies)
            {
                if (zombie.KickReaload != 0)
                    zombie.KickReaload--;
                distance = Collision.DistanceBoxBox(ref _character.BoxCollider, ref zombie.BoxCollider);
                if (distance < 200)
                {
                    zombie.Render(_renderer, _viewMatrix, _projectionMatrix);
                    if(distance < 50)
                    {
                        if (zombie.damage)
                        {
                            zombie.damageSound.Play();
                            zombie.damage = false;
                        }
                        v = zombie.GetLeftLeg().Position;
                        zombie.Walk();
                        zombie.RotateToPlayer(_character);
                        foreach (Wall wall in nearestWalls)
                        {
                            if (Collision.DistanceBoxBox(ref zombie.BoxCollider, ref wall.BoxCollider) < 0.5f)
                                zombie.MoveTo(v);
                        }
                    }
                }
            }
        }

        private void RenderTarget()
        {
            Matrix rotation = Matrix.RotationYawPitchRoll(_character._yaw, _character._pitch, _character._roll);
            Vector4 viewTo = Vector4.Transform(-Vector4.UnitZ, rotation);
            viewTo *= 0.6f;
            target.Position = _character.Position + viewTo;
            target.Pitch = _camera.Pitch;
            target.Yaw = _camera.Yaw;
            target.Roll = _camera.Roll;
            target.Render(_renderer, _viewMatrix, _projectionMatrix);
        }

        private void RenderWalls()
        {
            nearestWalls.Clear();
            int wallsCount = gameField.GetWallsCount();
            double way;
            for (int i = 0; i < wallsCount; i++)
            {
                Wall wall = gameField.GetWall(i);
                MeshObject mesh = wall.Mesh;
                way = Collision.DistanceBoxBox(ref wall.BoxCollider, ref _character.BoxCollider);
                if (way < 200)
                {
                    countMeshes++;
                    mesh.Render(_renderer, _viewMatrix, _projectionMatrix);
                    if(way < 80)
                    {
                        nearestWalls.Add(wall);
                    }
                }
            }
        }

        private void RenderChests()
        {
            double way;
            foreach(Chest chest in gameField.chests)
            {
                way = Collision.DistanceBoxBox(ref chest.BoxCollider, ref _character.BoxCollider);
                if (way < 200)
                {
                    chest.Render(_renderer, _viewMatrix, _projectionMatrix);
                }
            }
        }

        private void CollisionChests()
        {
            double way;
            foreach (Chest chest in gameField.chests)
            {
                if (chest.State == ChestState.Openning)
                    chest.Open();
                way = Collision.DistanceBoxBox(ref chest.BoxCollider, ref _character.BoxCollider);
                if (way < 8)
                {
                    float d;
                    if (chest.BoxCollider.Intersects(ref _character.ray))
                    {
                        chest.SetOver(_materials[8]);
                        targetChest = chest;
                    }
                    else if(targetChest == chest)
                    {
                        chest.SetOver(_materials[6]);
                        targetChest = null;
                    }
                }
                else if(targetChest == chest && way < 25)
                {
                    chest.SetOver(_materials[6]);
                    targetChest = null;
                }
            }
        }

        private void DrawHearts(float rotateAngle)
        {
            float leftBias = 0.0f;
            for(int i = 0; i < _character.Health; i++)
            {
                Matrix rotation = Matrix.RotationYawPitchRoll(_character._yaw, _character._pitch, _character._roll);

                Vector4 viewTo = Vector4.Transform(-Vector4.UnitZ, rotation);
                Vector4 viewUp = Vector4.Transform(Vector4.UnitY, rotation);
                Vector4 viewSide = Vector4.Transform(Vector4.UnitX, rotation);
                Vector4 viewToNorm = Vector4.Transform(viewTo, Matrix.RotationAxis((Vector3)viewUp, -(float)Math.PI / 2));
                Vector4 viewToNorm2 = Vector4.Transform(viewTo, Matrix.RotationAxis((Vector3)viewSide, (float)Math.PI / 2));
                viewTo *= 0.6f;
                viewToNorm *= (0.4f - leftBias);
                viewToNorm2 *= -0.2f;

                var resultPoint = viewTo + viewToNorm + viewToNorm2;


                hearts[i].mesh.Pitch = _camera.Pitch + rotateAngle;
                hearts[i].mesh.Yaw = _camera.Yaw + rotateAngle;
                hearts[i].mesh.Roll = _camera.Roll;
                hearts[i].mesh.Position = _character.Position + resultPoint;
                hearts[i].Render(_renderer, _viewMatrix, _projectionMatrix);
                leftBias += 0.06f;
            }
        }

        private void DrawKeys(float rotateAngle)
        {
            float leftBias = 0.0f;
            for (int i = 0; i < keyCount; i++)
            {
                Matrix rotation = Matrix.RotationYawPitchRoll(_character._yaw, _character._pitch, _character._roll);

                Vector4 viewTo = Vector4.Transform(-Vector4.UnitZ, rotation);
                Vector4 viewUp = Vector4.Transform(Vector4.UnitY, rotation);
                Vector4 viewSide = Vector4.Transform(Vector4.UnitX, rotation);
                Vector4 viewToNorm = Vector4.Transform(viewTo, Matrix.RotationAxis((Vector3)viewUp, -(float)Math.PI / 2));
                Vector4 viewToNorm2 = Vector4.Transform(viewTo, Matrix.RotationAxis((Vector3)viewSide, (float)Math.PI / 2));
                viewTo *= 0.6f;
                viewToNorm *= (-0.4f + leftBias);
                viewToNorm2 *= -0.2f;

                var resultPoint = viewTo + viewToNorm + viewToNorm2;


                keys[i].mesh.Pitch = _camera.Pitch + rotateAngle;
                keys[i].mesh.Yaw = _camera.Yaw - rotateAngle;
                keys[i].mesh.Roll = _camera.Roll;
                keys[i].mesh.Position = _character.Position + resultPoint;
                keys[i].Render(_renderer, _viewMatrix, _projectionMatrix);
                leftBias += 0.06f;
            }
        }

        private void DrawShields(float rotateAngle)
        {
            float leftBias = 0.0f;
            for (int i = 0; i < shieldCount; i++)
            {
                Matrix rotation = Matrix.RotationYawPitchRoll(_character._yaw, _character._pitch, _character._roll);

                Vector4 viewTo = Vector4.Transform(-Vector4.UnitZ, rotation);
                Vector4 viewUp = Vector4.Transform(Vector4.UnitY, rotation);
                Vector4 viewSide = Vector4.Transform(Vector4.UnitX, rotation);
                Vector4 viewToNorm = Vector4.Transform(viewTo, Matrix.RotationAxis((Vector3)viewUp, -(float)Math.PI / 2));
                Vector4 viewToNorm2 = Vector4.Transform(viewTo, Matrix.RotationAxis((Vector3)viewSide, (float)Math.PI / 2));
                viewTo *= 0.6f;
                viewToNorm *= (0.4f - leftBias);
                viewToNorm2 *= -0.14f;

                var resultPoint = viewTo + viewToNorm + viewToNorm2;


                shields[i].mesh.Pitch = _camera.Pitch + rotateAngle;
                shields[i].mesh.Yaw = _camera.Yaw + rotateAngle;
                shields[i].mesh.Roll = _camera.Roll;
                shields[i].mesh.Position = _character.Position + resultPoint;
                shields[i].Render(_renderer, _viewMatrix, _projectionMatrix);
                leftBias += 0.06f;
            }
        }

        private void CollisionWalls()
        {
            int wallsCount = gameField.GetWallsCount();
            double way;
            for (int i = 0; i < wallsCount; i++)
            {
                Wall wall = gameField.GetWall(i);
                MeshObject mesh = wall.Mesh;
                way = Collision.DistanceBoxBox(ref wall.BoxCollider, ref _character.BoxCollider);
                if (way < 10)
                {
                    if (wall.CharacterCollision(_character))
                    {
                        _character.Position = prePos;
                    }
                }
            }
        }

        /// <summary>Render HUD.</summary>
        private void RenderHUD()
        {
            string text = $"FPS: {_timeHelper.FPS,3:d2}\ntime: {_timeHelper.Time:f1}\n" +
                                $"MX: {_inputController.MouseRelativePositionX,3:d2} MY: {_inputController.MouseRelativePositionY,3:d2} MZ: {_inputController.MouseRelativePositionZ,4:d3}\n" +
                                $"LB: {(_inputController.MouseButtons[0] ? 1 : 0)} MB: {(_inputController.MouseButtons[2] ? 1 : 0)} RB: {(_inputController.MouseButtons[1] ? 1 : 0)}\n" +
                                $"Pos: {_character.Position.X,6:f1}, {_character.Position.Y,6:f1}, {_character.Position.Z,6:f1}\n" +
                                $"Count of Meshes: {countMeshes,6:f1}\n" +
                                $"Coin: " + coinCount +
                                $"Key: " + keyCount;
            if (_displayHelp) text += "\n\n" + _helpString;
            float armorWidthInDIP = -1;//_directX2DGraphics.Bitmaps[_HUDResources.armorIconIndex].Size.Width;
            float armorHeightInDIP = -1;//_directX2DGraphics.Bitmaps[_HUDResources.armorIconIndex].Size.Height;
            if (_inputController.MPressed)
            {
                armorWidthInDIP = ((_renderForm.Width - 600) / 2) + 600; //983;
                armorHeightInDIP = ((_renderForm.Height - 600) / 2) + 600;
                
            }
            Matrix3x2 armorTransformMatrix = Matrix3x2.Translation(new Vector2(_directX2DGraphics.RenderTargetClientRectangle.Right - armorWidthInDIP - 1, _directX2DGraphics.RenderTargetClientRectangle.Bottom - armorHeightInDIP - 1));
            
            _directX2DGraphics.BeginDraw();
            _directX2DGraphics.DrawText(text, _HUDResources.textFPSTextFormatIndex,
                _directX2DGraphics.RenderTargetClientRectangle, _HUDResources.textFPSBrushIndex);
            _directX2DGraphics.DrawBitmap(_HUDResources.armorIconIndex, armorTransformMatrix, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
            _directX2DGraphics.EndDraw();
        }

        /// <summary>Rum main render loop.</summary>
        public void Run()
        {
            RenderLoop.Run(_renderForm, RenderLoopCallback);
        }

        /// <summary>Realise all resources</summary>
        public void Dispose()
        {
            // MeshObjects disposing
            _textures.Dispose();
            _samplerStates.Dispose();
            _inputController.Dispose();
            _directX2DGraphics.Dispose();
            _renderer.Dispose();
            _directX3DGraphics.Dispose();
            _renderForm.Dispose();
            voice.Dispose();
        }
    }
}
