using System;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Particle3DSample;
using Asteroids.Game_Types;

namespace Asteroids
{
    public class asterGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Camera / view
        Vector3 cameraPosition = new Vector3(0.0f, 0.0f, GameConstants.CameraHeight);
        Matrix projectionMatrix;
        Matrix viewMatrix;

        // Background
        Texture2D stars;
        particleEffects particleEffect = new particleEffects();

        // Music
        Song Music_default;

        // State machine
        GameState currentState = GameState.MainMenu;
        int menuIndex = 0;
        SpriteFont menuFont;

        // High scores
        HighScoreManager highScoreManager = new HighScoreManager();
        StringBuilder playerName = new StringBuilder();

        // Input tracking (for edge-triggered menu navigation)
        GamePadState lastGState = GamePad.GetState(PlayerIndex.One);
        KeyboardState lastKState = Keyboard.GetState();

        // The gameplay session
        standardGameType stand = new standardGameType();

        public asterGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "3D ModelContent";
            particleEffect.initialize(this);
        }

        protected override void Initialize()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                GraphicsDevice.DisplayMode.AspectRatio,
                GameConstants.CameraHeight - 2000.0f,
                GameConstants.CameraHeight + 2000.0f);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            stars      = Content.Load<Texture2D>("Textures/B1_stars");
            menuFont   = Content.Load<SpriteFont>("Fonts/Lucida Console");

            Music_default = Content.Load<Song>("Audio/Music/Hyper Space");
            MediaPlayer.Volume     = GameConstants.MusicVolume;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(Music_default);
        }

        protected override void UnloadContent() { }

        // ── Input helpers ──────────────────────────────────────────────────────

        bool PressedKey(KeyboardState ks, Keys k)  => ks.IsKeyDown(k)    && lastKState.IsKeyUp(k);
        bool PressedBtn(GamePadState gs, Buttons b) => gs.IsButtonDown(b) && lastGState.IsButtonUp(b);

        // ── Update ─────────────────────────────────────────────────────────────

        protected override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            GamePadState  gs = GamePad.GetState(PlayerIndex.One);

            particleEffect.UpdateStars();

            switch (currentState)
            {
                case GameState.MainMenu:   UpdateMainMenu(ks, gs);          break;
                case GameState.Playing:    UpdatePlaying(gameTime, ks, gs); break;
                case GameState.GameOver:   UpdateGameOver(gameTime, ks, gs);break;
                case GameState.EnterName:  UpdateEnterName(ks, gs);         break;
                case GameState.HighScores: UpdateHighScores(ks, gs);        break;
            }

            lastKState = ks;
            lastGState = gs;

            base.Update(gameTime);
        }

        void UpdateMainMenu(KeyboardState ks, GamePadState gs)
        {
            if (PressedKey(ks, Keys.Escape) || PressedBtn(gs, Buttons.Back))
            {
                this.Exit();
                return;
            }

            if (PressedKey(ks, Keys.Up) || PressedBtn(gs, Buttons.DPadUp))
                menuIndex = (menuIndex + 2) % 3; // wrap up

            if (PressedKey(ks, Keys.Down) || PressedBtn(gs, Buttons.DPadDown))
                menuIndex = (menuIndex + 1) % 3; // wrap down

            if (PressedKey(ks, Keys.Enter) || PressedKey(ks, Keys.Space) || PressedBtn(gs, Buttons.A))
            {
                switch (menuIndex)
                {
                    case 0: // Play
                        if (stand.HasInitialized)
                            stand.Reset();
                        currentState = GameState.Playing;
                        break;
                    case 1: // High Scores
                        currentState = GameState.HighScores;
                        break;
                    case 2: // Quit
                        this.Exit();
                        break;
                }
            }
        }

        void UpdatePlaying(GameTime gameTime, KeyboardState ks, GamePadState gs)
        {
            if (PressedKey(ks, Keys.Escape) || PressedBtn(gs, Buttons.Back))
            {
                currentState = GameState.MainMenu;
                return;
            }

            stand.RunGame(this, gameTime, GraphicsDevice, projectionMatrix, viewMatrix);

            if (stand.IsGameOver)
                currentState = GameState.GameOver;
        }

        void UpdateGameOver(GameTime gameTime, KeyboardState ks, GamePadState gs)
        {
            // Keep the game world ticking for background animation
            if (stand.HasInitialized)
                stand.RunGame(this, gameTime, GraphicsDevice, projectionMatrix, viewMatrix);

            if (PressedKey(ks, Keys.Escape) || PressedBtn(gs, Buttons.Back))
            {
                currentState = GameState.MainMenu;
                return;
            }

            if (PressedKey(ks, Keys.Enter) || PressedKey(ks, Keys.Space) || PressedBtn(gs, Buttons.A))
            {
                if (highScoreManager.IsHighScore(stand.Score))
                {
                    playerName.Clear();
                    Window.TextInput += HandleTextInput;
                    currentState = GameState.EnterName;
                }
                else
                {
                    currentState = GameState.MainMenu;
                }
            }
        }

        void UpdateEnterName(KeyboardState ks, GamePadState gs)
        {
            if (PressedKey(ks, Keys.Escape) || PressedBtn(gs, Buttons.Back))
            {
                Window.TextInput -= HandleTextInput;
                currentState = GameState.MainMenu;
                return;
            }

            if (PressedKey(ks, Keys.Enter))
            {
                Window.TextInput -= HandleTextInput;
                string name = playerName.ToString().Trim();
                if (name.Length == 0) name = "???";
                highScoreManager.AddEntry(name, stand.Score);
                currentState = GameState.HighScores;
            }
        }

        void HandleTextInput(object sender, TextInputEventArgs e)
        {
            if (e.Character == '\b') // backspace
            {
                if (playerName.Length > 0)
                    playerName.Remove(playerName.Length - 1, 1);
            }
            else if (e.Character >= ' ' && playerName.Length < 10)
            {
                playerName.Append(e.Character);
            }
        }

        void UpdateHighScores(KeyboardState ks, GamePadState gs)
        {
            if (PressedKey(ks, Keys.Enter) || PressedKey(ks, Keys.Space) ||
                PressedKey(ks, Keys.Escape) || PressedBtn(gs, Buttons.A) || PressedBtn(gs, Buttons.Back))
                currentState = GameState.MainMenu;
        }

        // ── Draw ───────────────────────────────────────────────────────────────

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Star texture — normal pass + additive boost
            spriteBatch.Begin();
            spriteBatch.Draw(stars, new Rectangle(0, 0, 800, 600), Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            spriteBatch.Draw(stars, new Rectangle(0, 0, 800, 600), Color.White * 0.9f);
            spriteBatch.End();

            // Animated star particles (always)
            particleEffect.draw(viewMatrix, projectionMatrix);

            switch (currentState)
            {
                case GameState.MainMenu:
                    DrawMainMenu();
                    break;

                case GameState.Playing:
                    if (stand.HasInitialized)
                    {
                        stand.particleEffect.draw(viewMatrix, projectionMatrix);
                        stand.drawGame(GraphicsDevice);
                    }
                    break;

                case GameState.GameOver:
                    if (stand.HasInitialized)
                    {
                        stand.particleEffect.draw(viewMatrix, projectionMatrix);
                        stand.drawGame(GraphicsDevice);
                    }
                    DrawGameOver();
                    break;

                case GameState.EnterName:
                    if (stand.HasInitialized)
                    {
                        stand.particleEffect.draw(viewMatrix, projectionMatrix);
                        stand.drawGame(GraphicsDevice);
                    }
                    DrawEnterName();
                    break;

                case GameState.HighScores:
                    DrawHighScores();
                    break;
            }

            base.Draw(gameTime);
        }

        // ── Draw helpers ───────────────────────────────────────────────────────

        /// Returns the X coordinate that centres `text` on an 800-wide screen.
        float CenterX(string text, float scale = 1f) =>
            (800f - menuFont.MeasureString(text).X * scale) / 2f;

        void DrawMainMenu()
        {
            string[] items  = { "PLAY", "HIGH SCORES", "QUIT" };

            spriteBatch.Begin();

            // Title
            const string title = "ASTEROIDS";
            spriteBatch.DrawString(menuFont, title,
                new Vector2(CenterX(title, 2f), 120f),
                Color.Yellow, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

            // Menu items
            for (int i = 0; i < items.Length; i++)
            {
                string prefix = (menuIndex == i) ? "> " : "  ";
                string label  = prefix + items[i];
                Color  color  = (menuIndex == i) ? Color.White : Color.Gray;
                spriteBatch.DrawString(menuFont, label,
                    new Vector2(CenterX(label), 280f + i * 55f), color);
            }

            // Controls hint
            const string hint = "UP/DOWN to select   ENTER to confirm";
            spriteBatch.DrawString(menuFont, hint,
                new Vector2(CenterX(hint), 540f), Color.DimGray);

            spriteBatch.End();
        }

        void DrawGameOver()
        {
            spriteBatch.Begin();

            const string title = "GAME OVER";
            spriteBatch.DrawString(menuFont, title,
                new Vector2(CenterX(title, 2f), 180f),
                Color.Red, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

            string scoreStr = "SCORE: " + stand.Score;
            spriteBatch.DrawString(menuFont, scoreStr,
                new Vector2(CenterX(scoreStr), 290f), Color.White);

            string prompt = highScoreManager.IsHighScore(stand.Score)
                ? "NEW HIGH SCORE!  PRESS ENTER"
                : "PRESS ENTER TO CONTINUE";
            spriteBatch.DrawString(menuFont, prompt,
                new Vector2(CenterX(prompt), 350f), Color.LightGray);

            spriteBatch.End();
        }

        void DrawEnterName()
        {
            spriteBatch.Begin();

            const string title = "NEW HIGH SCORE!";
            spriteBatch.DrawString(menuFont, title,
                new Vector2(CenterX(title, 1.5f), 160f),
                Color.Yellow, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);

            string scoreStr = "SCORE: " + stand.Score;
            spriteBatch.DrawString(menuFont, scoreStr,
                new Vector2(CenterX(scoreStr), 250f), Color.White);

            const string prompt = "ENTER YOUR NAME (max 10 chars):";
            spriteBatch.DrawString(menuFont, prompt,
                new Vector2(CenterX(prompt), 310f), Color.LightGray);

            string nameDisplay = playerName.ToString() + "_";
            spriteBatch.DrawString(menuFont, nameDisplay,
                new Vector2(CenterX(nameDisplay, 1.5f), 355f),
                Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);

            const string confirm = "ENTER to confirm   ESC to cancel";
            spriteBatch.DrawString(menuFont, confirm,
                new Vector2(CenterX(confirm), 540f), Color.DimGray);

            spriteBatch.End();
        }

        void DrawHighScores()
        {
            spriteBatch.Begin();

            const string title = "HIGH SCORES";
            spriteBatch.DrawString(menuFont, title,
                new Vector2(CenterX(title, 2f), 60f),
                Color.Yellow, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

            var entries = highScoreManager.Entries;
            if (entries.Count == 0)
            {
                const string empty = "NO SCORES YET";
                spriteBatch.DrawString(menuFont, empty,
                    new Vector2(CenterX(empty), 280f), Color.Gray);
            }
            else
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    Color c = (i == 0) ? Color.Gold
                            : (i == 1) ? Color.Silver
                            : Color.LightGray;

                    // Fixed-width columns: rank (2), name (10), score (7)
                    string line = string.Format("{0,2}. {1,-10}  {2,7}",
                        i + 1, entries[i].Name, entries[i].Score);
                    spriteBatch.DrawString(menuFont, line,
                        new Vector2(CenterX(line), 160f + i * 35f), c);
                }
            }

            const string back = "PRESS ENTER TO RETURN";
            spriteBatch.DrawString(menuFont, back,
                new Vector2(CenterX(back), 560f), Color.DimGray);

            spriteBatch.End();
        }
    }
}
