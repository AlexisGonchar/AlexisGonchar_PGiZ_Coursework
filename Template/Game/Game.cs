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

        /// <summary>Camera object.</summary>
        private Camera _camera;

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

        private Random _random;

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
            _samplerStates = new SamplerStates(_directX3DGraphics);
            _textures = new Textures();
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\floor.png", true, _samplerStates.TexturedFloor));
            _renderer.SetWhiteTexture(_textures["floor.png"]);
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\wall.png", true, _samplerStates.TexturedFloor));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\torch.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\chest.png", true, _samplerStates.Textured));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\sword.png", true, _samplerStates.TexturedFloor));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\ceiling.png", true, _samplerStates.TexturedFloor));
            _textures.Add(loader.LoadTextureFromFile("Resources\\Textures\\ghost.png", true, _samplerStates.Textured));
            _materials = loader.LoadMaterials("Resources\\Models\\materials.txt", _textures);
            gameField = new GameField();
            // 6. Load meshes.
            _meshObjects = new MeshObjects();
            _floor = loader.LoadMeshObject("Resources\\Models\\floor.txt", _materials);
            _ceiling = loader.LoadMeshObject("Resources\\Models\\ceiling.txt", _materials);
            _ceiling.MoveBy(0, 14, 0);
            AddCubes();
            _sword = loader.LoadMeshObject("Resources\\Models\\sword.txt", _materials);

            // 6. Load HUD resources into DirectX 2D object.
            InitHUDResources();

            loader = null;

            _illumination = new Illumination(Vector4.Zero, new Vector4(1.0f, 1.0f, 0.9f, 1.0f), new LightSource[]
            {
                //new LightSource(LightSource.LightType.DirectionalLight,
                //    new Vector4(0.0f, 20.0f, 0.0f, 1.0f),   // Position
                //    new Vector4(0.0f, -1.0f, 0.0f, 1.0f),   // Direction
                //    new Vector4(1.0f, 0.9f, 0.8f, 1.0f),    // Color
                //    0.0f,                                   // Spot angle
                //    1.0f,                                   // Const atten
                //    0.0f,                                   // Linear atten
                //    0.0f,                                   // Quadratic atten
                //    1),
                //new LightSource(LightSource.LightType.SpotLight,
                //    new Vector4(0.0f, 8.0f, 0.0f, 1.0f),
                //    new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                //    new Vector4(0.7f, 0.7f, 1.0f, 1.0f),
                //    Game3DObject._PI2 / 4.0f,
                //    1.0f,
                //    0.02f,
                //    0.005f,
                //    1),
                new LightSource(LightSource.LightType.PointLight,
                    new Vector4(-4.0f, 4.0f, 0.0f, 1.0f),
                    Vector4.Zero,
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    0.0f,
                    0.2f,
                    0.00002f,
                    0.000f,
                    1)
            });

            // Character and camera. X0Z - ground, 0Y - to up.
            _character = new Character(new Vector4(0.0f, 6.0f, -12.0f, 1.0f), Game3DObject._PI, 0.0f, 0.0f, 8.0f); //********
            _camera = new Camera(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            _camera.AttachToObject(_character);

            // Input controller and time helper.
            _inputController = new InputController(_renderForm);
            _timeHelper = new TimeHelper();
            _random = new Random();
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

            _illumination = new Illumination(Vector4.Zero, new Vector4(1.0f, 1.0f, 0.9f, 1.0f), new LightSource[]
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

        public void AddCubes()
        {
            Loader loader = new Loader(_directX3DGraphics, _directX2DGraphics, _renderer, _directX2DGraphics.ImagingFactory);
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
                            m.MoveBy((y - 100) * 2 + 1, 6, (x - 100) * 2 + 1);
                            RotateTorch(m, bmp, x, y);
                            lightPositions.Add(new Vector4((y - 100) * 2, 6, (x - 100) * 2, 1));
                            break;
                        case ColorBMP.Grey:
                            m = loader.LoadMeshObject("Resources\\Models\\chest.txt", _materials);
                            m.MoveBy((y - 100) * 2 + 1, 1, (x - 100) * 2 + 1);
                            RotateChest(m, bmp, x, y);
                            break;
                        case ColorBMP.Green:
                            m = loader.LoadMeshObject("Resources\\Models\\Walls\\wall_block.txt", _materials);
                            m.MoveBy((y - 100) * 2 + 1, 13, (x - 100) * 2 + 1);
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

        public void RotateChest(MeshObject m, System.Drawing.Bitmap bmp, int x, int y)
        {
            if (ColorDetect.Detect(bmp.GetPixel(x - 1, y)) == ColorBMP.Black)
                m.Yaw = (float)Math.PI;
            if (ColorDetect.Detect(bmp.GetPixel(x, y + 1)) == ColorBMP.Black)
                m.Yaw = (float)Math.PI / 2;
            if (ColorDetect.Detect(bmp.GetPixel(x, y - 1)) == ColorBMP.Black)
                m.Yaw = (float)-Math.PI / 2;
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
                if (_inputController.ShiftPressed) _character.Speed /= 3;
                _character.Crouch(6);
                if (_inputController.ControlPressed) _character.Crouch(3);     
                if (_inputController.WPressed) _character.MoveForwardBy(_timeHelper.DeltaT * _character.Speed);
                if (_inputController.SPressed) _character.MoveForwardBy(-_timeHelper.DeltaT * _character.Speed);
                if (_inputController.DPressed) _character.MoveRightBy(_timeHelper.DeltaT * _character.Speed);
                if (_inputController.APressed) _character.MoveRightBy(-_timeHelper.DeltaT * _character.Speed);
                CollisionWalls();
                if (_inputController.Esc) _renderForm.Close();                               // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Toggle help by F1.
                if (_inputController.Func[0]) _displayHelp = !_displayHelp;
                // Switch solid and wireframe modes by F2, F3.
                if (_inputController.Func[1]) _directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Solid;
                if (_inputController.Func[2]) _directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Wireframe;
                // Toggle fullscreen mode by F4, F5.
                if (_inputController.Func[3]) _directX3DGraphics.IsFullScreen = false;
                if (_inputController.Func[4]) _directX3DGraphics.IsFullScreen = true;
                if (_inputController.MouseButtons[0]) swordAnim = true;
            }
            
            _viewMatrix = _camera.GetViewMatrix();
            countMeshes = 0;
            _renderer.BeginRender();
            initLight();
            RenderWalls();
            _illumination.EyePosition = _camera.Position;
            //LightSource light2 = _illumination[2];
            //if (RandomUtil.NextFloat(_random, 0.0f, 1.0f) < 0.2f) light2.Enabled = (1 ==light2.Enabled ? 0 : 1);
            //_illumination[2] = light2;
            _renderer.UpdateIlluminationProperties(_illumination);
            _renderer.SetPerObjectConstants(_timeHelper.Time, 0);
            Matrix worldMatrix;
            
            
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
                    worldMatrix = meshObject.GetWorldMatrix();
                    _renderer.UpdatePerObjectConstantBuffer(meshObject.Index, worldMatrix, _viewMatrix, _projectionMatrix);
                    meshObject.Render();
                }
            }
            float time = _timeHelper.Time;
            _renderer.SetPerObjectConstants(time, 0); //1);
            //worldMatrix = _cube.GetWorldMatrix();
            //_renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, _viewMatrix, _projectionMatrix);
            //_cube.Render();
            //_renderer.SetPerObjectConstants(time, 0);
            worldMatrix = _floor.GetWorldMatrix();
            _renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, _viewMatrix, _projectionMatrix);
            _floor.Render();
            worldMatrix = _ceiling.GetWorldMatrix();
            _renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, _viewMatrix, _projectionMatrix);
            _ceiling.Render();

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
            if(swordAnim)
                swordAnim = Animations.ImpactBySword(_sword);
            
            
            worldMatrix = _sword.GetWorldMatrix();
            _renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, _viewMatrix, _projectionMatrix);
            _sword.Render();

            RenderHUD();

            _renderer.EndRender();
        }

        private void RenderWalls()
        {
            int wallsCount = gameField.GetWallsCount();
            Matrix worldMatrix;
            double way;
            for (int i = 0; i < wallsCount; i++)
            {
                Wall wall = gameField.GetWall(i);
                MeshObject mesh = wall.Mesh;
                way = Collision.DistanceBoxBox(ref wall.BoxCollider, ref _character.BoxCollider);
                if (way < 100)
                {
                    countMeshes++;
                    worldMatrix = mesh.GetWorldMatrix();
                    _renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, _viewMatrix, _projectionMatrix);
                    mesh.Render();
                }
            }
        }

        private void CollisionWalls()
        {
            float x = _character.Position.X;
            float z = _character.Position.Z;
            float y = _character.Position.Y;
            _character.BoxCollider.Minimum = new Vector3(x - 1, 0, z - 1);
            _character.BoxCollider.Maximum = new Vector3(x + 1, y, z + 1);
            int wallsCount = gameField.GetWallsCount();
            double way;
            for (int i = 0; i < wallsCount; i++)
            {
                Wall wall = gameField.GetWall(i);
                MeshObject mesh = wall.Mesh;
                way = Collision.DistanceBoxBox(ref wall.BoxCollider, ref _character.BoxCollider);
                if (way < 10)
                {
                    if (gameField.CharachterWallCollision(_character, i))
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
                                $"Count of Meshes: {countMeshes,6:f1}";
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
        }
    }
}
