using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Advertising.Mobile.Xna;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;
using Microsoft.Devices.Sensors;
using FarseerPhysics.Dynamics.Contacts;
using System.IO;
using System.IO.IsolatedStorage;

namespace ShuffleBowl
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>

    static class GAMEMODE
    {
        public const int MENU = 0;
        public const int GAMERUNNING = 1;
        public const int RUNNING_SWIPING = 2;
        public const int RUNNING_INMOTION = 3;
        public const int WAITINGFORNAME = 4;
        public const int NAMECOMPLETE = 5;
        public const int GAMESTART = 6;
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        static int gameState = GAMEMODE.MENU;
        static int lastState = GAMEMODE.MENU;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D hsTexture;
        Texture2D puckLeft;
        Texture2D sGame;
        Texture2D eGame;
        Texture2D rGame;
        Texture2D sOn;
        Texture2D sOff;
        Texture2D cGame;
        Texture2D title;
        float button0y;
        float button1y;
        float button2y;  

        Sprite puck1;
        Sprite puck2;
        Sprite puck3;
        Sprite puck4;
        Sprite pin1;
        Sprite pin2;
        Sprite pin3;
        Sprite pin4;
        Sprite pin5;
        Sprite pin6;
        Sprite pin7;
        Sprite pin8;
        Sprite pin9;
        Sprite pin10;

        World world;
        Body boundary;
        Body playerBody;
        HighScoreData hs;
        Viewport viewport;

        float max, min;
        HUD hud;

        Vector2 curPos;
        Vector2 lastPos;
        Texture2D background;
        Texture2D ErrorLine;
        TouchLocationState isPressed = TouchLocationState.Released;
        int turn = 0;
        int lastA = -1;
        int lastB = -1;
        int scoreInc = 0;
        float redLine;
        static char[] newName;

        bool gameStarted = false;
        bool soundEnabled = false;
        bool multitouch = false;

        public Game1()
        {
            newName = new char[3];
            max = 0;
            min = 0;
            button0y = 0f;
            button0y = 1f;
            button0y = 2f;
            graphics = new GraphicsDeviceManager(this);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
            graphics.IsFullScreen = true;
            lastPos.X = -1;
            lastPos.Y = -1;
            redLine = 0;
            hs = new HighScoreData(5);
            Content.RootDirectory = "Content";

            // Initialize the AdGameComponent and add it to the game’s Component object
            AdGameComponent.Initialize(this, "30c27ec6-9f5b-46e9-9bac-89620c5fcdda");
            Components.Add(AdGameComponent.Current);

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (savegameStorage.FileExists(HighScoresFilename))
                {
                    using (IsolatedStorageFileStream fs = savegameStorage.OpenFile(HighScoresFilename, System.IO.FileMode.Open))
                    {
                        if (fs != null)
                        {
                            hs = LoadHighScores(fs, hs.Count);
                        }
                    }
                }
            }
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            viewport = this.GraphicsDevice.Viewport;
            hsTexture = Content.Load<Texture2D>("hsTable");
            puckLeft = Content.Load<Texture2D>("PucksLeft");
            sGame = Content.Load<Texture2D>("StartGame");
            eGame = Content.Load<Texture2D>("EndGame");
            cGame = Content.Load<Texture2D>("ContinueGame");
            rGame = Content.Load<Texture2D>("RestartGame");
            sOff = Content.Load<Texture2D>("SoundOff");
            sOn = Content.Load<Texture2D>("SoundOn");
            title = Content.Load<Texture2D>("Title");
            SoundEffect soundEngine = Content.Load<SoundEffect>("collisionSoundStereo");
            button0y = viewport.Height * .64f;
            button1y = viewport.Height * .73f;
            button2y = viewport.Height * .82f;

            Texture2D playerTexture = Content.Load<Texture2D>("puck");
            background = Content.Load<Texture2D>("BowlingLane");
            ErrorLine = Content.Load<Texture2D>("RedLine");

            //boundries
            world = new World(Vector2.Zero);
            var bounds = GetBounds();
            boundary = BodyFactory.CreateLoopShape(world, bounds);
            boundary.CollisionCategories = Category.All;
            boundary.CollidesWith = Category.All;
            boundary.Friction = 0f;
            boundary.Restitution = 1f;
            boundary.BodyType = BodyType.Static;
            boundary.UserData = "BORDER";

            Vector2 initPin = new Vector2(ConvertUnits.ToSimUnits(viewport.Width / 2f), ConvertUnits.ToSimUnits(viewport.Height * .8f));
            redLine = ConvertUnits.ToSimUnits(viewport.Height * .75f);
            puck1 = new Sprite(playerTexture, initPin, world, "PUCK");
            puck2 = new Sprite(playerTexture, initPin, world, "PUCK");
            puck3 = new Sprite(playerTexture, initPin, world, "PUCK");
            puck4 = new Sprite(playerTexture, initPin, world, "PUCK");
//            puck1.body.Mass = .6f;
            puck1.body.OnCollision += body_OnCollision;
            puck1.body.BodyId = 10;
//            puck2.body.Mass = .6f;
            puck2.body.OnCollision += body_OnCollision;
            puck2.body.BodyId = 11;
//            puck3.body.Mass = .6f;
            puck3.body.OnCollision += body_OnCollision;
            puck3.body.BodyId = 12;
//            puck4.body.Mass = .6f;
            puck4.body.OnCollision += body_OnCollision;
            puck4.body.BodyId = 13;

            //disable pucks 2-4
            puck2.body.CollidesWith = Category.None;
            puck3.body.CollidesWith = Category.None;
            puck4.body.CollidesWith = Category.None;
            playerBody = puck1.body;
            playerBody.CollidesWith = Category.None;
      
            //init the pins
            playerTexture = Content.Load<Texture2D>("pin");
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.280f);
            initPin.Y = ConvertUnits.ToSimUnits(viewport.Height * 0.25f);
            pin1 = new Sprite(playerTexture, initPin, world, "PIN");
            pin1.body.OnCollision += body_OnCollision;
            pin1.body.BodyId = 0;
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.427f);
            pin2 = new Sprite(playerTexture, initPin, world, "PIN");
            pin2.body.BodyId = 1;
            pin2.body.OnCollision += body_OnCollision;
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.573f);
            pin3 = new Sprite(playerTexture, initPin, world, "PIN");
            pin3.body.BodyId = 2;
            pin3.body.OnCollision += body_OnCollision;
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.720f);
            pin4 = new Sprite(playerTexture, initPin, world, "PIN");
            pin4.body.BodyId = 3;
            pin4.body.OnCollision += body_OnCollision;
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.354f);
            initPin.Y = ConvertUnits.ToSimUnits(viewport.Height * 0.326f);
            pin5 = new Sprite(playerTexture, initPin, world, "PIN");
            pin5.body.BodyId = 4;
            pin5.body.OnCollision += body_OnCollision;
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.5f);
            pin6 = new Sprite(playerTexture, initPin, world, "PIN");
            pin6.body.BodyId = 5;
            pin6.body.OnCollision += body_OnCollision;
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.646f);
            pin7 = new Sprite(playerTexture, initPin, world, "PIN");
            pin7.body.BodyId = 6;
            pin7.body.OnCollision += body_OnCollision;
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.427f);
            initPin.Y = ConvertUnits.ToSimUnits(viewport.Height * 0.405f);
            pin8 = new Sprite(playerTexture, initPin, world, "PIN");
            pin8.body.BodyId = 7;
            pin8.body.OnCollision += body_OnCollision;
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.573f);
            pin9 = new Sprite(playerTexture, initPin, world, "PIN");
            pin9.body.BodyId = 8;
            pin9.body.OnCollision += body_OnCollision;
            initPin.X = ConvertUnits.ToSimUnits(viewport.Width * 0.5f);
            initPin.Y = ConvertUnits.ToSimUnits(viewport.Height * 0.481f);
            pin10 = new Sprite(playerTexture, initPin, world, "PIN");
            pin10.body.BodyId = 9;
            pin10.body.OnCollision += body_OnCollision;

            turn = 0;
            hud = new HUD(soundEngine);
            hud.adGameComponent = AdGameComponent.Current;
            hud.CreateAd(viewport);
            hud.Font = Content.Load<SpriteFont>("Arial");
        }

        bool body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureA.Body.UserData == "PUCK" && fixtureB.Body.UserData == "PUCK")
            {
                if (fixtureA.Body.BodyId != lastB || fixtureB.Body.BodyId != lastA)
                {
                    hud.Score += 2400;
                    hud.add(fixtureA.Body.Position, 2400, Color.Gold, soundEnabled);
                    lastA = fixtureA.Body.BodyId;
                    lastB = fixtureB.Body.BodyId;
                }
                else
                {
                    lastA = -1;
                    lastB = -1;
                }
            }
            else if (fixtureA.Body.UserData == "BORDER" || fixtureB.Body.UserData == "BORDER")
            {

            }
            else
            {
                if (fixtureA.Body.BodyId != lastB || fixtureB.Body.BodyId != lastA)
                {
                    if (scoreInc < 240)
                        scoreInc += 10;
                    hud.Score += scoreInc;
                    hud.add(fixtureA.Body.Position, scoreInc, Color.White, soundEnabled);
                    lastA = fixtureA.Body.BodyId;
                    lastB = fixtureB.Body.BodyId;
                }
                else
                {
                    lastA = -1;
                    lastB = -1;
                }

            }
            return true;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// Provides a snapshot of timing values.
        protected override void Update(GameTime gameTime)
        {
            TouchCollection touchCollection = TouchPanel.GetState();
            if (gameStarted)
            {
                if (gameState != GAMEMODE.RUNNING_INMOTION)
                {
                    foreach (TouchLocation tl in touchCollection)
                    {
                        isPressed = tl.State;
                        if (touchCollection.Count>1)
                        {
                            multitouch = true;
                            break;
                        }
                        else if (!multitouch)
                        {
                            if ((isPressed == TouchLocationState.Pressed)
                                    || (isPressed == TouchLocationState.Moved))
                            {
                                if (lastPos.X == -1)
                                {
                                    lastPos.X = tl.Position.X;
                                    lastPos.Y = tl.Position.Y;
                                    //lastPos = ConvertUnits.ToSimUnits(lastPos);
                                }
                                else
                                {
                                    lastPos = curPos;
                                }
                                curPos.X = tl.Position.X;
                                curPos.Y = tl.Position.Y;
                                curPos = ConvertUnits.ToSimUnits(curPos);

                            }
                            if (curPos.Y >= redLine)
                            {
                                gameState = GAMEMODE.RUNNING_SWIPING;
                                playerBody.Position = curPos;
                            }
                            else if (gameState == GAMEMODE.RUNNING_SWIPING && curPos.Y < redLine)
                            {
                                //gameState = GAMEMODE.RUNNING_INMOTION;
                                isPressed = TouchLocationState.Released;
                            }
                            if ((isPressed == TouchLocationState.Released && gameState == GAMEMODE.RUNNING_SWIPING))
                            {
                                if (curPos != lastPos)
                                {
                                    playerBody.CollidesWith = Category.All;
                                    gameState = GAMEMODE.RUNNING_INMOTION;
                                    curPos = (curPos - lastPos) * 10;
                                    if (curPos.X < 0)
                                        curPos.X = Math.Max(curPos.X, -25);
                                    else
                                        curPos.X = Math.Min(curPos.X, 25);
                                    if (curPos.Y < 0)
                                        curPos.Y = Math.Max(curPos.Y, -25);
                                    else
                                        curPos.Y = Math.Min(curPos.Y, 25);
                                    playerBody.LinearVelocity = curPos;
                                    //reset curpos
                                    max = playerBody.LinearVelocity.X;
                                    min = playerBody.LinearVelocity.Y;
                                    curPos = Vector2.Zero;
                                }
                            }
                        }
                        else
                        {
                            if (isPressed == TouchLocationState.Released)
                                multitouch = false;
                        }
                        break; //ignore all other touches
                    }
                }
                else //in motion
                {
                    if (isStopped())
                    {
                        scoreInc = 0;
                        gameState = GAMEMODE.GAMERUNNING;
                        turn++;
                        switch (turn)
                        {
                            case (4):
                                if (checkIfHighScore(hs, hud.Score))
                                    gameState = GAMEMODE.MENU;
                                gameStarted = false;
                                hud.bannerAd.Refresh();
                                break;
                            case (3):
                                puck4.body.CollidesWith = Category.All;
                                playerBody = puck4.body;
                                break;
                            case (2):
                                puck3.body.CollidesWith = Category.All;
                                playerBody = puck3.body;
                                break;
                            case (1):
                                puck2.body.CollidesWith = Category.All;
                                playerBody = puck2.body;
                                break;
                            default:
                                playerBody = puck1.body;
                                break;
                        }
                        playerBody.CollidesWith = Category.None;
                    }
                }
            }
            else
            {
                if (gameState == GAMEMODE.NAMECOMPLETE)
                {
                    using (IsolatedStorageFile savegameStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        IsolatedStorageFileStream fs = null;
                        using (fs = savegameStorage.CreateFile(HighScoresFilename))
                        {
                            if (fs != null)
                            {
                                SaveHighScores(hs, fs, hud.Score);
                            }
                        }
                    }
                    gameState = GAMEMODE.MENU;
                }
                else if (turn == 4 && gameState == GAMEMODE.MENU)
                {
                    turn = 0;
                    lastState = GAMEMODE.MENU;
                    Initialize();
                }
                else if(gameState != GAMEMODE.WAITINGFORNAME)
                {
                    foreach (TouchLocation tl in touchCollection)
                    {
                        if (tl.State == TouchLocationState.Pressed)
                        {
                            gameState = GAMEMODE.GAMESTART;
                        } if (tl.State == TouchLocationState.Released && gameState == GAMEMODE.GAMESTART)
                        {
                            switch (hitbox(tl.Position))
                            {
                                case 0: //start game
                                    if (lastState == GAMEMODE.WAITINGFORNAME || lastState == GAMEMODE.GAMESTART || lastState == GAMEMODE.MENU)
                                    {
                                        gameState = GAMEMODE.GAMERUNNING;
                                        gameStarted = true;
                                    }
                                    else
                                    {
                                        gameState = lastState;
                                        gameStarted = true;
                                    }
                                    break;
                                case 1: //restart or exit
                                    if (lastState == GAMEMODE.GAMERUNNING || lastState == GAMEMODE.RUNNING_INMOTION || lastState == GAMEMODE.RUNNING_SWIPING)
                                    {
                                        turn = 0;
                                        Initialize();
                                        gameState = GAMEMODE.GAMERUNNING;
                                        gameStarted = true;
                                    }
                                    else
                                    {
                                        this.Exit();
                                    }
                                    break;
                                case 2: //sound
                                    if (soundEnabled)
                                        soundEnabled = false;
                                    else
                                        soundEnabled = true;
                                    gameState = GAMEMODE.MENU;
                                    break;
                                case 3: //ad click!!!
                                    gameState = GAMEMODE.MENU;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                lastState = gameState;
                if (lastState == GAMEMODE.MENU)
                    this.Exit();
                else
                {
                    gameState = GAMEMODE.MENU;
                }
                gameStarted = false;
            }
                //this.Exit();
            if(gameStarted)
                world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// Provides a snapshot of timing values.
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(background, Vector2.Zero, Color.White);
            Vector2 rLine = new Vector2(0,ConvertUnits.ToDisplayUnits(redLine));
            spriteBatch.Draw(ErrorLine, rLine, Color.White);
            if (gameStarted && gameState != GAMEMODE.MENU)
            {
                hud.adGameComponent.Visible = false;
                //hud.BannerAd.Visible = false;
                switch (turn)
                {
                    case (3):
                        puck4.body = playerBody;
                        Sprite.Draw(spriteBatch, puck1, puck1.body.Position, 0f);
                        Sprite.Draw(spriteBatch, puck2, puck2.body.Position, 0f);
                        Sprite.Draw(spriteBatch, puck3, puck3.body.Position, 0f);
                        Sprite.Draw(spriteBatch, puck4, puck4.body.Position, 0f);
                        break;
                    case (2):
                        puck3.body = playerBody;
                        Sprite.Draw(spriteBatch, puck1, puck1.body.Position, 0f);
                        Sprite.Draw(spriteBatch, puck2, puck2.body.Position, 0f);
                        Sprite.Draw(spriteBatch, puck3, puck3.body.Position, 0f);
                        spriteBatch.Draw(puckLeft, new Vector2(viewport.Width - puckLeft.Width - 1, 1), Color.White);
                        break;
                    case (1):
                        puck2.body = playerBody;
                        Sprite.Draw(spriteBatch, puck1, puck1.body.Position, 0f);
                        Sprite.Draw(spriteBatch, puck2, puck2.body.Position, 0f);
                        spriteBatch.Draw(puckLeft, new Vector2(viewport.Width - puckLeft.Width - 1, 1), Color.White);
                        spriteBatch.Draw(puckLeft, new Vector2(viewport.Width - puckLeft.Width * 2 - 1, 1), Color.White);
                        break;
                    default:
                        puck1.body = playerBody;
                        Sprite.Draw(spriteBatch, puck1, puck1.body.Position, 0f);
                        spriteBatch.Draw(puckLeft, new Vector2(viewport.Width - puckLeft.Width - 1, 1), Color.White);
                        spriteBatch.Draw(puckLeft, new Vector2(viewport.Width - puckLeft.Width * 2 - 1, 1), Color.White);
                        spriteBatch.Draw(puckLeft, new Vector2(viewport.Width - puckLeft.Width * 3 - 1, 1), Color.White);
                        break;
                }

                Sprite.Draw(spriteBatch, pin1, pin1.body.Position, 0f);
                Sprite.Draw(spriteBatch, pin2, pin2.body.Position, 0f);
                Sprite.Draw(spriteBatch, pin3, pin3.body.Position, 0f);
                Sprite.Draw(spriteBatch, pin4, pin4.body.Position, 0f);
                Sprite.Draw(spriteBatch, pin5, pin5.body.Position, 0f);
                Sprite.Draw(spriteBatch, pin6, pin6.body.Position, 0f);
                Sprite.Draw(spriteBatch, pin7, pin7.body.Position, 0f);
                Sprite.Draw(spriteBatch, pin8, pin8.body.Position, 0f);
                Sprite.Draw(spriteBatch, pin9, pin9.body.Position, 0f);
                Sprite.Draw(spriteBatch, pin10, pin10.body.Position, 0f);
                hud.Draw(spriteBatch);
            }
            else
            {
                //hud.BannerAd.Visible = true;
                spriteBatch.Draw(title, new Vector2(viewport.Width / 2 - title.Width / 2, 10f), Color.White);
                spriteBatch.Draw(hsTexture, new Vector2(viewport.Width / 2 - hsTexture.Width / 2, 110f), Color.White);
                hs.Draw(spriteBatch, viewport, hs, Content.Load<SpriteFont>("HSfont"), Content.Load<SpriteFont>("Titlefont"));
                if(lastState == GAMEMODE.GAMERUNNING || lastState == GAMEMODE.RUNNING_INMOTION || lastState == GAMEMODE.RUNNING_SWIPING)
                    spriteBatch.Draw(cGame, new Vector2(viewport.Width / 2 - cGame.Width / 2, viewport.Height * .64f), Color.White);
                else
                    spriteBatch.Draw(sGame, new Vector2(viewport.Width / 2 - sGame.Width / 2, viewport.Height * .64f), Color.White);
                if (lastState == GAMEMODE.GAMERUNNING || lastState == GAMEMODE.RUNNING_INMOTION || lastState == GAMEMODE.RUNNING_SWIPING)
                    spriteBatch.Draw(rGame, new Vector2(viewport.Width / 2 - rGame.Width / 2, viewport.Height * .73f), Color.White);
                else
                    spriteBatch.Draw(eGame, new Vector2(viewport.Width / 2 - eGame.Width / 2, viewport.Height * .73f), Color.White);
                if (soundEnabled)
                    spriteBatch.Draw(sOff, new Vector2(viewport.Width / 2 - sOff.Width / 2, viewport.Height * .82f), Color.White);
                else
                    spriteBatch.Draw(sOn, new Vector2(viewport.Width / 2 - sOn.Width / 2, viewport.Height * .82f), Color.White);
                hud.adGameComponent.Visible = true;
            }
            spriteBatch.End();
            hud.adGameComponent.Draw(gameTime);
            base.Draw(gameTime);
        }

        private Vertices GetBounds()
        {
            float width = ConvertUnits.ToSimUnits(this.GraphicsDevice.Viewport.Width);
            float height = ConvertUnits.ToSimUnits(this.GraphicsDevice.Viewport.Height);

            Vertices bounds = new Vertices(4);
            bounds.Add(new Vector2(0, 0));
            bounds.Add(new Vector2(width, 0));
            bounds.Add(new Vector2(width, height));
            bounds.Add(new Vector2(0, height));

            return bounds;
        }
        private bool isStopped()
        {
            if (Math.Abs(puck1.body.LinearVelocity.X) > .04 && Math.Abs(puck1.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(puck2.body.LinearVelocity.X) > .04 && Math.Abs(puck2.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(puck3.body.LinearVelocity.X) > .04 && Math.Abs(puck3.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(puck4.body.LinearVelocity.X) > .04 && Math.Abs(puck4.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin1.body.LinearVelocity.X) > .04 && Math.Abs(pin1.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin2.body.LinearVelocity.X) > .04 && Math.Abs(pin2.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin3.body.LinearVelocity.X) > .04 && Math.Abs(pin3.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin4.body.LinearVelocity.X) > .04 && Math.Abs(pin4.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin5.body.LinearVelocity.X) > .04 && Math.Abs(pin5.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin6.body.LinearVelocity.X) > .04 && Math.Abs(pin6.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin7.body.LinearVelocity.X) > .04 && Math.Abs(pin7.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin8.body.LinearVelocity.X) > .04 && Math.Abs(pin8.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin9.body.LinearVelocity.X) > .04 && Math.Abs(pin9.body.LinearVelocity.Y) > .04)
                return false;
            if (Math.Abs(pin10.body.LinearVelocity.X) > .04 && Math.Abs(pin10.body.LinearVelocity.Y) > .04)
                return false;
            puck1.body.LinearVelocity = Vector2.Zero;
            puck2.body.LinearVelocity = Vector2.Zero;
            puck3.body.LinearVelocity = Vector2.Zero;
            puck4.body.LinearVelocity = Vector2.Zero;
            pin1.body.LinearVelocity = Vector2.Zero;
            pin2.body.LinearVelocity = Vector2.Zero;
            pin3.body.LinearVelocity = Vector2.Zero;
            pin4.body.LinearVelocity = Vector2.Zero;
            pin5.body.LinearVelocity = Vector2.Zero;
            pin6.body.LinearVelocity = Vector2.Zero;
            pin7.body.LinearVelocity = Vector2.Zero;
            pin8.body.LinearVelocity = Vector2.Zero;
            pin9.body.LinearVelocity = Vector2.Zero;
            pin10.body.LinearVelocity = Vector2.Zero;

            gameState = GAMEMODE.GAMERUNNING;
            return true;
        }


        public readonly string HighScoresFilename = "shufflebowlhs.lst";


        public static void SaveHighScores(HighScoreData data, IsolatedStorageFileStream fs, int newScore)
        {
            int y = 0;
            bool overWrite = true;
            for (int x = 0; x < data.Count; x++)
            {
                byte[] bits = new byte[3];
                if (newScore > data.Score[x] && overWrite)
                {
                    bits[0] = (Byte)(newName[0]);
                    bits[1] = (Byte)(newName[1]);
                    bits[2] = (Byte)(newName[2]);
                    fs.Write(bits, 0, bits.Length);
                    bits = System.BitConverter.GetBytes(newScore);
                    fs.Write(bits, 0, bits.Length);
                    overWrite = false;
                }
                else
                {
                    bits[0] = (Byte)(data.PlayerName[y][0]);
                    bits[1] = (Byte)(data.PlayerName[y][1]);
                    bits[2] = (Byte)(data.PlayerName[y][2]);
                    fs.Write(bits, 0, bits.Length);
                    bits = System.BitConverter.GetBytes(data.Score[y]);
                    fs.Write(bits, 0, bits.Length);
                    y++;
                }
            }
        }
        public static bool checkIfHighScore(HighScoreData data, int newScore)
        {
            for (int x = 0; x < data.Count; x++)
            {
                if (newScore > data.Score[x])
                {
                    gameState = GAMEMODE.WAITINGFORNAME;
                    Guide.BeginShowKeyboardInput(
                       PlayerIndex.One,
                       "You Have a New High Score!",
                       "Enter your initials:",
                       "",
                       new AsyncCallback(OnEndShowKeyboardInput),
                       null);
                    return false;
                }
            }
            return true;
        }
        static private void OnEndShowKeyboardInput(IAsyncResult result)
        {
            char[] temp = new char[3];
            int initialCount = 0;
            bool clean = false;
            try
            {
                temp = Guide.EndShowKeyboardInput(result).ToCharArray();
                for (initialCount = 0; initialCount < 3; initialCount++)
                {
                    try
                    {
                        newName[initialCount] = temp[initialCount];
                        clean = true;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        if (clean)
                            newName[initialCount] = ' ';
                        else
                            newName[initialCount] = 'A';
                    }
                }
            }
            catch (NullReferenceException)
            {
                newName[0] = 'A';
                newName[1] = 'A';
                newName[2] = 'A';
            }
            gameState = GAMEMODE.NAMECOMPLETE;
        }
        public static HighScoreData LoadHighScores(IsolatedStorageFileStream fs, int count)
        {
            HighScoreData data = new HighScoreData(count);
            char[] tempName = new char[3];
            for (int x = 0; x < count; x++)
            {
                // Reload the saved high-score data.
                byte[] saveBytes = new byte[3];
                fs.Read(saveBytes, 0, saveBytes.Length);
                tempName[0] = (char)saveBytes[0];
                tempName[1] = (char)saveBytes[1];
                tempName[2] = (char)saveBytes[2];
                tempName.CopyTo(data.PlayerName[x],0);
                //data.PlayerName[x] = System.BitConverter.ToString(saveBytes);
                saveBytes = new byte[4];
                fs.Read(saveBytes, 0, saveBytes.Length);
                data.Score[x] = System.BitConverter.ToInt32(saveBytes, 0);
            }
            return (data);
        }
        public int hitbox(Vector2 pos)
        {
            if (pos.Y >= button0y && pos.Y < button0y + 75)
                return 0;
            else if (pos.Y >= button1y && pos.Y < button1y + 75)
                return 1;
            else if (pos.Y >= button2y && pos.Y < button2y + 64)
                return 2;
            else if (pos.Y > 720)
                return 3;
            else
                return 4;
        }
    }
}
