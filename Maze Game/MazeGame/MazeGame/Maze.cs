using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MazeGame
{
    public class Maze
    {
        public const int mazeWidth = 20;
        public const int mazeHeight = 20;

        GraphicsDevice device;
        VertexBuffer floorBuffer;
        VertexBuffer[] wallBuffers;
        Vector3[] wallPoints = new Vector3[8];
        Color[] floorColors = new Color[2] { Color.White, Color.Gray };
        Color[] wallColors = new Color[4] { Color.Red, Color.Orange, Color.Red, Color.Orange };

        Texture2D[] textures;

        private Random RNG = new Random();
        public MazeCell[,] MazeCells = new MazeCell[mazeWidth, mazeHeight];

        public float lightIntensity = 0.8f;
        public Vector3 ambientColor = Color.White.ToVector3();
        public bool fogToggle;

        public Maze(GraphicsDevice device, Texture2D[] textures)
        {
            this.device = device;
            this.textures = textures;
            BuildFloorBuffer();

            for(int x = 0; x < mazeWidth; x++)
            {
                for(int z = 0; z < mazeHeight; z++)
                {
                    MazeCells[x, z] = new MazeCell();
                }
            }
            GenerateMaze();

            wallPoints[0] = new Vector3(0, 1, 0);
            wallPoints[1] = new Vector3(0, 1, 1);
            wallPoints[2] = new Vector3(0, 0, 0);
            wallPoints[3] = new Vector3(0, 0, 1);
            wallPoints[4] = new Vector3(1, 1, 0);
            wallPoints[5] = new Vector3(1, 1, 1);
            wallPoints[6] = new Vector3(1, 0, 0);
            wallPoints[7] = new Vector3(1, 0, 1);
            wallBuffers = new VertexBuffer[4];
            BuildWallBuffer();
            fogToggle = false;
        }

        private void BuildFloorBuffer()
        {
            List<VertexPositionNormalTexture> vertexList = new List<VertexPositionNormalTexture>();
            int counter = 0;

            for(int x = 0; x < mazeWidth; x++)
            {
                counter++;
                for(int z = 0; z < mazeHeight; z++)
                {
                    counter++;
                    foreach(VertexPositionNormalTexture vertex in FloorTile(x, z))
                    {
                        vertexList.Add(vertex);
                    }
                }
            }

            floorBuffer = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration,
                vertexList.Count, BufferUsage.WriteOnly);

            floorBuffer.SetData<VertexPositionNormalTexture>(vertexList.ToArray());
        }

        private void BuildWallBuffer()
        {
            List<VertexPositionNormalTexture>[] wallVertexList = new List<VertexPositionNormalTexture>[4];
            List<VertexPositionNormalTexture> wall1 = new List<VertexPositionNormalTexture>();
            List<VertexPositionNormalTexture> wall2 = new List<VertexPositionNormalTexture>();
            List<VertexPositionNormalTexture> wall3 = new List<VertexPositionNormalTexture>();
            List<VertexPositionNormalTexture> wall4 = new List<VertexPositionNormalTexture>();

            for(int x = 0; x < mazeWidth; x++)
            {
                for(int z = 0; z < mazeHeight; z++)
                {
                    foreach(KeyValuePair<int, VertexPositionNormalTexture> vertex in BuildMazeWall(x, z))
                    {
                        if(vertex.Key >= 0 && vertex.Key <= 5)
                        {
                            wall1.Add(vertex.Value);
                        }
                        if(vertex.Key >= 6 && vertex.Key <= 11)
                        {
                            wall2.Add(vertex.Value);
                        }
                        if(vertex.Key >= 12 && vertex.Key <= 17)
                        {
                            wall3.Add(vertex.Value);
                        }
                        if(vertex.Key >= 18 && vertex.Key <= 23)
                        {
                            wall4.Add(vertex.Value);
                        }
                    }
                }
            }

            wallVertexList[0] = wall1;
            wallVertexList[1] = wall2;
            wallVertexList[2] = wall3;
            wallVertexList[3] = wall4;

            int i = 0;
            foreach(List<VertexPositionNormalTexture> wall in wallVertexList)
            {
                VertexBuffer wallBuffer = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration,
                    wall.Count, BufferUsage.WriteOnly);
                wallBuffer.SetData<VertexPositionNormalTexture>(wall.ToArray());
                wallBuffers[i] = wallBuffer;
                i++;
            }
        }

        private List<VertexPositionNormalTexture> FloorTile(int xOffset, int zOffset)
        {
            List<VertexPositionNormalTexture> vList = new List<VertexPositionNormalTexture>();

            vList.Add(new VertexPositionNormalTexture(new Vector3(0 + xOffset, 0, 0 + zOffset), Vector3.UnitZ, new Vector2(0, 0)));
            vList.Add(new VertexPositionNormalTexture(new Vector3(1 + xOffset, 0, 0 + zOffset), Vector3.UnitZ, new Vector2(0, 1)));
            vList.Add(new VertexPositionNormalTexture(new Vector3(0 + xOffset, 0, 1 + zOffset), Vector3.UnitZ, new Vector2(1, 0)));
            vList.Add(new VertexPositionNormalTexture(new Vector3(1 + xOffset, 0, 0 + zOffset), Vector3.UnitZ, new Vector2(0, 1)));
            vList.Add(new VertexPositionNormalTexture(new Vector3(1 + xOffset, 0, 1 + zOffset), Vector3.UnitZ, new Vector2(1, 1)));
            vList.Add(new VertexPositionNormalTexture(new Vector3(0 + xOffset, 0, 1 + zOffset), Vector3.UnitZ, new Vector2(1, 0)));

            return vList;
        }

        private Dictionary<int, VertexPositionNormalTexture> BuildMazeWall(int x, int z)
        {
            Dictionary<int, VertexPositionNormalTexture> triangles = new Dictionary<int, VertexPositionNormalTexture>();

            if(MazeCells[x, z].Walls[0])
            {
                triangles.Add(0, CalcPoint(0, x, z, Vector3.UnitZ, new Vector2(0, 0)));
                triangles.Add(1, CalcPoint(4, x, z, Vector3.UnitZ, new Vector2(0, 1)));
                triangles.Add(2, CalcPoint(2, x, z, Vector3.UnitZ, new Vector2(1, 0)));
                triangles.Add(3, CalcPoint(4, x, z, Vector3.UnitZ, new Vector2(0, 1)));
                triangles.Add(4, CalcPoint(6, x, z, Vector3.UnitZ, new Vector2(1, 1)));
                triangles.Add(5, CalcPoint(2, x, z, Vector3.UnitZ, new Vector2(1, 0)));
            }

            if(MazeCells[x, z].Walls[1])
            {
                triangles.Add(6, CalcPoint(4, x, z, -Vector3.UnitX, new Vector2(0, 0)));
                triangles.Add(7, CalcPoint(5, x, z, -Vector3.UnitX, new Vector2(0, 1)));
                triangles.Add(8, CalcPoint(6, x, z, -Vector3.UnitX, new Vector2(1, 0)));
                triangles.Add(9, CalcPoint(5, x, z, -Vector3.UnitX, new Vector2(0, 1)));
                triangles.Add(10, CalcPoint(7, x, z, -Vector3.UnitX, new Vector2(1, 1)));
                triangles.Add(11, CalcPoint(6, x, z, -Vector3.UnitX, new Vector2(1, 0)));
            }

            if(MazeCells[x, z].Walls[2])
            {
                triangles.Add(12, CalcPoint(5, x, z, -Vector3.UnitZ, new Vector2(0, 0)));
                triangles.Add(13, CalcPoint(1, x, z, -Vector3.UnitZ, new Vector2(0, 1)));
                triangles.Add(14, CalcPoint(7, x, z, -Vector3.UnitZ, new Vector2(1, 0)));
                triangles.Add(15, CalcPoint(1, x, z, -Vector3.UnitZ, new Vector2(0, 1)));
                triangles.Add(16, CalcPoint(3, x, z, -Vector3.UnitZ, new Vector2(1, 1)));
                triangles.Add(17, CalcPoint(7, x, z, -Vector3.UnitZ, new Vector2(1, 0)));
            }

            if(MazeCells[x, z].Walls[3])
            {
                triangles.Add(18, CalcPoint(1, x, z, Vector3.UnitX, new Vector2(0, 0)));
                triangles.Add(19, CalcPoint(0, x, z, Vector3.UnitX, new Vector2(0, 1)));
                triangles.Add(20, CalcPoint(3, x, z, Vector3.UnitX, new Vector2(1, 0)));
                triangles.Add(21, CalcPoint(0, x, z, Vector3.UnitX, new Vector2(0, 1)));
                triangles.Add(22, CalcPoint(2, x, z, Vector3.UnitX, new Vector2(1, 1)));
                triangles.Add(23, CalcPoint(3, x, z, Vector3.UnitX, new Vector2(1, 0)));
            }

            return triangles;
        }

        private VertexPositionNormalTexture CalcPoint(int wallPoint, int xOffset, int zOffset, Vector3 normal, Vector2 texCoords)
        {
            return new VertexPositionNormalTexture(wallPoints[wallPoint] + new Vector3(xOffset, 0, zOffset), normal, texCoords);
        }

        public void GenerateMaze()
        {
            for(int x = 0; x < mazeWidth; x++)
            {
                for(int z = 0; z < mazeHeight; z++)
                {
                    MazeCells[x, z].Walls[0] = true;
                    MazeCells[x, z].Walls[1] = true;
                    MazeCells[x, z].Walls[2] = true;
                    MazeCells[x, z].Walls[3] = true;
                    MazeCells[x, z].Visited = false;
                }
            }

            MazeCells[0, 0].Visited = true;
            EvaluateCell(new Vector2(0, 0));
        }

        private void EvaluateCell(Vector2 cell)
        {
            List<int> neighborCells = new List<int>();
            neighborCells.Add(0);
            neighborCells.Add(1);
            neighborCells.Add(2);
            neighborCells.Add(3);

            while(neighborCells.Count > 0)
            {
                int random = RNG.Next(0, neighborCells.Count);
                int selectedNeighbor = neighborCells[random];
                neighborCells.RemoveAt(random);

                Vector2 neighbor = cell;

                switch(selectedNeighbor)
                {
                    case 0:
                        neighbor += new Vector2(0, -1);
                        break;
                    case 1:
                        neighbor += new Vector2(1, 0);
                        break;
                    case 2:
                        neighbor += new Vector2(0, 1);
                        break;
                    case 3:
                        neighbor += new Vector2(-1, 0);
                        break;
                }

                if((neighbor.X >= 0) && (neighbor.X < mazeWidth)
                    && (neighbor.Y >= 0) && (neighbor.Y < mazeHeight))
                {
                    if(!MazeCells[(int)neighbor.X, (int)neighbor.Y].Visited)
                    {
                        MazeCells[(int)neighbor.X, (int)neighbor.Y].Visited = true;
                        MazeCells[(int)cell.X, (int)cell.Y].Walls[selectedNeighbor] = false;
                        MazeCells[(int)neighbor.X, (int)neighbor.Y].Walls[(selectedNeighbor + 2) % 4] = false;
                        EvaluateCell(neighbor);
                    }
                }
            }
        }

        public List<BoundingBox> GetBoundsForCell(int x, int z)
        {
            List<BoundingBox> boxes = new List<BoundingBox>();

            if(MazeCells[x, z].Walls[0])
            {
                boxes.Add(BuildBoundingBox(x, z, 2, 4));
            }

            if(MazeCells[x, z].Walls[1])
            {
                boxes.Add(BuildBoundingBox(x, z, 6, 5));
            }

            if(MazeCells[x, z].Walls[2])
            {
                boxes.Add(BuildBoundingBox(x, z, 3, 5));
            }

            if(MazeCells[x, z].Walls[3])
            {
                boxes.Add(BuildBoundingBox(x, z, 2, 1));
            }

            return boxes;
        }

        private BoundingBox BuildBoundingBox(int x, int z, int point1, int point2)
        {
            BoundingBox thisBox = new BoundingBox(wallPoints[point1], wallPoints[point2]);

            thisBox.Min.X += x;
            thisBox.Min.Z += z;
            thisBox.Max.X += x;
            thisBox.Max.Z += z;

            thisBox.Min.X -= 0.1f;
            thisBox.Min.Z -= 0.1f;
            thisBox.Max.X += 0.1f;
            thisBox.Max.Z += 0.1f;

            return thisBox;
        }

        public void Draw(Camera camera, Effect[] effects)
        {
            effects[0].Parameters["World"].SetValue(Matrix.Identity);
            effects[0].Parameters["View"].SetValue(camera.View);
            effects[0].Parameters["Projection"].SetValue(camera.Projection);
            effects[0].Parameters["ViewVector"].SetValue(camera.Position);

            Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(Matrix.Identity));
            effects[0].Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

            effects[0].Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
            effects[0].Parameters["AmbientIntensity"].SetValue(lightIntensity);
            for (int i = 1; i < effects.Length; i++)
            {
                effects[i] = effects[0].Clone();
                
            }

            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].Parameters["ModelTexture"].SetValue(textures[i]);
            }

            device.SamplerStates[0] = SamplerState.LinearClamp;
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            
            foreach (EffectPass pass in effects[0].CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(floorBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, floorBuffer.VertexCount / 3);
            }

            for (int i = 1; i < effects.Length; i++)
            {
                foreach (EffectPass pass in effects[i].CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.SetVertexBuffer(wallBuffers[i - 1]);
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, wallBuffers[i - 1].VertexCount / 3);
                }
            }
        }

        public void Draw(Camera camera, BasicEffect[] effects)
        {
            for(int i = 0; i < effects.Length; i++)
            {
                effects[i].TextureEnabled = true;
                effects[i].World = Matrix.Identity;
                effects[i].View = camera.View;
                effects[i].Projection = camera.Projection;
                effects[i].AmbientLightColor = ambientColor;
                effects[i].FogColor = Color.Transparent.ToVector3();
                effects[i].FogEnabled = fogToggle;
                effects[i].FogStart = 1f;
                effects[i].FogEnd = 3f;
                effects[i].LightingEnabled = true;
                //effects[i].DirectionalLight0.Enabled = false;
                //effects[i].DirectionalLight0.Direction = camera.View.Forward;
                //effects[i].DirectionalLight0.DiffuseColor = Color.Transparent.ToVector3();
                //effects[i].DirectionalLight0.SpecularColor = Color.White.ToVector3();
                //effects[i].EmissiveColor = Color.White.ToVector3();
            }

            for(int i = 0; i < effects.Length; i++)
            {
                effects[i].Texture = textures[i];
            }
            device.Clear(Color.Black);
            device.SamplerStates[0] = SamplerState.LinearClamp;
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            foreach(EffectPass pass in effects[0].CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(floorBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, floorBuffer.VertexCount / 3);
            }

            for(int i = 1; i < effects.Length; i++)
            {
                foreach(EffectPass pass in effects[i].CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.SetVertexBuffer(wallBuffers[i - 1]);
                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, wallBuffers[i - 1].VertexCount / 3);
                }
            }
        }
    }
}
