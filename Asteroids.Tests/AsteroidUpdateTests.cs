using Asteroids;
using Microsoft.Xna.Framework;
using Xunit;

namespace Asteroids.Tests
{
    public class AsteroidUpdateTests
    {
        [Fact]
        public void Update_AdvancesPositionInDirection()
        {
            var a = new Asteroid { direction = Vector3.UnitX, speed = 100f };
            float delta = 0.1f;
            a.Update(delta);
            float expected = 100f * GameConstants.AsteroidSpeedAdjustment * delta;
            Assert.Equal(expected, a.position.X, 4);
        }

        [Fact]
        public void Update_WrapsPositionPastRightEdge()
        {
            var a = new Asteroid
            {
                direction = Vector3.UnitX,
                speed = 0f,
                position = new Vector3(GameConstants.PlayfieldSizeX + 1f, 0f, 0f)
            };
            a.Update(0f);
            Assert.True(a.position.X < GameConstants.PlayfieldSizeX);
        }

        [Fact]
        public void Update_WrapsPositionPastLeftEdge()
        {
            var a = new Asteroid
            {
                direction = -Vector3.UnitX,
                speed = 0f,
                position = new Vector3(-GameConstants.PlayfieldSizeX - 1f, 0f, 0f)
            };
            a.Update(0f);
            Assert.True(a.position.X > -GameConstants.PlayfieldSizeX);
        }

        [Fact]
        public void Update_DecrementsImmunityTimer()
        {
            var a = new Asteroid { spawnImmunityTimer = 1.0f };
            a.Update(0.5f);
            Assert.Equal(0.5f, a.spawnImmunityTimer, 5);
        }

        [Fact]
        public void Update_ClampsImmunityTimerAtZero()
        {
            var a = new Asteroid { spawnImmunityTimer = 0.1f };
            a.Update(5.0f);
            Assert.Equal(0f, a.spawnImmunityTimer);
        }

        [Fact]
        public void Update_ImmunityTimerDoesNotTickWhenAlreadyZero()
        {
            var a = new Asteroid { spawnImmunityTimer = 0f };
            a.Update(1.0f);
            Assert.Equal(0f, a.spawnImmunityTimer);
        }
    }
}
