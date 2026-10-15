using System;
using System.Collections.Generic;
using System.IO;

namespace Asteroids
{
    class HighScoreEntry
    {
        public string Name;
        public int Score;
        public HighScoreEntry(string name, int score) { Name = name; Score = score; }
    }

    class HighScoreManager
    {
        private string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GameConstants.HighScoresFile);

        public List<HighScoreEntry> Entries { get; private set; } = new List<HighScoreEntry>();

        public HighScoreManager() { Load(); }

        public void Load()
        {
            Entries.Clear();
            if (!File.Exists(FilePath)) return;
            foreach (string line in File.ReadAllLines(FilePath))
            {
                int sep = line.LastIndexOf(GameConstants.HighScoreSeparator);
                if (sep < 0) continue;
                string name = line.Substring(0, sep);
                if (int.TryParse(line.Substring(sep + 1), out int score))
                    Entries.Add(new HighScoreEntry(name, score));
            }
        }

        public void Save()
        {
            var lines = new List<string>();
            foreach (var e in Entries)
                lines.Add(e.Name + GameConstants.HighScoreSeparator + e.Score);
            File.WriteAllLines(FilePath, lines);
        }

        public bool IsHighScore(int score)
        {
            return Entries.Count < GameConstants.MaxHighScores || score > Entries[Entries.Count - 1].Score;
        }

        public void AddEntry(string name, int score)
        {
            Entries.Add(new HighScoreEntry(name, score));
            Entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            if (Entries.Count > GameConstants.MaxHighScores)
                Entries.RemoveRange(GameConstants.MaxHighScores, Entries.Count - GameConstants.MaxHighScores);
            Save();
        }
    }
}
