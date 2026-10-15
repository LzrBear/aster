using System.IO;
using Asteroids;
using Xunit;

namespace Asteroids.Tests
{
    public class HighScoreManagerTests
    {
        private static string TempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            return dir;
        }

        [Fact]
        public void SaveLoad_RoundTrip_PreservesEntries()
        {
            string dir = TempDir();
            try
            {
                var mgr = new HighScoreManager(dir);
                mgr.AddEntry("Alice", 500);
                mgr.AddEntry("Bob", 300);

                var loaded = new HighScoreManager(dir);
                Assert.Equal(2, loaded.Entries.Count);
                Assert.Equal("Alice", loaded.Entries[0].Name);
                Assert.Equal(500, loaded.Entries[0].Score);
                Assert.Equal("Bob", loaded.Entries[1].Name);
                Assert.Equal(300, loaded.Entries[1].Score);
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void TamperedFile_IsDiscarded()
        {
            string dir = TempDir();
            try
            {
                var mgr = new HighScoreManager(dir);
                mgr.AddEntry("Alice", 500);

                string path = Path.Combine(dir, GameConstants.HighScoresFile);
                byte[] data = File.ReadAllBytes(path);
                data[data.Length / 2] ^= 0xFF;
                File.WriteAllBytes(path, data);

                var loaded = new HighScoreManager(dir);
                Assert.Empty(loaded.Entries);
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void Entries_AreSorted_Descending()
        {
            string dir = TempDir();
            try
            {
                var mgr = new HighScoreManager(dir);
                mgr.AddEntry("Third",  100);
                mgr.AddEntry("First",  500);
                mgr.AddEntry("Second", 300);

                Assert.Equal("First",  mgr.Entries[0].Name);
                Assert.Equal("Second", mgr.Entries[1].Name);
                Assert.Equal("Third",  mgr.Entries[2].Name);
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void MaxHighScores_Limit_IsEnforced()
        {
            string dir = TempDir();
            try
            {
                var mgr = new HighScoreManager(dir);
                for (int i = 0; i < GameConstants.MaxHighScores + 5; i++)
                    mgr.AddEntry($"Player{i}", i * 10);

                Assert.Equal(GameConstants.MaxHighScores, mgr.Entries.Count);
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void IsHighScore_True_WhenListNotFull()
        {
            string dir = TempDir();
            try
            {
                var mgr = new HighScoreManager(dir);
                Assert.True(mgr.IsHighScore(1));
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void IsHighScore_False_WhenBelowLowestEntry()
        {
            string dir = TempDir();
            try
            {
                var mgr = new HighScoreManager(dir);
                for (int i = 0; i < GameConstants.MaxHighScores; i++)
                    mgr.AddEntry($"Player{i}", 1000 + i * 10);

                Assert.False(mgr.IsHighScore(0));
                Assert.True(mgr.IsHighScore(9999));
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void EmptyFile_LoadsEmpty_NoCrash()
        {
            string dir = TempDir();
            try
            {
                var mgr = new HighScoreManager(dir);
                Assert.Empty(mgr.Entries);
            }
            finally { Directory.Delete(dir, true); }
        }

        [Fact]
        public void Save_IsDeterministicallyLoadable_MultipleTimes()
        {
            string dir = TempDir();
            try
            {
                var mgr = new HighScoreManager(dir);
                mgr.AddEntry("Persistent", 999);

                var a = new HighScoreManager(dir);
                var b = new HighScoreManager(dir);
                Assert.Equal(a.Entries[0].Score, b.Entries[0].Score);
            }
            finally { Directory.Delete(dir, true); }
        }
    }
}
