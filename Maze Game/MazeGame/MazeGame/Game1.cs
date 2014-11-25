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
using SkinnedModel;

namespace MazeGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static bool collisionOn = true;

        public SoundEffect walking;
        public SoundEffect wallHit;
        public SoundEffectInstance walkingInstance;
        public SoundEffectInstance wallHitInstance;
        public Song dayMusic;
        public Song nightMusic;
        public bool musicPlaying;
        bool day;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Camera camera;
        Maze maze;
        Enemy enemy;

        BasicEffect floorEffect;
        BasicEffect brickWallEffect;
        BasicEffect glassWallEffect;
        BasicEffect metalWallEffect;
        BasicEffect pebbleWallEffect;

        Texture2D floorTexture;
        Texture2D brickWallTexture;
        Texture2D glassWallTexture;
        Texture2D metalWallTexture;
        Texture2D pebbleWallTexture;

        float moveScale = 1.5f;
        float rotateScale = MathHelper.PiOver2;

        bool moveOK, oldMoveOK;
        KeyboardState oldKeyState, newKeyState;
        GamePadState oldPadState, newPadState;
        SpriteFont font;
        RenderTarget2D renderTarget;
        Effect texture;
        Texture2D lightTex;
        Texture2D darkTex;
        bool isLight;
        Model currentModel;
        AnimationPlayer animationPlayer;
        float baseVolume;
        
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
            LoadContent();
            maze = new Maze(GraphicsDevice, new Texture2D[5] { floorTexture, brickWallTexture, 
                glassWallTexture, pebbleWallTexture, metalWallTexture });
            camera = new Camera(GraphicsDevice.Viewport.AspectRatio);
            floorEffect = new BasicEffect(GraphicsDevice);
            brickWallEffect = new BasicEffect(GraphicsDevice);
            glassWallEffect = new BasicEffect(GraphicsDevice);
            metalWallEffect = new BasicEffect(GraphicsDevice);
            pebbleWallEffect = new BasicEffect(GraphicsDevice);
            oldKeyState = Keyboard.GetState();
            newKeyState = Keyboard.GetState();
            oldPadState = GamePad.GetState(PlayerIndex.One);
            newPadState = GamePad.GetState(PlayerIndex.One);
            renderTarget = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            isLight = true;
            musicPlaying = false;
            day = true;
            moveOK = true;
            oldMoveOK = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("SpriteFont1");
            floorTexture = Content.Load<Texture2D>("stampedconcrete");
            brickWallTexture = Content.Load<Texture2D>("brickwall");
            glassWallTexture = Content.Load<Texture2D>("hammeredglasswall");
            metalWallTexture = Content.Load<Texture2D>("metalwall");
            pebbleWallTexture = Content.Load<Texture2D>("pebblewall");
            //floorEffect = Content.Load<Effect>("Shader");
            //brickWallEffect = Content.Load<Effect>("Shader");
            //glassWallEffect = Content.Load<Effect>("Shader");
            //metalWallEffect = Content.Load<Effect>("Shader");
            //pebbleWallEffect = Content.Load<Effect>("Shader");
            texture = Content.Load<Effect>("Texture");
            lightTex = Content.Load<Texture2D>("Light");
            darkTex = Content.Load<Texture2D>("LightDark");
            walking = Content.Load<SoundEffect>("0514");
            wallHit = Content.Load<SoundEffect>("wallHit");
            dayMusic = Content.Load<Song>("dayMusic");
            nightMusic = Content.Load<Song>("nightMusic");

            currentModel = Content.Load<Model>("dude");
            SkinningData skinningData = currentModel.Tag as SkinningData;
            if(skinningData == null)
            {
                throw new InvalidOperationException("This model does not contain a SkinningData tag");
            }
            animationPlayer = new AnimationPlayer(skinningData);
            AnimationClip clip = skinningData.AnimationClips["Take 001"];
            animationPlayer.StartClip(clip);
            enemy = new Enemy(currentModel, maze);
            baseVolume = MediaPlayer.Volume;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


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
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float moveAmount = 0;

            oldKeyState = newKeyState;
            oldPadState = newPadState;
            newKeyState = Keyboard.GetState();
            newPadState = GamePad.GetState(PlayerIndex.One);

            CheckMusic();

            if(walkingInstance != null)
            {
                walkingInstance.Pause();
            }

            animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || newKeyState.IsKeyDown(Keys.Escape))
                this.Exit();

            if(newKeyState.IsKeyDown(Keys.Up) || newPadState.ThumbSticks.Left.Y > 0)
            {
                moveAmount = moveScale * elapsed;
            }

            if(newKeyState.IsKeyDown(Keys.Down) || newPadState.ThumbSticks.Left.Y < 0)
            {
                moveAmount = -moveScale * elapsed;
            }

            if(newKeyState.IsKeyDown(Keys.Right) || newPadState.ThumbSticks.Right.X > 0)
            {
                camera.LeftRightRotation = MathHelper.WrapAngle(camera.LeftRightRotation - (rotateScale * elapsed));
            }

            if(newKeyState.IsKeyDown(Keys.Left) || newPadState.ThumbSticks.Right.X < 0)
            {
                camera.LeftRightRotation = MathHelper.WrapAngle(camera.LeftRightRotation + (rotateScale * elapsed));
            }

            if(newKeyState.IsKeyDown(Keys.X) || newPadState.ThumbSticks.Right.Y > 0)
            {
                if(camera.UpDownRotation > -1.5)
                {
                    camera.UpDownRotation = MathHelper.WrapAngle(camera.UpDownRotation - (rotateScale * elapsed));
                }
            }

            if(newKeyState.IsKeyDown(Keys.C) || newPadState.ThumbSticks.Right.Y < 0)
            {
                if(camera.UpDownRotation < 1.5)
                {
                    camera.UpDownRotation = MathHelper.WrapAngle(camera.UpDownRotation + (rotateScale * elapsed));
                }
            }

            if((newKeyState.IsKeyDown(Keys.W) && !oldKeyState.IsKeyDown(Keys.W))
                || (newPadState.Buttons.Y == ButtonState.Pressed && oldPadState.Buttons.Y != ButtonState.Pressed))
            {
                collisionOn = !collisionOn;
            }

            if((newKeyState.IsKeyDown(Keys.LeftShift) && newKeyState.IsKeyDown(Keys.Z) && !oldKeyState.IsKeyDown(Keys.Z))
                || (newPadState.Buttons.A == ButtonState.Pressed && oldPadState.Buttons.A != ButtonState.Pressed))
            {
                if(camera.currentFOVLevel > 0)
                {
                    camera.currentFOVLevel--;
                    camera.UpdateProjection();
                }
            }

            if((!newKeyState.IsKeyDown(Keys.LeftShift) && newKeyState.IsKeyDown(Keys.Z) && !oldKeyState.IsKeyDown(Keys.Z))
                || (newPadState.Buttons.B == ButtonState.Pressed && oldPadState.Buttons.B != ButtonState.Pressed))
            {
                if(camera.currentFOVLevel < 2)
                {
                    camera.currentFOVLevel++;
                    camera.UpdateProjection();
                }
            }

            if(newKeyState.IsKeyDown(Keys.Home) || newPadState.Buttons.Start == ButtonState.Pressed)
            {
                camera.currentFOVLevel = 0;
                camera.Position = camera.startingPosition;
                camera.LeftRightRotation = 0;
                camera.UpDownRotation = 0;
                camera.UpdateProjection();
            }

            if(moveAmount != 0)
            {
                Vector3 newLocation = camera.PreviewMove(moveAmount);
                oldMoveOK = moveOK;
                moveOK = true;

                if(newLocation.X < 0 || newLocation.X > Maze.mazeWidth)
                {
                    moveOK = false;
                }
                if(newLocation.Z < 0 || newLocation.Z > Maze.mazeHeight)
                {
                    moveOK = false;
                }

                if(collisionOn)
                {
                    foreach (BoundingBox box in maze.GetBoundsForCell((int)newLocation.X, (int)newLocation.Z))
                    {
                        if (box.Contains(newLocation) == ContainmentType.Contains)
                        {
                            moveOK = false;
                        }
                    }
                }

                if(moveOK)
                {
                    camera.MoveForward(moveAmount);
                    if(walkingInstance == null)
                    {
                        walkingInstance = walking.CreateInstance();
                        walkingInstance.Play();
                    }

                    else
                    {
                        walkingInstance.Resume();
                    }
                }

                else
                {
                    if(moveOK != oldMoveOK)
                    {
                        if (wallHitInstance == null)
                        {
                            wallHitInstance = wallHit.CreateInstance();
                            wallHitInstance.Play();
                        }

                        else
                        {
                            wallHitInstance.Resume();
                        }
                    }
                }
            }

            if ((newKeyState.IsKeyDown(Keys.P) && !oldKeyState.IsKeyDown(Keys.P))
                || (newPadState.Buttons.X == ButtonState.Pressed && oldPadState.Buttons.X != ButtonState.Pressed))
            {
                isLight = !isLight;
            }

            if ((newKeyState.IsKeyDown(Keys.I) && !oldKeyState.IsKeyDown(Keys.I))
                || (newPadState.Buttons.RightShoulder == ButtonState.Pressed && oldPadState.Buttons.RightShoulder != ButtonState.Pressed))
            {
                maze.fogToggle = !maze.fogToggle;
            }

            enemy.UpdateAI();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            

            GraphicsDevice.SetRenderTarget(renderTarget);
            maze.Draw(camera, new BasicEffect[5] { floorEffect, brickWallEffect, glassWallEffect, metalWallEffect, pebbleWallEffect });
            Matrix[] bones = animationPlayer.GetSkinTransforms();

            foreach (ModelMesh mesh in currentModel.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.World = bones[mesh.ParentBone.Index] * Matrix.CreateScale(0.008f) * Matrix.CreateRotationY(enemy.rotation) * Matrix.CreateTranslation(enemy.position);

                    effect.View = camera.View;
                    effect.Projection = camera.Projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }
                mesh.Draw();
            }
            GraphicsDevice.SetRenderTarget(null);

            texture.Parameters["ScreenTexture"].SetValue(renderTarget);
            if (isLight)
                texture.Parameters["LightTexture"].SetValue(lightTex);
            else texture.Parameters["LightTexture"].SetValue(darkTex);
            //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
            //    SamplerState.LinearClamp, DepthStencilState.Default,
            //    RasterizerState.CullNone);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
                SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullCounterClockwise, texture);
            
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            

            //if(collisionOn)
            //{
            //    spriteBatch.DrawString(font, "Collision Detection: Yes", new Vector2(0, 0), Color.Black);
            //}
            //else
            //{
            //    spriteBatch.DrawString(font, "Collision Detection: No", new Vector2(0, 0), Color.Black);
            //}
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void CheckMusic()
        {
            bool oldDay = day;
            day = isLight;

            if(day != oldDay)
            {
                MediaPlayer.Stop();
            }

            if((newKeyState.IsKeyDown(Keys.M) && !oldKeyState.IsKeyDown(Keys.M))
                || (newPadState.DPad.Up == ButtonState.Pressed && oldPadState.DPad.Up == ButtonState.Released))
            {
                musicPlaying = !musicPlaying;
            }

           if(musicPlaying)
           {
                if(day)
                {
                    if (MediaPlayer.State == MediaState.Stopped)
                    {
                        MediaPlayer.Play(dayMusic);
                    }

                    else if (MediaPlayer.State == MediaState.Paused)
                    {
                       MediaPlayer.Resume();
                    }
                }

                else
                {
                    if(MediaPlayer.State == MediaState.Stopped)
                    {
                        MediaPlayer.Play(nightMusic);
                    }

                    else if(MediaPlayer.State == MediaState.Paused)
                    {
                        MediaPlayer.Resume();
                    }
                }

                ModulateMusicVolume();
            }

            else if(!musicPlaying && day == oldDay && MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Pause();
            }
        }

        private void ModulateMusicVolume()
        {
            float distance = CalculateManhattanDistance();
            Console.Out.WriteLine(distance);

            if(distance < 1.5f)
            {
                if(maze.fogToggle)
                {
                    MediaPlayer.Volume = baseVolume * 0.5f;
                }
                else
                {
                    MediaPlayer.Volume = baseVolume;
                }
                
            }
            else if (distance < 3f && distance > 1.5f)
            {
                if (maze.fogToggle)
                {
                    MediaPlayer.Volume = baseVolume * 0.9f * 0.5f;
                }
                else
                {
                    MediaPlayer.Volume = baseVolume * 0.9f;
                }
            }
            else if (distance < 4.5f && distance > 3f)
            {
                if (maze.fogToggle)
                {
                    MediaPlayer.Volume = baseVolume * 0.8f * 0.5f;
                }
                else
                {
                    MediaPlayer.Volume = baseVolume * 0.8f;
                }
            }
            else if (distance < 6f && distance > 4.5f)
            {
                if (maze.fogToggle)
                {
                    MediaPlayer.Volume = baseVolume * 0.7f * 0.5f;
                }
                else
                {
                    MediaPlayer.Volume = baseVolume * 0.7f;
                }
            }
            else if (distance < 7.5f && distance > 6f)
            {
                if (maze.fogToggle)
                {
                    MediaPlayer.Volume = baseVolume * 0.6f * 0.5f;
                }
                else
                {
                    MediaPlayer.Volume = baseVolume * 0.6f;
                }
            }
            else if (distance > 7.5f)
            {
                if (maze.fogToggle)
                {
                    MediaPlayer.Volume = baseVolume * 0.5f * 0.5f;
                }
                else
                {
                    MediaPlayer.Volume = baseVolume * 0.5f;
                }
            }
        }

        private float CalculateManhattanDistance()
        {
            return Math.Abs(camera.Position.X - enemy.position.X) + Math.Abs(camera.Position.Z - enemy.position.Z);
        }
    }
}
