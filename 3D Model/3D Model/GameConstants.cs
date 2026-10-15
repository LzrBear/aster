namespace Asteroids
{
    public class GameConstants
    {
        // ── Window ────────────────────────────────────────────────────────────
        public const int WindowWidth  = 1280;
        public const int WindowHeight = 720;

        // ── Camera / projection ───────────────────────────────────────────────
        public const float CameraHeight      = 25000.0f;
        public const float CameraFov         = 45.0f;    // degrees
        public const float CameraClipMargin  = 2000.0f;  // near/far plane offset from CameraHeight

        // ── Playfield boundaries ──────────────────────────────────────────────
        // Set at runtime from the projection matrix so they always match the
        // actual view frustum regardless of monitor aspect ratio.
        // Spawn + wrap happen at ±PlayfieldSizeX/Y.  _min = exact visible half-extent.
        public static float PlayfieldSizeX     = 36000f;  // fallback until runtime init
        public static float PlayfieldSizeY     = 21000f;
        public static float PlayfieldSizeX_min = 18000f;
        public static float PlayfieldSizeY_min = 10500f;
        public const  float PlayfieldOverscale = 1.15f;   // playfield = visible * this

        // ── Ship ──────────────────────────────────────────────────────────────
        public const float maxSpeed = 5.0f;
        public const int   lives    = 3;

        // ── Asteroids ─────────────────────────────────────────────────────────
        public const int   NumAsteroids           = 60;    // pool size (allows for splits)
        public const int   NumInitialAsteroids    = 20;    // target count at full difficulty
        public const int   NumInitialAsteroidsMin = 3;     // count at game start
        public const float AsteroidMinSpeed       = 100.0f;
        public const float AsteroidMaxSpeed       = 300.0f;

        // ── Difficulty progression ────────────────────────────────────────────
        // Asteroid count and speed scale linearly from their Min/Initial values
        // up to full difficulty over DifficultyRampDuration seconds.
        public const float DifficultyRampDuration  = 120f;  // seconds to reach full difficulty
        public const float InitialSpeedMultiplier  = 0.25f; // fraction of full speed at game start
        public const float AsteroidSpawnInterval   = 4f;    // seconds between spawn-check ticks
        public const float AsteroidSpeedAdjustment = 5.0f;
        public const float AsteroidSplitSeparation = 500f;  // offset between split fragments (must stay inside smoke cloud)

        // Crack thresholds — hits before an asteroid splits (per size)
        public const int LargeCracksMin  = 3;
        public const int LargeCracksMax  = 5;
        public const int MediumCracksMin = 2;
        public const int MediumCracksMax = 3;
        public const int SmallCracksMin  = 1;
        public const int SmallCracksMax  = 2;

        // Visual scale per asteroid size
        public const float LargeAsteroidScale  = 1.0f;
        public const float MediumAsteroidScale = 0.5f;
        public const float SmallAsteroidScale  = 0.25f;

        // Size rank used to compute collision crack damage:
        //   cracks dealt = max(1, attacker_rank - target_rank + 1)
        // A larger asteroid deals more cracks to a smaller one; a smaller
        // asteroid can only ever deal 1 crack to a larger one.
        public const int AsteroidSizeRankLarge  = 3;
        public const int AsteroidSizeRankMedium = 2;
        public const int AsteroidSizeRankSmall  = 1;

        // Number of fragments spawned when an asteroid is destroyed (= its size rank).
        // Large → 3 Medium, Medium → 2 Small, Small → gone (respawn off-screen).
        public const int LargeAsteroidFragments  = 3;
        public const int MediumAsteroidFragments = 2;

        // ── Bullets ───────────────────────────────────────────────────────────
        public const int   NumBullets          = 30;
        public const float BulletSpeedAdjustment = 100.0f;
        public const float BulletSpawnOffset   = 1000f;   // distance ahead of ship nose

        // ── Collision ─────────────────────────────────────────────────────────
        public const float AsteroidBoundingSphereScale = 0.70f;
        public const float ShipBoundingSphereScale     = 0.5f;

        // ── Gameplay ──────────────────────────────────────────────────────────
        public const float VelocityDamping               = 0.99f;
        public const float RespawnInvincibilityDuration  = 3.0f;  // seconds
        public const int   RespawnFlashRate              = 8;     // flashes per second
        public const float ExplosionDistanceFactor       = 2000f; // normalises sound volume by distance
        public const float ExplosionSoundThreshold       = 1.0f;  // max normalised distance for audible explosion
        public const float StarAdditiveBoost             = 0.9f;  // opacity of the additive star pass
        public const float FontSpaceWidthScale           = 0.5f;  // space glyph width = M-width * this
        public const int   MaxNameLength                 = 10;    // max characters in a player name
        public const string DefaultPlayerName            = "???"; // fallback when name left blank

        // ── Score ─────────────────────────────────────────────────────────────
        public const int ShotPenalty         = 1;
        public const int DeathPenalty        = 100;
        public const int WarpPenalty         = 50;
        public const int KillBonus           = 25;   // legacy — use size-specific bonuses
        public const int LargeAsteroidBonus  = 10;
        public const int MediumAsteroidBonus = 25;
        public const int SmallAsteroidBonus  = 50;

        // ── High scores ───────────────────────────────────────────────────────
        public const int    MaxHighScores      = 10;
        public const string HighScoresFile     = "highscores.dat";
        public const char   HighScoreSeparator = '|';

        // AES-256 key for encrypting the high scores file.
        // HMAC-SHA256 key for integrity verification (encrypt-then-MAC).
        public static readonly byte[] HsAesKey = new byte[]
        {
            0x4A, 0x7F, 0x2E, 0xB3, 0xC1, 0x95, 0x6D, 0xA8,
            0x3E, 0xF4, 0x71, 0xD2, 0x88, 0x0C, 0x5B, 0x96,
            0xE7, 0x43, 0xAA, 0x1F, 0x69, 0xBC, 0x27, 0x5E,
            0xD0, 0x84, 0xF3, 0x12, 0x7A, 0x4E, 0x9C, 0x6B
        };
        public static readonly byte[] HsHmacKey = new byte[]
        {
            0xB5, 0x3D, 0x8A, 0xF6, 0x21, 0xC4, 0x7E, 0x19,
            0x5C, 0xA2, 0xED, 0x70, 0x4F, 0x93, 0x16, 0xDB,
            0x82, 0x5A, 0xC9, 0x47, 0xFE, 0x3B, 0xA6, 0x60,
            0x1D, 0x74, 0xE8, 0x2F, 0x91, 0xB0, 0x53, 0xC7
        };

        // ── Volume levels (0.0 = silent, 1.0 = full) ─────────────────────────
        public const float MusicVolume      = 0.4f;
        public const float EngineVolume     = 0.25f;
        public const float ExplosionVolume  = 0.5f;
        public const float WeaponVolume     = 0.45f;
        public const float HyperspaceVolume = 0.4f;

        // ── Particle counts ───────────────────────────────────────────────────
        // Smoke and sparks emitted on asteroid collision/destruction.
        // Actual count = max(Min, (int)(AsteroidScale * Scale)) so larger
        // asteroids produce a proportionally bigger cloud.
        public const float SmokeParticleCountScale  = 60f;
        public const float SparksParticleCountScale = 40f;
        public const int   SmokeParticleMin         = 5;
        public const int   SparksParticleMin        = 8;

        // ── Misc ──────────────────────────────────────────────────────────────
        public const int numStars = 10;

        // ── Content paths ─────────────────────────────────────────────────────
        public const string ContentRoot    = "3D ModelContent";
        public const string FontMain       = "Fonts/Lucida Console";
        public const string TextureStars   = "Textures/B1_stars";
        public const string ModelShip      = "Models/p1_wedge";
        public const string ModelAsteroid  = "Models/asteroid1";
        public const string ModelBullet    = "Models/pea_proj";
        public const string SfxEngine      = "Audio/Waves/engine_2";
        public const string SfxHyperspace  = "Audio/Waves/hyperspace_activate";
        public const string SfxExplosion1  = "Audio/Waves/explosion1";
        public const string SfxExplosion2  = "Audio/Waves/explosion2";
        public const string SfxExplosion3  = "Audio/Waves/explosion3";
        public const string SfxWeaponFire  = "Audio/Waves/tx0_fire1";
        public const string MusicDefault   = "Audio/Music/Hyper Space";

        // ── UI layout ─────────────────────────────────────────────────────────
        public const float UiTitleScale    = 2f;
        public const float UiSubtitleScale = 1.5f;

        // Main menu
        public const float UiMenuTitleY     = 120f;
        public const float UiMenuFirstItemY = 280f;
        public const float UiMenuItemSpacing = 55f;
        public const float UiMenuHintY       = 540f;

        // Game over screen
        public const float UiGameOverTitleY  = 180f;
        public const float UiGameOverScoreY  = 290f;
        public const float UiGameOverPromptY = 350f;

        // Enter name screen
        public const float UiEnterNameTitleY  = 160f;
        public const float UiEnterNameScoreY  = 250f;
        public const float UiEnterNamePromptY = 310f;
        public const float UiEnterNameInputY  = 355f;
        public const float UiEnterNameHintY   = 540f;

        // High scores screen
        public const float UiHighScoresTitleY        = 60f;
        public const float UiHighScoresFirstEntryY   = 160f;
        public const float UiHighScoresEntrySpacing  = 35f;
        public const float UiHighScoresHintY         = 560f;

        // ── UI text ───────────────────────────────────────────────────────────
        public const string UiTitleGame        = "ASTEROIDS";
        public const string UiMenuPlay         = "PLAY";
        public const string UiMenuHighScores   = "HIGH SCORES";
        public const string UiMenuQuit         = "QUIT";
        public const string UiHintMenuNav      = "UP/DOWN to select   ENTER to confirm";
        public const string UiTitleGameOver    = "GAME OVER";
        public const string UiTitleHighScore   = "NEW HIGH SCORE!";
        public const string UiPromptHighScore  = "NEW HIGH SCORE!  PRESS ENTER";
        public const string UiPromptContinue   = "PRESS ENTER TO CONTINUE";
        public const string UiPromptEnterName  = "ENTER YOUR NAME (max 10 chars):";
        public const string UiHintNameEntry    = "ENTER to confirm   ESC to cancel";
        public const string UiHintReturn       = "PRESS ENTER TO RETURN";
        public const string UiMsgNoScores      = "NO SCORES YET";
        public const string UiHudScore         = "Score: ";
        public const string UiHudLives         = "Lives: ";
        public const string UiHudShipDestroyed = "SHIP DESTROYED";
        public const string UiHudRespawn       = "Press HOME to respawn";
    }
}
