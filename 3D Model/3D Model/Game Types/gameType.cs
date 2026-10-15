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
    abstract class gameType
    {
        //Simple checking condition to see if the game has been initialized
        private bool initialized = false;

        abstract protected void Initialize(Game game, GraphicsDevice graphics, Matrix projectionMatrix, Matrix viewMatrix);
        abstract protected void updateGame(GameTime gameTime);
        abstract protected void updateGameInput();
        abstract public void drawGame(GraphicsDevice graphics);

        public void RunGame(Game game, GameTime gameTime, GraphicsDevice graphics, Matrix projectionMatrix, Matrix viewMatrix)
        {
            if (!initialized)
            {
                Initialize(game, graphics, projectionMatrix, viewMatrix);
                initialized = true;
            }
            updateGame(gameTime);
        }

        public static void DrawModel(Model model, Matrix modelTransform,
            Matrix[] absoluteBoneTransforms)
        {

            //Draw the model, a model can have multiple meshes so loop
            foreach (ModelMesh mesh in model.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World =
                        absoluteBoneTransforms[mesh.ParentBone.Index] *
                        modelTransform;
                }
                //Draw the mesh, will use the effects set above
                mesh.Draw();
            }
        }

        public Matrix[] SetupEffectDefaults(Model myModel, Matrix projectionMatrix, Matrix viewMatrix)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(1.0f, 1.0f, 1.0f);
                    effect.DirectionalLight0.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
                    effect.DirectionalLight1.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);
                    effect.Projection = projectionMatrix;
                    effect.View = viewMatrix;
                }
            }
            return absoluteTransforms;
        }

        /*protected void UpdateGameInput()
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
                    UpdateThrusterFire();

                    if (soundEngineInstance.State == SoundState.Stopped)
                    {
                        soundEngineInstance.Volume = 0.99f;
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
                            soundWeaponsFire.Play();
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
                soundHyperspaceActivation.Play();
                score -= GameConstants.WarpPenalty;
                ship.isActive = true;
            }

            lastGState = currentGamePadSate;
            lastKState = currentKeyState;

            //Console.WriteLine(ship.Position);
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
                    soundExplosion3.Play();
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
                                soundExplosion1.Play(dist, 0.2f, 0.3f);

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
                        if (asteroidList[i].position.X < GameConstants.PlayfieldSizeX_min &&
                            asteroidList[i].position.X > -GameConstants.PlayfieldSizeX_min &&
                            asteroidList[i].position.Y < GameConstants.PlayfieldSizeY_min &&
                            asteroidList[i].position.Y > -GameConstants.PlayfieldSizeY_min)
                        {
                            UpdateSmoke(tmpPos);
                            if (dist < 1.0f)
                                soundExplosion2.Play(dist, 0.0f, 0.0f);
                        }

                        break; //no need to check against other asteroids
                    }
                }
            }
        }*/
    }
}
