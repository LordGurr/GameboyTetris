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
        private Sprite player;
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
            Texture2D texture = Content.Load<Texture2D>("linktransdown0");
            titleScreen = Content.Load<Texture2D>("tetrisTitleScreenCropCol");
            player = new Sprite(texture, new Vector2(screenWidth / 2, screenHeight / 2));
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

            if (gs == GameState.playing || gs == GameState.paused)
            {
                player.Draw(_spriteBatch);
            }
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
            _spriteBatch.Draw(renderTarget, screenSize, Color.Lerp(Color.White, Color.Black, lerpAmount));
        }
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
}