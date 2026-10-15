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
    enum AsteroidSize { Large, Medium, Small }

    class Asteroid
    {

        public Vector3 position;
        public Vector3 direction;

        public float speed;
        public BoundingSphere bounds;
        public bool isActive = false;
        public AsteroidSize Size = AsteroidSize.Large;
        public int cracks = 0;
        public int cracksToBreak = 3;

        public Matrix RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) *
            Matrix.CreateRotationZ(10.0f) * Matrix.CreateRotationY(10.0f);
        public float rotation;
        private float rot;

        public void Update(float delta)
        {
            rot += rotation;

            //We only rotate in the Z & Y axis
            //I have a feeling that since rotation is just increasing at one point it may crash the game. (if run long enough)
            RotationMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) *
                Matrix.CreateRotationZ(rot) *
                Matrix.CreateRotationY(rot);

            //direction.X += -(float)Math.Sin(10.0f);
            //direction.Y += -(float)Math.Cos(10.0f);

            //Update the position of the asteroid
            position += direction * speed * GameConstants.AsteroidSpeedAdjustment * delta;
            
            //Update the angle of the asteroid


            //Check if asteroids go out of playfied and warp them around
            if (position.X > GameConstants.PlayfieldSizeX)
                position.X -= 2 * GameConstants.PlayfieldSizeX;
            if (position.X < -GameConstants.PlayfieldSizeX)
                position.X += 2 * GameConstants.PlayfieldSizeX;
            if (position.Y > GameConstants.PlayfieldSizeY)
                position.Y -= 2 * GameConstants.PlayfieldSizeY;
            if (position.Y < -GameConstants.PlayfieldSizeY)
                position.Y += 2 * GameConstants.PlayfieldSizeY;

        }

    }
}
