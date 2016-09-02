using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace KinectDepthRender
{
    public class KinectWorld : GameBase
    {
        private const int ScaleFactor = 50;

        public KinectManager kinectManager;

        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;
        public GraphicsDevice Device;
        
        private SpriteBatch _spriteBatch;
        public ContentManager Content;

        //Define our coordinates
        private VertexPositionColor[] vertices;
        private int[] indices;
        private float[,] depthData;

        //The expected width of our kinect depth stream
        private int _kinectDepthWidth = 16;
        private int _kinectDepthHeight = 24;

        public Effect Effect;

        private float _YRotationAngle = 0f;
        private float _XRotationAngle = 0f;

        public KinectWorld(ContentManager content)
        {
            Content = content;
        }

        public void SetUpKinectWorld()
        {
            SetupCameraMatrices();

            kinectManager = new KinectManager(this);
            this.kinectManager.SetupKinectHandler();
            this.kinectManager.StartSensors();
        }

        public void Update()
        {
            HandleControls();
            MapKinectDepthToTriangles();
        }

        public void HandleControls()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                _YRotationAngle += 0.005f;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                _YRotationAngle -= 0.005f;
            }
            
            
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                _XRotationAngle -= 0.005f;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                _XRotationAngle += 0.005f;
            }
        }

        public void Load(GraphicsDevice device)
        {
            this.Device = device;
            _spriteBatch = new SpriteBatch(Device);
            this.Effect = Content.Load<Effect>("effects");

            
        }

        private void SetupCameraMatrices()
        {
            //ViewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 50), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            ViewMatrix = Matrix.CreateLookAt(new Vector3(60, 80, -80), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Device.Viewport.AspectRatio, 1.0f, 300.0f);
        }

        private void MapKinectDepthToTriangles()
        {
            //Map our kinect info to useful data
            short[] depth = this.kinectManager.Sensors[0].DepthPixelData;
            LoadDepthData(depth);

            //stop using this image and make our own
            //Texture2D heightMap = Content.Load<Texture2D>("heightmap");
            //LoadHeightData(heightMap);
            
            SetUpVertices();
            SetUpIndices();
        }

        private void SetUpVertices()
        {
            vertices = new VertexPositionColor[_kinectDepthWidth * _kinectDepthHeight];
            for (int x = 0; x < _kinectDepthWidth; x++)
            {
                for (int y = 0; y < _kinectDepthHeight; y++)
                {
                    vertices[x + y * _kinectDepthWidth].Position = new Vector3(x, depthData[x, y], -y);
                    vertices[x + y * _kinectDepthWidth].Color = Color.White;
                }
            }
        }

        private void SetUpIndices()
        {
            indices = new int[(_kinectDepthWidth - 1) * (_kinectDepthHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < _kinectDepthHeight - 1; y++)
            {
                for (int x = 0; x < _kinectDepthWidth - 1; x++)
                {
                    int lowerLeft = x + y * _kinectDepthWidth;
                    int lowerRight = (x + 1) + y * _kinectDepthWidth;
                    int topLeft = x + (y + 1) * _kinectDepthWidth;
                    int topRight = (x + 1) + (y + 1) * _kinectDepthWidth;

                    //IsDataUsable(depthData[], depthData[], depthData[]))

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }

        private bool IsDataUseable(int a, int b, int c)
        {
            int minimumDepth = 10;
            int maximumDepth = 3000;
            int maximumSeparationDistance = 1000;

            if (a < minimumDepth || b < minimumDepth || c < minimumDepth)
            {
                return false;
            }

            if (a > maximumDepth || b > maximumDepth || c > maximumDepth)
            {
                return false;
            }

            if (Difference(a, b) > maximumSeparationDistance || Difference(a, c) > maximumSeparationDistance || Difference(c, b) > maximumSeparationDistance)
            {
                return false;
            }

            return true;
        }

        private int Difference(int a, int b)
        {
            int result = a - b;

            if (result < 0)
            {
                result = b - a;
            }

            return result;
        }

        private void LoadHeightData(Texture2D heightMap)
        {
            _kinectDepthWidth = heightMap.Width;
            _kinectDepthHeight = heightMap.Height;

            Color[] heightMapColors = new Color[_kinectDepthWidth * _kinectDepthHeight];
            heightMap.GetData(heightMapColors);

            depthData = new float[_kinectDepthWidth, _kinectDepthHeight];
            for (int x = 0; x < _kinectDepthWidth; x++)
                for (int y = 0; y < _kinectDepthHeight; y++)
                    depthData[x, y] = heightMapColors[x + y * _kinectDepthWidth].R / 5.0f;
        }

        private void LoadDepthData(short[] depthFrame)
        {
            if (depthFrame.Length == 4800)
            {
                _kinectDepthWidth = 80;
                _kinectDepthHeight = 60;
            }
            else if (depthFrame.Length == 307200)
            {
                _kinectDepthWidth = 640;
                _kinectDepthHeight = 480;
            }
            else if (depthFrame.Length == 76800)
            {
                _kinectDepthWidth = 320;
                _kinectDepthHeight = 240;
            }

            short max = depthFrame.Max();
            short min = depthFrame.Min();

            Color[] heightMapColors = new Color[_kinectDepthWidth * _kinectDepthHeight];
            for (int i = 0; i < depthFrame.Length; i++)
            {
                heightMapColors[i] = new Color(depthFrame[i], 0f, 0f);
            }

            depthData = new float[_kinectDepthWidth, _kinectDepthHeight];
            for (int x = 0; x < _kinectDepthWidth; x++)
                for (int y = 0; y < _kinectDepthHeight; y++)
                    depthData[x, y] = ConvertKinectDepthToCm(heightMapColors[x + y * _kinectDepthWidth]);
        }

        public float ConvertKinectDepthToCm(Color value)
        {
            //if (value.R < UInt32.MaxValue)
            //{
            return (float)(0.1236 * Math.Tan(value.R / 2842.5 + 1.1863) * 10) ;
            //return (float)(0.1236f * Math.Tan(value.R / 2842.5 + 1.1863));
            //}

            //return 0.0f;
        }

        public void Draw()
        {
            Device.Clear(Color.Black);

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.WireFrame;
            Device.RasterizerState = rs;

            Matrix worldMatrix = Matrix.CreateTranslation(-_kinectDepthWidth / 2.0f, 0, _kinectDepthHeight / 2.0f) * Matrix.CreateRotationY(_YRotationAngle) * Matrix.CreateRotationX(_XRotationAngle);
            Effect.CurrentTechnique = Effect.Techniques["ColoredNoShading"];
            Effect.Parameters["xView"].SetValue(this.ViewMatrix);
            Effect.Parameters["xProjection"].SetValue(this.ProjectionMatrix);
            Effect.Parameters["xWorld"].SetValue(worldMatrix);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, VertexPositionColor.VertexDeclaration);
            }
        }


        public void Unload()
        {
        }

    }
}
