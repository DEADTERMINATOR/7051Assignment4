using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pong
{
    class Ball
    {
        public static float speed = 1;
        public Vector2 velocity;
        Vector2 position;
        Texture2D ball;
        Vector2 lastPosition;
        public Rectangle bound { get; set; }

        #region Constructors

        public Ball(Vector2 pos, Texture2D tex, Vector2 vel)
        {
            ball = tex;
            position = pos;
            lastPosition = pos;
            
            bound = ball.Bounds;
            velocity = vel * speed;
        }

        #endregion

        public void Move()
        {
            position += velocity;
        }

        public void FlipXVelocity()
        {
            velocity.X *= -1;
        }

        public void FlipYVelocity()
        {
            velocity.Y *= -1;
        }

        public void Update()
        {
            Move();
            bound = new Rectangle((int)position.X, (int)position.Y,
                    ball.Width, ball.Height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(ball, position, Color.White);
            spriteBatch.End();
        }
    }
}
