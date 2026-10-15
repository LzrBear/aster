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

using Particle3DSample;

namespace Asteroids
{
    class particleEffects
    {
        //Particle Effects
        ParticleSystem bulletParticles;
        ParticleSystem thrusterParticles;
        ParticleSystem explosionParticles;
        ParticleSystem sparksParticles;
        ParticleSystem smokeParticles;
        ParticleSystem starsParticles;

        Random random = new Random();

        public void initialize(Game game)
        {
            //Thurster particle effect
            thrusterParticles = new thrusterParticleSystem(game, game.Content);
            thrusterParticles.DrawOrder = 500;
            game.Components.Add(thrusterParticles);

            //Bullet particle effect
            bulletParticles = new bulletParticleSystem(game, game.Content);
            bulletParticles.DrawOrder = 400;
            game.Components.Add(bulletParticles);

            //Explosion Particle effect
            explosionParticles = new explosionParticleSystem(game, game.Content);
            explosionParticles.DrawOrder = 300;
            game.Components.Add(explosionParticles);

            //Explosion Sparks particle effect
            sparksParticles = new sparksParticleSystem(game, game.Content);
            sparksParticles.DrawOrder = 100;
            game.Components.Add(sparksParticles);

            //Explosion Smoke particle effect
            smokeParticles = new smokeParticleSystem(game, game.Content);
            smokeParticles.DrawOrder = 200;
            game.Components.Add(smokeParticles);

            //Star Particles
            starsParticles = new starsParticleSystem(game, game.Content);
            starsParticles.DrawOrder = 600;
            game.Components.Add(starsParticles);
        }

        public void draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            thrusterParticles.SetCamera(viewMatrix, projectionMatrix);
            bulletParticles.SetCamera(viewMatrix, projectionMatrix);
            explosionParticles.SetCamera(viewMatrix, projectionMatrix);
            sparksParticles.SetCamera(viewMatrix, projectionMatrix);
            smokeParticles.SetCamera(viewMatrix, projectionMatrix);
            starsParticles.SetCamera(viewMatrix, projectionMatrix);
        }

        Vector3 tmp = new Vector3();
        public void UpdateThrusterFire(Ship ship)
        {
            //We just create one new flame particle per frame
            tmp = new Vector3((float)(Math.Sin(ship.Rotation) * 900 + ship.Position.X), (float)(-Math.Cos(ship.Rotation) * 900 + ship.Position.Y), ship.Position.Z);
            thrusterParticles.AddParticle(tmp, Vector3.Zero);
        }

        public void UpdateBulletFire(Bullet[] bulletList, int i)
        {
            bulletParticles.AddParticle(bulletList[i].position, Vector3.Zero);
        }

        public void UpdateExplosion(Vector3 collisionLoc)
        {
            int k = 10;
            while (k > 0)
            {
                explosionParticles.AddParticle(collisionLoc, Vector3.Zero);
                k--;
            }

            k = 35;
            while (k > 0)
            {
                tmp = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
                sparksParticles.AddParticle(collisionLoc, tmp);
                k--;
            }

            k = 16;
            while (k > 0)
            {
                tmp = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
                smokeParticles.AddParticle(collisionLoc, tmp);
                k--;
            }
        }

        // sizeScale = AsteroidScale(size): 1.0 Large, 0.5 Medium, 0.25 Small.
        // Particle counts grow proportionally so larger asteroids produce a
        // cloud big enough to hide the mesh being removed.
        public void UpdateSmoke(Vector3 collisionLoc, float sizeScale = 1f)
        {
            int smokeCount  = Math.Max(GameConstants.SmokeParticleMin,  (int)(sizeScale * GameConstants.SmokeParticleCountScale));
            int sparksCount = Math.Max(GameConstants.SparksParticleMin, (int)(sizeScale * GameConstants.SparksParticleCountScale));

            for (int k = smokeCount; k > 0; k--)
            {
                tmp = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
                smokeParticles.AddParticle(collisionLoc, tmp);
            }
            for (int k = sparksCount; k > 0; k--)
            {
                tmp = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
                sparksParticles.AddParticle(collisionLoc, tmp);
            }
        }

        public void UpdateStars()
        {
            int k = GameConstants.numStars;
            while (k > 0)
            {
                if (random.Next(2) == 0)
                    tmp.X = (float)random.NextDouble() * -GameConstants.PlayfieldSizeX;
                else
                    tmp.X = (float)random.NextDouble() * GameConstants.PlayfieldSizeX;

                if (random.Next(2) == 0)
                    tmp.Y = (float)random.NextDouble() * -GameConstants.PlayfieldSizeY;
                else
                    tmp.Y = (float)random.NextDouble() * GameConstants.PlayfieldSizeY;

                tmp.Z = -2000.0f;

                starsParticles.AddParticle(tmp, Vector3.Zero);
                k--;
            }
        }
    }
}
