using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pong
{
    class Ball
    {
        public int width, height;
        public Texture2D texture;
        public Color[] color;
        public Vector2 position, velocity;

        public Ball(GraphicsDevice graphicsDevice, Rectangle playingField, Color ballColor)
        {
            width = 10;
            height = 10;
            texture = new Texture2D(graphicsDevice, width, height);

            color = new Color[width * height];
            for (int i = 0; i < color.Length; i++)
            {
                color[i] = ballColor;
            }
            texture.SetData(color);

            position = new Vector2(playingField.Width / 2, playingField.Height / 2);
            velocity = new Vector2(7, 6);
        }
    }
}
