using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace GameboyTetris
{
    internal enum GameState
    { logo, startscreen, playing, paused };

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RetroScreen gameboy;
        private MapScreen background;
        private Texture2D titleScreen;
        private Texture2D logo;
        private GameState gs;
        private float timeForLogo = 4;
        private Stopwatch stopwatch;
        private const int screenWidth = 160;
        private const int screenHeight = 144;
        private SpriteFont font;
        private Texture2D pixel;
        private List<MapScreen> screens = new List<MapScreen>();
        private bool selectedLeft = true;
        private SpriteText cursor;
        private bool ShapeActive = false;

        private List<Shape> shapes = new List<Shape>();
        private Shape active;
        private float timeforUpdate = 1;
        private float timeSinceUpdate = 0;
        private float timeforMove = 0.25f;
        private float timeSinceMove = 0;
        private int ids = 0;
        private Texture2D[] blocks;
        private Random rng = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            SetDefaultSize();
            gs = GameState.logo;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        private void SetDefaultSize()
        {
            int height = MaxHeight();
            _graphics.PreferredBackBufferHeight = height;

            _graphics.PreferredBackBufferWidth = (height / screenHeight) * screenWidth;
            _graphics.ApplyChanges();
        }

        private void SetDefaultSize(Screen screen)
        {
            int height = MaxHeight(screen);
            _graphics.PreferredBackBufferHeight = height;

            _graphics.PreferredBackBufferWidth = (height / screenHeight) * screenWidth;
            _graphics.ApplyChanges();
        }

        private void CenterScreen(Screen screen)
        {
            //Window.Position = new Point((screen.Bounds.Width / 2) - (Window.ClientBounds.Width / 2), (screen.Bounds.Height / 2) - (Window.ClientBounds.Height / 2));
            Window.Position += new Point((screen.Bounds.Width / 2) - (Window.ClientBounds.Width / 2), (screen.Bounds.Height / 2) - (Window.ClientBounds.Height / 2));
        }

        private bool previouslyMaximized = false;

        private void SwitchFullscreen()
        {
            var f = (Form)Control.FromHandle(Window.Handle);
            Screen screen = Screen.FromControl(f);
            _graphics.HardwareModeSwitch = false;
            Window.BeginScreenDeviceChange(!_graphics.IsFullScreen);
            bool wasFullscreen = _graphics.IsFullScreen;

            if (!_graphics.IsFullScreen)
            {
                _graphics.PreferredBackBufferWidth = screen.Bounds.Width;
                _graphics.PreferredBackBufferHeight = screen.Bounds.Height;
                previouslyMaximized = f.WindowState == FormWindowState.Maximized;
            }
            else
            {
                SetDefaultSize(screen);
            }
            _graphics.ToggleFullScreen();
            _graphics.ApplyChanges();
            _graphics.HardwareModeSwitch = true;
            if (wasFullscreen && previouslyMaximized)
            {
                CenterScreen(screen);
            }
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //renderTarget = new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight);
            gameboy = new RetroScreen(new RenderTarget2D(GraphicsDevice, screenWidth, screenHeight), ScreenSize());
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += ScreenChange;
            //screenSize = ScreenSize();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            titleScreen = Content.Load<Texture2D>("tetrisTitleScreenCropCol");
            logo = Content.Load<Texture2D>("Logo");
            //background = new MapScreen(logo, new Rectangle(0, 0, screenWidth, screenHeight));
            background = new MapScreen(logo, "logo");
            screens.Add(background);
            screens.Add(new MapScreen(titleScreen, "title"));
            font = Content.Load<SpriteFont>("font");
            font.DefaultCharacter = '#';
            pixel = Content.Load<Texture2D>("Square1");
            MapScreen map = screens.Find(o => o.name == "title");
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(80, 135), SpriteText.DrawMode.Middle, font, "© 2021 Gustav"));
            //map.textOnScreen.Add(new SpriteText(pixel, new Vector2(40, 115), SpriteText.DrawMode.MiddleUnderline, "1"))
            for (int i = 0; i < 2; i++)
            {
                string temp = (i + 1) + " player";
                //_spriteBatch.DrawString(font, temp, new Vector2(40 + 80 * i, 115) - (font.MeasureString(temp) / 2 * 0.25f), new Color(7, 24, 33), 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
                //_spriteBatch.Draw(pixel, new Rectangle(40 + 80 * i - (int)Math.Round((font.MeasureString(temp).X / 2 * 0.25f)), (int)Math.Round(114 + (font.MeasureString(temp).Y / 2 * 0.25f)), (int)Math.Round(font.MeasureString(temp).X * 0.25f), 1), new Color(48, 104, 80));
                map.textOnScreen.Add(new SpriteText(pixel, new Vector2(40 + 80 * i, 115), SpriteText.DrawMode.MiddleUnderline, font, temp));
            }
            cursor = new SpriteText(pixel, new Vector2(11, 115), SpriteText.DrawMode.Middle, font, "€");
            map.textOnScreen.Add(cursor);
            Texture2D texture = Content.Load<Texture2D>("tetrisPlayingTextlessCol");
            screens.Add(new MapScreen(texture, "playing"));
            map = screens.Find(o => o.name == "playing");
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(133, 11), SpriteText.DrawMode.Middle, font, "Score"));
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(133, 51), SpriteText.DrawMode.Middle, font, "Level"));
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(131, 75), SpriteText.DrawMode.Middle, font, "Lines"));
            int xCount = 8;
            int yCount = 1;
            blocks = AdvancedMath.Split(Content.Load<Texture2D>("TetrisSpriteSheet"), 8, 8, out xCount, out yCount);
            //font.            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            Input.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                Exit();
            if (Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.F11))
            {
                SwitchFullscreen();
            }
            if (gs == GameState.logo && stopwatch.Elapsed.TotalSeconds > timeForLogo)
            {
                gs = GameState.startscreen;
                //background.SetTex(titleScreen);
                background = screens.Find(o => o.name == "title");
                stopwatch.Restart();
            }
            if (gs == GameState.startscreen)
            {
                if (!selectedLeft && (Input.directional.X < 0) && !(Input.GetButton(Buttons.Start) || Input.GetButton(Microsoft.Xna.Framework.Input.Keys.Enter)))
                {
                    selectedLeft = true;
                    cursor.position.X = 11;
                }
                else if (selectedLeft && Input.directional.X > 0 && !(Input.GetButton(Buttons.Start) || Input.GetButton(Microsoft.Xna.Framework.Input.Keys.Enter)))
                {
                    selectedLeft = false;
                    cursor.position.X = 90;
                }
                if (Input.GetButtonDown(Buttons.Start) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Enter))
                {
                    if (!selectedLeft)
                    {
                        selectedLeft = true;
                        cursor.position.X = 11;
                    }
                    else
                    {
                        gs = GameState.playing;
                        background = screens.Find(o => o.name == "playing");
                    }
                }
            }
            if (gs == GameState.playing)
            {
                timeSinceUpdate += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (!ShapeActive)
                {
                    active = new Shape(blocks[rng.Next(blocks.Length)], ids, rng);
                    ids++;
                    ShapeActive = true;
                    screens.Find(o => o.name == "playing").spritesInScreen.AddRange(active.sprites);
                    shapes.Add(active);
                }
                else
                {
                    if (timeSinceUpdate > timeforUpdate || Input.directional.Y > 0 && timeSinceUpdate > timeforUpdate / 8)
                    {
                        timeSinceUpdate = 0;
                        active.Update(shapes.FindAll(o => o.id != active.id));
                        if (!active.active)
                        {
                            ShapeActive = false;
                            if (active.AboveBorder())
                            {
                                screens.Find(o => o.name == "playing").spritesInScreen.Clear();
                                gs = GameState.startscreen;
                                shapes.Clear();
                                background = screens.Find(o => o.name == "title");
                            }
                            return;
                        }
                    }
                    if (Input.GetButtonDown(Buttons.A) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.X))
                    {
                        active.RotateRight(shapes.FindAll(o => o.id != active.id));
                    }
                    else if (Input.GetButtonDown(Buttons.B) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Z))
                    {
                        active.RotateLeft(shapes.FindAll(o => o.id != active.id));
                    }
                    else if (Input.GetButtonDown(Buttons.DPadUp) || Input.GetButtonDown(Buttons.LeftThumbstickUp) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Up) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.W))
                    {
                        timeSinceUpdate = 0;
                        while (active.active)
                        {
                            active.Update(shapes.FindAll(o => o.id != active.id));
                        }
                        ShapeActive = false;
                        if (active.AboveBorder())
                        {
                            screens.Find(o => o.name == "playing").spritesInScreen.Clear();
                            gs = GameState.startscreen;
                            shapes.Clear();
                            background = screens.Find(o => o.name == "title");
                        }
                        return;
                    }
                    timeSinceMove += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timeSinceMove > timeforMove)
                    {
                        if (Input.directional.X > 0)
                        {
                            active.MoveRight(shapes.FindAll(o => o.id != active.id));
                            timeSinceMove = 0;
                        }
                        else if (Input.directional.X < 0)
                        {
                            active.MoveLeft(shapes.FindAll(o => o.id != active.id));
                            timeSinceMove = 0;
                        }
                    }
                }
            }
            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(gameboy.renderTarget);
            //Color color = new Color(48, 104, 80);
            Color color = new Color(224, 248, 207);
            GraphicsDevice.Clear(color);

            //_spriteBatch.Draw();
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            background.Draw(_spriteBatch);
            /*string temp = "© 2021 Gustav";
            _spriteBatch.DrawString(font, temp, new Vector2(80, 135) - (font.MeasureString(temp) / 2 * 0.25f), new Color(7, 24, 33), 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
            for (int i = 0; i < 2; i++)
            {
                temp = (i + 1) + " player";
                _spriteBatch.DrawString(font, temp, new Vector2(40 + 80 * i, 115) - (font.MeasureString(temp) / 2 * 0.25f), new Color(7, 24, 33), 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
                _spriteBatch.Draw(pixel, new Rectangle(40 + 80 * i - (int)Math.Round((font.MeasureString(temp).X / 2 * 0.25f)), (int)Math.Round(114 + (font.MeasureString(temp).Y / 2 * 0.25f)), (int)Math.Round(font.MeasureString(temp).X * 0.25f), 1), new Color(48, 104, 80));
            }*/

            //temp = "1 player";
            //_spriteBatch.DrawString(font, temp, new Vector2(40, 115) - (font.MeasureString(temp) / 2 * 0.25f), new Color(7, 24, 33), 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
            //temp = "2 player";
            //_spriteBatch.DrawString(font, temp, new Vector2(120, 115) - (font.MeasureString(temp) / 2 * 0.25f), new Color(7, 24, 33), 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            // TODO: Add your drawing code here
            float transitionTime = 0.5f;
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            if (gs == GameState.logo && stopwatch.Elapsed.TotalSeconds - timeForLogo > -transitionTime)
            {
                gameboy.Draw(_spriteBatch, ((float)stopwatch.Elapsed.TotalSeconds - timeForLogo) * 1 / transitionTime + 1);
            }
            else if (gs == GameState.startscreen && stopwatch.Elapsed.TotalSeconds < transitionTime)
            {
                gameboy.Draw(_spriteBatch, 1 - ((float)stopwatch.Elapsed.TotalSeconds) * 1 / transitionTime);
            }
            else
            {
                gameboy.Draw(_spriteBatch);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void ScreenChange(object sender, EventArgs e)
        {
            //int height = GetNearestMultiple(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, screenHeight);
            //if (height > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
            //{
            //    height -= screenHeight;
            //}
            //float ratio = height / screenHeight;
            //int width = (int)Math.Round(ratio * screenWidth);
            //_graphics.PreferredBackBufferWidth = width;
            //_graphics.PreferredBackBufferHeight = height;
            //_graphics.ApplyChanges();
            if (Window.ClientBounds.Width < screenWidth || Window.ClientBounds.Height < screenHeight)
            {
                if (Window.ClientBounds.Width < screenWidth)
                {
                    _graphics.PreferredBackBufferWidth = screenWidth;
                }
                if (Window.ClientBounds.Height < screenHeight)
                {
                    _graphics.PreferredBackBufferHeight = screenHeight;
                }
                _graphics.ApplyChanges();
            }
            gameboy.screenSize = ScreenSize();
        }

        private Rectangle ScreenSize()
        {
            Rectangle rectangle;
            int height = GetNearestMultiple(Window.ClientBounds.Height, screenHeight);
            if (height > Window.ClientBounds.Height && height > screenHeight)
            {
                height -= screenHeight;
            }
            else if (height < screenHeight)
            {
                height = screenHeight;
            }
            int ratio = height / screenHeight;
            int width = (ratio * screenWidth);
            if (width > Window.ClientBounds.Width)
            {
                width = GetNearestMultiple(Window.ClientBounds.Width, screenWidth);
                if (width > Window.ClientBounds.Width && width > screenWidth)
                {
                    width -= screenWidth;
                }
                else if (width < screenWidth)
                {
                    width = screenWidth;
                }
                ratio = width / screenWidth;
                height = ratio * screenHeight;
            }
            return new Rectangle(Window.ClientBounds.Width / 2 - width / 2, Window.ClientBounds.Height / 2 - height / 2, width, height);
        }

        private int MaxHeight()
        {
            int height = GetNearestMultiple(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, screenHeight);
            if (height > GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
            {
                height -= screenHeight;
            }
            return height - screenHeight;
        }

        private int MaxHeight(Screen screen)
        {
            int height = GetNearestMultiple(screen.Bounds.Height, screenHeight);
            if (height > screen.Bounds.Height)
            {
                height -= screenHeight;
            }
            return height - screenHeight;
        }

        private int GetNearestMultiple(int value, int factor)
        {
            return (int)Math.Round(
                              (value / (double)factor),
                              MidpointRounding.AwayFromZero
                          ) * factor;
        }
    }

    internal class RetroScreen
    {
        public RenderTarget2D renderTarget { private set; get; }
        public Rectangle screenSize;

        public RetroScreen(RenderTarget2D _renderTarget, Rectangle _rectangle)
        {
            renderTarget = _renderTarget;
            screenSize = _rectangle;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(renderTarget, screenSize, Color.White);
        }

        public void Draw(SpriteBatch _spriteBatch, float lerpAmount)
        {
            _spriteBatch.Draw(renderTarget, screenSize, Color.Lerp(Color.White, new Color(48, 104, 80), lerpAmount));
        }

        // new Color(224, 248, 207)
        // new Color(7, 24, 33)
        // new Color(48, 104, 80)
    }

    internal class MapScreen
    {
        public Texture2D texture { private set; get; }

        //public Rectangle rectangle { private set; get; }
        public List<Sprite> spritesInScreen = new List<Sprite>();

        //public List<string> textOnScreen = new List<string>();
        public List<SpriteText> textOnScreen = new List<SpriteText>();

        public string name;

        public MapScreen(Texture2D _texture, string _name)
        {
            texture = _texture;
            name = _name;
            //rectangle = _rectangle;
        }

        public void SetTex(Texture2D _texture)
        {
            texture = _texture;
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(texture, new Vector2(), Color.White);
            for (int i = 0; i < spritesInScreen.Count; i++)
            {
                spritesInScreen[i].Draw(_spriteBatch);
            }
            for (int i = 0; i < textOnScreen.Count; i++)
            {
                textOnScreen[i].Draw(_spriteBatch);
            }
        }
    }

    public class Vector2Int
    {
        public int X { get; set; }

        public int Y { get; set; }

        public Vector2Int()
        {
            X = 0;
            Y = 0;
        }

        public Vector2Int(int _x)
        {
            X = _x;
            Y = 0;
        }

        public Vector2Int(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2Int operator *(Vector2Int a, int b)
        {
            return new Vector2Int(a.X * b, a.Y * b);
        }

        public static Vector2Int operator *(Vector2Int a, float b)
        {
            return new Vector2Int((int)Math.Round(a.X * b, MidpointRounding.AwayFromZero), (int)Math.Round(a.Y * b, MidpointRounding.AwayFromZero));
        }

        public static Vector2Int operator /(Vector2Int a, int b)
        {
            return new Vector2Int(a.X / b, a.Y / b);
        }

        public static Vector2Int operator /(Vector2Int a, float b)
        {
            return new Vector2Int((int)Math.Round(a.X / b, MidpointRounding.AwayFromZero), (int)Math.Round(a.Y / b, MidpointRounding.AwayFromZero));
        }

        public static Vector2Int operator %(Vector2Int a, int b)
        {
            return new Vector2Int(a.X % b, a.Y % b);
        }

        public static Vector2Int operator %(Vector2Int a, float b)
        {
            return new Vector2Int((int)Math.Round(a.X % b, MidpointRounding.AwayFromZero), (int)Math.Round(a.Y % b, MidpointRounding.AwayFromZero));
        }

        public static readonly Vector2Int One = new Vector2Int(1, 1);
        public static readonly Vector2Int Zero = new Vector2Int(0, 0);

        public float Distance(Vector2Int b)
        {
            return AdvancedMath.Vector2Distance(new Vector2(X, Y), new Vector2(b.X, b.Y));
        }

        public Vector2Int Normalize
        {
            get
            {
                Vector2 temp = AdvancedMath.Normalize(new Vector2(X, Y));
                return new Vector2Int((int)Math.Round(temp.X, MidpointRounding.AwayFromZero), (int)Math.Round(temp.Y, MidpointRounding.AwayFromZero));
            }
        }
    }
}