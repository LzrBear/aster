using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Asteroids
{
    class Ship
    {
        public Model Model;
        public Matrix[] Transforms;

        //Position of the model in the world space
        public Vector3 Position = Vector3.Zero;

        //Velocity of the model, applied in each frame to the models positions
        public Vector3 Velocity = Vector3.Zero;

        //amplifies controller speed input
        private const float VelocityScale = 1.5f;

        //Determines if the ship is active
        public bool isActive = true;

        //Number of remaining lives
        public int lives = GameConstants.lives;

        public Matrix RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2);
        private float rotation;
        public float acceleration = 0.0f;

        public float Rotation
        {

            get { return rotation; }
            set
            {

                float newVal = value;

                while (newVal >= MathHelper.TwoPi)
                {
                    newVal -= MathHelper.TwoPi;
                }

                while (newVal < 0)
                {
                    newVal += MathHelper.TwoPi;
                }

                if (rotation != value)
                {
                    rotation = value;
                    RotationMatrix =
                        Matrix.CreateRotationX(MathHelper.PiOver2) *
                        Matrix.CreateRotationZ(rotation);

                }
            }
        }

        public void Update(GamePadState currentGamePadSate, KeyboardState currentKeyState)
        {

            // Rotate the model using the A and D keys if there is no keyboard input then rotate the model using the left thumbstick and scale it down
            if (currentKeyState.IsKeyDown(Keys.A) || currentKeyState.IsKeyDown(Keys.Left))
                Rotation += 0.10f;
            else if (currentKeyState.IsKeyDown(Keys.D) || currentKeyState.IsKeyDown(Keys.Right))
                Rotation -= 0.10f;
            else
                Rotation -= currentGamePadSate.ThumbSticks.Left.X * 0.10f;

            // Thrust ramps up logarithmically: fast response at first, then tapering.
            if (currentKeyState.IsKeyDown(Keys.W) || currentKeyState.IsKeyDown(Keys.Up))
            {
                if (acceleration <= GameConstants.maxSpeed)
                    acceleration += 0.1f;

                float logFraction = MathF.Log(1f + acceleration) / MathF.Log(1f + GameConstants.maxSpeed);
                Velocity += RotationMatrix.Forward * VelocityScale * GameConstants.maxSpeed * logFraction;
            }
           else
            {
                acceleration = 0.0f;
                // If there is no keyboard input check for gamepad input and scale by how hard the trigger is down
                Velocity += RotationMatrix.Forward * VelocityScale * currentGamePadSate.Triggers.Right;
            }


            //Check if the ship goes out of playfield and warp them around
            if (Position.X > GameConstants.PlayfieldSizeX)
                Position.X -= 2 * GameConstants.PlayfieldSizeX;
            if (Position.X < -GameConstants.PlayfieldSizeX)
                Position.X += 2 * GameConstants.PlayfieldSizeX;
            if (Position.Y > GameConstants.PlayfieldSizeY)
                Position.Y -= 2 * GameConstants.PlayfieldSizeY;
            if (Position.Y < -GameConstants.PlayfieldSizeY)
                Position.Y += 2 * GameConstants.PlayfieldSizeY;

        }
    }
}
