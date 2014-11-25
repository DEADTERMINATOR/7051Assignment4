using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MazeGame
{
    public class ThirdPersonCamera
    {
        public const float nearClip = 1.0f;
        public const float farClip = 100f;
        public const float viewAngle = MathHelper.PiOver4;
        public const float movementSpeed = 1f / 30f;
        public const float rotationSpeed = 1f / 40f;
        public Vector3 zoomLevel1 = new Vector3(0, 5, -5);
        public Vector3 zoomLevel2 = new Vector3(0, 13, -5);
        public Vector3 zoomLevel3 = new Vector3(0, 20, -5);
        public Vector3 startingPosition = new Vector3(0.5f, 0, 0.5f);

        public GraphicsDevice device;

        public float playerYaw;
        public float playerPitch;

        public Vector3 baseReference;
        public Vector3 playerPosition;
        public Vector3[] zoomLevels;
        public int currentZoomLevel;

        public Matrix view;
        public Matrix projection;

        public Maze maze;

        KeyboardState oldKeyboard;
        KeyboardState newKeyboard;

        GamePadState oldGamepad;
        GamePadState newGamepad;

        public ThirdPersonCamera(GraphicsDevice device, Maze maze)
        {
            zoomLevels = new Vector3[3] { zoomLevel1, zoomLevel2, zoomLevel3 };
            baseReference = zoomLevels[0];
            currentZoomLevel = 0;
            playerPosition = startingPosition;
            this.device = device;
            this.maze = maze;

            oldKeyboard = Keyboard.GetState();
            newKeyboard = Keyboard.GetState();

            oldGamepad = GamePad.GetState(PlayerIndex.One);
            newGamepad = GamePad.GetState(PlayerIndex.One);
        }

        public void UpdatePlayerPosition()
        {
            oldKeyboard = newKeyboard;
            newKeyboard = Keyboard.GetState();

            oldGamepad = newGamepad;
            newGamepad = GamePad.GetState(PlayerIndex.One);

            if(newKeyboard.IsKeyDown(Keys.Left) || newGamepad.ThumbSticks.Left.X < 0)
            {
                playerPitch = 0;
                playerYaw += rotationSpeed;
            }

            if(newKeyboard.IsKeyDown(Keys.Right) || newGamepad.ThumbSticks.Left.X > 0)
            {
                playerPitch = 0;
                playerYaw -= rotationSpeed;
            }

            if((!newKeyboard.IsKeyDown(Keys.LeftShift) && newKeyboard.IsKeyDown(Keys.Up)) || newGamepad.ThumbSticks.Left.Y > 0)
            {
                bool collision = false;
                Matrix forwardMovement = Matrix.CreateRotationY(playerYaw);
                Vector3 v = new Vector3(0, 0, movementSpeed);
                v = Vector3.Transform(v, forwardMovement);

                if((playerPosition.X + v.X) < 0 && (playerPosition.X + v.X) > Maze.mazeWidth)
                {
                    collision = true;
                }

                if((playerPosition.Z + v.Z) < 0 && (playerPosition.Z + v.Z) > Maze.mazeHeight)
                {
                    collision = true;
                }

                if(!collision)
                {
                    Vector3 temp = new Vector3(playerPosition.X + v.X, 0, playerPosition.Z + v.Z);
                    foreach (BoundingBox box in maze.GetBoundsForCell((int)temp.X, (int)temp.Z))
                    {
                        if (box.Contains(temp) == ContainmentType.Contains)
                        {
                            collision = true;
                        }
                    }
                }

                if(!collision || !Game1.collisionOn)
                {
                    playerPosition.X += v.X;
                    playerPosition.Z += v.Z;
                }
            }

            if((!newKeyboard.IsKeyDown(Keys.LeftShift) && newKeyboard.IsKeyDown(Keys.Down)) || newGamepad.ThumbSticks.Left.Y < 0)
            {
                bool collision = false;
                Matrix forwardMovement = Matrix.CreateRotationY(playerYaw);
                Vector3 v = new Vector3(0, 0, -movementSpeed);
                v = Vector3.Transform(v, forwardMovement);

                if ((playerPosition.X + v.X) < 0 && (playerPosition.X + v.X) > Maze.mazeWidth)
                {
                    collision = true;
                }

                if ((playerPosition.Z + v.Z) < 0 && (playerPosition.Z + v.Z) > Maze.mazeHeight)
                {
                    collision = true;
                }

                if (!collision)
                {
                    Vector3 temp = new Vector3(playerPosition.X + v.X, 0, playerPosition.Z + v.Z);
                    foreach (BoundingBox box in maze.GetBoundsForCell((int)temp.X, (int)temp.Z))
                    {
                        if (box.Contains(temp) == ContainmentType.Contains)
                        {
                            collision = true;
                        }
                    }
                }

                if (!collision || !Game1.collisionOn)
                {
                    playerPosition.X += v.X;
                    playerPosition.Z += v.Z;
                }
            }

            if((newKeyboard.IsKeyDown(Keys.LeftShift) && newKeyboard.IsKeyDown(Keys.Up)) || newGamepad.ThumbSticks.Right.Y > 0)
            {
                if(playerPitch < 0.65)
                {
                    playerPitch += rotationSpeed;
                }
            }

            if((newKeyboard.IsKeyDown(Keys.LeftShift) && newKeyboard.IsKeyDown(Keys.Down)) || newGamepad.ThumbSticks.Right.Y < 0)
            {
                if(playerPitch > -0.65)
                {
                    playerPitch -= rotationSpeed;
                }
            }

            if ((newKeyboard.IsKeyDown(Keys.LeftShift) && newKeyboard.IsKeyDown(Keys.Z) && !oldKeyboard.IsKeyDown(Keys.Z))
                || (newGamepad.Buttons.A == ButtonState.Pressed && oldGamepad.Buttons.A != ButtonState.Pressed))
            {
                if (currentZoomLevel < 2)
                {
                    currentZoomLevel++;
                    baseReference = zoomLevels[currentZoomLevel];
                }
            }

            if ((!newKeyboard.IsKeyDown(Keys.LeftShift) && newKeyboard.IsKeyDown(Keys.Z) && !oldKeyboard.IsKeyDown(Keys.Z))
                || (newGamepad.Buttons.B == ButtonState.Pressed && oldGamepad.Buttons.B != ButtonState.Pressed))
            {
                if (currentZoomLevel > 0)
                {
                    currentZoomLevel--;
                    baseReference = zoomLevels[currentZoomLevel];
                }
            }

            if(newKeyboard.IsKeyDown(Keys.Home) || newGamepad.Buttons.Start == ButtonState.Pressed)
            {
                currentZoomLevel = 0;
                baseReference = zoomLevels[currentZoomLevel];
                playerPosition = startingPosition;
            }
        }

        public void UpdateCamera()
        {
            Matrix rotationMatrixY = Matrix.CreateRotationY(playerYaw);
            Matrix rotationMatrixX = Matrix.CreateRotationX(playerPitch);
            Vector3 transformedReference = Vector3.Transform(baseReference, rotationMatrixY);
            Vector3 finalReference = Vector3.Transform(transformedReference, rotationMatrixX);
            Vector3 cameraPosition = finalReference + playerPosition;
            view = Matrix.CreateLookAt(cameraPosition, playerPosition, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(viewAngle, device.Viewport.AspectRatio,
                nearClip, farClip);
        }
    }
}
