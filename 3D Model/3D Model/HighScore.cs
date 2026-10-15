using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Asteroids
{
    public class HighScoreEntry
    {
        public string Name;
        public int Score;
        public HighScoreEntry(string name, int score) { Name = name; Score = score; }
    }

    public class HighScoreManager
    {
        // File layout: [16 bytes IV][32 bytes HMAC-SHA256][AES-256-CBC ciphertext]
        private const int IvSize   = 16;
        private const int MacSize  = 32;
        private const int HeaderSize = IvSize + MacSize;

        private readonly string _baseDir;
        private string FilePath => Path.Combine(_baseDir, GameConstants.HighScoresFile);

        public List<HighScoreEntry> Entries { get; private set; } = new List<HighScoreEntry>();

        public HighScoreManager(string baseDir = null)
        {
            _baseDir = baseDir ?? AppDomain.CurrentDomain.BaseDirectory;
            Load();
        }

        public void Load()
        {
            Entries.Clear();
            if (!File.Exists(FilePath)) return;

            try
            {
                byte[] fileBytes = File.ReadAllBytes(FilePath);
                if (fileBytes.Length < HeaderSize) return;

                byte[] iv         = new byte[IvSize];
                byte[] storedMac  = new byte[MacSize];
                byte[] ciphertext = new byte[fileBytes.Length - HeaderSize];

                Buffer.BlockCopy(fileBytes, 0,          iv,         0, IvSize);
                Buffer.BlockCopy(fileBytes, IvSize,     storedMac,  0, MacSize);
                Buffer.BlockCopy(fileBytes, HeaderSize, ciphertext, 0, ciphertext.Length);

                // Verify HMAC over IV + ciphertext before decrypting
                byte[] ivAndCipher = new byte[IvSize + ciphertext.Length];
                Buffer.BlockCopy(iv,         0,      ivAndCipher, 0,      IvSize);
                Buffer.BlockCopy(ciphertext, 0,      ivAndCipher, IvSize, ciphertext.Length);

                using (var hmac = new HMACSHA256(GameConstants.HsHmacKey))
                {
                    byte[] expectedMac = hmac.ComputeHash(ivAndCipher);
                    if (!CryptographicOperations.FixedTimeEquals(storedMac, expectedMac))
                        return; // tampered or corrupted — discard
                }

                // Decrypt
                string plaintext;
                using (var aes = Aes.Create())
                {
                    aes.Key = GameConstants.HsAesKey;
                    aes.IV  = iv;
                    using (var ms  = new MemoryStream(ciphertext))
                    using (var cs  = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var sr  = new StreamReader(cs, Encoding.UTF8))
                        plaintext = sr.ReadToEnd();
                }

                // Parse
                foreach (string line in plaintext.Split('\n'))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    int sep = line.LastIndexOf(GameConstants.HighScoreSeparator);
                    if (sep < 0) continue;
                    string name = line.Substring(0, sep);
                    if (int.TryParse(line.Substring(sep + 1).Trim(), out int score))
                        Entries.Add(new HighScoreEntry(name, score));
                }
            }
            catch
            {
                Entries.Clear(); // any unexpected error — start fresh
            }
        }

        public void Save()
        {
            // Serialise
            var sb = new StringBuilder();
            foreach (var e in Entries)
                sb.Append(e.Name).Append(GameConstants.HighScoreSeparator).Append(e.Score).Append('\n');
            byte[] plainBytes = Encoding.UTF8.GetBytes(sb.ToString());

            // Encrypt with a fresh random IV each save
            byte[] iv;
            byte[] ciphertext;
            using (var aes = Aes.Create())
            {
                aes.Key = GameConstants.HsAesKey;
                aes.GenerateIV();
                iv = aes.IV;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        cs.Write(plainBytes, 0, plainBytes.Length);
                    ciphertext = ms.ToArray();
                }
            }

            // Compute HMAC over IV + ciphertext (encrypt-then-MAC)
            byte[] ivAndCipher = new byte[IvSize + ciphertext.Length];
            Buffer.BlockCopy(iv,         0,      ivAndCipher, 0,      IvSize);
            Buffer.BlockCopy(ciphertext, 0,      ivAndCipher, IvSize, ciphertext.Length);

            byte[] mac;
            using (var hmac = new HMACSHA256(GameConstants.HsHmacKey))
                mac = hmac.ComputeHash(ivAndCipher);

            // Write: IV | HMAC | ciphertext
            using (var fs = File.Create(FilePath))
            {
                fs.Write(iv,         0, iv.Length);
                fs.Write(mac,        0, mac.Length);
                fs.Write(ciphertext, 0, ciphertext.Length);
            }
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
