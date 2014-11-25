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
    class Divider
    {
        public int width, height;
        public Texture2D texture;
        public Color[] color;
        public Vector2 position;

        public Divider(GraphicsDevice graphicsDevice, Rectangle playingField)
        {
            width = 20;
            height = 65;
            texture = new Texture2D(graphicsDevice, width, height);

            color = new Color[width * height];
            for (int i = 0; i < color.Length; i++)
            {
                color[i] = Color.White;
            }
            texture.SetData(color);
            position = new Vector2(playingField.Width / 2 + 25, 0);
        }
    }
}
