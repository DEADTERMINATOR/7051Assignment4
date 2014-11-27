using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const float SafeAreaPortion = 0.03f;

        GameState currentState;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        KeyboardHandler handler;
        bool consoleUp, backgroundSwitch, multiplayer, random, paused;

        Rectangle playingField;
        Paddle playerOnePaddle, playerTwoPaddle;
        Ball ball, invisibleBall;
        float movementChange;
        int p1Score, p2Score, winner;

        KeyboardState state = Keyboard.GetState();
        GamePadState newGamepadState = GamePad.GetState(PlayerIndex.One);
        GamePadState oldGamepadState = GamePad.GetState(PlayerIndex.One);

        SoundEffect ballBouncing;
        Song bgMusic;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Viewport viewport = graphics.GraphicsDevice.Viewport;
            playingField = new Rectangle(
                (int)(viewport.Width * SafeAreaPortion),
                (int)(viewport.Height * SafeAreaPortion),  // * SafeAreaPortion
                (int)(viewport.Width * (1 - 2 * SafeAreaPortion)),  // * (1 - 2 * SafeAreaPortion)
                (int)(viewport.Height * (1 - 2 * SafeAreaPortion)));

            playerOnePaddle = new Paddle(GraphicsDevice, playingField, 1);
            playerTwoPaddle = new Paddle(GraphicsDevice, playingField, 2);
            ball = new Ball(GraphicsDevice, playingField, Color.White);

            movementChange = 5.5f;
            p1Score = 0;
            p2Score = 0;
            winner = 0;

            handler = new KeyboardHandler();
            consoleUp = false;
            backgroundSwitch = false;
            multiplayer = false;
            random = false;
            paused = true;
            currentState = GameState.INGAME;

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

            font = Content.Load<SpriteFont>("MessageFont");
            ballBouncing = Content.Load<SoundEffect>("ballBouncing");
            bgMusic = Content.Load<Song>("thunderstruck");
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
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            state = Keyboard.GetState();
            oldGamepadState = newGamepadState;
            newGamepadState = GamePad.GetState(PlayerIndex.One);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                state.IsKeyDown(Keys.Escape))
                this.Exit();

            switch (currentState)
            {
                case GameState.INGAME:
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        MediaPlayer.Play(bgMusic);
                    }

                    if ((state.IsKeyDown(Keys.P) || newGamepadState.Buttons.Start == ButtonState.Pressed) && !consoleUp)
                    {
                        paused = true;
                    }

                    if ((state.IsKeyDown(Keys.Space) || newGamepadState.Buttons.A == ButtonState.Pressed) && !consoleUp)
                    {
                        paused = false;
                    }

                    if (paused)
                    {
                        MediaPlayer.Pause();
                    }

                    Handle360Commands(oldGamepadState, newGamepadState);

                    if (consoleUp)
                    {
                        handler.Update();
                        HandleConsoleCommands();
                        if (state.IsKeyDown(Keys.OemTilde))
                        {
                            consoleUp = false;
                        }
                    }
                    else if (!paused)
                    {
                        MediaPlayer.Resume();

                        if (winner != 0)
                        {
                            winner = 0;
                        }

                        ball.position += ball.velocity;

                        if (state.IsKeyDown(Keys.W) || newGamepadState.ThumbSticks.Left.Y > 0)
                        {
                            MoveUp(1);
                        }
                        if (state.IsKeyDown(Keys.S) || newGamepadState.ThumbSticks.Left.Y < 0)
                        {
                            MoveDown(1);
                        }

                        if (multiplayer)
                        {
                            MultiplayerUpdate(state);
                        }
                        else
                        {
                            SingleUpdate();
                        }

                        playerOnePaddle.position.Y = MathHelper.Clamp(playerOnePaddle.position.Y, playingField.Top, playingField.Bottom - playerOnePaddle.height);
                        playerTwoPaddle.position.Y = MathHelper.Clamp(playerTwoPaddle.position.Y, playingField.Top, playingField.Bottom - playerTwoPaddle.height);

                        CheckForCollision();
                    }

                    if (state.IsKeyDown(Keys.C))
                    {
                        handler.firstKey = true;
                        consoleUp = true;
                        paused = true;
                    }
                    break;
            }



            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (backgroundSwitch)
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
            }
            else
            {
                GraphicsDevice.Clear(Color.Black);
            }

            spriteBatch.Begin();

            spriteBatch.Draw(playerOnePaddle.texture, playerOnePaddle.position, Color.White);
            spriteBatch.Draw(playerTwoPaddle.texture, playerTwoPaddle.position, Color.White);
            spriteBatch.Draw(ball.texture, ball.position, Color.White);

            Divider divider = new Divider(GraphicsDevice, playingField);
            for (int i = 10; i < GraphicsDevice.Viewport.Height; i += 100)
            {
                spriteBatch.Draw(divider.texture, new Vector2(divider.position.X, i), Color.White);
            }

            spriteBatch.DrawString(font, p1Score.ToString(), new Vector2(playingField.Width / 2 - 40, 0), Color.White);
            spriteBatch.DrawString(font, p2Score.ToString(), new Vector2(playingField.Width / 2 + 90, 0), Color.White);

            if (winner == 1)
            {
                spriteBatch.DrawString(font, "Player One Wins", new Vector2(playingField.Width / 2 - 90, playingField.Height / 2), Color.Red);
            }
            else if (winner == 2)
            {
                spriteBatch.DrawString(font, "Player Two Wins", new Vector2(playingField.Width / 2 - 90, playingField.Height / 2), Color.Red);
            }

            /* For testing purposes. Draws the invisible ball so that the AI pattern can be followed.
            if(invisibleBall != null)
            {
                spriteBatch.Draw(invisibleBall.texture, invisibleBall.position, Color.Green);
            }*/

            // Draws a console window if the user has pressed the 'c' key to open the console
            if (consoleUp)
            {
                int consoleWidth = this.GraphicsDevice.Viewport.Width;
                int consoleHeight = this.GraphicsDevice.Viewport.Height;
                int commandCount = handler.previousCommands.Count;
                Texture2D console = new Texture2D(GraphicsDevice, consoleWidth, consoleHeight);
                Color[] data = new Color[consoleWidth * consoleHeight];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = Color.Black * 0.8f;
                }
                console.SetData(data);
                spriteBatch.Draw(console, new Rectangle(0, 0, consoleWidth, consoleHeight), Color.White);

                /* Draws all previously entered commands to the console. Finds the Y coordinates by multiplying the number
                 * of commands remaining by a set value that creates sufficient spacing between each command and subtracting
                 * that from the Y coordinates of the currently entered command. */
                foreach (Command command in handler.previousCommands)
                {
                    spriteBatch.DrawString(font, "> " + command.ToString(), new Vector2(0, 400 - (commandCount * 40)), Color.White);
                    commandCount--;
                }
                spriteBatch.DrawString(font, "> " + handler.currentCommand, new Vector2(0, 400), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Handles various collision cases. The cases are as follows:
        /// The main ball collides with either of the paddles. In this case, the ball's movement
        /// if reflected in the other along the X axis direction. If it is the player one paddle
        /// and we're not inmultiplyer mode, then it creates an invisible ball.
        /// 
        /// The main ball goes past either of the paddles. In this case, the appropriate player
        /// is awarded a point and the next round or match is set up.
        /// 
        /// The main ball collides with either the top or the bottom of the playing field. In this
        /// case, the ball's movement is reflected in the other direction along the Y axis.
        /// 
        /// The invisible ball collides with AI paddle. In this case, the invisible ball is destroyed.
        /// 
        /// The invisible ball collides with either the top or the bottom of the playing field. This
        /// case is the same as when the regular ball collides with the top or bottom.
        /// </summary>
        private void CheckForCollision()
        {
            Rectangle boundingPaddleOne = new Rectangle((int)playerOnePaddle.position.X, (int)playerOnePaddle.position.Y, playerOnePaddle.width, playerOnePaddle.height);
            Rectangle boundingPaddleTwo = new Rectangle((int)playerTwoPaddle.position.X, (int)playerTwoPaddle.position.Y, playerTwoPaddle.width, playerTwoPaddle.height);
            Rectangle boundingBall = new Rectangle((int)ball.position.X, (int)ball.position.Y, ball.width, ball.height);

            if (invisibleBall != null)
            {
                Rectangle boundingInvisibleBall = new Rectangle((int)invisibleBall.position.X, (int)invisibleBall.position.Y, invisibleBall.width, invisibleBall.height);
                if (boundingInvisibleBall.Intersects(boundingPaddleTwo) || boundingInvisibleBall.Left > playingField.Right)
                {
                    DestroyInvisibleBall();
                }
                else if (boundingInvisibleBall.Top < playingField.Top || boundingInvisibleBall.Bottom > playingField.Bottom)
                {
                    invisibleBall.velocity.Y *= -1;
                    invisibleBall.position += invisibleBall.velocity;
                }
            }

            if (boundingBall.Intersects(boundingPaddleOne))
            {
                ball.velocity.X *= -1;
                ball.position += ball.velocity;
                ballBouncing.Play();
                if (!multiplayer)
                {
                    CreateInvisibleBall();
                }
            }

            else if (boundingBall.Intersects(boundingPaddleTwo))
            {
                ball.velocity.X *= -1;
                ball.position += ball.velocity;
                ballBouncing.Play();
            }

            else if (boundingBall.Top < playingField.Top || boundingBall.Bottom > playingField.Bottom)
            {
                ball.velocity.Y *= -1;
                ball.position += ball.velocity;
                ballBouncing.Play();
            }

            else if (boundingBall.Right < playingField.Left)
            {
                HandleScore(2);
            }
            else if (boundingBall.Left > playingField.Right)
            {
                HandleScore(1);
            }

            /* Backups in case of some sort of collision bug so the game can continue */
            else if (ball.position.X < (-1 * GraphicsDevice.Viewport.Width))
            {
                HandleScore(2);
            }
            else if (ball.position.X > GraphicsDevice.Viewport.Width)
            {
                HandleScore(1);
            }
        }

        /// <summary>
        /// Resets the position of the ball, randomizes the direction of the ball 
        /// and pauses the game before the next round.
        /// </summary>
        private void Reset()
        {
            Random RNG = new Random();
            int negative = RNG.Next(2);

            ball.position.X = playingField.Width / 2;
            ball.position.Y = playingField.Height / 2;
            ball.velocity.X = 7;
            ball.velocity.Y = 6;

            if (negative == 1)
            {
                ball.velocity.X -= 2 * ball.velocity.X;
                ball.velocity.Y -= 2 * ball.velocity.Y;
            }

            if (invisibleBall != null)
            {
                DestroyInvisibleBall();
            }

            paused = true;
        }

        /// <summary>
        /// Resets the position of the ball, randomizes the speed, angle, and direction of the ball
        /// and pauses the game before the next round. Used if the random modifier is on.
        /// </summary>
        private void RandomReset()
        {
            Random RNG = new Random();
            int x = RNG.Next(6, 11);
            int y = RNG.Next(6, 11);
            int negative = RNG.Next(2);

            ball = null;
            ball = new Ball(GraphicsDevice, playingField, Color.White);
            ball.position.X = playingField.Width / 2;
            ball.position.Y = playingField.Height / 2;

            if (negative == 1)
            {
                x -= 2 * x;
                y -= 2 * y;
            }
            ball.velocity.X = x;
            ball.velocity.Y = y;

            if (invisibleBall != null)
            {
                DestroyInvisibleBall();
            }

            paused = true;
        }

        /// <summary>
        /// Resets the scores when the game ends because a player reached a score of 10.
        /// Then calls the appropriate round reset function to set the first round of the
        /// next match up.
        /// </summary>
        private void GameReset()
        {
            p1Score = 0;
            p2Score = 0;
            if (random)
            {
                RandomReset();
            }
            else
            {
                Reset();
            }
        }

        /// <summary>
        /// Performs updates that pertain to multiplayer mode only.
        /// </summary>
        /// <param name="state">The keyboard state</param>
        private void MultiplayerUpdate(KeyboardState state)
        {
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.Two);
            if (state.IsKeyDown(Keys.Up) || gamepadState.ThumbSticks.Left.Y > 0)
            {
                MoveUp(2);
            }
            if (state.IsKeyDown(Keys.Down) || gamepadState.ThumbSticks.Left.Y < 0)
            {
                MoveDown(2);
            }
        }

        /// <summary>
        /// Performs updates that pertain to single player mode only.
        /// </summary>
        private void SingleUpdate()
        {
            if (invisibleBall != null)
            {
                invisibleBall.position += invisibleBall.velocity;
            }
            AI();
        }

        /// <summary>
        /// The AI that takes control of paddle two when single player mode is active.
        /// It works as follows:
        /// When the player hits the ball, a second ball that is invisible is created that moves
        /// slightly faster than the main ball. The AI will track this ball and attempt to hit it. When
        /// it hits it or the invisible ball passes its paddle, the AI will begin to track the actual
        /// ball. This creates the illusion that the AI is trying to track where the actual ball will
        /// end up and also causes the AI to occasionally miss, meaning it can be beaten without the
        /// need to gimp the AI paddle's movement abilities.
        /// </summary>
        private void AI()
        {
            /* The AI will not react to the ball's movement until it is headed in its direction.
             * Makes it seem slightly more human */
            if (ball.velocity.X >= 0)
            {
                if (invisibleBall != null)
                {
                    if (invisibleBall.position.Y < playerTwoPaddle.position.Y)
                    {
                        MoveUp(2);
                    }
                    else if (invisibleBall.position.Y > playerTwoPaddle.position.Y)
                    {
                        MoveDown(2);
                    }
                }
                else
                {
                    if (ball.position.Y < playerTwoPaddle.position.Y)
                    {
                        MoveUp(2);
                    }
                    else if (ball.position.Y > playerTwoPaddle.position.Y)
                    {
                        MoveDown(2);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the invisible ball, setting its starting position to that of the
        /// main ball and its velocity to a slight increase of the main ball's velocity.
        /// </summary>
        /// <returns>A Ball object representing the invisible ball</returns>
        private Ball CreateInvisibleBall()
        {
            invisibleBall = new Ball(GraphicsDevice, playingField, Color.Green);
            invisibleBall.position = ball.position;
            invisibleBall.velocity = ball.velocity * 1.3f;
            return invisibleBall;
        }

        /// <summary>
        /// Sets the invisible ball to null so the AI stops tracking it.
        /// </summary>
        private void DestroyInvisibleBall()
        {
            invisibleBall = null;
        }

        /// <summary>
        /// Handles incrementing the score, checking if there is a winnner and calls
        /// the appropriate reset function.
        /// </summary>
        /// <param name="player">The player who scored</param>
        private void HandleScore(int player)
        {
            if (player == 1)
            {
                p1Score++;
                if (p1Score == 10)
                {
                    winner = 1;
                    GameReset();
                }
            }
            else
            {
                p2Score++;
                if (p2Score == 10)
                {
                    winner = 2;
                    GameReset();
                }
            }

            if (winner == 0)
            {
                if (random)
                {
                    RandomReset();
                }
                else
                {
                    Reset();
                }
            }
        }

        /// <summary>
        /// Iterates through the collection of commands and if any commands that
        /// have not been processed are found, they are processed.
        /// </summary>
        private void HandleConsoleCommands()
        {
            foreach (Command command in handler.previousCommands)
            {
                if (!command.processed)
                {
                    switch (command.ToString().ToLower())
                    {
                        case "switch background":
                            if (backgroundSwitch)
                            {
                                backgroundSwitch = false;
                            }
                            else
                            {
                                backgroundSwitch = true;
                            }
                            command.processed = true;
                            break;
                        case "single":
                            multiplayer = false;
                            GameReset();
                            command.processed = true;
                            break;
                        case "multi":
                            multiplayer = true;
                            GameReset();
                            command.processed = true;
                            break;
                        case "random":
                            random = true;
                            GameReset();
                            command.processed = true;
                            break;
                        case "no random":
                            random = false;
                            GameReset();
                            command.processed = true;
                            break;
                        case "reset":
                            if (random)
                            {
                                RandomReset();
                            }
                            else
                            {
                                Reset();
                            }
                            command.processed = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks for 360 button input that replicate the console commands.
        /// </summary>
        /// <param name="state">The gamepad state.</param>
        private void Handle360Commands(GamePadState oldState, GamePadState newState)
        {
            if (newState.Buttons.LeftShoulder == ButtonState.Pressed && oldState.Buttons.LeftShoulder != ButtonState.Pressed)
            {
                if (!backgroundSwitch)
                {
                    backgroundSwitch = true;
                }
                else
                {
                    backgroundSwitch = false;
                }
            }

            if (newState.Buttons.Y == ButtonState.Pressed && oldState.Buttons.Y != ButtonState.Pressed)
            {
                if (!multiplayer)
                {
                    multiplayer = true;
                }
                else
                {
                    multiplayer = false;
                }
                GameReset();
            }

            if (newState.Buttons.B == ButtonState.Pressed && oldState.Buttons.B != ButtonState.Pressed)
            {
                if (!random)
                {
                    random = true;
                }
                else
                {
                    random = false;
                }
                GameReset();
            }

            if (newState.Buttons.X == ButtonState.Pressed && oldState.Buttons.X != ButtonState.Pressed)
            {
                if (random)
                {
                    RandomReset();
                }
                else
                {
                    Reset();
                }
            }
        }

        /// <summary>
        /// Performs the action of moving the paddle downward.
        /// </summary>
        /// <param name="player">The player number</param>
        private void MoveDown(int player)
        {
            if (player == 1)
            {
                playerOnePaddle.position.Y += movementChange;
            }
            else
            {
                playerTwoPaddle.position.Y += movementChange;
            }
        }

        /// <summary>
        /// Performs the action of moving the paddle upward.
        /// </summary>
        /// <param name="player">The player number</param>
        private void MoveUp(int player)
        {
            if (player == 1)
            {
                playerOnePaddle.position.Y -= movementChange;
            }
            else
            {
                playerTwoPaddle.position.Y -= movementChange;
            }
        }
    }
}
