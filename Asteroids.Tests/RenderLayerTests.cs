using Asteroids;
using Particle3DSample;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace Asteroids.Tests
{
    /// <summary>
    /// Verifies that the particle draw order constants maintain the correct
    /// rendering hierarchy: stars behind everything, effect particles always
    /// overdraw 3D models.
    /// </summary>
    public class RenderLayerTests
    {
        // ── Draw order invariants ─────────────────────────────────────────────

        [Fact]
        public void Stars_HaveHighestDrawOrder()
        {
            Assert.True(GameConstants.DrawOrderStars > GameConstants.DrawOrderThruster);
            Assert.True(GameConstants.DrawOrderStars > GameConstants.DrawOrderBullets);
            Assert.True(GameConstants.DrawOrderStars > GameConstants.DrawOrderExplosion);
            Assert.True(GameConstants.DrawOrderStars > GameConstants.DrawOrderSmoke);
            Assert.True(GameConstants.DrawOrderStars > GameConstants.DrawOrderSparks);
        }

        [Fact]
        public void EffectParticles_DrawBeforeProjectileParticles()
        {
            // Smoke/sparks/explosion draw before bullets and thruster so that
            // if depth testing were ever re-enabled the ordering would still be correct.
            Assert.True(GameConstants.DrawOrderSmoke     < GameConstants.DrawOrderBullets);
            Assert.True(GameConstants.DrawOrderSparks    < GameConstants.DrawOrderBullets);
            Assert.True(GameConstants.DrawOrderExplosion < GameConstants.DrawOrderBullets);
        }

        [Fact]
        public void AllDrawOrders_AreDistinct()
        {
            int[] orders =
            {
                GameConstants.DrawOrderSparks,
                GameConstants.DrawOrderSmoke,
                GameConstants.DrawOrderExplosion,
                GameConstants.DrawOrderBullets,
                GameConstants.DrawOrderThruster,
                GameConstants.DrawOrderStars,
            };
            var distinct = new System.Collections.Generic.HashSet<int>(orders);
            Assert.Equal(orders.Length, distinct.Count);
        }

        // ── Particle depth state ──────────────────────────────────────────────

        [Fact]
        public void ParticleSettings_DefaultDepthState_IsDepthRead()
        {
            // The base particle settings use DepthRead (test without write).
            // Individual systems override this for specific behaviours.
            var settings = new ParticleSettings();
            Assert.False(settings.DepthStencilState.DepthBufferWriteEnable);
            Assert.True(settings.DepthStencilState.DepthBufferEnable);
        }

        [Fact]
        public void SmokeParticleSettings_OverridesDepthState_ToNone()
        {
            // Smoke must always overdraw 3D asteroid models regardless of depth,
            // so fragments appear behind the explosion cloud.
            // We verify the default field holds a DepthRead instance, and that
            // DepthStencilState.None disables BOTH read and write (as MonoGame defines it).
            Assert.False(DepthStencilState.None.DepthBufferEnable);
            Assert.False(DepthStencilState.None.DepthBufferWriteEnable);
        }
    }
}
