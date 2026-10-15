using Asteroids;
using Xunit;

namespace Asteroids.Tests
{
    public class GameConstantsTests
    {
        [Fact]
        public void SizeRanks_AreCorrectlyOrdered()
        {
            Assert.True(GameConstants.AsteroidSizeRankLarge > GameConstants.AsteroidSizeRankMedium);
            Assert.True(GameConstants.AsteroidSizeRankMedium > GameConstants.AsteroidSizeRankSmall);
            Assert.True(GameConstants.AsteroidSizeRankSmall > 0);
        }

        [Fact]
        public void FragmentCounts_MatchSizeRanks()
        {
            Assert.Equal(GameConstants.AsteroidSizeRankLarge,  GameConstants.LargeAsteroidFragments);
            Assert.Equal(GameConstants.AsteroidSizeRankMedium, GameConstants.MediumAsteroidFragments);
        }

        [Fact]
        public void AsteroidScales_AreCorrectlyOrdered()
        {
            Assert.True(GameConstants.LargeAsteroidScale > GameConstants.MediumAsteroidScale);
            Assert.True(GameConstants.MediumAsteroidScale > GameConstants.SmallAsteroidScale);
            Assert.True(GameConstants.SmallAsteroidScale > 0f);
        }

        [Fact]
        public void AsteroidPool_IsLargeEnoughForFragments()
        {
            // NumAsteroids must fit at least the max possible fragments from one destruction
            Assert.True(GameConstants.NumAsteroids >= GameConstants.LargeAsteroidFragments);
        }

        [Fact]
        public void CrackThresholds_ArePositive()
        {
            Assert.True(GameConstants.LargeCracksMin  > 0);
            Assert.True(GameConstants.MediumCracksMin > 0);
            Assert.True(GameConstants.SmallCracksMin  > 0);
        }

        [Fact]
        public void CrackThresholds_MinIsNotGreaterThanMax()
        {
            Assert.True(GameConstants.LargeCracksMin  <= GameConstants.LargeCracksMax);
            Assert.True(GameConstants.MediumCracksMin <= GameConstants.MediumCracksMax);
            Assert.True(GameConstants.SmallCracksMin  <= GameConstants.SmallCracksMax);
        }

        [Fact]
        public void SpeedRange_IsValid()
        {
            Assert.True(GameConstants.AsteroidMinSpeed > 0f);
            Assert.True(GameConstants.AsteroidMaxSpeed > GameConstants.AsteroidMinSpeed);
        }

        [Fact]
        public void DifficultyRamp_InitialAsteroids_IsLessThanFull()
        {
            Assert.True(GameConstants.NumInitialAsteroidsMin < GameConstants.NumInitialAsteroids);
            Assert.True(GameConstants.NumInitialAsteroids    <= GameConstants.NumAsteroids);
        }

        [Fact]
        public void MaxHighScores_IsPositive()
        {
            Assert.True(GameConstants.MaxHighScores > 0);
        }

        [Fact]
        public void ShipMaxSpeed_IsAtLeast5xMaxAsteroidSpeed()
        {
            // Asteroid max speed in units/frame at 60 fps.
            // Ship cap must be at least 5× that.
            float maxAsteroidFrameSpeed = GameConstants.AsteroidMaxSpeed
                * GameConstants.AsteroidSpeedAdjustment / 60f;
            Assert.True(GameConstants.ShipMaxSpeed >= 5f * maxAsteroidFrameSpeed);
        }

        [Fact]
        public void SpawnImmunityDuration_IsThreeSeconds()
        {
            Assert.Equal(3.0f, GameConstants.AsteroidSpawnImmunityDuration);
        }
    }
}
