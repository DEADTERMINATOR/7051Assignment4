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
using Microsoft.Xna.Framework.Net;

namespace Pong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Rectangle screenBounds;
        Texture2D paddleTex;
        Texture2D ballTex;
        //Texture2D bgTex;

        SpriteFont myFont;
        //Paddle player1;
        //Paddle player2;
        Ball ball;
        Background bg;
        Console console;
        Viewport v;

        KeyboardState lastPressed;
        KeyboardState currentState;
        GamePadState currentPad;
        bool consoleKeyPressed;
        bool consoleUp;
        bool gameOver;
        Color selectedColor;

        NetworkSession networkSession;

        PacketWriter packetWriter = new PacketWriter();
        PacketReader packetReader = new PacketReader();
        string errorMessage;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Components.Add(new GamerServicesComponent(this));
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            selectedColor = Color.CornflowerBlue;
            consoleKeyPressed = false;
            consoleUp = false;
            gameOver = false;

            base.Initialize();
            v = GraphicsDevice.Viewport;
            lastPressed = Keyboard.GetState();
            currentState = lastPressed;

            screenBounds = v.Bounds;
            console = new Console(new Vector2(v.Width, v.Height / 2f), new Vector2(0, v.Height / 2f), GraphicsDevice, myFont);
            ball = new Ball(new Vector2((v.Width / 2f) - (ballTex.Width / 2f), (v.Height / 2f) - (ballTex.Height / 2f)), ballTex, new Vector2(-5f, 0));
            bg = new Background(v, myFont);


        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            paddleTex = Content.Load<Texture2D>("paddle");
            ballTex = Content.Load<Texture2D>("ball");
            myFont = Content.Load<SpriteFont>("MessageFont");
            // TODO: use this.Content to load your game content here
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
        /// Menu screen provides options to create or join network sessions.
        /// </summary>
        void UpdateMenuScreen()
        {
            if (IsActive)
            {
                if (Gamer.SignedInGamers.Count == 0)
                {
                    // If there are no profiles signed in, we cannot proceed.
                    // Show the Guide so the user can sign in.
                    Guide.ShowSignIn(1, false);
                }
                else if (IsPressed(Keys.A, Buttons.A))
                {
                    // Create a new session?
                    CreateSession();
                }
                else if (IsPressed(Keys.B, Buttons.B))
                {
                    // Join an existing session?
                    JoinSession();
                }
            }
        }

        /// <summary>
        /// Starts hosting a new network session.
        /// </summary>
        void CreateSession()
        {
            DrawMessage("Creating session...");

            try
            {
                networkSession = NetworkSession.Create(NetworkSessionType.SystemLink,
                                                       1, 2);

                HookSessionEvents();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        /// <summary>
        /// Joins an existing network session.
        /// </summary>
        void JoinSession()
        {
            DrawMessage("Joining session...");

            try
            {
                // Search for sessions.
                using (AvailableNetworkSessionCollection availableSessions =
                            NetworkSession.Find(NetworkSessionType.SystemLink,
                                                1, null))
                {
                    if (availableSessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }

                    // Join the first session we found.
                    networkSession = NetworkSession.Join(availableSessions[0]);

                    HookSessionEvents();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        /// <summary>
        /// After creating or joining a network session, we must subscribe to
        /// some events so we will be notified when the session changes state.
        /// </summary>
        void HookSessionEvents()
        {
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.SessionEnded += SessionEndedEventHandler;
        }

        /// <summary>
        /// This event handler will be called whenever a new gamer joins the session.
        /// We use it to allocate a Tank object, and associate it with the new gamer.
        /// </summary>
        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            int gamerIndex = networkSession.AllGamers.IndexOf(e.Gamer);
            if (gamerIndex == 0)
            {
                e.Gamer.Tag = new Paddle(new Vector2(1, v.Height / 2f - paddleTex.Height / 2f), paddleTex);


            }
            else
            {
                e.Gamer.Tag = new Paddle(new Vector2(v.Width - paddleTex.Width - 1, v.Height / 2f - paddleTex.Height / 2f), paddleTex);
            }
        }


        /// <summary>
        /// Event handler notifies us when the network session has ended.
        /// </summary>
        void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            errorMessage = e.EndReason.ToString();

            networkSession.Dispose();
            networkSession = null;
        }

        /// <summary>
        /// Updates the state of the network session, moving the tanks
        /// around and synchronizing their state over the network.
        /// </summary>
        void UpdateNetworkSession()
        {
            // Update our locally controlled tanks, and send their
            // latest position data to everyone in the session.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                UpdateLocalGamer(gamer);
            }


            ball.Update();


            lastPressed = Keyboard.GetState();

            // Pump the underlying session object.
            networkSession.Update();

            // Make sure the session has not ended.
            if (networkSession == null)
                return;

            // Read any packets telling us the positions of remotely controlled tanks.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                ReadIncomingPackets(gamer);
            }
        }

        /// <summary>
        /// Helper for updating a locally controlled gamer.
        /// </summary>
        void UpdateLocalGamer(LocalNetworkGamer gamer)
        {
            // Look up what tank is associated with this local player.
            Paddle localPaddle = gamer.Tag as Paddle;

            // Update the tank.
            ReadPaddleInputs(localPaddle, gamer.SignedInGamer.PlayerIndex);



            if (localPaddle.bound.Bottom > v.Height)
            {
                localPaddle.MoveUp();
            }

            if (localPaddle.bound.Top < 0)
            {
                localPaddle.MoveDown();
            }


            if (ball.bound.Intersects(localPaddle.bound))
            {
                ball.FlipXVelocity();
                ball.velocity.Y =
                    (ball.bound.Top - (localPaddle.bound.Top + localPaddle.bound.Height / 2f)) * 0.25f;
                
            }


            if (ball.bound.Bottom > v.Height)
            {
                ball.FlipYVelocity();
            }
            if (ball.bound.Top < 0)
            {
                ball.FlipYVelocity();
            }

            if (ball.bound.Right > v.Bounds.Width)
            {
                //Increment player1 score
                bg.IncrementP1();
                //reposition ball
                ball = new Ball(new Vector2((v.Width / 2f) - (ballTex.Width / 2f), (v.Height / 2f) - (ballTex.Height / 2f)), ballTex, new Vector2(-5f, 0));
            }
            if (ball.bound.Left < 0)
            {
                //Increment player2 score
                bg.IncrementP2();
                //reposition ball
                ball = new Ball(new Vector2((v.Width / 2f) - (ballTex.Width / 2f), (v.Height / 2f) - (ballTex.Height / 2f)), ballTex, new Vector2(5f, 0));
            }
            ball.Update();
            if (bg.p1Score >= 5)
            {
                gameOver = true;
                ball.velocity = Vector2.Zero;
            }
            else if (bg.p2Score >= 5)
            {
                gameOver = true;
                ball.velocity = Vector2.Zero;
            }

            // Write the tank state into a network packet.
            packetWriter.Write(localPaddle.position);

            // Send the data to everyone in the session.
            gamer.SendData(packetWriter, SendDataOptions.InOrder);
        }

        /// <summary>
        /// Helper for reading incoming network packets.
        /// </summary>
        void ReadIncomingPackets(LocalNetworkGamer gamer)
        {
            // Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(packetReader, out sender);

                // Discard packets sent by local gamers: we already know their state!
                if (sender.IsLocal)
                    continue;

                // Look up the tank associated with whoever sent this packet.
                Paddle remoteTank = sender.Tag as Paddle;

                // Read the state of this tank from the network packet.
                remoteTank.position = packetReader.ReadVector2();

            }
        }


        /*
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected void UpdateLocalGamer(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
            player1.Update(gameTime);
            player2.Update(gameTime);


            if (consoleUp)
            {
                Keys[] pressed = Keyboard.GetState().GetPressedKeys();
                foreach (Keys i in pressed)
                {
                    if (lastPressed.IsKeyUp(i))
                    {
                        if (i == Keys.Back) console.Backspace();
                        else if (i == Keys.Enter)
                        {
                            RunConsoleCommand(console.returnCommand());
                        }
                        else console.Append(i.ToString());
                    }
                }
                //lastPressed = Keyboard.GetState();
            }
            else if (gameOver)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed ||
                    GamePad.GetState(PlayerIndex.Two).Buttons.Start == ButtonState.Pressed)
                {
                    bg = new Background(v, myFont);
                    ball.velocity.X = -5f;
                    gameOver = false;
                }
            }
            else //console is not up & not gameOver
            {
                ball.Update(gameTime);
                KeyboardState state = Keyboard.GetState();
                GamePadState p1State = GamePad.GetState(PlayerIndex.One);
                GamePadState p2State = GamePad.GetState(PlayerIndex.Two);

                //Keyboard input
                if (state.IsKeyDown(Keys.Down))
                {
                    player2.MoveDown();
                    if (player2.bound.Bottom > v.Height)
                    {
                        player2.MoveUp();
                    }
                }
                if (state.IsKeyDown(Keys.Up))
                {
                    player2.MoveUp();
                    if (player2.bound.Top < 0)
                    {
                        player2.MoveDown();
                    }
                }
                if (state.IsKeyDown(Keys.S))
                {
                    player1.MoveDown();
                    if (player1.bound.Bottom > v.Height)
                    {
                        player1.MoveUp();
                    }
                }
                if (state.IsKeyDown(Keys.W))
                {
                    player1.MoveUp();
                    if (player1.bound.Top < 0)
                    {
                        player1.MoveDown();
                    }
                }

                //Gamepad input
                if (p1State.DPad.Up == ButtonState.Pressed)
                {
                    player1.MoveUp();
                    if (player1.bound.Top < 0)
                    {
                        player1.MoveDown();
                    }
                }
                if (p1State.DPad.Down == ButtonState.Pressed)
                {
                    player1.MoveDown();
                    if (player1.bound.Bottom > v.Height)
                    {
                        player1.MoveUp();
                    }
                }
                if (p2State.DPad.Up == ButtonState.Pressed)
                {
                    player2.MoveUp();
                    if (player2.bound.Top < 0)
                    {
                        player2.MoveDown();
                    }
                }
                if (p2State.DPad.Down == ButtonState.Pressed)
                {
                    player2.MoveDown();
                    if (player2.bound.Bottom > v.Height)
                    {
                        player2.MoveUp();
                    }
                }

                if (ball.bound.Intersects(player1.bound))
                {
                    ball.FlipXVelocity();
                    ball.velocity.Y =
                        (ball.bound.Top - (player1.bound.Top + player1.bound.Height / 2f)) * 0.25f;
                }
                else if (ball.bound.Intersects(player2.bound))
                {
                    ball.FlipXVelocity();
                    ball.velocity.Y =
                        (ball.bound.Top - (player2.bound.Top + player2.bound.Height / 2f)) * 0.25f;
                }

                if (ball.bound.Bottom > v.Height)
                {
                    ball.FlipYVelocity();
                }
                if (ball.bound.Top < 0)
                {
                    ball.FlipYVelocity();
                }

                if (ball.bound.Right > v.Bounds.Width)
                {
                    //Increment player1 score
                    bg.IncrementP1();
                    //reposition ball
                    ball = new Ball(new Vector2((v.Width / 2f) - (ballTex.Width / 2f), (v.Height / 2f) - (ballTex.Height / 2f)), ballTex, new Vector2(-5f, 0));
                }
                if (ball.bound.Left < 0)
                {
                    //Increment player2 score
                    bg.IncrementP2();
                    //reposition ball
                    ball = new Ball(new Vector2((v.Width / 2f) - (ballTex.Width / 2f), (v.Height / 2f) - (ballTex.Height / 2f)), ballTex, new Vector2(5f, 0));
                }
                if (bg.p1Score >= 5)
                {
                    gameOver = true;
                    ball.velocity = Vector2.Zero;
                }
                else if (bg.p2Score >= 5)
                {
                    gameOver = true;
                    ball.velocity = Vector2.Zero;
                }

            }

            if (!consoleKeyPressed)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.C) && !consoleUp)
                {
                    consoleUp = true;
                    console.Clear();
                    consoleKeyPressed = true;
                }

            }
            else if (!Keyboard.GetState().IsKeyDown(Keys.C)) consoleKeyPressed = false;

            lastPressed = Keyboard.GetState();

            //base.Update(gameTime);
        }*/






        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    this.Exit();
            if (IsPressed(Keys.Escape, Buttons.Back))
                this.Exit();

            if (networkSession == null)
            {
                // If we are not in a network session, update the
                // menu screen that will let us create or join one.
                UpdateMenuScreen();
            }
            else
            {
                // If we are in a network session, update it.
                UpdateNetworkSession();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (networkSession == null)
            {
                // If we are not in a network session, draw the
                // menu screen that will let us create or join one.
                DrawMenuScreen();
            }
            else
            {
                // If we are in a network session, draw it.
                DrawNetworkSession();
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the startup screen used to create and join network sessions.
        /// </summary>
        void DrawMenuScreen()
        {
            string message = string.Empty;

            if (!string.IsNullOrEmpty(errorMessage))
                message += "Error:\n" + errorMessage.Replace(". ", ".\n") + "\n\n";

            message += "A = create session\n" +
                       "B = join session";

            spriteBatch.Begin();

            spriteBatch.DrawString(myFont, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(myFont, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();
        }


        /// <summary>
        /// Draws the state of an active network session.
        /// </summary>
        void DrawNetworkSession()
        {
            spriteBatch.Begin();

            // For each person in the session...
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                // Look up the tank object belonging to this network gamer.
                Paddle tank = gamer.Tag as Paddle;

                // Draw the tank.
                tank.Draw(spriteBatch);

                // Draw a gamertag label.
                string label = gamer.Gamertag;
                Color labelColor = Color.Black;
                Vector2 labelOffset = new Vector2(100, 150);

                if (gamer.IsHost)
                    label += " (host)";

                // Flash the gamertag to yellow when the player is talking.
                if (gamer.IsTalking)
                    labelColor = Color.Yellow;

                spriteBatch.DrawString(myFont, label, tank.position, labelColor, 0,
                                       labelOffset, 0.6f, SpriteEffects.None, 0);
            }

            spriteBatch.End();
            ball.Draw(spriteBatch);
            bg.Draw(spriteBatch);

            if (gameOver)
            {
                spriteBatch.Begin();
                if (bg.p1Score >= 5)
                    spriteBatch.DrawString(myFont, "Player 1 wins!", new Vector2(v.Width / 2f - 50f, v.Height / 2f - 50f), Color.White);
                else spriteBatch.DrawString(myFont, "Player 2 wins!", new Vector2(v.Width / 2f - 50f, v.Height / 2f - 50f), Color.White);
                spriteBatch.End();
            }

            // TODO: Add your drawing code here
            if (consoleUp)
            {
                console.RenderConsole(spriteBatch);
            }
        }

        /// <summary>
        /// Checks if the specified button is pressed on either keyboard or gamepad.
        /// </summary>
        bool IsPressed(Keys key, Buttons button)
        {
            return (currentState.IsKeyDown(key) ||
                    currentPad.IsButtonDown(button));
        }


        /// <summary>
        /// Reads input data from keyboard and gamepad, and stores
        /// it into the specified tank object.
        /// </summary>
        void ReadPaddleInputs(Paddle paddle, PlayerIndex playerIndex)
        {
            // Read the gamepad.
            GamePadState gamePad = GamePad.GetState(playerIndex);
            float paddleInput = 0;
            paddleInput = gamePad.ThumbSticks.Left.Y;

            KeyboardState keyboard = Keyboard.GetState(playerIndex);
            if (keyboard.IsKeyDown(Keys.Up))
                paddleInput = 1;
            else if (keyboard.IsKeyDown(Keys.Down))
                paddleInput = -1;

            paddle.paddleInput = paddleInput;

        }

        /// <summary>
        /// Helper draws notification messages before calling blocking network methods.
        /// </summary>
        void DrawMessage(string message)
        {
            if (!BeginDraw())
                return;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.DrawString(myFont, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(myFont, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();

            EndDraw();
        }





        public void RunConsoleCommand(string command)
        {
            switch (command)
            {
                case "QUIT":
                    consoleUp = false;
                    break;
                case "BLUE":
                    selectedColor = Color.CornflowerBlue;
                    break;
                case "RED":
                    selectedColor = Color.Red;
                    break;
                case "SPEEDUP":
                    Ball.speed += 0.5f;
                    ball.velocity *= Ball.speed;
                    Paddle.speed += 2f;
                    break;
                case "SPEEDDOWN":
                    Ball.speed -= 0.5f;
                    ball.velocity /= Ball.speed;
                    Paddle.speed -= 2f;
                    break;
                default:
                    break;
            }
        }
    }
}
