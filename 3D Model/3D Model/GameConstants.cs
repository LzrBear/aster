using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asteroids
{
    class GameConstants
    {
        //camera constants
        public const float CameraHeight = 25000.0f;

        // Playfield boundaries — set at runtime from the projection matrix so they
        // always match the actual view frustum regardless of monitor aspect ratio.
        // Spawn + wrap happen at ±PlayfieldSizeX/Y (2× the visible half-extent).
        // _min values equal the exact visible half-extent (used for on-screen checks).
        public static float PlayfieldSizeX = 36000f;   // fallback until runtime init
        public static float PlayfieldSizeY = 21000f;
        public static float PlayfieldSizeX_min = 18000f;
        public static float PlayfieldSizeY_min = 10500f;

        //ship constant
        public const float maxSpeed = 5.0f;
        public const int lives = 3;

        //asteroid constants
        public const int NumAsteroids = 20;
        public const float AsteroidMinSpeed = 100.0f;
        public const float AsteroidMaxSpeed = 300.0f;
        public const float AsteroidSpeedAdjustment = 5.0f;

        //Bullet constants
        public const int NumBullets = 30;
        public const float BulletSpeedAdjustment = 100.0f;

        //Bounding Sphere Scales - For collision detection
        public const float AsteroidBoundingSphereScale = 0.70f; //95% size
        public const float ShipBoundingSphereScale = 0.5f; //50% size

        //score constants
        public const int ShotPenalty = 1;
        public const int DeathPenalty = 100;
        public const int WarpPenalty = 50;
        public const int KillBonus = 25;

        public const int numStars = 10;

        // Volume levels (0.0 = silent, 1.0 = full)
        public const float MusicVolume = 0.4f;
        public const float EngineVolume = 0.25f;
        public const float ExplosionVolume = 0.5f;
        public const float WeaponVolume = 0.45f;
        public const float HyperspaceVolume = 0.4f;
    }
}
