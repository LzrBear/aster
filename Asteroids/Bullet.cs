using Microsoft.Xna.Framework;

namespace Asteroids
{
    class Bullet
    {
        public Vector3 position;
        public Vector3 direction;
        public float speed;
        public BoundingSphere bounds;
        public bool isActive = false;

        public void Update(float delta)
        {

            position += direction * speed *
                GameConstants.BulletSpeedAdjustment * delta;

            if (position.X > GameConstants.PlayfieldSizeX ||
                position.X < -GameConstants.PlayfieldSizeX ||
                position.Y > GameConstants.PlayfieldSizeY ||
                position.Y < -GameConstants.PlayfieldSizeY)
                isActive = false;
        }

    }
}
