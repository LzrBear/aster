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

using Particle3DSample;
using Asteroids.Game_Types;

namespace Asteroids
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class asterGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Camera/View Information
        Vector3 cameraPosition = new Vector3(0.0f, 0.0f, GameConstants.CameraHeight);
        Matrix projectionMatrix;
        Matrix viewMatrix;

        Song Music_default;

        //Visual components
        Ship ship = new Ship();

        Texture2D stars;
        
        //Particle Effects
        particleEffects particleEffect = new particleEffects();

        //other
        Random random = new Random();
        GamePadState lastGState = GamePad.GetState(PlayerIndex.One);
        KeyboardState lastKState = Keyboard.GetState();

        standardGameType stand = new standardGameType();

        public asterGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            particleEffect.initialize(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                GraphicsDevice.DisplayMode.AspectRatio,
                GameConstants.CameraHeight - 2000.0f, GameConstants.CameraHeight + 2000.0f);
            viewMatrix = Matrix.CreateLookAt(cameraPosition,
                Vector3.Zero, Vector3.Up);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.Initialize();
        }

        private Matrix[] SetupEffectDefaults(Model myModel)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = projectionMatrix;
                    effect.View = viewMatrix;
                }
            }
            return absoluteTransforms;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        ///

        protected override void LoadContent()
        {
            //Background
            stars =
                Content.Load<Texture2D>("Textures/B1_stars");

            //Music
            Music_default =
                Content.Load<Song>("Audio/Music/Hyper Space");

            //IMPORTANT
            //temporary solution to playing music
            //needs to be moved later so that we can change the music according to whats on screen
            //i.e. music feel in accordance to whats being displayed
            MediaPlayer.Volume = 0.1f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(Music_default);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        ///

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            //Determine the change in time
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Make the stars flash
            particleEffect.UpdateStars();

            //Run the standard game type
            stand.RunGame(this, gameTime, GraphicsDevice, projectionMatrix, viewMatrix);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        ///
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque;

            //Draw the background
            spriteBatch.Begin();
            spriteBatch.Draw(stars, new Rectangle(0, 0, 800, 600), Color.White);
            spriteBatch.End();

            //Draw the star particle effects
            particleEffect.draw(viewMatrix, projectionMatrix);

            base.Draw(gameTime);

            //Draw the standard game type
            stand.drawGame(GraphicsDevice);

            base.Draw(gameTime);
        }
    }
}
