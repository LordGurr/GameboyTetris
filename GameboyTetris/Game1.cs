using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

//using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace GameboyTetris
{
    internal enum GameState
    { logo, startscreen, playing, paused, gameover, settings };

    internal enum Used
    {
        thisTurn, PreviusTurn, Longer
    };

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
        private float timeforMove = 0.15f;
        private float timeSinceMove = 0;
        private int ids = 0;
        private Texture2D[] blocks;
        private Random rng = new Random();
        private int linescleared = 0;
        private SpriteText linesClearedText;
        private SpriteText scoreText;
        private SpriteText pausedText;

        private List<int> upComingShapes;

        private bool debug = false;
        private SpriteText originText;
        private SpriteText timeSinceUpdateText;

        private int[] lineScore = new int[4]
        {
            40,
            100,
            300,
            1200,
        };

        private int score = 0;

        private SoundEffect logoSound;
        private SoundEffect movePiece;
        private SoundEffect rotatePiece;

        private SoundEffect pieceLanded;
        private SoundEffect lineCleared;
        private SoundEffect tetris;

        private Song[] mySong = new Song[6];
        private Shape ghost;

        private Shape carry;
        private Used carryUsed = Used.Longer;

        private Shape next;
        private SpriteText nextOrCarry;

        private bool showGhost = true;
        private bool modernControls = false;
        private bool carryOn = true;
        private bool soundOn = true;
        private bool musicOn = true;
        private bool lockDelay = true;
        private int currentSetting = 0;

        private float timeForFlash = 0.3f;
        private float timeSinceFlash = 0;

        private float lockDelayLength = 0.5f;
        private bool lastMoveFatal = false;

        /*bool[] settings = new bool[]
        {
            showGhost,
            modernControls,
            carryOn,
        };*/

        private Vector2 slotPosition = new Vector2(136, 117);

        private SpriteText[] settingCursors;

        private bool clearLineAnim = false;
        private float lengthOfClearLineAnim = 0.5f;
        private float timeSinceClearLineAnim = 0;
        private float timeForClearFlash = 0.125f;
        private float timeSinceClearFlash = 0;

        private List<Sprite> clearedSprites = new List<Sprite>();
        private List<Texture2D> clearedTextures = new List<Texture2D>();
        private List<int> clearedLinesToRemove = new List<int>();
        private bool clearedSetToWhite = true;
        private Texture2D clearedTex;

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
            screens.Add(new MapScreen(Content.Load<Texture2D>("tetrisSettings"), "settings"));
            font = Content.Load<SpriteFont>("font");
            font.DefaultCharacter = '#';
            pixel = Content.Load<Texture2D>("Square1");
            MapScreen map = screens.Find(o => o.name == "title");
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(80, 135), SpriteText.DrawMode.Middle, font, "© " + DateTime.Now.Year + " Gustav"));
            //map.textOnScreen.Add(new SpriteText(pixel, new Vector2(40, 115), SpriteText.DrawMode.MiddleUnderline, "1"))
            for (int i = 0; i < 1; i++)
            {
                string temp = (i + 1) + " player";
                //_spriteBatch.DrawString(font, temp, new Vector2(40 + 80 * i, 115) - (font.MeasureString(temp) / 2 * 0.25f), new Color(7, 24, 33), 0, new Vector2(), 0.25f, SpriteEffects.None, 0);
                //_spriteBatch.Draw(pixel, new Rectangle(40 + 80 * i - (int)Math.Round((font.MeasureString(temp).X / 2 * 0.25f)), (int)Math.Round(114 + (font.MeasureString(temp).Y / 2 * 0.25f)), (int)Math.Round(font.MeasureString(temp).X * 0.25f), 1), new Color(48, 104, 80));
                map.textOnScreen.Add(new SpriteText(pixel, new Vector2(40 + 80 * i, 115), SpriteText.DrawMode.MiddleUnderline, font, temp));
            }
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(40 + 80, 115), SpriteText.DrawMode.MiddleUnderline, font, "Settings"));
            cursor = new SpriteText(pixel, new Vector2(11, 115), SpriteText.DrawMode.Middle, font, "€");
            map.textOnScreen.Add(cursor);
            settingCursors = new SpriteText[6];
            map = screens.Find(o => o.name == "settings");
            int start = 30;
            for (int i = 0; i < settingCursors.Length; i++)
            {
                settingCursors[i] = new SpriteText(pixel, new Vector2(30, start + 7 + 17 * i), SpriteText.DrawMode.Middle, font, "€");
                map.textOnScreen.Add(settingCursors[i]);
                map.textOnScreen.Add(new SpriteText(pixel, new Vector2(45, start + 7 + 17 * i), SpriteText.DrawMode.Middle, font, "On"));
                map.textOnScreen.Add(new SpriteText(pixel, new Vector2(100, start + 7 + 17 * i), SpriteText.DrawMode.Middle, font, "Off"));
                string temp = string.Empty;
                switch (i)
                {
                    case 0:
                        temp = "Show Ghost";
                        break;

                    case 1:
                        temp = "Modern Controls";
                        break;

                    case 2:
                        temp = "Activate Hold";
                        break;

                    case 3:
                        temp = "Sound";
                        break;

                    case 4:
                        temp = "Music";
                        break;

                    case 5:
                        temp = "Lock Delay";
                        break;
                }
                bool setTrue = true;
                switch (i)
                {
                    case 0:
                        setTrue = showGhost;
                        break;

                    case 1:
                        setTrue = modernControls;
                        break;

                    case 2:
                        setTrue = carryOn;
                        break;

                    case 3:
                        setTrue = soundOn;
                        break;

                    case 4:
                        setTrue = musicOn;
                        break;

                    case 5:
                        setTrue = lockDelay;
                        break;
                }
                if (!setTrue)
                {
                    settingCursors[i].position.X = 80;
                }
                map.textOnScreen.Add(new SpriteText(pixel, new Vector2(80, start + 17 * i), SpriteText.DrawMode.Middle, font, temp));
            }
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(80, 18), SpriteText.DrawMode.MiddleUnderline, font, "Settings"));
            Texture2D texture = Content.Load<Texture2D>("tetrisPlayingTextlessCol");
            screens.Add(new MapScreen(texture, "playing"));
            map = screens.Find(o => o.name == "playing");
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(133, 11), SpriteText.DrawMode.Middle, font, "Score"));
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(131, 52), SpriteText.DrawMode.Middle, font, "Level"));
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(131, 75), SpriteText.DrawMode.Middle, font, "Lines"));
            linesClearedText = new SpriteText(pixel, new Vector2(133, 77), SpriteText.DrawMode.Normal, font, "0");
            map.textOnScreen.Add(linesClearedText);
            scoreText = new SpriteText(pixel, new Vector2(120, 21), SpriteText.DrawMode.Normal, font, "0");
            map.textOnScreen.Add(scoreText);
            pausedText = new SpriteText(pixel, new Vector2(57, 72), SpriteText.DrawMode.Middle, font, "Paused");
            map.textOnScreen.Add(new SpriteText(pixel, new Vector2(120, 100), SpriteText.DrawMode.Normal, font, carryOn ? "H" : "N"));
            nextOrCarry = map.textOnScreen[^1];

            originText = new SpriteText(pixel, new Vector2(17, -2), SpriteText.DrawMode.Normal, font, "");
            timeSinceUpdateText = new SpriteText(pixel, new Vector2(17, 10), SpriteText.DrawMode.Normal, font, "");

            int xCount = 8;
            int yCount = 1;
            blocks = AdvancedMath.Split(Content.Load<Texture2D>("TetrisSpriteSheet"), 8, 8, out xCount, out yCount);
            Texture2D[] tempBlocks = new Texture2D[blocks.Length - 2];
            for (int i = 0; i < tempBlocks.Length; i++)
            {
                tempBlocks[i] = blocks[i + 2];
            }
            blocks = tempBlocks;

            upComingShapes = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                upComingShapes.Add(i);
            }
            upComingShapes.Shuffle();

            mySong[1] = Content.Load<Song>("Audio/Music/01. Title");
            mySong[5] = mySong[1];
            mySong[2] = Content.Load<Song>("Audio/Music/03. A-Type Music (Korobeiniki)");
            mySong[4] = Content.Load<Song>("Audio/Music/18. Game Over");

            logoSound = Content.Load<SoundEffect>("Audio/SoundEffects/nintendo-game-boy-startup");
            if (soundOn)
            {
                logoSound.Play();
            }
            movePiece = Content.Load<SoundEffect>("Audio/SoundEffects/tetris-gb-18-move-piece");
            rotatePiece = Content.Load<SoundEffect>("Audio/SoundEffects/tetris-gb-19-rotate-piece");

            pieceLanded = Content.Load<SoundEffect>("Audio/SoundEffects/tetris-gb-27-piece-landed");
            lineCleared = Content.Load<SoundEffect>("Audio/SoundEffects/tetris-gb-21-line-clear"); ;
            tetris = Content.Load<SoundEffect>("Audio/SoundEffects/tetris-gb-22-tetris-4-lines");

            clearedTex = Content.Load<Texture2D>("clearedTex");
            //font.            // TODO: use this.Content to load your game content here
        }

        private void SetMusic()
        {
            if (musicOn && mySong[(int)gs] != null)
            {
                MediaPlayer.Play(mySong[(int)gs]);
                MediaPlayer.IsRepeating = true;
            }
            else
            {
                MediaPlayer.Stop();
            }
        }

        private bool GetSetting()
        {
            bool isTrue = true;
            switch (currentSetting)
            {
                case 0:
                    isTrue = showGhost;
                    break;

                case 1:
                    isTrue = modernControls;
                    break;

                case 2:
                    isTrue = carryOn;
                    break;

                case 3:
                    isTrue = soundOn;
                    break;

                case 4:
                    isTrue = musicOn;
                    break;

                case 5:
                    isTrue = lockDelay;
                    break;
            }
            return isTrue;
        }

        private void SetSetting(bool isTrue)
        {
            switch (currentSetting)
            {
                case 0:
                    showGhost = isTrue;
                    break;

                case 1:
                    modernControls = isTrue;
                    break;

                case 2:
                    carryOn = isTrue;
                    break;

                case 3:
                    soundOn = isTrue;
                    break;

                case 4:
                    if (musicOn != isTrue)
                    {
                        if (!isTrue)
                        {
                            MediaPlayer.Stop();
                        }
                        else
                        {
                            musicOn = isTrue;
                            SetMusic();
                        }
                    }
                    musicOn = isTrue;
                    break;

                case 5:
                    lockDelay = isTrue;
                    break;
            }
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
            var f = (Form)Control.FromHandle(Window.Handle);
            if (f.WindowState != FormWindowState.Maximized)
            {
                if (Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.PageUp))
                {
                    IncreaseScreenSize();
                }
                if (Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.PageDown))
                {
                    DecreaseScreenSize();
                }
            }
            if (Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.PrintScreen))
            {
                debug = !debug;
            }
            if (gs == GameState.logo && stopwatch.Elapsed.TotalSeconds > timeForLogo)
            {
                gs = GameState.startscreen;
                SetMusic();
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
                else if (selectedLeft && (Input.directional.X > 0) && !(Input.GetButton(Buttons.Start) || Input.GetButton(Microsoft.Xna.Framework.Input.Keys.Enter)))
                {
                    selectedLeft = false;
                    cursor.position.X = 89;
                }
                else if (Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || Input.GetButtonDown(Buttons.Back))
                {
                    selectedLeft = !selectedLeft;
                    if (selectedLeft)
                    {
                        cursor.position.X = 11;
                    }
                    else
                    {
                        cursor.position.X = 89;
                    }
                }
                if (Input.GetButtonDown(Buttons.Start) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Enter))
                {
                    if (!selectedLeft)
                    {
                        /*selectedLeft = true;
                        cursor.position.X = 11;*/
                        gs = GameState.settings;
                        background = screens.Find(o => o.name == "settings");
                        SetMusic();
                    }
                    else
                    {
                        gs = GameState.playing;
                        background = screens.Find(o => o.name == "playing");
                        SetMusic();
                    }
                }
            }
            else if (gs == GameState.settings)
            {
                timeSinceFlash += (float)gameTime.ElapsedGameTime.TotalSeconds;
                timeSinceMove += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timeSinceFlash > timeForFlash)
                {
                    settingCursors[currentSetting].text = settingCursors[currentSetting].text == "€" ? "§" : "€";
                    timeSinceFlash = 0;
                }
                if (timeSinceMove > timeforMove * 7 || Input.yDirectionDown)
                {
                    if (Input.directional.Y > 0)
                    {
                        timeSinceMove = 0;
                        currentSetting++;
                        if (currentSetting > settingCursors.Length - 1)
                        {
                            currentSetting = settingCursors.Length - 1;
                        }
                        else
                        {
                            settingCursors[currentSetting - 1].text = "€";
                            settingCursors[currentSetting].text = "§";
                        }
                        if (soundOn)
                        {
                            movePiece.Play();
                        }
                    }
                    else if (Input.directional.Y < 0)
                    {
                        timeSinceMove = 0;
                        currentSetting--;
                        if (currentSetting < 0)
                        {
                            currentSetting = 0;
                        }
                        else
                        {
                            settingCursors[currentSetting + 1].text = "€";
                            settingCursors[currentSetting].text = "§";
                        }
                        if (soundOn)
                        {
                            movePiece.Play();
                        }
                    }
                }
                bool isTrue = GetSetting();
                if (timeSinceMove > timeforMove || Input.xDirectionDown) // 30 80
                {
                    bool tempSet = false;
                    if (Input.directional.X > 0 && isTrue)
                    {
                        timeSinceMove = 0;
                        isTrue = !isTrue;
                        settingCursors[currentSetting].position.X = 80;
                        tempSet = true;
                    }
                    else if (Input.directional.X < 0 && !isTrue)
                    {
                        timeSinceMove = 0;
                        isTrue = !isTrue;
                        settingCursors[currentSetting].position.X = 30;
                        tempSet = true;
                    }
                    SetSetting(isTrue);
                    if (tempSet && soundOn)
                    {
                        movePiece.Play();
                    }
                }
                if (Input.GetButtonDown(Keys.Space) || Input.GetButtonDown(Keys.Back) || Input.GetButtonDown(Keys.Z) || Input.GetButtonDown(Keys.X) || Input.GetButtonDown(Keys.C) || Input.GetButtonDown(Keys.Enter) || Input.GetButtonDown(Keys.LeftShift))
                {
                    gs = GameState.startscreen;
                    background = screens.Find(o => o.name == "title");
                    SetMusic();
                    nextOrCarry.text = carryOn ? "H" : "N";
                }
            }
            if (gs == GameState.playing)
            {
                linesClearedText.text = linescleared.ToString();
                scoreText.text = score.ToString();
                timeSinceUpdate += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (clearLineAnim)
                {
                    timeSinceClearLineAnim += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    timeSinceClearFlash += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timeSinceClearLineAnim < lengthOfClearLineAnim)
                    {
                        if (timeSinceClearFlash > timeForClearFlash)
                        {
                            if (clearedSetToWhite)
                            {
                                for (int i = 0; i < clearedSprites.Count; i++)
                                {
                                    clearedSprites[i].tex = clearedTex;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < clearedSprites.Count; i++)
                                {
                                    clearedSprites[i].tex = clearedTextures[i];
                                }
                            }
                            timeSinceClearFlash = 0;
                            clearedSetToWhite = !clearedSetToWhite;
                        }
                    }
                    else
                    {
                        MapScreen temp = screens.Find(o => o.name == "playing");
                        for (int i = 0; i < clearedSprites.Count; i++)
                        {
                            temp.spritesInScreen.Remove(clearedSprites[i]);
                            for (int j = 0; j < shapes.Count; j++)
                            {
                                if (shapes[j].sprites.Contains(clearedSprites[i]))
                                {
                                    shapes[j].sprites.Remove(clearedSprites[i]);
                                    break;
                                }
                            }
                        }
                        for (int i = 0; i < shapes.Count; i++)
                        {
                            for (int j = 0; j < shapes[i].sprites.Count; j++)
                            {
                                for (int k = 0; k < clearedLinesToRemove.Count; k++)
                                {
                                    if (shapes[i].sprites[j].position.Y < clearedLinesToRemove[k])
                                    {
                                        shapes[i].sprites[j].position.Y += shapes[i].sprites[j].rectangle.Height;
                                    }
                                }
                            }
                        }
                        clearLineAnim = false;
                        clearedSprites.Clear();
                        clearedLinesToRemove.Clear();
                        clearedTextures.Clear();
                        clearedSetToWhite = true;
                        timeSinceClearLineAnim = 0;
                        timeSinceClearFlash = 0;
                        if (soundOn)
                        {
                            pieceLanded.Play();
                        }
                    }
                    return;
                }
                if (!ShapeActive)
                {
                    active = new Shape(blocks[upComingShapes[0] > 5 ? 0 : upComingShapes[0]], ids, rng, upComingShapes[0]);
                    if (showGhost)
                    {
                        if (ghost != null)
                        {
                            for (int i = 0; i < ghost.sprites.Count; i++)
                            {
                                screens.Find(o => o.name == "playing").spritesInScreen.Remove(ghost.sprites[i]);
                            }
                        }
                        ghost = new Shape(active, Color.White * 0.4f);
                        while (ghost.active)
                        {
                            ghost.Update(shapes.FindAll(o => o.id != active.id));
                        }
                        screens.Find(o => o.name == "playing").spritesInScreen.AddRange(ghost.sprites);
                    }
                    upComingShapes.RemoveAt(0);
                    ids++;
                    ShapeActive = true;
                    screens.Find(o => o.name == "playing").spritesInScreen.AddRange(active.sprites);
                    shapes.Add(active);
                    if (upComingShapes.Count < 7)
                    {
                        List<int> tempUpComingShapes = new List<int>();
                        for (int i = 0; i < 7; i++)
                        {
                            tempUpComingShapes.Add(i);
                        }
                        tempUpComingShapes.Shuffle();
                        upComingShapes.AddRange(tempUpComingShapes);
                    }
                    if (!carryOn)
                    {
                        if (next != null)
                        {
                            for (int i = 0; i < next.sprites.Count; i++)
                            {
                                screens.Find(o => o.name == "playing").spritesInScreen.Remove(next.sprites[i]);
                            }
                        }
                        next = new Shape(blocks[upComingShapes[0] > 5 ? 0 : upComingShapes[0]], -1, rng, upComingShapes[0]);
                        next.GoToSlot(slotPosition);
                        screens.Find(o => o.name == "playing").spritesInScreen.AddRange(next.sprites);
                    }
                    if (carryUsed == Used.thisTurn)
                    {
                        carryUsed = Used.PreviusTurn;
                    }
                    else if (carryUsed == Used.PreviusTurn)
                    {
                        carryUsed = Used.Longer;
                    }

                    lastMoveFatal = false;

                    if (active.IsColliding(shapes.FindAll(o => o.id != active.id)))
                    {
                        screens.Find(o => o.name == "playing").spritesInScreen.Clear();
                        gs = GameState.startscreen;
                        shapes.Clear();
                        linescleared = 0;
                        background = screens.Find(o => o.name == "title");
                        SetMusic();
                        carry = null;
                        ShapeActive = false;
                        score = 0;
                    }
                }
                else
                {
                    float speed = timeforUpdate - ((float)linescleared / 60);
                    speed = speed < 0.2f ? 0.2f : speed;
                    if ((timeSinceUpdate > speed || Input.directional.Y > 0 && timeSinceUpdate > timeforUpdate / 10) && (!lockDelay || (lockDelay && !lastMoveFatal)) || (lockDelay && lastMoveFatal && timeSinceUpdate > speed + lockDelayLength))
                    {
                        timeSinceUpdate = 0;
                        active.Update(shapes.FindAll(o => o.id != active.id));
                        if (!active.active)
                        {
                            if (lockDelay && Input.directional.Y <= 0 && !lastMoveFatal)
                            {
                                lastMoveFatal = true;
                                active.SetActive(true);
                            }
                            else
                            {
                                ShapeActive = false;
                                /*
                                if (active.AboveBorder())
                                {
                                    screens.Find(o => o.name == "playing").spritesInScreen.Clear();
                                    gs = GameState.startscreen;
                                    shapes.Clear();
                                    linescleared = 0;
                                    background = screens.Find(o => o.name == "title");
                                    SetMusic();
                                    carry = null;
                                }
                                else*/
                                {
                                    int tempLinesCleared = CheckForLine();
                                    if (tempLinesCleared > 0)
                                    {
                                        score += lineScore[tempLinesCleared - 1];
                                        if (soundOn)
                                        {
                                            if (tempLinesCleared > 3)
                                            {
                                                tetris.Play();
                                            }
                                            else
                                            {
                                                lineCleared.Play();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (soundOn)
                                        {
                                            pieceLanded.Play();
                                        }
                                    }
                                    if (carry != null)
                                    {
                                        screens.Find(o => o.name == "playing").spritesInScreen.AddRange(carry.sprites);
                                    }
                                }
                                return;
                            }
                        }
                        else
                        {
                            lastMoveFatal = false;
                            //if (showGhost)
                            //{
                            //    ghost.SetActive(true);
                            //    for (int i = 0; i < active.sprites.Count; i++)
                            //    {
                            //        ghost.sprites[i].position = active.sprites[i].position;
                            //    }
                            //    while (ghost.active)
                            //    {
                            //        ghost.Update(shapes.FindAll(o => o.id != active.id));
                            //    }
                            //}
                        }
                    }
                    if (Input.GetButtonDown(Buttons.A) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.X))
                    {
                        active.RotateRight(shapes.FindAll(o => o.id != active.id));
                        if (soundOn)
                        {
                            rotatePiece.Play();
                        }
                        if (showGhost)
                        {
                            ghost.SetActive(true);
                            for (int i = 0; i < active.sprites.Count; i++)
                            {
                                ghost.sprites[i].position = active.sprites[i].position;
                            }
                            while (ghost.active)
                            {
                                ghost.Update(shapes.FindAll(o => o.id != active.id));
                            }
                        }
                        //if (lastMoveFatal)
                        //{
                        //    hasMovedSinceFatal = false;
                        //}
                        //lastMoveFatal = false;
                    }
                    else if (Input.GetButtonDown(Buttons.B) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Z))
                    {
                        active.RotateLeft(shapes.FindAll(o => o.id != active.id));
                        if (soundOn)
                        {
                            rotatePiece.Play();
                        }
                        if (showGhost)
                        {
                            ghost.SetActive(true);
                            for (int i = 0; i < active.sprites.Count; i++)
                            {
                                ghost.sprites[i].position = active.sprites[i].position;
                            }
                            while (ghost.active)
                            {
                                ghost.Update(shapes.FindAll(o => o.id != active.id));
                            }
                        }
                        //if (lastMoveFatal)
                        //{
                        //    hasMovedSinceFatal = false;
                        //}
                        //lastMoveFatal = false;
                    }
                    else if (!modernControls && (Input.GetButtonDown(Buttons.DPadUp) || Input.GetButtonDown(Buttons.LeftThumbstickUp) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Up) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.W)) || modernControls && (Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Space) || Input.GetButtonDown(Buttons.Y)))
                    {
                        timeSinceUpdate = 0;
                        while (active.active)
                        {
                            active.Update(shapes.FindAll(o => o.id != active.id));
                        }
                        ShapeActive = false;
                        /*if (active.AboveBorder())
                        {
                            screens.Find(o => o.name == "playing").spritesInScreen.Clear();
                            gs = GameState.startscreen;
                            shapes.Clear();
                            linescleared = 0;
                            background = screens.Find(o => o.name == "title");
                            SetMusic();
                            carry = null;
                        }
                        else*/
                        {
                            int tempLinesCleared = CheckForLine();
                            if (tempLinesCleared > 0)
                            {
                                score += lineScore[tempLinesCleared - 1];
                                if (soundOn)
                                {
                                    if (tempLinesCleared > 3)
                                    {
                                        tetris.Play();
                                    }
                                    else
                                    {
                                        lineCleared.Play();
                                    }
                                }
                            }
                            else
                            {
                                if (soundOn)
                                {
                                    pieceLanded.Play();
                                }
                            }
                            if (carry != null)
                            {
                                screens.Find(o => o.name == "playing").spritesInScreen.AddRange(carry.sprites);
                            }
                        }
                        return;
                    }
                    else if (modernControls && (Input.GetButtonDown(Buttons.DPadUp) || Input.GetButtonDown(Buttons.LeftThumbstickUp) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Up) || Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.W)))
                    {
                        active.RotateLeft(shapes.FindAll(o => o.id != active.id));
                        if (soundOn)
                        {
                            rotatePiece.Play();
                        }
                        if (showGhost)
                        {
                            ghost.SetActive(true);
                            for (int i = 0; i < active.sprites.Count; i++)
                            {
                                ghost.sprites[i].position = active.sprites[i].position;
                            }
                            while (ghost.active)
                            {
                                ghost.Update(shapes.FindAll(o => o.id != active.id));
                            }
                        }
                        //if (lastMoveFatal)
                        //{
                        //    hasMovedSinceFatal = false;
                        //}
                        //lastMoveFatal = false;
                    }
                    else if (carryOn && carryUsed == Used.Longer && (Input.GetButtonDown(Keys.C) || Input.GetButtonDown(Buttons.LeftShoulder) || Input.GetButtonDown(Buttons.RightShoulder)))
                    {
                        carryUsed = Used.thisTurn;
                        bool carryIsNull = !(carry != null);// if carry is null. Written like this due to horror stories heard
                        Shape tempCarry;
                        if (!carryIsNull)
                        {
                            for (int i = 0; i < carry.sprites.Count; i++)
                            {
                                screens.Find(o => o.name == "playing").spritesInScreen.Remove(carry.sprites[i]);
                            }
                            tempCarry = new Shape(carry, Color.White);
                            upComingShapes.Insert(0, tempCarry.GetShape);
                        }
                        carry = new Shape(active, Color.White);
                        carry.ResetRotation();
                        carry.GoToSlot(slotPosition);
                        screens.Find(o => o.name == "playing").spritesInScreen.AddRange(carry.sprites);
                        shapes.Remove(active);
                        for (int i = 0; i < active.sprites.Count; i++)
                        {
                            screens.Find(o => o.name == "playing").spritesInScreen.Remove(active.sprites[i]);
                        }
                        active.SetActive(false);
                        ShapeActive = false;
                    }
                    timeSinceMove += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timeSinceMove > timeforMove || Input.xDirectionDown)
                    {
                        if (Input.directional.X > 0)
                        {
                            active.MoveRight(shapes.FindAll(o => o.id != active.id));
                            timeSinceMove = 0;
                            if (soundOn)
                            {
                                movePiece.Play();
                            }
                            if (showGhost)
                            {
                                ghost.SetActive(true);
                                for (int i = 0; i < active.sprites.Count; i++)
                                {
                                    ghost.sprites[i].position = active.sprites[i].position;
                                }
                                while (ghost.active)
                                {
                                    ghost.Update(shapes.FindAll(o => o.id != active.id));
                                }
                            }
                            //if (lastMoveFatal)
                            //{
                            //    hasMovedSinceFatal = false;
                            //}
                            //lastMoveFatal = false;
                        }
                        else if (Input.directional.X < 0)
                        {
                            active.MoveLeft(shapes.FindAll(o => o.id != active.id));
                            timeSinceMove = 0;
                            if (soundOn)
                            {
                                movePiece.Play();
                            }
                            if (showGhost)
                            {
                                ghost.SetActive(true);
                                for (int i = 0; i < active.sprites.Count; i++)
                                {
                                    ghost.sprites[i].position = active.sprites[i].position;
                                }
                                while (ghost.active)
                                {
                                    ghost.Update(shapes.FindAll(o => o.id != active.id));
                                }
                            }
                            //if (lastMoveFatal)
                            //{
                            //    hasMovedSinceFatal = false;
                            //}
                            //lastMoveFatal = false;
                        }
                    }
                    if (Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Enter))
                    {
                        gs = GameState.paused;
                        MapScreen map = screens.Find(o => o.name == "playing");
                        map.textOnScreen.Add(pausedText);
                        SetMusic();
                    }
                }
            }
            else if (gs == GameState.paused)
            {
                if (Input.GetButtonDown(Microsoft.Xna.Framework.Input.Keys.Enter))
                {
                    gs = GameState.playing;
                    MapScreen map = screens.Find(o => o.name == "playing");
                    map.textOnScreen.Remove(pausedText);
                    SetMusic();
                }
            }
            // TODO: Add your update logic here
            base.Update(gameTime);
        }

        private int CheckForLine()
        {
            clearedSprites.Clear();
            clearedLinesToRemove.Clear();
            clearedTextures.Clear();
            int clearedLines = 0;
            for (int i = 1; i < 138; i += 8)
            {
                int blocks = 0;
                for (int a = 0; a < shapes.Count; a++)
                {
                    blocks += shapes[a].sprites.FindAll(o => o.position.Y == i).Count;
                }
                if (blocks == 10)
                {
                    linescleared++;
                    clearedLines++;
                    clearedLinesToRemove.Add(i);
                    clearLineAnim = true;
                    for (int a = 0; a < shapes.Count; a++)
                    {
                        for (int b = 0; b < shapes[a].sprites.Count; b++)
                        {
                            if (shapes[a].sprites[b].position.Y == i)
                            {
                                clearedSprites.Add(shapes[a].sprites[b]);
                                clearedTextures.Add(shapes[a].sprites[b].tex);

                                /*
                                MapScreen temp = screens.Find(o => o.name == "playing");
                                int index = temp.spritesInScreen.FindIndex(o => o.position == shapes[a].sprites[b].position);
                                if (index >= 0)
                                {
                                    temp.spritesInScreen.RemoveAt(index);
                                }
                                shapes[a].sprites.RemoveAt(b);
                                b--;*/
                            }
                            else if (shapes[a].sprites[b].position.Y < i)
                            {
                                /*
                                shapes[a].sprites[b].position.Y += shapes[a].sprites[b].rectangle.Height;
                                */
                            }
                        }
                    }
                }
            }
            MapScreen screen = screens.Find(o => o.name == "playing");
            for (int i = 0; i < screen.spritesInScreen.Count; i++)
            {
                bool found = false;
                for (int a = 0; a < shapes.Count; a++)
                {
                    if (shapes[a].sprites.Any(o => o.position == screen.spritesInScreen[i].position))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    screen.spritesInScreen.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i].sprites.Count < 1)
                {
                    shapes.RemoveAt(i);
                    i--;
                }
                else
                {
                    for (int a = 0; a < shapes[i].sprites.Count; a++)
                    {
                        if (!screen.spritesInScreen.Any(o => o.position == shapes[i].sprites[a].position))
                        {
                            screen.spritesInScreen.Add(shapes[i].sprites[a]);
                        }
                    }
                }
            }
            return clearedLines;
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

            background.Draw(_spriteBatch, gs == GameState.playing);
            if (debug && gs == GameState.playing)
            {
                int size = 4;
                _spriteBatch.Draw(pixel, active.AccessOrigin, null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
                _spriteBatch.Draw(pixel, slotPosition, null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
                originText.text = active.AccessOrigin.ToString();
                originText.Draw(_spriteBatch);
                timeSinceUpdateText.text = timeSinceUpdate.ToString("F1");
                timeSinceUpdateText.Draw(_spriteBatch);
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

        private void IncreaseScreenSize()
        {
            int screenSize = gameboy.screenSize.Width / screenWidth;
            int tempScreenWidth = (screenSize + 1) * screenWidth;
            int tempScreenHeight = (screenSize + 1) * screenHeight;

            if (tempScreenWidth < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width && tempScreenHeight < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
            {
                _graphics.PreferredBackBufferWidth = tempScreenWidth;
                _graphics.PreferredBackBufferHeight = tempScreenHeight;
                gameboy.screenSize = new Rectangle(0, 0, tempScreenWidth, tempScreenHeight);
                _graphics.ApplyChanges();
            }
        }

        private void DecreaseScreenSize()
        {
            int screenSize = gameboy.screenSize.Width / screenWidth;
            if (screenSize > 1)
            {
                int tempScreenWidth = (screenSize - 1) * screenWidth;
                int tempScreenHeight = (screenSize - 1) * screenHeight;

                _graphics.PreferredBackBufferWidth = tempScreenWidth;
                _graphics.PreferredBackBufferHeight = tempScreenHeight;
                gameboy.screenSize = new Rectangle(0, 0, tempScreenWidth, tempScreenHeight);
                _graphics.ApplyChanges();
            }
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

        public void Draw(SpriteBatch _spriteBatch, bool drawSprites)
        {
            _spriteBatch.Draw(texture, new Vector2(), Color.White);
            if (drawSprites)
            {
                for (int i = 0; i < spritesInScreen.Count; i++)
                {
                    spritesInScreen[i].Draw(_spriteBatch);
                }
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

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
    }

    internal static class MyExtensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}