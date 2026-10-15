using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Asteroids
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Set the position of the model in world space, and set the rotation.
        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;

        // Set the position of the camera in world space, for our view matrix.
        Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 5000.0f);

        // Set the velocity of the model, applied each frame to the model's position.
        Vector3 modelVelocity = Vector3.Zero;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 

        // Set the 3D model to draw
        Model myModel;

        //Set the sound effects to use
        SoundEffect soundEngine;
        SoundEffectInstance soundEngineInstance;
        SoundEffect soundHyperspaceActivation;

        // The aspect ratio determines how to scale 3d to 2d projection
        float aspectRatio;

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            myModel = Content.Load<Model>("Models\\asteroid1");
            soundEngine = Content.Load<SoundEffect>("Audio\\Waves\\engine_2");
            soundEngineInstance = soundEngine.CreateInstance();
            soundHyperspaceActivation =
                Content.Load<SoundEffect>("Audio\\Waves\\hyperspace_activate");

            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Get some input
            UpdateInput();

            // Add velocity to the current position.
            modelPosition += modelVelocity;

            // Bleed off velocity over time
            modelVelocity *= 0.99f;

            base.Update(gameTime);
        }


        float acceleration = 0;
        protected void UpdateInput()
        {

            // Get the keyboard state
            KeyboardState currentKeyState = Keyboard.GetState();
            GamePadState currentGamePadSate = GamePad.GetState(PlayerIndex.One);

            // Rotate the model using the A and D keys if there is no keyboard input then rotate the model using the left thumbstick and scale it down
            if (currentKeyState.IsKeyDown(Keys.A) || currentKeyState.IsKeyDown(Keys.Left))
                modelRotation += 0.10f;
            else if (currentKeyState.IsKeyDown(Keys.D) || currentKeyState.IsKeyDown(Keys.Right))
                modelRotation -= 0.10f;
            else
                modelRotation -= currentGamePadSate.ThumbSticks.Left.X * 0.10f;

            // Create some velocity if the right trigger is down.
            Vector3 modelVelocityAdd = Vector3.Zero;
            
            // Find out what direction we should be thrusting,
            // using rotation.
            modelVelocityAdd.X = -(float)Math.Sin(modelRotation);
            modelVelocityAdd.Z = -(float)Math.Cos(modelRotation);

            // Now scale our direction by how long the key has been pressed down
            if (currentKeyState.IsKeyDown(Keys.W) || currentKeyState.IsKeyDown(Keys.Up))
            {
                if (acceleration <= 30f)
                    acceleration += 0.1f;

                modelVelocityAdd *= acceleration;
            }
            else
            {
                acceleration = 0.0f;
                //modelVelocityAdd *= acceleration;
                // If there is no keyboard input check for gamepad input and scale by how hard the trigger is down
                // I guess if there is no gamepad plugged in this will return 0.
                modelVelocityAdd *= currentGamePadSate.Triggers.Right;
            }

            //Finally, add this vector to our velocity.
            modelVelocity += modelVelocityAdd;

            GamePad.SetVibration(PlayerIndex.One,
                currentGamePadSate.Triggers.Right,
                currentGamePadSate.Triggers.Right);

            //Play engine sound only when the engine is on.
            if (currentGamePadSate.Triggers.Right > 0 || currentKeyState.IsKeyDown(Keys.W) || currentKeyState.IsKeyDown(Keys.Up))
            {
                if (soundEngineInstance.State == SoundState.Stopped)
                {
                    soundEngineInstance.Volume = 0.75f;
                    soundEngineInstance.IsLooped = true;
                    soundEngineInstance.Play();
                }
                else
                    soundEngineInstance.Resume();
            }
            else if (currentGamePadSate.Triggers.Right == 0 || currentKeyState.IsKeyUp(Keys.W) || currentKeyState.IsKeyUp(Keys.Up))
            {
                if (soundEngineInstance.State == SoundState.Playing)
                    soundEngineInstance.Pause();
            }

            // In case you get lost, press A to warp back to the center.
            if (currentGamePadSate.Buttons.A == ButtonState.Pressed || currentKeyState.IsKeyDown(Keys.Home))
            {
                modelPosition = Vector3.Zero;
                modelVelocity = Vector3.Zero;
                modelRotation = 0.0f;
                soundHyperspaceActivation.Play();
            }

            if (currentKeyState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
        }

            

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        ///
 
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Copy any parent transforms.
            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);

            // Draw the model. A model can have multiple meshes so loop.
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                // This is where the mest orientation is set, as well
                // as our camera and projection
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] *
                        Matrix.CreateRotationY(modelRotation)
                        * Matrix.CreateTranslation(modelPosition);
                    effect.View = Matrix.CreateLookAt(cameraPosition,
                        Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(60.0f), aspectRatio,
                        1.0f, 100000000.0f);
                }
                // Draw the mesh, using the effects set above.
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
