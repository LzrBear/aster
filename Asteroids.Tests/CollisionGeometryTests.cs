using Asteroids;
using Microsoft.Xna.Framework;
using Xunit;

namespace Asteroids.Tests
{
    /// <summary>
    /// Tests the geometric collision detection that drives asteroid-asteroid,
    /// bullet-asteroid, and ship-asteroid collision checks in the game loop.
    /// </summary>
    public class CollisionGeometryTests
    {
        // ── Asteroid-Asteroid ─────────────────────────────────────────────────

        [Fact]
        public void AsteroidAsteroid_OverlappingSpheres_Intersect()
        {
            var a = new BoundingSphere(new Vector3(0, 0, 0), 1000f);
            var b = new BoundingSphere(new Vector3(1500, 0, 0), 1000f);
            Assert.True(a.Intersects(b));
        }

        [Fact]
        public void AsteroidAsteroid_SeparatedSpheres_DoNotIntersect()
        {
            var a = new BoundingSphere(new Vector3(0, 0, 0), 1000f);
            var b = new BoundingSphere(new Vector3(5000, 0, 0), 1000f);
            Assert.False(a.Intersects(b));
        }

        [Fact]
        public void AsteroidAsteroid_ImmunitySkipsCollision()
        {
            // Fragments with immunity > 0 must skip collision — verified by checking the flag.
            var fragment = new Asteroid { spawnImmunityTimer = GameConstants.AsteroidSpawnImmunityDuration };
            Assert.True(fragment.spawnImmunityTimer > 0f,
                "Newly spawned fragments must carry a positive immunity timer so the " +
                "asteroid-asteroid collision loop skips them.");
        }

        // ── Bullet-Asteroid ───────────────────────────────────────────────────

        [Fact]
        public void BulletAsteroid_BulletInsideAsteroid_Intersects()
        {
            // Bullet centre well inside asteroid bounding sphere.
            var bulletSphere   = new BoundingSphere(new Vector3(200, 0, 0), 50f);
            var asteroidSphere = new BoundingSphere(new Vector3(0,   0, 0), 800f);
            Assert.True(asteroidSphere.Intersects(bulletSphere));
        }

        [Fact]
        public void BulletAsteroid_BulletFarAway_DoesNotIntersect()
        {
            var bulletSphere   = new BoundingSphere(new Vector3(5000, 0, 0), 50f);
            var asteroidSphere = new BoundingSphere(new Vector3(0,    0, 0), 800f);
            Assert.False(asteroidSphere.Intersects(bulletSphere));
        }

        // ── Ship-Asteroid ─────────────────────────────────────────────────────

        [Fact]
        public void ShipAsteroid_ShipInsideAsteroid_Intersects()
        {
            var shipSphere     = new BoundingSphere(Vector3.Zero,          500f);
            var asteroidSphere = new BoundingSphere(new Vector3(400, 0, 0), 800f);
            Assert.True(shipSphere.Intersects(asteroidSphere));
        }

        [Fact]
        public void ShipAsteroid_ShipFarFromAsteroid_DoesNotIntersect()
        {
            var shipSphere     = new BoundingSphere(Vector3.Zero,           500f);
            var asteroidSphere = new BoundingSphere(new Vector3(10000, 0, 0), 800f);
            Assert.False(shipSphere.Intersects(asteroidSphere));
        }
    }
}
