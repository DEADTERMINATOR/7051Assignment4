using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pong
{
    class Paddle
    {
        public static float speed = 5f;
        public Vector2 position;
        Texture2D paddle;
        Vector2 lastPosition;

        public float paddleInput;

        public Rectangle bound { get; set; }
        #region Constructors

        public Paddle(Vector2 pos, Texture2D pad)
        {
            paddle = pad;
            position = pos;
            lastPosition = position;
            bound = pad.Bounds;
        }

        #endregion

        public void MoveUp()
        {
            position.Y -= speed;
        }

        public void MoveDown()
        {
            position.Y += speed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Begin();
            spriteBatch.Draw(paddle, position, Color.White);
            //spriteBatch.End();
        }

        public void Update()
        {
            if (paddleInput > 0)
                MoveUp();
            else if (paddleInput < 0)
                MoveDown();
            


            bound = new Rectangle((int)position.X, (int)position.Y,
                    paddle.Width, paddle.Height);
        }
    }
}
