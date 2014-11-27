using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pong
{
    class Console
    {
        Vector2 size;
        Vector2 position;
        Vector2 textSize;
        Vector2 textPosition;
        Texture2D rectangle;
        SpriteFont font;
        String currentLine;

        public Console(Vector2 siz, Vector2 pos, GraphicsDevice g, SpriteFont f)
        {
            rectangle = CreateRectangle((Int32)siz.X, (Int32)siz.Y, g);
            size = siz;
            position = pos;
            font = f;
            currentLine = "";
            textPosition = new Vector2(position.X + 10, position.Y + 10);
            textSize = new Vector2(size.X - 20, size.Y - 20);
        }

        //COMP7051 Function to create rectangle
        private Texture2D CreateRectangle(int width, int height, GraphicsDevice g)
        {
            // create the rectangle texture, ,but it will have no color! lets fix that
            Texture2D rectangleTexture = new Texture2D(g, width, height, false, SurfaceFormat.Color);

            // set the color to the amount of pixels
            Color[] color = new Color[width * height];

            // loop through all the colors setting them to whatever values we want
            for (int i = 0; i < color.Length; i++)
            {
                color[i] = new Color(255, 0, 0, 255);
            }

            // set the color data on the texture
            rectangleTexture.SetData(color);

            return rectangleTexture;
        }

        public void Append(string c)
        {
            currentLine = currentLine + c;
        }

        public void Clear()
        {
            currentLine = "";
        }

        public void Backspace()
        {
            if (currentLine.Length != 0)
            {
                currentLine = currentLine.Substring(0, currentLine.Length - 1);
            }
        }

        public string returnCommand()
        {
            string line = currentLine;
            currentLine = "";
            return line;
        }

        public void setFont(SpriteFont font)
        {
            this.font = font;
        }

        public void RenderConsole(SpriteBatch spriteBatch)
        {
            //COMP7051 Draw rectangle primitive
            spriteBatch.Begin();
            spriteBatch.Draw(rectangle, position, Color.Black);

            spriteBatch.DrawString(font, currentLine, textPosition, Color.White);


            spriteBatch.End();
        }
    }
}
