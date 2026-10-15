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
            }
            ResetAsteroids();

            //Initialize the bullets
            for (int i = 0; i < GameConstants.NumBullets; i++)
            {
                bulletList[i] = new Bullet();
            }

            //Load the fonts
            kootenay = game.Content.Load<SpriteFont>("Fonts/Lucida Console");

            //Initialize the particle Effects
            particleEffect.initialize(game);

            //Initialize the models
            ship.Model = game.Content.Load<Model>("Models/p1_wedge");
            ship.Transforms = SetupEffectDefaults(ship.Model, projectionMatrix, viewMatrix);

            asteroidModel = game.Content.Load<Model>("Models/asteroid1");
            asteroidTransforms = SetupEffectDefaults(asteroidModel, projectionMatrix, viewMatrix);

            bulletModel = game.Content.Load<Model>("Models/pea_proj");
            bulletTransforms = SetupEffectDefaults(bulletModel, projectionMatrix, viewMatrix);

            //Initialize the Sound Effects
            soundEngine = game.Content.Load<SoundEffect>("Audio/Waves/engine_2");
            soundEngineInstance = soundEngine.CreateInstance();
            soundHyperspaceActivation =
                game.Content.Load<SoundEffect>("Audio/Waves/hyperspace_activate");
            soundExplosion1 =
                game.Content.Load<SoundEffect>("Audio/Waves/explosion1");
            soundExplosion2 =
                game.Content.Load<SoundEffect>("Audio/Waves/explosion2");
            soundExplosion3 =
                game.Content.Load<SoundEffect>("Audio/Waves/explosion3");
            soundWeaponsFire =
                game.Content.Load<SoundEffect>("Audio/Waves/tx0_fire1");

        }

        //Resets all the asteroids in the asteroid list;
        public void ResetAsteroids()
        {
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                resetAster(i);
            }
        }

        public void resetAster(int i)
        {
            float xStart = 0.0f;
            float yStart = 0.0f;
            if (random.Next(2) == 0)
            {
                // Spawn on left or right edge (fully off-screen)
                xStart = random.Next(2) == 0 ? -GameConstants.PlayfieldSizeX : GameConstants.PlayfieldSizeX;
                yStart = (float)(random.NextDouble() * 2 - 1) * GameConstants.PlayfieldSizeY;
            }
            else
            {
                // Spawn on top or bottom edge (fully off-screen)
                yStart = random.Next(2) == 0 ? -GameConstants.PlayfieldSizeY : GameConstants.PlayfieldSizeY;
                xStart = (float)(random.NextDouble() * 2 - 1) * GameConstants.PlayfieldSizeX;
            }

            asteroidList[i].position = new Vector3(xStart, yStart, 0.0f);

            // Pick a direction that points generally toward the visible play area.
            // Restrict the inward half-circle based on which edge the asteroid spawned on.
            double angle;
            if (Math.Abs(xStart) >= Math.Abs(yStart))
            {
                // Spawned on left/right edge — must travel inward (X toward zero)
                // Right edge: direction.X = -sin(angle) < 0  →  sin(angle) > 0  →  angle in (0, π)
                // Left  edge: direction.X = -sin(angle) > 0  →  sin(angle) < 0  →  angle in (π, 2π)
                if (xStart > 0)
                    angle = random.NextDouble() * Math.PI;           // 0..π  → X moves left
                else
                    angle = Math.PI + random.NextDouble() * Math.PI; // π..2π → X moves right
            }
            else
            {
                // Spawned on top/bottom edge — must travel inward (Y toward zero)
                // Top  edge (yStart > 0): cos(angle) < 0 → angle in (π/2, 3π/2)
                // Bottom edge (yStart < 0): cos(angle) > 0 → angle in (-π/2, π/2) = (3π/2..2π) ∪ (0..π/2)
                if (yStart > 0)
                    angle = Math.PI / 2 + random.NextDouble() * Math.PI;   // π/2..3π/2 → Y moves down
                else
                {
                    // (3π/2..2π) or (0..π/2) — pick uniformly from the inward half
                    angle = random.NextDouble() < 0.5
                        ? random.NextDouble() * Math.PI / 2                  // 0..π/2
                        : 3 * Math.PI / 2 + random.NextDouble() * Math.PI / 2; // 3π/2..2π
                }
            }
            asteroidList[i].direction.X = -(float)Math.Sin(angle);
            asteroidList[i].direction.Y = (float)Math.Cos(angle);
            asteroidList[i].speed = GameConstants.AsteroidMinSpeed +
                (float)random.NextDouble() * GameConstants.AsteroidMaxSpeed;
        }

        protected override void updateGame(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Update Input
            updateGameInput();

            // Add velocity to the current position.
            ship.Position += ship.Velocity;

            // Bleed off velocity over time
            ship.Velocity *= 0.99f;

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
                asteroidList[i].Update(timeDelta);

                //Ship-asteroid collision check
                BoundingSphere b = new BoundingSphere(asteroidList[i].position,
                    asteroidModel.Meshes[0].BoundingSphere.Radius *
                    GameConstants.AsteroidBoundingSphereScale);
                asteroidList[i].bounds = b;
            }

            //Possible for collision check codes to be improved for efficiency (only check nearby asteroids)
            for (int i = 0; i < asteroidList.Length; i++)
            {
                //ship-asteroid collision check
                if (asteroidList[i].bounds.Intersects(shipSphere) && ship.isActive)
                {
                    //blow up the ship
                    soundExplosion3.Play(GameConstants.ExplosionVolume, 0f, 0f);
                    particleEffect.UpdateExplosion(ship.Position);
                    ship.isActive = false;
                    ship.Position = Vector3.Zero;
                    ship.lives--;
                    //resetAster(i);
                    score -= GameConstants.DeathPenalty;
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
                            dist = 2000 / (float)Math.Sqrt((bulletList[j].position.X * bulletList[j].position.X) +
                                (bulletList[j].position.Y * bulletList[j].position.Y));

                            if (dist < 1.0f)
                                soundExplosion1.Play(dist * GameConstants.ExplosionVolume, 0.2f, 0.3f);

                            particleEffect.UpdateExplosion(asteroidList[i].position);

                            asteroidList[i].isActive = false;
                            bulletList[j].isActive = false;

                            resetAster(i);
                            score += GameConstants.KillBonus;

                            break; //no need to check the other bullets
                        }
                    }
                }

                //asteroid-asteroid collision check
                Vector3 tmpPos = new Vector3();
                for (int j = i + 1; j < asteroidList.Length; j++)
                {
                    if (asteroidList[i].bounds.Intersects(asteroidList[j].bounds))
                    {
                        tmpPos = (asteroidList[i].position + asteroidList[j].position) / 2;

                        //After a collision reverse the rotation
                        asteroidList[i].rotation *= -1;
                        asteroidList[j].rotation *= -1;

                        //Determine the new direction for the asteroids
                        Vector3 Vni = new Vector3();
                        Vector3 Vnpi = new Vector3();

                        Vector3 Vnj = new Vector3();
                        Vector3 Vnpj = new Vector3();

                        //Determmine the normal vector for i
                        Vni = asteroidList[j].position - asteroidList[i].position;
                        Vni.Normalize();
                        Vnpi = asteroidList[i].direction - Vni;
                        Vnpi.Normalize();

                        asteroidList[i].direction = -Vni + Vnpi;
                        asteroidList[i].direction.Normalize();

                        //Determine the normal vector for j
                        Vnj = asteroidList[i].position - asteroidList[j].position;
                        Vnj.Normalize();
                        Vnpj = asteroidList[j].direction - Vnj;
                        Vnpj.Normalize();

                        asteroidList[j].direction = -Vnj + Vnpj;
                        asteroidList[j].direction.Normalize();

                        //I think asteroid collision noise is a bit over the top
                        //maybe I need a better sound effect?
                        dist = 2000 / (float)Math.Sqrt((tmpPos.X * tmpPos.X) + (tmpPos.Y * tmpPos.Y));
                        if (asteroidList[i].position.X < visibleHalfX &&
                            asteroidList[i].position.X > -visibleHalfX &&
                            asteroidList[i].position.Y < visibleHalfY &&
                            asteroidList[i].position.Y > -visibleHalfY)
                        {
                            particleEffect.UpdateSmoke(tmpPos);
                            if (dist < 1.0f)
                                soundExplosion2.Play(dist * GameConstants.ExplosionVolume, 0.0f, 0.0f);
                        }

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
                            bulletList[i].position = ship.Position + (1000 * bulletList[i].direction);
                            bulletList[i].isActive = true;
                            soundWeaponsFire.Play(GameConstants.WeaponVolume, 0f, 0f);
                            score -= GameConstants.ShotPenalty;
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
                score -= GameConstants.WarpPenalty;
                ship.isActive = true;
            }

            lastGState = currentGamePadSate;
            lastKState = currentKeyState;
        }

        public override void drawGame(GraphicsDevice graphics)
        {
            graphics.BlendState = BlendState.Opaque;

            //Draw the ship
            Matrix shipTransformMatrix = ship.RotationMatrix
                * Matrix.CreateTranslation(ship.Position);

            if (ship.isActive)
                DrawModel(ship.Model, shipTransformMatrix, ship.Transforms);

            //Draw the asteroids
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                Matrix asteroidTransform = asteroidList[i].RotationMatrix *
                    Matrix.CreateTranslation(asteroidList[i].position);
                DrawModel(asteroidModel, asteroidTransform, asteroidTransforms);
            }

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

            //Draw the particle effects
            particleEffect.draw(viewMatrix, projectionMatrix);

            //Draw the score
            spriteBatch.Begin();
            spriteBatch.DrawString(kootenay, "Score: " + score,
                                   scorePosition, Color.LightGreen);
            spriteBatch.DrawString(kootenay, "Lives: " + ship.lives,
                                   livesPosition, Color.LightGreen);
            spriteBatch.End();
        }

        /*protected void RunGame(float timeDelta, Ship ship, Bullet[] bulletList, Asteroid[] asteroidList)
        {
            //Set the game state to running
            gameRunning = true;

            // Get some input
            UpdateGameInput();

            // Add velocity to the current position.
            ship.Position += ship.Velocity;

            // Bleed off velocity over time
            ship.Velocity *= 0.99f;

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
                    UpdateBulletFire(i);
                }
            }

            //Update asteroids
            for (int i = 0; i < asteroidList.Length; i++)
            {
                asteroidList[i].Update(timeDelta);

                //Ship-asteroid collision check
                BoundingSphere b = new BoundingSphere(asteroidList[i].position,
                    asteroidModel.Meshes[0].BoundingSphere.Radius *
                    GameConstants.AsteroidBoundingSphereScale);
                asteroidList[i].bounds = b;
            }

            //Possible for collision check codes to be improved for efficiency (only check nearby asteroids)
            for (int i = 0; i < asteroidList.Length; i++)
            {
                //ship-asteroid collision check
                if (asteroidList[i].bounds.Intersects(shipSphere) && ship.isActive)
                {
                    //blow up the ship
                    soundExplosion3.Play(GameConstants.ExplosionVolume, 0f, 0f);
                    UpdateExplosion(ship.Position);
                    ship.isActive = false;
                    ship.lives--;
                    //resetAster(i);
                    score -= GameConstants.DeathPenalty;
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
                            dist = 2000 / (float)Math.Sqrt((bulletList[j].position.X * bulletList[j].position.X) +
                                (bulletList[j].position.Y * bulletList[j].position.Y));

                            if (dist < 1.0f)
                                soundExplosion1.Play(dist * GameConstants.ExplosionVolume, 0.2f, 0.3f);

                            UpdateExplosion(asteroidList[i].position);

                            asteroidList[i].isActive = false;
                            bulletList[j].isActive = false;

                            resetAster(i);
                            score += GameConstants.KillBonus;

                            break; //no need to check the other bullets
                        }
                    }
                }

                //asteroid-asteroid collision check
                Vector3 tmpPos = new Vector3();
                for (int j = i + 1; j < asteroidList.Length; j++)
                {
                    if (asteroidList[i].bounds.Intersects(asteroidList[j].bounds))
                    {
                        tmpPos = (asteroidList[i].position + asteroidList[j].position) / 2;

                        //After a collision reverse the rotation
                        asteroidList[i].rotation *= -1;
                        asteroidList[j].rotation *= -1;

                        //Determine the new direction for the asteroids
                        Vector3 Vni = new Vector3();
                        Vector3 Vnpi = new Vector3();

                        Vector3 Vnj = new Vector3();
                        Vector3 Vnpj = new Vector3();

                        //Determmine the normal vector for i
                        Vni = asteroidList[j].position - asteroidList[i].position;
                        Vni.Normalize();
                        Vnpi = asteroidList[i].direction - Vni;
                        Vnpi.Normalize();

                        asteroidList[i].direction = -Vni + Vnpi;
                        asteroidList[i].direction.Normalize();

                        //Determine the normal vector for j
                        Vnj = asteroidList[i].position - asteroidList[j].position;
                        Vnj.Normalize();
                        Vnpj = asteroidList[j].direction - Vnj;
                        Vnpj.Normalize();

                        asteroidList[j].direction = -Vnj + Vnpj;
                        asteroidList[j].direction.Normalize();

                        //I think asteroid collision noise is a bit over the top
                        //maybe I need a better sound effect?
                        dist = 2000 / (float)Math.Sqrt((tmpPos.X * tmpPos.X) + (tmpPos.Y * tmpPos.Y));
                        if (asteroidList[i].position.X < visibleHalfX &&
                            asteroidList[i].position.X > -visibleHalfX &&
                            asteroidList[i].position.Y < visibleHalfY &&
                            asteroidList[i].position.Y > -visibleHalfY)
                        {
                            UpdateSmoke(tmpPos);
                            if (dist < 1.0f)
                                soundExplosion2.Play(dist * GameConstants.ExplosionVolume, 0.0f, 0.0f);
                        }

                        break; //no need to check against other asteroids
                    }
                }
        }

        //{
        //}

        //public void RunGame()
        //}

        /*protected void RunGame(float timeDelta, Ship ship, Bullet[] bulletList, Asteroid[] asteroidList, Model bulletModel)
        {

            // Get some input
            UpdateGameInput();

            // Add velocity to the current position.
            ship.Position += ship.Velocity;

            // Bleed off velocity over time
            ship.Velocity *= 0.99f;

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
                    UpdateBulletFire(i);
                }
            }

            //Update asteroids
            for (int i = 0; i < asteroidList.Length; i++)
            {
                asteroidList[i].Update(timeDelta);

                //Ship-asteroid collision check
                BoundingSphere b = new BoundingSphere(asteroidList[i].position,
                    asteroidModel.Meshes[0].BoundingSphere.Radius *
                    GameConstants.AsteroidBoundingSphereScale);
                asteroidList[i].bounds = b;
            }

            //Possible for collision check codes to be improved for efficiency (only check nearby asteroids)
            for (int i = 0; i < asteroidList.Length; i++)
            {
                //ship-asteroid collision check
                if (asteroidList[i].bounds.Intersects(shipSphere) && ship.isActive)
                {
                    //blow up the ship
                    soundExplosion3.Play(GameConstants.ExplosionVolume, 0f, 0f);
                    UpdateExplosion(ship.Position);
                    ship.isActive = false;
                    ship.lives--;
                    //resetAster(i);
                    score -= GameConstants.DeathPenalty;
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
                            dist = 2000 / (float)Math.Sqrt((bulletList[j].position.X * bulletList[j].position.X) +
                                (bulletList[j].position.Y * bulletList[j].position.Y));

                            if (dist < 1.0f)
                                soundExplosion1.Play(dist * GameConstants.ExplosionVolume, 0.2f, 0.3f);

                            UpdateExplosion(asteroidList[i].position);

                            asteroidList[i].isActive = false;
                            bulletList[j].isActive = false;

                            resetAster(i);
                            score += GameConstants.KillBonus;

                            break; //no need to check the other bullets
                        }
                    }
                }

                //asteroid-asteroid collision check
                Vector3 tmpPos = new Vector3();
                for (int j = i + 1; j < asteroidList.Length; j++)
                {
                    if (asteroidList[i].bounds.Intersects(asteroidList[j].bounds))
                    {
                        tmpPos = (asteroidList[i].position + asteroidList[j].position) / 2;

                        //After a collision reverse the rotation
                        asteroidList[i].rotation *= -1;
                        asteroidList[j].rotation *= -1;

                        //Determine the new direction for the asteroids
                        Vector3 Vni = new Vector3();
                        Vector3 Vnpi = new Vector3();

                        Vector3 Vnj = new Vector3();
                        Vector3 Vnpj = new Vector3();

                        //Determmine the normal vector for i
                        Vni = asteroidList[j].position - asteroidList[i].position;
                        Vni.Normalize();
                        Vnpi = asteroidList[i].direction - Vni;
                        Vnpi.Normalize();

                        asteroidList[i].direction = -Vni + Vnpi;
                        asteroidList[i].direction.Normalize();

                        //Determine the normal vector for j
                        Vnj = asteroidList[i].position - asteroidList[j].position;
                        Vnj.Normalize();
                        Vnpj = asteroidList[j].direction - Vnj;
                        Vnpj.Normalize();

                        asteroidList[j].direction = -Vnj + Vnpj;
                        asteroidList[j].direction.Normalize();

                        //I think asteroid collision noise is a bit over the top
                        //maybe I need a better sound effect?
                        dist = 2000 / (float)Math.Sqrt((tmpPos.X * tmpPos.X) + (tmpPos.Y * tmpPos.Y));
                        if (asteroidList[i].position.X < visibleHalfX &&
                            asteroidList[i].position.X > -visibleHalfX &&
                            asteroidList[i].position.Y < visibleHalfY &&
                            asteroidList[i].position.Y > -visibleHalfY)
                        {
                            UpdateSmoke(tmpPos);
                            if (dist < 1.0f)
                                soundExplosion2.Play(dist * GameConstants.ExplosionVolume, 0.0f, 0.0f);
                        }

                        break; //no need to check against other asteroids
                    }
                }
            }
        }*/
    }
}
