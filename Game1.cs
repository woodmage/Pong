using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pong
{
    public class Game1 : Game
    {
        static readonly Random rand = new(); //set up random number generator
        Texture2D ballTexture; //the ball
        Texture2D broomTexture; //spritesheet for broom
        Texture2D dustTexture; //spritesheet for dust
        Texture2D pongTexture; //spritesheet for pong intro
        Song backgroundMusic, bgmusic2, bgmusic3, bgmusic4, bgmusic5, bgmusic6, bgmusic7; //songs for background music
        SoundEffect hitsound, misssound, compmisssound, gamelose, gamewin; //sound effects
        static Vector2 ballPosition; //ball position
        static Vector2 ballMovement; //ball movement
        static float ballSpeed; //ball speed
        static int compScore = 0, userScore = 0; //computer and user scores
        static GameState gamePhase = GameState.IntroGraphic, lastphase = GameState.IntroGraphic; //game phase
        static Paddle userPaddle, compPaddle, topBorder, bottomBorder; //paddles and borders
        SpriteFont arialfont; //font file
        SpriteFont menufont; //font file
        static string winner = ""; //string for winner's name
        static private GraphicsDeviceManager _graphics; //graphics device manager
        private SpriteBatch _spriteBatch; //sprite batch
        Color extradarkred = new(0.25f, 0f, 0f); //very dark red color
        Color extradarkblue = new(0f, 0f, 0.45f); //very dark blue color
        readonly List<string> menuitems = new() { "Pause", "Restart", "Exit", "Help", "About" }; //list of menu items
        private readonly List<Action> menucommands = new() { DoPause, DoRestart, DoExit, DoHelp, DoAbout }; //list of menu commands
        readonly MenuClass menu = new(); //menu
        KeyboardState kstate, previouskeys = Keyboard.GetState(); //keyboard state and previous keyboard state
        private System.Timers.Timer songchecktimer = new(); //timer to check if a song is still playing
        int songnumber = 0;

        public static void DoPause() => lastphase = GameState.PausedPlay; //pause game menu action

        /// <summary>
        /// do restart is a menu action to restart the game
        /// </summary>
        public static void DoRestart()
        {
            compScore = userScore = 0; //set computer score and user score to 0
            ResetBall(); //reset ball
            ResetPaddles(); //reset paddles
            lastphase = gamePhase = GameState.IntroGraphic; //switch game phase to intro graphics
        }

        /// <summary>
        /// do exit is a menu action to exit the game
        /// </summary>
        public static void DoExit()
        {
            EndGame("Menu"); //set endgame
            lastphase = GameState.GameOver; //we will exit menu mode into game over
        }

        public static void DoHelp() => lastphase = GameState.Help; //we will exit menu mode into help mode

        public static void DoAbout() => lastphase = GameState.About; //we will exit menu mode into about mode

        /// <summary>
        /// default constructor
        /// </summary>
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this) //get Graphics Device Manager
            {
                PreferredBackBufferWidth = 500, //we want 500 width
                PreferredBackBufferHeight = 1000 //we want 1000 height
            };
            _graphics.ApplyChanges(); //make changes to graphics
            Content.RootDirectory = "Content"; //root directory for content
            IsMouseVisible = false; //mouse should not show
        }

        /// <summary>
        /// initialization method, sets ball, paddles, and borders
        /// </summary>
        protected override void Initialize()
        {
            ResetBall(); //reset the ball
            userPaddle = new(225, 940, 50, 10, Color.Blue); //set user paddle
            compPaddle = new(225, 50, 50, 10, Color.Red); //set computer paddle
            topBorder = new(0, 0, 500, 50, extradarkblue); //set top border
            bottomBorder = new(0, 950, 500, 50, extradarkred); //set bottom border
            gamePhase = GameState.IntroGraphic; //set game phase to intro graphic
            for (int i = 0; i < menuitems.Count; i++) //for each menu item
            {
                menu.AddMenuItem(new(menuitems[i], i)); //add the menu item
            }
            base.Initialize(); //do MonoGame initialization
        }

        /// <summary>
        /// reset ball method, puts ball in center of screen, resets ball speed, and randomly sets ball movement
        /// </summary>
        private static void ResetBall()
        {
            ballPosition = new(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2); //set ball position
            ballSpeed = 100f; //set ball speed
            ballMovement.Y = Math.Sign(ballMovement.Y); //set vertical ball movement
            if (ballMovement.Y == 0) //if no vertical ball movement
            {
                ballMovement.Y = 1; //move the ball downward
            }
            ballMovement.X = (float)(rand.NextDouble() * 2 - 1); //randomly set horizontal ball movement
        }

        /// <summary>
        /// reset paddles method, resets the position of the paddles to horizontally centered
        /// </summary>
        private static void ResetPaddles()
        {
            userPaddle.X = compPaddle.X = 225; //set user paddle and computer paddle x position
            userPaddle.Y = 940; //set user paddle y position
            compPaddle.Y = 50; //set computer paddle y position
        }

        /// <summary>
        /// load content method, sets up sprite batch, loads textures and font
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice); //get sprite batch
            ballTexture = Content.Load<Texture2D>("ball"); //load ball texture
            arialfont = Content.Load<SpriteFont>("ArialBlack"); //load arial black font
            broomTexture = Content.Load<Texture2D>("broomby8"); //load broom texture
            dustTexture = Content.Load<Texture2D>("dustby8"); //load dust texture
            pongTexture = Content.Load<Texture2D>("pongby16"); //load pong intro texture
            menufont = Content.Load<SpriteFont>("MenuFont"); //load menu font
            backgroundMusic = Content.Load<Song>("lovers-walk"); //load background music
            bgmusic2 = Content.Load<Song>("bgmusic2");
            bgmusic3 = Content.Load<Song>("bgmusic3");
            bgmusic4 = Content.Load<Song>("bgmusic4");
            bgmusic5 = Content.Load<Song>("bgmusic5");
            bgmusic6 = Content.Load<Song>("bgmusic6");
            bgmusic7 = Content.Load<Song>("bgmusic7");
            hitsound = Content.Load<SoundEffect>("hit2"); //load sound effect for hit
            misssound = Content.Load<SoundEffect>("miss"); //load miss sound effect
            compmisssound = Content.Load<SoundEffect>("compmiss"); //load computer miss sound effect
            gamelose = Content.Load<SoundEffect>("gamelose");
            gamewin = Content.Load<SoundEffect>("gamewin");
            PlaySong(0); //play song # 0
            songchecktimer.Interval = 100; // 1/10th of a second
            songchecktimer.Elapsed += Songchecktimer_Elapsed; //event handler
            songchecktimer.AutoReset = true; //automatically reset timer so it keeps going off
            songchecktimer.Start(); //start timer
        }

        private void Songchecktimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            songchecktimer.Stop();
            if (MediaPlayer.State == MediaState.Stopped)
            {
                songnumber++;
                if (songnumber > 6)
                {
                    songnumber = 0;
                }
                PlaySong(songnumber);
            }
            songchecktimer.Start();
            //throw new NotImplementedException();
        }

        private void PlaySong(int number)
        {
            MediaPlayer.Stop(); //stop media player if it is playing
            MediaPlayer.Play(number switch
            {
                0 => backgroundMusic,
                1 => bgmusic2,
                2 => bgmusic3,
                3 => bgmusic4,
                4 => bgmusic5,
                5 => bgmusic6,
                6 => bgmusic7,
                _ => backgroundMusic
            });
            //MediaPlayer.Play(backgroundMusic); //play background music
            MediaPlayer.IsRepeating = false; //set music to not auto-repeat
        }

        private bool IsKey(Keys key) => kstate.IsKeyUp(key) && previouskeys.IsKeyDown(key);

        /// <summary>
        /// update method, gets user input and deals with paddle and ball movement
        /// </summary>
        /// <param name="gameTime">this is a time variable to keep track of time playing</param>
        protected override void Update(GameTime gameTime)
        {
            kstate = Keyboard.GetState(); //get keyboard state
            var mstate = Mouse.GetState(); //get mouse state
            float inc = ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; //set up increment based on game speed
            if (gamePhase != GameState.IntroGraphic) //if we are not in the intro graphic game phase - that game phase has nothing to update
            {
                if (gamePhase == GameState.Menu) //if game phase is menu
                {
                    int menusel = -1;
                    if (IsKey(Keys.Escape)) //if escape key pressed and released
                    {
                        gamePhase = lastphase; //return from menu
                    }
                    if (IsKey(Keys.Left)) //if left arrow key pressed and released
                    {
                        menu.MoveSelect(DirectionType.Left);
                    }
                    if (IsKey(Keys.Right)) //if right arrow key pressed and released
                    {
                        menu.MoveSelect(DirectionType.Right);
                    }
                    if (IsKey(Keys.Up))
                    {
                        menu.MoveSelect(DirectionType.Up);
                    }
                    if (IsKey(Keys.Down))
                    {
                        menu.MoveSelect(DirectionType.Down);
                    }
                    if (IsKey(Keys.Enter))
                    {
                        menusel = menu.Select();
                    }
                    if (mstate.LeftButton == ButtonState.Pressed)
                    {
                        menusel = menu.Clicked(mstate.X, mstate.Y);
                        if (menusel != -1)
                        {
                            menusel = menu.Select(menusel);
                        }
                    }
                    if (menusel != -1)
                    {
                        menucommands[menusel]();
                        gamePhase = lastphase;
                    }
                }
                else if (gamePhase == GameState.NormalPlay) //or if game phase is normal play
                {
                    if (IsKey(Keys.Left)) //if left key pressed and released
                    {
                        userPaddle.X -= (int)inc; //move user paddle left
                    }
                    if (IsKey(Keys.Right)) //if right key pressed and released
                    {
                        userPaddle.X += (int)inc; //move user paddle right
                    }
                    if (IsKey(Keys.Add)) //if plus key pressed and released
                    {
                        ballSpeed += 100; //add 100 to ball speed
                    }
                    if (IsKey(Keys.Subtract)) //if minus key pressed and released
                    {
                        ballSpeed -= 100; //subtract 100 from ball speed
                    }
                    if (mstate.LeftButton == ButtonState.Pressed || mstate.RightButton == ButtonState.Pressed || mstate.MiddleButton == ButtonState.Pressed) //if mouse button pressed
                    {
                        userPaddle.X = mstate.X; //set user paddle to mouse horizontal position
                    }
                    ballPosition.X += ballMovement.X * ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; //update ball position
                    ballPosition.Y += ballMovement.Y * ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    int compMid = compPaddle.MidX; //get computer paddle middle x position
                    if (compMid > ballPosition.X) //if computer paddle middle x position greater than ball middle x position
                    {
                        compPaddle.X -= (int)inc; //move computer paddle left
                    }
                    if (compMid < ballPosition.X) //if computer paddle middle x position less than ball middle x position
                    {
                        compPaddle.X += (int)inc; //move computer paddle right
                    }
                    if (ballMovement.Y > 0) //if ball is travelling downward
                    {
                        HandleCollision(userPaddle); //handle any collision with user paddle
                    }
                    if (ballMovement.Y < 0) //if ball is travelling upward
                    {
                        HandleCollision(compPaddle); //handle any collision with computer paddle
                    }
                    if (ballPosition.X > _graphics.PreferredBackBufferWidth - ballTexture.Width / 2 || ballPosition.X < ballTexture.Width / 2) //if ball is horizontally out of bounds
                    {
                        ballMovement.X = -ballMovement.X; //reverse ball's horizontal movement
                    }
                    if (ballPosition.Y > _graphics.PreferredBackBufferHeight - ballTexture.Height / 2) //if ball went to bottom of screen
                    {
                        misssound.Play();
                        ResetBall(); //reset the ball
                        compScore++; //increment computer score
                    }
                    else if (ballPosition.Y < ballTexture.Height / 2) //or if ball went to top of screen
                    {
                        compmisssound.Play();
                        ResetBall(); //reset the ball
                        userScore++; //increment player score
                    }
                    if (IsKey(Keys.Escape)) //if escape key pressed and released
                    {
                        EndGame("NOBODY"); //end the game - nobody won
                    }
                    else if (compScore > 20) //or if computer score is greater than 20
                    {
                        gamelose.Play();
                        EndGame("The Computer"); //end the game - computer won
                        if (userScore == 0) //if user score is 0
                        {
                            gamePhase = GameState.GameOverCleanSweep; //set game phase to game over clean sweep
                        }
                    }
                    else if (userScore > 20) //or if user score is greater than 20
                    {
                        gamewin.Play();
                        EndGame("YOU"); //end the game - user won
                        if (compScore == 0) //if computer score is 0
                        {
                            gamePhase = GameState.GameOverCleanSweep; //set game phase to game over clean sweep
                        }
                    }
                    else if (IsKey(Keys.P) || IsKey(Keys.Space)) //or if P key or spacebar pressed and released
                    {
                        gamePhase = GameState.PausedPlay; //switch game phase to paused play
                    }
                    else if (IsKey(Keys.F2)) //or if F2 pressed and released
                    {
                        lastphase = gamePhase; //back up gamePhase
                        gamePhase = GameState.Menu;
                    }
                }
                else if (gamePhase == GameState.GameOver || gamePhase == GameState.GameOverCleanSweep) //or if game phase is game over (waiting for Yes / No at end of game)
                {
                    if (IsKey(Keys.Y) || IsKey(Keys.Enter)) //if Y key or Enter pressed and released
                    {
                        DoRestart();
                    }
                    if (IsKey(Keys.N) || IsKey(Keys.Escape)) //if N key or Escape pressed and released
                    {
                        Exit(); //exit the program
                    }
                }
                else if (gamePhase == GameState.PausedPlay)  //or if game phase is paused play
                {
                    if (IsKey(Keys.Space) || IsKey(Keys.P)) //if space bar or P key pressed and released
                    {
                        gamePhase = GameState.NormalPlay; //set game phase to normal play
                    }
                }
            }
            previouskeys = kstate;
            base.Update(gameTime); //do MonoGame's update
        }

        /// <summary>
        /// end game method, sets winner and sets game phase to game over
        /// </summary>
        /// <param name="win">the name of the winner</param>
        private static void EndGame(string win)
        {
            winner = win; //set game winner
            gamePhase = GameState.GameOver; //set game phase to game over
        }

        /// <summary>
        /// collision method, checks for a collision between the ball and a paddle
        /// </summary>
        /// <param name="paddle">the paddle to check</param>
        /// <returns>true if there is a collision, false otherwise</returns>
        private bool Collision(Paddle paddle)
        {
            Paddle ball = new(ballPosition, ballTexture.Width, ballTexture.Height, Color.White); //make a paddle of ball
            if (ball.Right < paddle.Left || ball.Left > paddle.Right || ball.Bottom < paddle.Top || ball.Top > paddle.Bottom) //if ball is outside of paddle
            {
                return false; //return false (no collision)
            }
            return true; //return true (collision)
        }

        /// <summary>
        /// handle collision method, checks for a collision with a paddle and adjusts ball movement and ball speed if there is
        /// </summary>
        /// <param name="paddle">the paddle to check</param>
        private void HandleCollision(Paddle paddle)
        {
            if (Collision(paddle)) //if ball collided with paddle
            {
                hitsound.Play();
                ballMovement.Y = -ballMovement.Y; //reverse ball's vertical movement
                ballMovement.X += (float)(ballPosition.X - paddle.MidX) * 2 / ballTexture.Width; //add "English" to ball's horizontal movement
                ballSpeed += 50f; //speed up the ball
            }
        }

        int count = 0; //counter for intro animation

        /// <summary>
        /// draw method, handles all the output
        /// </summary>
        /// <param name="gameTime">a time variable</param>
        protected override void Draw(GameTime gameTime)
        {
            string text; //variable for text display
            GraphicsDevice.Clear(Color.Black); //clear the screen to black
            _spriteBatch.Begin(); //begin sprite batch processing
            if (gamePhase == GameState.IntroGraphic) //if game phase is intro graphic
            {
                _spriteBatch.DoText("Press ^F2^ for menu", arialfont, _graphics, Color.Aqua, Color.White, posY: _graphics.PreferredBackBufferHeight - 50); //display menu notice
                Rectangle dst; //rectangle for destination
                dst = _spriteBatch.DrawAnimDstRect(pongTexture, 16, new(0, 0)); //get destination to use for centering
                dst.X = (_graphics.PreferredBackBufferWidth - dst.Width) / 2; //compute center position
                dst.Y = (_graphics.PreferredBackBufferHeight - dst.Height) / 2;
                _spriteBatch.DrawAnimated(pongTexture, 16, new(dst.X, dst.Y), 10, _graphics.PreferredBackBufferWidth); //draw animated pong intro
                if (_spriteBatch.GetPos() == 15) //if we are in the final position of the pong animation
                {
                    count++; //increment count
                    if (count > 9) //if count is 10 or more
                    {
                        gamePhase = GameState.PausedPlay; //set game phase to paused play
                        count = 0; //set count to 0 so we can reuse it
                    }
                }
            }
            else if (gamePhase == GameState.NormalPlay || gamePhase == GameState.PausedPlay) //or if game phase is regular play or paused play
            {
                text = $"Computer: ^{compScore}^.  Player: ^{userScore}^."; //set text to display
                topBorder.Paint(_spriteBatch); //paint top border
                bottomBorder.Paint(_spriteBatch); //paint bottom border
                compPaddle.Paint(_spriteBatch); //paint computer paddle
                userPaddle.Paint(_spriteBatch); //paint user paddle
                _spriteBatch.DoText(text, arialfont, _graphics, Color.Aqua, Color.White); //display text
                _spriteBatch.Draw(ballTexture, ballPosition, null, Color.White, 0f, 
                    new Vector2(ballTexture.Width / 2, ballTexture.Height / 2), Vector2.One, SpriteEffects.None, 0f); //display ball
                if (gamePhase == GameState.PausedPlay) //if game phase is paused play
                {
                    text = "Paused - ^space^ to continue"; //set text to display
                    _spriteBatch.DoText(text, arialfont, _graphics, Color.Aqua, Color.White, centerY: true); //display text in center of screen
                }
            }
            else if (gamePhase == GameState.GameOver || gamePhase == GameState.GameOverCleanSweep) //or if game phase is game over or game over clean sweep
            {
                text = $"^{winner}^ won!  Play again?"; //set text for prompt
                _spriteBatch.DoText(text, arialfont, _graphics, Color.MistyRose, Color.White, centerY: true); //display prompt in center of screen
                if (gamePhase == GameState.GameOverCleanSweep) //if game phase is game over clean sweep
                {
                    text = "Clean Sweep!"; //set text for prompt
                    Vector2 pos = _spriteBatch.DoText(text, arialfont, _graphics, Color.White, Color.White); //display message and get position
                    pos.Y = 900; //put broom at bottom of screen
                    Vector2 pos2 = _spriteBatch.DrawAnimated(broomTexture, 8, pos, 5, _graphics.PreferredBackBufferWidth, true); //draw animated broom
                    Rectangle src, dst; //rectangle storage
                    (src, dst) = _spriteBatch.DrawAnimRects(dustTexture, 8, pos2); //get rectangles
                    _spriteBatch.Draw(dustTexture, dst, src, Color.White); //draw sprite from dustTexture
                }
            }
            else if (gamePhase == GameState.Menu) //or if game phase is menu
            {
                menu.Paint(_spriteBatch, menufont, _graphics, 30); //paint the menu
            }
            _spriteBatch.End(); //end sprite batch processing
            base.Draw(gameTime); //do MonoGame's drawing
        }
    }

    /// <summary>
    /// game state enum is used to keep track of what we are doing
    /// </summary>
    public enum GameState
    {
        IntroGraphic,
        PausedPlay,
        NormalPlay,
        GameOver,
        GameOverCleanSweep,
        Menu,
        Help,
        About,
        //we can add more as needed
    }

    public enum MenuItemType
    {
        Normal,
        SubMenu,
        Separator,
        //we can add more as needed
    }

    /// <summary>
    /// sprite batch extensions class has some static methods that extend the capabilities of the sprite batch class
    /// </summary>
    public static class SpriteBatchExtensions
    {
        public static int Pos { get => _pos; set => _pos = value; } //public accessor for position used in DrawAnimated
        private static int _pos = 0; //position used for DrawAnimated
        private static int _count = 0; //counter used for DrawAnimated
        private static int _xpos = 0; //horizontal position used for DrawAnimated
        private static bool _moveleft = false; //flag for direction of horizontal movement

        public static int GetPos(this SpriteBatch sbatch)
        {
            _ = sbatch.GetType();
            return _pos;
        }


        /// <summary>
        /// draw animation rectangles method, sets up source and destination rectangles
        /// </summary>
        /// <param name="sbatch">sprite batch instance - required</param>
        /// <param name="spritesheet">texture2d for sheet of sprites</param>
        /// <param name="numbersprites">number of sprites on sheet</param>
        /// <param name="position">position it will be painted</param>
        /// <returns>a tuple of source and destination rectangles</returns>
        public static (Rectangle, Rectangle) DrawAnimRects(this SpriteBatch sbatch, Texture2D spritesheet, int numbersprites, Vector2 position)
        {
            int spritewide = spritesheet.Width / numbersprites; //get sprite width
            Rectangle src = new(Pos * spritewide, 0, spritewide, spritesheet.Height); //set up source rectangle
            Rectangle dst = new((int)position.X, (int)position.Y, spritewide, spritesheet.Height); //set up destination rectangle
            _ = sbatch.GetType(); //keep visual studio from fussing
            return (src, dst); //return the source and destination rectangles
        }

        /// <summary>
        /// draw animation source rectangle method, returns source rectangle
        /// </summary>
        /// <param name="sbatch">sprite batch instance - required</param>
        /// <param name="spritesheet">texture2d for sheet of sprites</param>
        /// <param name="numbersprites">number of sprites on sheet</param>
        /// <param name="position">position it will be painted</param>
        /// <returns>the source rectangle</returns>
        public static Rectangle DrawAnimSrcRect(this SpriteBatch sbatch, Texture2D spritesheet, int numbersprites, Vector2 position)
        {
            Rectangle src; //make rectangle
            (src, _) = DrawAnimRects(sbatch, spritesheet, numbersprites, position); //get rectangles
            return src; //return source rectangle
        }

        /// <summary>
        /// draw animation destination rectangle method, returns destination rectangle
        /// </summary>
        /// <param name="sbatch">sprite batch instance - required</param>
        /// <param name="spritesheet">texture2d for sheet of sprites</param>
        /// <param name="numbersprites">number of sprites on sheet</param>
        /// <param name="position">position it will be painted</param>
        /// <returns></returns>
        public static Rectangle DrawAnimDstRect(this SpriteBatch sbatch, Texture2D spritesheet, int numbersprites, Vector2 position)
        {
            Rectangle dst; //make rectangles
            (_, dst) = DrawAnimRects(sbatch, spritesheet, numbersprites, position); //get rectangles
            return dst; //return destination rectangle
        }

        /// <summary>
        /// draw animated method, flips through sprites on a sprite sheet, optionally moving them back and forth horizontally
        /// </summary>
        /// <param name="sbatch">sprite batch instance</param>
        /// <param name="spritesheet">texture2d for sheet of sprites</param>
        /// <param name="numbersprites">number of sprites on sheet</param>
        /// <param name="position">position to paint</param>
        /// <param name="countpersprite">how much to count before switching sprites</param>
        /// <param name="horizontalsize">total horizontal width to work with</param>
        /// <param name="horizontalmove">(optional) whether to move horizontally</param>
        /// <returns>position before output position or right after</returns>
        public static Vector2 DrawAnimated(this SpriteBatch sbatch, Texture2D spritesheet, int numbersprites, Vector2 position, int countpersprite, int horizontalsize, bool horizontalmove = false)
        {
            if (!horizontalmove) //if we are not moving horizontally
            {
                _xpos = (int)position.X; //set horizontal position to position supplied
            }
            int spritewide = spritesheet.Width / numbersprites; //get sprite width
            Rectangle src = new(_pos * spritewide, 0, spritewide, spritesheet.Height); //make source rectangle
            Rectangle dst = new(_xpos, (int)position.Y, spritewide, spritesheet.Height); //make destination rectangle
            sbatch.Draw(spritesheet, dst, src, Color.White); //draw sprite from sprite sheet
            if (horizontalmove) //if we are moving horizontally
            {
                if (_moveleft) //if we are moving left
                {
                    _xpos--; //decrement horizontal position
                    if (_xpos < 0) //if we are out of bounds
                    {
                        _xpos = 0; //reset horizontal position
                        _moveleft = false; //clear move left flag
                    }
                }
                else //otherwise (we are moving right)
                {
                    _xpos++; //increment horizontal position
                    int maximumright = horizontalsize - spritewide - 1; //get maximum size
                    if (_xpos > maximumright) //if we are out of bounds
                    {
                        _xpos = maximumright; //reset horizontal position to maximum
                        _moveleft = true; //set clear left flag
                    }
                }
            }
            _count++; //increment counter
            if (_count >= countpersprite) //if counter is greater than or equal to count per sprite
            {
                _pos++; //increment position
                if (_pos >= numbersprites) //if position greater than or equal to numbersprites
                {
                    _pos = 0; //reset position
                }
                _count = 0; //reset counter
            }
            if (_moveleft) //if we are moving left
            {
                return new(_xpos - spritewide, position.Y); //return position
            }
            else //otherwise
            {
                return new(_xpos + spritewide, position.Y); //return position
            }
        }

        /// <summary>
        /// draw box method, draws a box without disturbing the "insides"
        /// </summary>
        /// <param name="sbatch">sprite batch instance</param>
        /// <param name="rectangle">the rectangle defining the box</param>
        /// <param name="color">the color to make it</param>
        /// <param name="bordersize">the size of the borders</param>
        public static void DrawBox(this SpriteBatch sbatch, Rectangle rectangle, Color color, int bordersize)
        {
            Texture2D pixel = new(sbatch.GraphicsDevice, 1, 1); //make a 2d texture for the color
            pixel.SetData(new[] { color }); //set it to the proper color
            sbatch.Draw(pixel, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, bordersize), color); // Draw top border
            sbatch.Draw(pixel, new Rectangle(rectangle.Left, rectangle.Bottom - bordersize, rectangle.Width, bordersize), color); // Draw bottom border
            sbatch.Draw(pixel, new Rectangle(rectangle.Left, rectangle.Top, bordersize, rectangle.Height), color); // Draw left border
            sbatch.Draw(pixel, new Rectangle(rectangle.Right - bordersize, rectangle.Top, bordersize, rectangle.Height), color); // Draw right border
        }
        
        public static void DrawBox(this SpriteBatch sbatch, int xpos, int ypos, int width, int height, Color color, int bordersize) =>
            DrawBox(sbatch, new Rectangle(xpos, ypos, width, height), color, bordersize); //just a different way to call it

        /// <summary>
        /// draw rectangle method, draws a filled rectangle
        /// </summary>
        /// <param name="spriteBatch">sprite batch instance</param>
        /// <param name="rectangle">the rectangle for source</param>
        /// <param name="color">tint coloring to use</param>
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            Texture2D pixel = new(spriteBatch.GraphicsDevice, 1, 1); //make a 2d texture for our color
            pixel.SetData(new[] { color }); //set it to the proper color
            spriteBatch.Draw(pixel, rectangle, color); //draw it stretched out to fill rectangle
        }
        
        public static void DrawRectangle(this SpriteBatch spriteBatch, int xpos, int ypos, int width, int height, Color color) => 
            DrawRectangle(spriteBatch, new Rectangle(xpos, ypos, width, height), color); //just a different way to call it

        /// <summary>
        /// draw text method, draws the text and returns the position to the right
        /// </summary>
        /// <param name="spriteBatch">sprite batch instance</param>
        /// <param name="font">the font to use</param>
        /// <param name="text">the text to output</param>
        /// <param name="position">the position to draw at</param>
        /// <param name="color">the color to use</param>
        /// <returns>updated position</returns>
        public static Vector2 DrawText(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color)
        {
            Vector2 size = font.MeasureString(text); //get size of text
            spriteBatch.DrawString(font, text, position, color); //draw string
            position.X += size.X; //add horizontal size to position
            return position; //return position
        }

        /// <summary>
        /// draw text method, this version uses an array of text strings and a corresponding array of colors
        /// </summary>
        /// <param name="sbatch">the sprite batch instance</param>
        /// <param name="sfont">the font to use</param>
        /// <param name="text">the array of text strings</param>
        /// <param name="position">the starting position to use</param>
        /// <param name="colors">array of colors to use - 1 per each text string</param>
        /// <param name="drawit">(optional) a boolean for whether to draw the text or simply return the updated position</param>
        /// <returns>the updatred position, including height</returns>
        public static Vector2 DrawText(this SpriteBatch sbatch, SpriteFont sfont, string[] text, Vector2 position, Color[] colors, bool drawit = true)
        {
            Vector2 totalsize = new(0, 0); //make a total size variable
            Vector2 size = new(0, 0); //make a size variable
            if (text.Length != colors.Length) //if text length is not the same as colors length
            {
                return new(0, 0); //return with 0, 0 position
            }
            for (int index = 0; index < text.Length; index++) //for each index position
            {
                size = sfont.MeasureString(text[index]); //get size of text portion
                if (drawit) //if we want to draw it
                {
                    sbatch.DrawString(sfont, text[index], position + totalsize, colors[index]); //draw that text portion in the proper color
                }
                totalsize.X += size.X; //add horizontal size to total size
            }
            totalsize.Y = size.Y; //set last vertical size to total size vertical (so we can use it for new line if needed)
            return totalsize; //return total size
        }

        /// <summary>
        /// convert string method, breaks apart a string with ^ character defining where to use highlight color and where to use normal color
        /// </summary>
        /// <param name="sbatch">sprite batch instance</param>
        /// <param name="text">the text to parse</param>
        /// <param name="highlightcolor">the highlight color</param>
        /// <param name="normalcolor">the normal color</param>
        /// <returns>a tuple of a list of strings and a list of colors</returns>
        public static (List<string>, List<Color>) ConvertString(this SpriteBatch sbatch, string text, Color highlightcolor, Color normalcolor)
        {
            List<string> outstr = new(); //set up a list of strings
            List<Color> outcolor = new(); //set up a list of colors
            bool highlightflag = false; //use highlight color (no at first)
            string buffer = ""; //set buffer to empty string
            for (int index = 0; index < text.Length; index++) //for each character of text
            {
                if (text[index] == '^') //if character is '^'
                {
                    if (index > 0) //if we are not at the beginning of text
                    {
                        outstr.Add(buffer); //add buffer to string list
                        outcolor.Add(highlightflag ? highlightcolor : normalcolor); //add the proper color to the colors list
                        buffer = ""; //reset buffer to an empty string
                    }
                    highlightflag = !highlightflag; //toggle highlight flag
                }
                else //otherwise (any other character)
                {
                    buffer += text[index]; //just add it to the buffer
                }
            }
            outstr.Add(buffer); //add the buffer to string list
            outcolor.Add(highlightflag ? highlightcolor : normalcolor); //add the proper color to the colors list
            sbatch.GetType(); //this actually does nothing but is here to keep visual studio from complaining about the "unused" variable
            return (outstr, outcolor); //return a tuple of the string list and the colors list
        }

        /// <summary>
        /// do text method, converts a string into an array of strings and an array of colors using normal and highlight colors, 
        /// optionally centering and optionally setting the position
        /// </summary>
        /// <param name="sbatch">sprite batch instance</param>
        /// <param name="text">text to use</param>
        /// <param name="sfont">font to use</param>
        /// <param name="graphics">graphics device manager (used for bounds for centering)</param>
        /// <param name="normal">normal color to use</param>
        /// <param name="highlight">highlight color to use</param>
        /// <param name="centerX">(optional) boolean for centering horizontally, defaults to true</param>
        /// <param name="centerY">(optional) boolean for centering vertically, defaults to false</param>
        /// <param name="posX">(optional) the horizontal starting position, defaults to 0</param>
        /// <param name="posY">(optional) the vertical starting position, defaults to 0</param>
        /// <returns>the updated position, including vertically</returns>
        public static Vector2 DoText(this SpriteBatch sbatch, string text, SpriteFont sfont, GraphicsDeviceManager graphics, Color normal, Color highlight, bool centerX = true, bool centerY = false, int posX = 0, int posY = 0)
        {
            List<string> outtext; //set up a string list
            List<Color> outcolor; //set up a colors list
            Vector2 position = new(posX, posY); //set position
            (outtext, outcolor) = sbatch.ConvertString(text, highlight, normal); //get string list and colors list from text
            Vector2 totalwide = sbatch.DrawText(sfont, outtext.ToArray(), new(0, 0), outcolor.ToArray(), false); //get total size from DrawText method
            if (centerX) //if we are to center horizontally
            {
                position.X = (graphics.PreferredBackBufferWidth - totalwide.X) / 2; //do it
            }
            if (centerY) //if we are to center vertically
            {
                position.Y = (graphics.PreferredBackBufferHeight -  totalwide.Y) / 2; //do it
            }
            return (sbatch.DrawText(sfont, outtext.ToArray(), position, outcolor.ToArray())); //draw the text and return updated position to user
        }
    }

    /// <summary>
    /// paddle class handles keeping track of paddle related variables and has a simple method for painting the paddle
    /// </summary>
    public class Paddle
    {
        public int X { get => _x; set => _x = value; } //public accessors for private variables
        public int Y { get => _y; set => _y = value; }
        public int Wide { get => _wide; set => _wide = value; }
        public int High { get => _high; set => _high = value; }
        public Color Color { get => _color; set => _color = value; }
        private int _x, _y, _wide, _high; //private variables
        private Color _color;
        
        public int HalfWide => Wide / 2; //half wide property
        
        public int HalfHigh => High / 2; //half high property
        
        public int MidX => X + Wide / 2; //middle x property
        
        public int MidY => Y + High / 2; //middle y property
        
        public int Left => X; //left property
        
        public int Right => X + Wide - 1; //right property
        
        public int Top => Y; //top property
        
        public int Bottom => Y + High - 1; //bottom property
        
        public Rectangle Rect => new(X, Y, Wide, High); //rectangle property
        
        public Paddle(int x, int y, int wide, int high, Color color) //constructor
        {
            _x = x;
            _y = y;
            _wide = wide;
            _high = high;
            _color = color;
        }
        
        public Paddle(Vector2 pos, int wide, int high, Color color) //constructor using a Vector2 for position
        {
            _x = (int)(pos.X - wide / 2);
            _y = (int)(pos.Y - high / 2);
            _wide = wide;
            _high = high;
            _color = color;
        }
        
        public void Paint(SpriteBatch sbatch) => sbatch.DrawRectangle(Rect, Color); //paint method
    }

    /// <summary>
    /// menu item class holds the information for each menu item used by the menu class
    /// </summary>
    public class MenuItemClass
    {
        public string Text { get => _text; set => _text = value; } //public accessors
        public MenuItemType Type { get => _type; set => _type = value; }
        public MenuClass Menu { get => _menu; set => _menu = value; }
        public int Return { get => _return; set => _return = value; }
        private string _text = ""; //private variable for item text
        private MenuItemType _type = MenuItemType.Normal; //private variable for item type
        private MenuClass _menu; //private variable to hold submenu
        private int _return = -1; //private variable for return value
        private int _x, _y, _w, _h; //private variables to hold position
        private Color _back, _fore; //private variables to hold colors

        public int GetX => _x; //access x position
        
        public int GetW => _w; //access width
        
        public MenuItemClass(string text, int returnvalue, MenuItemType type = MenuItemType.Normal, MenuClass submenu = null)
        {
            _text = text; //set text
            _return = returnvalue; //set return value
            _type = type; //set menu item type
            _menu = submenu; //set submenu
            if (type == MenuItemType.SubMenu && _menu == null) //if this is a submenu and the menu is not defined
            {
                _menu = new(this); //make a new submenu
            }
        }
        
        public void SetPosition(int x, int y, int w, int h) => (_x, _y, _w, _h) = (x, y, w, h); //set position and size
        
        public void SetColors(Color back, Color fore) => (_back, _fore) = (back, fore); //set colors
        
        public void Paint(SpriteBatch sbatch, SpriteFont sfont)
        {
            sbatch.DrawRectangle(_x, _y, _w, _h, _back); //draw the rectangle
            sbatch.DrawBox(_x, _y, _w, _h, _fore, 1); //draw a box around the rectangle
            Vector2 size = sfont.MeasureString(_text); //get size of text in current font
            sbatch.DrawText(sfont, _text, new(((_w - size.X) / 2 + _x), ((_h - size.Y) / 2) + _y), _fore); //draw the text
        }
        
        public bool Clicked(int x, int y)
        {
            if (_x <= x && x <= _x + _w - 1 && _y <= y && y <= _y + _h - 1) //if in bounds of position and size
            {
                return true; //return true (yes, this menu item was clicked)
            }
            return false; //otherwise return false (no, this menu item was not clicked)
        }
    }

    /// <summary>
    /// menu class holds the menu items from the menu item class
    /// </summary>
    public class MenuClass
    {
        public List<MenuItemClass> Items { get => _items; set => _items = value; } //public accessor for menu items
        private List<MenuItemClass> _items = new(); //private variable for menu items
        private GraphicsDeviceManager _graphics = null; //we will need this for HorizontalSize
        private Color _back = Color.Navy, _fore = Color.White, _highback = Color.Aqua, _highfore = Color.Black; //colors used by menu
        private readonly bool _issubmenu = false; //flag for submenu
        private readonly MenuItemClass _parent = null; //parent menu item for submenu
        private int _itemindex = 0;
        
        public MenuClass() { } //default constructor does nothing
        
        public MenuClass(MenuItemClass item) //constuctor used by MenuItemClass for submenus
        {
            _issubmenu = true; //set flag for submenu
            _parent = item; //record parent
        }
        
        public int HorizontalSize => _graphics.PreferredBackBufferWidth / Items.Count; //get horizontal size
        
        public void AddMenuItem(MenuItemClass item) => Items.Add(item); //add a menu item
        
        public void Paint(SpriteBatch sbatch, SpriteFont sfont, GraphicsDeviceManager graphics, int vertsize)
        {
            _graphics ??= graphics; //if _graphics is null, set it
            int xpos = 0, ypos = vertsize; //beginning positions
            foreach (var item in Items) //for each menu item
            {
                if (_issubmenu) //if we are in a submenu
                {
                    item.SetPosition(_parent.GetX, ypos, _parent.GetW, vertsize); //set position (going vertical)
                    ypos += vertsize; //adjust y position
                }
                else //otherwise (main menu)
                {
                    item.SetPosition(xpos, 0, HorizontalSize, vertsize); //set position (going horizontal)
                    xpos += HorizontalSize; //adjust x position
                }
                NotHighLit(item, sbatch, sfont); //paint the menu item without highlight
            }
            HighLight(_itemindex, sbatch, sfont); //highlight the current item
        }

        public void NotHighLit(MenuItemClass item, SpriteBatch sbatch, SpriteFont sfont)
        {
            item.SetColors(_back, _fore); //set colors for not highlighted
            item.Paint(sbatch, sfont); //paint the menu item
        }

        public void HighLight(int item, SpriteBatch sbatch, SpriteFont sfont)
        {
            Items[item].SetColors(_highback, _highfore); //set colors to highlight colors
            Items[item].Paint(sbatch, sfont); //paint the menu item
        }

        public int Clicked(int x, int y)
        {
            for (int index = 0; index < Items.Count; index++) //for each menu item
            {
                if (Items[index].Clicked(x, y)) //if that item was clicked
                {
                    return index; //return the index of the item
                }
            }
            return -1; //not found, return -1
        }

        public int MoveSelect(DirectionType dir)
        {
            if (dir == DirectionType.Left || dir == DirectionType.Up)
            {
                _itemindex--;
            }
            if (dir == DirectionType.Right || dir == DirectionType.Down)
            {
                _itemindex++;
            }
            if (_itemindex < 0)
            {
                _itemindex = Items.Count - 1;
            }
            if (_itemindex > Items.Count - 1)
            {
                _itemindex = 0;
            }
            return _itemindex;
        }
        public int Select(int item) => Items[item].Return; //returns the return value of the specified item
        
        public int Select() => Select(_itemindex); //returns the return value of the selected item
    }

    public enum DirectionType
    {
        Left,
        Right,
        Up,
        Down,
        //we can add to this if needed
    }
}
