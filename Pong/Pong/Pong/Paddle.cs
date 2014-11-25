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
    class Paddle
    {
        public int width, height, player;
        public Texture2D texture;
        public Color[] color;
        public Vector2 position;

        public Paddle(GraphicsDevice graphicsDevice, Rectangle playingField, int playerNumber)
        {
            width = 10;
            height = 100;
            player = playerNumber;
            texture = new Texture2D(graphicsDevice, width, height);

            color = new Color[width * height];
            for (int i = 0; i < color.Length; i++)
            {
                color[i] = Color.White;
            }
            texture.SetData(color);

            if (player == 1)
            {
                position = new Vector2(playingField.Left, playingField.Height / 2);
            }
            else
            {
                position = new Vector2(playingField.Right, playingField.Height / 2);
            }
        }
    }
}
