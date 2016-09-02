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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        string WindowTitle = "KinectWorld";

        GraphicsDeviceManager graphics;
        KinectWorld kinectWorld;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            kinectWorld = new KinectWorld(Content);
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = WindowTitle;

            base.Initialize();
            
            kinectWorld.SetUpKinectWorld();
        }

        protected override void LoadContent()
        {
            kinectWorld.Load(graphics.GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            kinectWorld.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            kinectWorld.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            kinectWorld.Draw();
            base.Draw(gameTime);
        }
    }
}