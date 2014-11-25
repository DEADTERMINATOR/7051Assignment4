using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGame
{
    public class Enemy
    {
        public Vector3 startingPosition = new Vector3(0.5f, 0, 0.5f);
        public float moveSpeed = 0.01f;

        public Model model;
        public Vector3 position;
        public float rotation;

        public Maze maze;

        public Enemy(Model model, Maze maze)
        {
            this.model = model;
            this.maze = maze;
            position = startingPosition;
            rotation = 0;
        }

        public void UpdateAI()
        {
            int x = (int)position.X;
            int z = (int)position.Z;
            bool canMove;

            if (canMove = CanItMove())
            {
                MoveForward();
            }

            MazeCell currentCell = maze.MazeCells[x, z];
            bool notClear = IsItClearAhead(currentCell);
            if (notClear)
            {
                MoveTo(new Vector3(x + 0.5f, 0, z + 0.5f), rotation);
                List<int> openWalls = new List<int>();
                int nextPath = -1;
                int backwardPath = -1;
                for (int i = 0; i < currentCell.Walls.Count(); i++)
                {
                    if (!currentCell.Walls[i])
                    {
                        if (rotation == 3)
                        {
                            backwardPath = 2;
                        }
                        else if (rotation == -1.5)
                        {
                            backwardPath = 1;
                        }
                        else if (rotation == 0)
                        {
                            backwardPath = 0;
                        }
                        else if (rotation == 1.5)
                        {
                            backwardPath = 3;
                        }
                        openWalls.Add(i);
                    }
                }

                if (openWalls.Count > 1)
                {
                    if (openWalls[0] != backwardPath)
                    {
                        nextPath = openWalls[0];
                    }
                    else
                    {
                        nextPath = openWalls[1];
                    }
                }
                else
                {
                    nextPath = openWalls[0];
                }

                switch (nextPath)
                {
                    case 0:
                        rotation = 3;
                        break;
                    case 1:
                        rotation = 1.5f;
                        break;
                    case 2:
                        rotation = 0;
                        break;
                    case 3:
                        rotation = -1.5f;
                        break;
                }
            }
        }

        private bool CanItMove()
        {
            Vector3 newLocation = PreviewMove();
            bool moveOK = true;

            if (newLocation.X < 0 || newLocation.X > Maze.mazeWidth)
            {
                moveOK = false;
            }
            if (newLocation.Z < 0 || newLocation.Z > Maze.mazeHeight)
            {
                moveOK = false;
            }

            if(moveOK)
            {
                foreach (BoundingBox box in maze.GetBoundsForCell((int)newLocation.X, (int)newLocation.Z))
                {
                    if (box.Contains(newLocation) == ContainmentType.Contains)
                    {
                        moveOK = false;
                    }
                }
            }

            if(!moveOK)
            {
                MoveTo(new Vector3((int)position.X + 0.5f, 0, (int)position.Z + 0.5f), rotation);
            }

            return moveOK;
        }

        private bool IsItClearAhead(MazeCell cell)
        {
            bool clear = false;

            if(rotation == 3)
            {
                clear = cell.Walls[0];
            }
            else if(rotation == -1.5)
            {
                clear = cell.Walls[3];
            }
            else if(rotation == 0)
            {
                clear = cell.Walls[2];
            }
            else if(rotation == 1.5)
            {
                clear = cell.Walls[1];
            }

            return clear;
        }

        private Vector3 PreviewMove()
        {
            Matrix rotate = Matrix.CreateRotationY(rotation);
            Vector3 forward;
            forward = new Vector3(0, 0, moveSpeed);
            forward = Vector3.Transform(forward, rotate);
            return position + forward;
        }

        private void MoveForward()
        {
            Matrix rotate = Matrix.CreateRotationY(rotation);
            MoveTo(PreviewMove(), rotation);
        }

        private void MoveTo(Vector3 position, float rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}
