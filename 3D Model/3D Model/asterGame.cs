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
        float _spaceWidth;   // computed from font metrics; used by DrawSp/MeasureSp

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
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth  = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "3D ModelContent";
            particleEffect.initialize(this);
        }

        protected override void Initialize()
        {
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height,
                GameConstants.CameraHeight - 2000.0f,
                GameConstants.CameraHeight + 2000.0f);
            viewMatrix = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.Initialize();
        }

        void OnResize(object sender, EventArgs e)
        {
            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;
            if (w == 0 || h == 0) return;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                (float)w / h,
                GameConstants.CameraHeight - 2000.0f,
                GameConstants.CameraHeight + 2000.0f);

            if (stand.HasInitialized)
                stand.UpdateProjection(projectionMatrix, viewMatrix);
        }

        protected override void LoadContent()
        {
            stars      = Content.Load<Texture2D>("Textures/B1_stars");
            menuFont    = Content.Load<SpriteFont>("Fonts/Lucida Console");
            // MonoGame pipeline gives the space glyph zero advance (empty bounding box).
            // We measure it ourselves and use DrawSp/MeasureSp for any text with spaces.
            _spaceWidth = menuFont.MeasureString("M").X * 0.5f;

            Music_default = Content.Load<Song>("Audio/Music/Hyper Space");
            MediaPlayer.Volume     = GameConstants.MusicVolume;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(Music_default);
        }

        // ── Space-aware text helpers ───────────────────────────────────────────
        // MonoGame's pipeline gives the space glyph zero advance because it has no
        // visible pixels.  DrawSp / MeasureSp split on spaces and insert _spaceWidth
        // manually so centering and rendering both work correctly.

        float MeasureSp(string text, float scale = 1f)
        {
            float w = 0f;
            int segStart = 0;
            for (int i = 0; i <= text.Length; i++)
            {
                if (i == text.Length || text[i] == ' ')
                {
                    if (i > segStart)
                        w += menuFont.MeasureString(text.Substring(segStart, i - segStart)).X;
                    if (i < text.Length)   // it's a space
                        w += _spaceWidth;
                    segStart = i + 1;
                }
            }
            return w * scale;
        }

        void DrawSp(string text, float x, float y, Color color, float scale = 1f)
        {
            float curX = x;
            int segStart = 0;
            for (int i = 0; i <= text.Length; i++)
            {
                if (i == text.Length || text[i] == ' ')
                {
                    if (i > segStart)
                    {
                        string seg = text.Substring(segStart, i - segStart);
                        spriteBatch.DrawString(menuFont, seg,
                            new Vector2(curX, y), color,
                            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                        curX += menuFont.MeasureString(seg).X * scale;
                    }
                    if (i < text.Length)   // it's a space
                        curX += _spaceWidth * scale;
                    segStart = i + 1;
                }
            }
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

            // Star texture — normal pass + additive boost, stretched to fill window
            var screen = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            spriteBatch.Begin();
            spriteBatch.Draw(stars, screen, Color.White);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            spriteBatch.Draw(stars, screen, Color.White * 0.9f);
            spriteBatch.End();

            // ── Pass 1: 3D models (before particles) ──────────────────────────
            // SetCamera on all particle systems so base.Draw renders them correctly.
            particleEffect.draw(viewMatrix, projectionMatrix);

            bool gameVisible = stand.HasInitialized &&
                (currentState == GameState.Playing  ||
                 currentState == GameState.GameOver  ||
                 currentState == GameState.EnterName);

            if (gameVisible)
            {
                stand.particleEffect.draw(viewMatrix, projectionMatrix);
                stand.drawGame(GraphicsDevice);
            }

            // ── Pass 2: particles rendered by DrawableGameComponents ───────────
            base.Draw(gameTime);

            // ── Pass 3: all 2D text — always on top of particles ──────────────
            switch (currentState)
            {
                case GameState.MainMenu:
                    DrawMainMenu();
                    break;

                case GameState.Playing:
                    if (stand.HasInitialized) stand.drawHUD();
                    break;

                case GameState.GameOver:
                    if (stand.HasInitialized) stand.drawHUD();
                    DrawGameOver();
                    break;

                case GameState.EnterName:
                    if (stand.HasInitialized) stand.drawHUD();
                    DrawEnterName();
                    break;

                case GameState.HighScores:
                    DrawHighScores();
                    break;
            }
        }

        // ── Draw helpers ───────────────────────────────────────────────────────

        /// Returns the X coordinate that centres `text` in the current viewport.
        float CenterX(string text, float scale = 1f) =>
            (GraphicsDevice.Viewport.Width - MeasureSp(text, scale)) / 2f;

        void DrawMainMenu()
        {
            string[] items = { "PLAY", "HIGH SCORES", "QUIT" };

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicClamp);

            const string title = "ASTEROIDS";
            DrawSp(title, CenterX(title, 2f), 120f, Color.Yellow, 2f);

            for (int i = 0; i < items.Length; i++)
            {
                string prefix = (menuIndex == i) ? "> " : "  ";
                string label  = prefix + items[i];
                Color  color  = (menuIndex == i) ? Color.White : Color.Gray;
                DrawSp(label, CenterX(label), 280f + i * 55f, color);
            }

            const string hint = "UP/DOWN to select   ENTER to confirm";
            DrawSp(hint, CenterX(hint), 540f, Color.DimGray);

            spriteBatch.End();
        }

        void DrawGameOver()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicClamp);

            const string title = "GAME OVER";
            DrawSp(title, CenterX(title, 2f), 180f, Color.Red, 2f);

            string scoreStr = "SCORE: " + stand.Score;
            DrawSp(scoreStr, CenterX(scoreStr), 290f, Color.White);

            string prompt = highScoreManager.IsHighScore(stand.Score)
                ? "NEW HIGH SCORE!  PRESS ENTER"
                : "PRESS ENTER TO CONTINUE";
            DrawSp(prompt, CenterX(prompt), 350f, Color.LightGray);

            spriteBatch.End();
        }

        void DrawEnterName()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicClamp);

            const string title = "NEW HIGH SCORE!";
            DrawSp(title, CenterX(title, 1.5f), 160f, Color.Yellow, 1.5f);

            string scoreStr = "SCORE: " + stand.Score;
            DrawSp(scoreStr, CenterX(scoreStr), 250f, Color.White);

            const string prompt = "ENTER YOUR NAME (max 10 chars):";
            DrawSp(prompt, CenterX(prompt), 310f, Color.LightGray);

            string nameDisplay = playerName.ToString() + "_";
            DrawSp(nameDisplay, CenterX(nameDisplay, 1.5f), 355f, Color.White, 1.5f);

            const string confirm = "ENTER to confirm   ESC to cancel";
            DrawSp(confirm, CenterX(confirm), 540f, Color.DimGray);

            spriteBatch.End();
        }

        void DrawHighScores()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.AnisotropicClamp);

            const string title = "HIGH SCORES";
            DrawSp(title, CenterX(title, 2f), 60f, Color.Yellow, 2f);

            var entries = highScoreManager.Entries;
            if (entries.Count == 0)
            {
                const string empty = "NO SCORES YET";
                DrawSp(empty, CenterX(empty), 280f, Color.Gray);
            }
            else
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    Color c = (i == 0) ? Color.Gold
                            : (i == 1) ? Color.Silver
                            : Color.LightGray;

                    string line = string.Format("{0,2}. {1,-10}  {2,7}",
                        i + 1, entries[i].Name, entries[i].Score);
                    DrawSp(line, CenterX(line), 160f + i * 35f, c);
                }
            }

            const string back = "PRESS ENTER TO RETURN";
            DrawSp(back, CenterX(back), 560f, Color.DimGray);

            spriteBatch.End();
        }
    }
}
