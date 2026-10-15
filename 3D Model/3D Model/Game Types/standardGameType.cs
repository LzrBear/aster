using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Asteroids.Game_Types
{
    class standardGameType : gameType
    {

        SpriteBatch spriteBatch;

        Ship ship = new Ship();
        public particleEffects particleEffect = new particleEffects();

        //Bullets
        Model bulletModel;
        Matrix[] bulletTransforms;
        Bullet[] bulletList = new Bullet[GameConstants.NumBullets];

        //Asteroids
        Model asteroidModel;
        Matrix[] asteroidTransforms;
        Asteroid[] asteroidList = new Asteroid[GameConstants.NumAsteroids];

        //score
        int score = 0;
        Vector2 scorePosition = new Vector2(100, 50);
        float _spaceWidth; // set after font load; used for HUD spacing

        // Invincibility after respawn
        float respawnInvincibleTimer = 0f;  // counts down from 3 → 0

        // Difficulty progression
        float gameElapsed          = 0f;
        float currentSpeedMult     = GameConstants.InitialSpeedMultiplier;
        float asteroidSpawnTimer   = 0f;

        //Lives
        Vector2 livesPosition = new Vector2(100, 70);

        SpriteFont kootenay;

        //Audio Components
        SoundEffect soundEngine;
        SoundEffectInstance soundEngineInstance;
        SoundEffect soundHyperspaceActivation;
        SoundEffect soundExplosion1;
        SoundEffect soundExplosion2;
        SoundEffect soundExplosion3;
        SoundEffect soundWeaponsFire;

        //Store for previous keyboard and gamepad states
        GamePadState lastGState = GamePad.GetState(PlayerIndex.One);
        KeyboardState lastKState = Keyboard.GetState();

        //View
        Matrix projectionMatrix;
        Matrix viewMatrix;

        // Visible world half-extents at Z=0 (computed from projection matrix)
        float visibleHalfX;
        float visibleHalfY;

        //Game
        Game game;

        //other
        Random random = new Random();

        protected override void Initialize(Game gme, GraphicsDevice graphics, Matrix pMatrix, Matrix vMatrix)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(graphics);

            //Set the view information
            projectionMatrix = pMatrix;
            viewMatrix = vMatrix;

            // Compute visible world half-extents at Z=0 from the projection matrix.
            // For perspective: M11 = 1/(aspect*tan(fovY/2)), M22 = 1/tan(fovY/2)
            // At camera distance CameraHeight: halfX = CameraHeight/M11, halfY = CameraHeight/M22
            visibleHalfX = GameConstants.CameraHeight / pMatrix.M11;
            visibleHalfY = GameConstants.CameraHeight / pMatrix.M22;

            // Set playfield to 1.15× the visible area — just off-screen but close enough
            // that asteroids drift into view quickly. Wrap boundary matches spawn boundary
            // to prevent instant teleport.
            GameConstants.PlayfieldSizeX = visibleHalfX * 1.15f;
            GameConstants.PlayfieldSizeY = visibleHalfY * 1.15f;
            GameConstants.PlayfieldSizeX_min = visibleHalfX;
            GameConstants.PlayfieldSizeY_min = visibleHalfY;

            //Set the game information
            game = gme;

            //Initialize the asteroids
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                asteroidList[i] = new Asteroid();
                asteroidList[i].rotation = (float)random.NextDouble() / 100;
                asteroidList[i].isActive = false;
            }
            ResetAsteroids();

            //Initialize the bullets
            for (int i = 0; i < GameConstants.NumBullets; i++)
            {
                bulletList[i] = new Bullet();
            }

            //Load the fonts
            kootenay = game.Content.Load<SpriteFont>(GameConstants.FontMain);
            _spaceWidth  = kootenay.MeasureString("M").X * GameConstants.FontSpaceWidthScale;
            livesPosition = new Vector2(100, scorePosition.Y + kootenay.LineSpacing);

            //Initialize the particle Effects
            particleEffect.initialize(game);

            //Initialize the models
            ship.Model = game.Content.Load<Model>(GameConstants.ModelShip);
            ship.Transforms = SetupEffectDefaults(ship.Model, projectionMatrix, viewMatrix);

            asteroidModel = game.Content.Load<Model>(GameConstants.ModelAsteroid);
            asteroidTransforms = SetupEffectDefaults(asteroidModel, projectionMatrix, viewMatrix);

            bulletModel = game.Content.Load<Model>(GameConstants.ModelBullet);
            bulletTransforms = SetupEffectDefaults(bulletModel, projectionMatrix, viewMatrix);

            //Initialize the Sound Effects
            soundEngine = game.Content.Load<SoundEffect>(GameConstants.SfxEngine);
            soundEngineInstance = soundEngine.CreateInstance();
            soundHyperspaceActivation =
                game.Content.Load<SoundEffect>(GameConstants.SfxHyperspace);
            soundExplosion1 =
                game.Content.Load<SoundEffect>(GameConstants.SfxExplosion1);
            soundExplosion2 =
                game.Content.Load<SoundEffect>(GameConstants.SfxExplosion2);
            soundExplosion3 =
                game.Content.Load<SoundEffect>(GameConstants.SfxExplosion3);
            soundWeaponsFire =
                game.Content.Load<SoundEffect>(GameConstants.SfxWeaponFire);

        }

        public bool IsGameOver => ship.lives <= 0;
        public int Score => score;

        // Called by asterGame whenever the window is resized.
        public void UpdateProjection(Matrix projection, Matrix view)
        {
            projectionMatrix = projection;
            viewMatrix = view;

            // Recompute visible half-extents and playfield boundaries
            visibleHalfX = GameConstants.CameraHeight / projection.M11;
            visibleHalfY = GameConstants.CameraHeight / projection.M22;
            GameConstants.PlayfieldSizeX     = visibleHalfX * 1.15f;
            GameConstants.PlayfieldSizeY     = visibleHalfY * 1.15f;
            GameConstants.PlayfieldSizeX_min = visibleHalfX;
            GameConstants.PlayfieldSizeY_min = visibleHalfY;

            // Re-apply projection/view to all model effects
            SetupEffectDefaults(ship.Model,      projectionMatrix, viewMatrix);
            SetupEffectDefaults(asteroidModel,   projectionMatrix, viewMatrix);
            SetupEffectDefaults(bulletModel,     projectionMatrix, viewMatrix);
        }

        public void Reset()
        {
            score = 0;
            ship.lives = GameConstants.lives;
            ship.isActive = true;
            ship.Position = Vector3.Zero;
            ship.Velocity = Vector3.Zero;
            ship.Rotation = 0.0f;
            for (int i = 0; i < GameConstants.NumBullets; i++)
                bulletList[i].isActive = false;
            soundEngineInstance.Stop();
            respawnInvincibleTimer = 0f;
            gameElapsed        = 0f;
            currentSpeedMult   = GameConstants.InitialSpeedMultiplier;
            asteroidSpawnTimer = 0f;
            ResetAsteroids();
            FlushInput();
        }

        // Snapshot current input state so the first gameplay frame doesn't misread
        // keys that were held during a menu transition as fresh presses.
        public void FlushInput()
        {
            lastKState = Keyboard.GetState();
            lastGState = GamePad.GetState(PlayerIndex.One);
        }

        //Resets all the asteroids in the asteroid list;
        public void ResetAsteroids()
        {
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
                asteroidList[i].isActive = false;
            for (int i = 0; i < GameConstants.NumInitialAsteroidsMin; i++)
                resetAster(i);
        }

        public void resetAster(int i)
        {
            // Pick one of four edges at random and place the asteroid just off-screen on it.
            float xStart, yStart;
            Vector3 inward; // unit vector pointing into the playfield from that edge

            switch (random.Next(4))
            {
                case 0: // left edge
                    xStart = -GameConstants.PlayfieldSizeX;
                    yStart = (float)(random.NextDouble() * 2 - 1) * GameConstants.PlayfieldSizeY;
                    inward = new Vector3(1, 0, 0);
                    break;
                case 1: // right edge
                    xStart = GameConstants.PlayfieldSizeX;
                    yStart = (float)(random.NextDouble() * 2 - 1) * GameConstants.PlayfieldSizeY;
                    inward = new Vector3(-1, 0, 0);
                    break;
                case 2: // top edge
                    xStart = (float)(random.NextDouble() * 2 - 1) * GameConstants.PlayfieldSizeX;
                    yStart = GameConstants.PlayfieldSizeY;
                    inward = new Vector3(0, -1, 0);
                    break;
                default: // bottom edge
                    xStart = (float)(random.NextDouble() * 2 - 1) * GameConstants.PlayfieldSizeX;
                    yStart = -GameConstants.PlayfieldSizeY;
                    inward = new Vector3(0, 1, 0);
                    break;
            }

            asteroidList[i].position = new Vector3(xStart, yStart, 0.0f);

            // Rotate the inward normal by a random spread of ±60° so asteroids approach
            // at various angles but always move toward the center of the playfield.
            float spread = (float)((random.NextDouble() * 2 - 1) * Math.PI / 3);
            Vector3 dir = Vector3.Transform(inward, Matrix.CreateRotationZ(spread));
            asteroidList[i].direction.X = dir.X;
            asteroidList[i].direction.Y = dir.Y;
            asteroidList[i].speed = (GameConstants.AsteroidMinSpeed +
                (float)random.NextDouble() * GameConstants.AsteroidMaxSpeed) * currentSpeedMult;
            asteroidList[i].Size = AsteroidSize.Large;
            asteroidList[i].isActive = true;
            asteroidList[i].cracks = 0;
            asteroidList[i].cracksToBreak = cracksForSize(AsteroidSize.Large);
        }

        // Bullet hit: destroys the asteroid and spawns N fragments of the next size down.
        // Returns the score bonus earned.
        private int shrinkAsteroid(int i)
        {
            AsteroidSize currentSize = asteroidList[i].Size;
            particleEffect.UpdateSmoke(asteroidList[i].position, AsteroidScale(currentSize));

            if (currentSize == AsteroidSize.Small)
            {
                resetAster(i);
                return GameConstants.SmallAsteroidBonus;
            }

            AsteroidSize nextSize = (currentSize == AsteroidSize.Large) ? AsteroidSize.Medium : AsteroidSize.Small;
            int count   = (currentSize == AsteroidSize.Large) ? GameConstants.LargeAsteroidFragments : GameConstants.MediumAsteroidFragments;
            int bonus   = (currentSize == AsteroidSize.Large) ? GameConstants.LargeAsteroidBonus : GameConstants.MediumAsteroidBonus;

            SpawnFragments(i, asteroidList[i].position, asteroidList[i].direction, nextSize, count);
            return bonus;
        }

        // Marks sourceSlot inactive and spawns count fragments of size evenly spread around a circle.
        private void SpawnFragments(int sourceSlot, Vector3 pos, Vector3 sourceDir, AsteroidSize size, int count)
        {
            asteroidList[sourceSlot].isActive = false;

            float baseAngle = (float)Math.Atan2(sourceDir.Y, sourceDir.X);
            int spawned = 0;
            for (int j = 0; j < GameConstants.NumAsteroids && spawned < count; j++)
            {
                if (!asteroidList[j].isActive)
                {
                    float angle = baseAngle + (float)(spawned * 2 * Math.PI / count);
                    Vector3 dir = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0f);
                    initSplitAsteroid(j, pos + dir * GameConstants.AsteroidSplitSeparation, dir, size);
                    spawned++;
                }
            }
        }

        private int cracksForSize(AsteroidSize size)
        {
            switch (size)
            {
                case AsteroidSize.Large:  return random.Next(GameConstants.LargeCracksMin,  GameConstants.LargeCracksMax  + 1);
                case AsteroidSize.Medium: return random.Next(GameConstants.MediumCracksMin, GameConstants.MediumCracksMax + 1);
                default:                  return random.Next(GameConstants.SmallCracksMin,  GameConstants.SmallCracksMax  + 1);
            }
        }

        // Initialises a split fragment into an existing (inactive) slot.
        private void initSplitAsteroid(int slot, Vector3 position, Vector3 direction, AsteroidSize size)
        {
            asteroidList[slot].position = position;
            asteroidList[slot].direction = direction;
            asteroidList[slot].speed = (GameConstants.AsteroidMinSpeed +
                (float)random.NextDouble() * GameConstants.AsteroidMaxSpeed) * currentSpeedMult;
            asteroidList[slot].Size = size;
            asteroidList[slot].isActive = true;
            asteroidList[slot].cracks = 0;
            asteroidList[slot].cracksToBreak = cracksForSize(size);
            // Set correct bounds immediately — the collision loop may still be running this frame
            // and would otherwise see the stale default BoundingSphere at origin (radius 0),
            // which intersects with the ship (also at/near origin) and causes a false death.
            asteroidList[slot].bounds = new BoundingSphere(position,
                asteroidModel.Meshes[0].BoundingSphere.Radius *
                GameConstants.AsteroidBoundingSphereScale *
                AsteroidScale(size));
        }

        // Adds cracks to an asteroid; splits it when threshold is reached.
        private void crackAsteroid(int i, int cracks = 1)
        {
            asteroidList[i].cracks += cracks;
            if (asteroidList[i].cracks >= asteroidList[i].cracksToBreak)
                splitAsteroid(i);
        }

        private static int SizeRank(AsteroidSize size)
        {
            switch (size)
            {
                case AsteroidSize.Large:  return GameConstants.AsteroidSizeRankLarge;
                case AsteroidSize.Medium: return GameConstants.AsteroidSizeRankMedium;
                default:                  return GameConstants.AsteroidSizeRankSmall;
            }
        }

        // Cracks dealt by attacker to target: larger asteroids do more damage to smaller ones.
        // A smaller asteroid always deals at least 1 crack (a glancing blow).
        private static int CollisionCracks(AsteroidSize attacker, AsteroidSize target)
        {
            return Math.Max(1, SizeRank(attacker) - SizeRank(target) + 1);
        }

        // Splits an asteroid into N fragments of the next size down, spread evenly around a full circle.
        // Large → 3×Medium, Medium → 2×Small, Small → gone (respawn Large off-screen).
        private void splitAsteroid(int i)
        {
            AsteroidSize currentSize = asteroidList[i].Size;
            particleEffect.UpdateSmoke(asteroidList[i].position, AsteroidScale(currentSize));

            if (currentSize == AsteroidSize.Small)
            {
                resetAster(i);
                return;
            }

            AsteroidSize nextSize = (currentSize == AsteroidSize.Large) ? AsteroidSize.Medium : AsteroidSize.Small;
            int count = (currentSize == AsteroidSize.Large) ? GameConstants.LargeAsteroidFragments : GameConstants.MediumAsteroidFragments;

            SpawnFragments(i, asteroidList[i].position, asteroidList[i].direction, nextSize, count);
        }

        private static float AsteroidScale(AsteroidSize size)
        {
            switch (size)
            {
                case AsteroidSize.Medium: return GameConstants.MediumAsteroidScale;
                case AsteroidSize.Small:  return GameConstants.SmallAsteroidScale;
                default:                  return GameConstants.LargeAsteroidScale;
            }
        }

        protected override void updateGame(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (respawnInvincibleTimer > 0f)
                respawnInvincibleTimer = Math.Max(0f, respawnInvincibleTimer - timeDelta);

            // ── Difficulty progression ────────────────────────────────────────
            gameElapsed += timeDelta;
            float difficultyFactor = Math.Min(1f, gameElapsed / GameConstants.DifficultyRampDuration);
            currentSpeedMult = MathHelper.Lerp(GameConstants.InitialSpeedMultiplier, 1f, difficultyFactor);

            // Periodically spawn an asteroid if we are below the current target count
            int targetCount = (int)MathHelper.Lerp(
                GameConstants.NumInitialAsteroidsMin,
                GameConstants.NumInitialAsteroids,
                difficultyFactor);

            asteroidSpawnTimer -= timeDelta;
            if (asteroidSpawnTimer <= 0f)
            {
                asteroidSpawnTimer = GameConstants.AsteroidSpawnInterval;
                int activeCount = 0;
                for (int idx = 0; idx < GameConstants.NumAsteroids; idx++)
                    if (asteroidList[idx].isActive) activeCount++;

                if (activeCount < targetCount)
                {
                    for (int idx = 0; idx < GameConstants.NumAsteroids; idx++)
                    {
                        if (!asteroidList[idx].isActive) { resetAster(idx); break; }
                    }
                }
            }
            // ─────────────────────────────────────────────────────────────────

            //Update Input
            updateGameInput();

            // Add velocity to the current position.
            ship.Position += ship.Velocity;

            // Bleed off velocity over time
            ship.Velocity *= GameConstants.VelocityDamping;

            //Create a bounding sphere around the ship
            BoundingSphere shipSphere = new BoundingSphere(
                ship.Position, ship.Model.Meshes[0].BoundingSphere.Radius *
                GameConstants.ShipBoundingSphereScale);

            //Update bullets
            for (int i = 0; i < bulletList.Length; i++)
            {
                bulletList[i].Update(timeDelta);

                if (bulletList[i].isActive)
                {
                    BoundingSphere bulletSphere = new BoundingSphere(
                        bulletList[i].position,
                        bulletModel.Meshes[0].BoundingSphere.Radius);
                    bulletList[i].bounds = bulletSphere;
                    particleEffect.UpdateBulletFire(bulletList, i);
                }
            }

            //Update asteroids
            for (int i = 0; i < asteroidList.Length; i++)
            {
                if (!asteroidList[i].isActive) continue;
                asteroidList[i].Update(timeDelta);

                BoundingSphere b = new BoundingSphere(asteroidList[i].position,
                    asteroidModel.Meshes[0].BoundingSphere.Radius *
                    GameConstants.AsteroidBoundingSphereScale *
                    AsteroidScale(asteroidList[i].Size));
                asteroidList[i].bounds = b;
            }

            //Possible for collision check codes to be improved for efficiency (only check nearby asteroids)
            for (int i = 0; i < asteroidList.Length; i++)
            {
                if (!asteroidList[i].isActive) continue;

                //ship-asteroid collision check
                if (asteroidList[i].bounds.Intersects(shipSphere) && ship.isActive && respawnInvincibleTimer <= 0f)
                {
                    //blow up the ship
                    soundExplosion3.Play(GameConstants.ExplosionVolume, 0f, 0f);
                    particleEffect.UpdateExplosion(ship.Position);
                    ship.isActive = false;
                    ship.Position = Vector3.Zero;
                    ship.lives--;
                    score = Math.Max(0, score - GameConstants.DeathPenalty);
                    break; //exit the loop
                }

                float dist;
                //bullet-asteroid collision check
                for (int j = 0; j < bulletList.Length; j++)
                {
                    if (bulletList[j].isActive)
                    {
                        if (asteroidList[i].bounds.Intersects(bulletList[j].bounds))
                        {
                            dist = GameConstants.ExplosionDistanceFactor / (float)Math.Sqrt((bulletList[j].position.X * bulletList[j].position.X) +
                                (bulletList[j].position.Y * bulletList[j].position.Y));

                            if (dist < GameConstants.ExplosionSoundThreshold)
                                soundExplosion1.Play(dist * GameConstants.ExplosionVolume, 0.2f, 0.3f);

                            particleEffect.UpdateExplosion(asteroidList[i].position);
                            bulletList[j].isActive = false;
                            score += shrinkAsteroid(i);

                            break; //no need to check the other bullets
                        }
                    }
                }

                //asteroid-asteroid collision check
                Vector3 tmpPos = new Vector3();
                for (int j = i + 1; j < asteroidList.Length; j++)
                {
                    if (!asteroidList[j].isActive) continue;
                    if (asteroidList[i].bounds.Intersects(asteroidList[j].bounds))
                    {
                        tmpPos = (asteroidList[i].position + asteroidList[j].position) / 2;

                        // Reverse spin on collision (visual only)
                        asteroidList[i].rotation *= -1;
                        asteroidList[j].rotation *= -1;

                        // Elastic collision: conserve momentum and kinetic energy.
                        // Mass is proportional to visual scale so large asteroids
                        // have more inertia and deflect smaller ones more strongly.
                        Vector3 vi = asteroidList[i].direction * asteroidList[i].speed;
                        Vector3 vj = asteroidList[j].direction * asteroidList[j].speed;
                        float   mi = AsteroidScale(asteroidList[i].Size);
                        float   mj = AsteroidScale(asteroidList[j].Size);

                        // Unit normal pointing from i toward j along the collision axis
                        Vector3 collNormal = asteroidList[j].position - asteroidList[i].position;
                        float   separation = collNormal.Length();
                        collNormal = (separation > 0f) ? collNormal / separation : Vector3.UnitX;

                        // Only resolve if the asteroids are still approaching
                        float relVn = Vector3.Dot(vi - vj, collNormal);
                        if (relVn > 0f)
                        {
                            float impulse = 2f * mi * mj / (mi + mj) * relVn;
                            vi -= (impulse / mi) * collNormal;
                            vj += (impulse / mj) * collNormal;

                            vi.Z = 0f; vj.Z = 0f; // keep motion in the XY plane

                            float newSpeedI = vi.Length();
                            float newSpeedJ = vj.Length();

                            asteroidList[i].speed = Math.Max(GameConstants.AsteroidMinSpeed, newSpeedI);
                            asteroidList[j].speed = Math.Max(GameConstants.AsteroidMinSpeed, newSpeedJ);

                            if (newSpeedI > 0f) asteroidList[i].direction = vi / newSpeedI;
                            if (newSpeedJ > 0f) asteroidList[j].direction = vj / newSpeedJ;
                        }

                        dist = GameConstants.ExplosionDistanceFactor / (float)Math.Sqrt((tmpPos.X * tmpPos.X) + (tmpPos.Y * tmpPos.Y));
                        if (asteroidList[i].position.X < visibleHalfX &&
                            asteroidList[i].position.X > -visibleHalfX &&
                            asteroidList[i].position.Y < visibleHalfY &&
                            asteroidList[i].position.Y > -visibleHalfY)
                        {
                            float smokeScale = Math.Max(
                                AsteroidScale(asteroidList[i].Size),
                                AsteroidScale(asteroidList[j].Size));
                            particleEffect.UpdateSmoke(tmpPos, smokeScale);
                            if (dist < GameConstants.ExplosionSoundThreshold)
                                soundExplosion2.Play(dist * GameConstants.ExplosionVolume, 0.0f, 0.0f);
                        }

                        // Damage is proportional to the attacker's size relative to the target
                        crackAsteroid(i, CollisionCracks(asteroidList[j].Size, asteroidList[i].Size));
                        crackAsteroid(j, CollisionCracks(asteroidList[i].Size, asteroidList[j].Size));

                        break; //no need to check against other asteroids
                    }
                }
            }
        }

        protected override void updateGameInput()
        {
            // Get the keyboard state
            KeyboardState currentKeyState = Keyboard.GetState();
            GamePadState currentGamePadSate = GamePad.GetState(PlayerIndex.One);

            //Check if the ship is active
            if (ship.isActive)
            {
                ship.Update(currentGamePadSate, currentKeyState);

                //Play engine sound only when the engine is on.
                if (currentGamePadSate.Triggers.Right > 0 || currentKeyState.IsKeyDown(Keys.W) || currentKeyState.IsKeyDown(Keys.Up))
                {
                    particleEffect.UpdateThrusterFire(ship);

                    if (soundEngineInstance.State == SoundState.Stopped)
                    {
                        soundEngineInstance.Volume = GameConstants.EngineVolume;
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

                //are we shooting?
                if ((currentGamePadSate.Buttons.A == ButtonState.Pressed && lastGState.Buttons.A == ButtonState.Released) ||
                    ((currentKeyState.IsKeyDown(Keys.Space)) && lastKState.IsKeyUp(Keys.Space)))
                {
                    //add another bullet. Find an inactive bullet slot and use it
                    //if all bullet slots are used, ignore the user input
                    for (int i = 0; i < GameConstants.NumBullets; i++)
                    {
                        if (!bulletList[i].isActive)
                        {
                            bulletList[i].direction = ship.RotationMatrix.Forward;
                            bulletList[i].speed = GameConstants.BulletSpeedAdjustment +
                                (float)Math.Sqrt((ship.Velocity.X * ship.Velocity.X) +
                                    (ship.Velocity.Y * ship.Velocity.Y) +
                                    (ship.Velocity.Z * ship.Velocity.Z));
                            bulletList[i].position = ship.Position + (GameConstants.BulletSpawnOffset * bulletList[i].direction);
                            bulletList[i].isActive = true;
                            soundWeaponsFire.Play(GameConstants.WeaponVolume, 0f, 0f);
                            score = Math.Max(0, score - GameConstants.ShotPenalty);
                            break; //exit loop
                        }
                    }
                }
            }
            else
            {
                //Stop the sound if the ship is in active/dead
                soundEngineInstance.Pause();
            }

            // In case you get lost, press Start/Home to warp back to the center.
            if (((currentGamePadSate.Buttons.Start == ButtonState.Pressed && lastGState.Buttons.Start == ButtonState.Released) ||
                (currentKeyState.IsKeyDown(Keys.Home) && lastKState.IsKeyUp(Keys.Home))) && ship.lives > 0)
            {
                ship.Position = Vector3.Zero;
                ship.Velocity = Vector3.Zero;
                ship.Rotation = 0.0f;
                soundHyperspaceActivation.Play(GameConstants.HyperspaceVolume, 0f, 0f);
                score = Math.Max(0, score - GameConstants.WarpPenalty);
                ship.isActive = true;
                respawnInvincibleTimer = GameConstants.RespawnInvincibilityDuration;
            }

            lastGState = currentGamePadSate;
            lastKState = currentKeyState;
        }

        // Space-aware helpers — splits on ' ' and inserts _spaceWidth manually.
        float MeasureSpHud(string text, float scale = 1f)
        {
            float w = 0f;
            int segStart = 0;
            for (int i = 0; i <= text.Length; i++)
            {
                if (i == text.Length || text[i] == ' ')
                {
                    if (i > segStart)
                        w += kootenay.MeasureString(text.Substring(segStart, i - segStart)).X;
                    if (i < text.Length) w += _spaceWidth;
                    segStart = i + 1;
                }
            }
            return w * scale;
        }

        void DrawSpHud(string text, Vector2 pos, Color color, float scale = 1f)
        {
            float curX = pos.X;
            int segStart = 0;
            for (int i = 0; i <= text.Length; i++)
            {
                if (i == text.Length || text[i] == ' ')
                {
                    if (i > segStart)
                    {
                        string seg = text.Substring(segStart, i - segStart);
                        if (scale == 1f)
                            spriteBatch.DrawString(kootenay, seg, new Vector2(curX, pos.Y), color);
                        else
                            spriteBatch.DrawString(kootenay, seg, new Vector2(curX, pos.Y), color,
                                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                        curX += kootenay.MeasureString(seg).X * scale;
                    }
                    if (i < text.Length) curX += _spaceWidth * scale;
                    segStart = i + 1;
                }
            }
        }

        void DrawHudCentered(string text, float y, Color color, float scale = 1f)
        {
            float x = (game.GraphicsDevice.Viewport.Width - MeasureSpHud(text, scale)) / 2f;
            DrawSpHud(text, new Vector2(x, y), color, scale);
        }

        public override void drawGame(GraphicsDevice graphics)
        {
            graphics.BlendState = BlendState.Opaque;

            //Draw the ship — flash at ~8 Hz during invincibility window
            Matrix shipTransformMatrix = ship.RotationMatrix
                * Matrix.CreateTranslation(ship.Position);

            bool shipVisible = ship.isActive &&
                (respawnInvincibleTimer <= 0f || (int)(respawnInvincibleTimer * GameConstants.RespawnFlashRate) % 2 == 0);
            if (shipVisible)
                DrawModel(ship.Model, shipTransformMatrix, ship.Transforms);

            //Draw the asteroids with crack damage tint (orange-red glow builds as cracks increase)
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                if (!asteroidList[i].isActive) continue;

                float crackRatio = asteroidList[i].cracksToBreak > 0
                    ? (float)asteroidList[i].cracks / asteroidList[i].cracksToBreak
                    : 0f;
                foreach (ModelMesh mesh in asteroidModel.Meshes)
                    foreach (BasicEffect effect in mesh.Effects)
                        effect.EmissiveColor = new Vector3(crackRatio * 0.7f, crackRatio * 0.15f, 0f);

                float scale = AsteroidScale(asteroidList[i].Size);
                Matrix asteroidTransform = Matrix.CreateScale(scale)
                    * asteroidList[i].RotationMatrix
                    * Matrix.CreateTranslation(asteroidList[i].position);
                DrawModel(asteroidModel, asteroidTransform, asteroidTransforms);
            }
            // Reset emissive so other models are unaffected
            foreach (ModelMesh mesh in asteroidModel.Meshes)
                foreach (BasicEffect effect in mesh.Effects)
                    effect.EmissiveColor = Vector3.Zero;

            //Draw the bullets
            for (int i = 0; i < GameConstants.NumBullets; i++)
            {
                if (bulletList[i].isActive)
                {
                    Matrix bulletTransform =
                        Matrix.CreateTranslation(bulletList[i].position);
                    DrawModel(bulletModel, bulletTransform, bulletTransforms);
                }
            }

        }

        // Drawn after particles so text always appears on top.
        public void drawHUD()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicClamp);
            DrawSpHud(GameConstants.UiHudScore + score, scorePosition, Color.LightGreen);
            DrawSpHud(GameConstants.UiHudLives + ship.lives, livesPosition, Color.LightGreen);

            if (!ship.isActive && ship.lives > 0)
            {
                DrawHudCentered(GameConstants.UiHudShipDestroyed, 255f, Color.Red, GameConstants.UiSubtitleScale);
                DrawHudCentered(GameConstants.UiHudRespawn, 300f, Color.White);
            }
            spriteBatch.End();
        }

    }
}
