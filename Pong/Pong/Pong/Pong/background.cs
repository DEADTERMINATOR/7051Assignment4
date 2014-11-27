using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Pong
{
    class Background
    {
        public int p1Score { get; set; }
        public int p2Score { get; set; }
        Vector2 p1Pos;
        Vector2 p2Pos;
        SpriteFont myFont;


        public Background(Viewport v, SpriteFont font)
        {
            p1Score = 0;
            p2Score = 0;
            myFont = font;
            p1Pos = new Vector2(20f, 0);
            p2Pos = new Vector2(v.Width - 20f, 0);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(myFont, p1Score + "", p1Pos , Color.White);
            spriteBatch.DrawString(myFont, p2Score + "", p2Pos, Color.White);
            spriteBatch.End();
        }

        public void IncrementP1()
        {
            p1Score++;
        }

        public void IncrementP2()
        {
            p2Score++;
        }
    }
}
