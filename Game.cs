using SFML.System;
using SFML.Graphics;
using SFML.Window;
using System;

namespace WolfRender
{
    public class Game : Singleton<Game>
    {
        static Effect[] effects;
        Clock gameTime;
        Time previousTime;
        RenderWindow window;

        public RenderWindow Window { get => window; private set => window = value; }
        public Vector2f MousePositionNormalized { get => new Vector2f((float)Mouse.GetPosition(window).X / window.Size.X, (float)Mouse.GetPosition(window).Y / window.Size.Y); }
        public Random Random { get; private set; }
        public Clock GameTime { get => gameTime; }
        public bool ShowHelpScreen { get; set; }
        public int FrameNumber { get; set; }
        public float DeltaTime { get => calculateDt(); }
        public Player Player { get; set; }

        public void Init(uint width, uint height)
        {
            Random = new Random();
            gameTime = new Clock();
            previousTime = gameTime.ElapsedTime;

            window = new RenderWindow(new VideoMode(width, height, VideoMode.DesktopMode.BitsPerPixel), "WolfRender");
            Player = new Player();
            effects = new Effect[]{
                new MapEffect()
                , new HelpScreenEffect()
                , new FpsCounterEffect()
            };
        }

        private float calculateDt()
        {
            var current = GameTime.ElapsedTime;
            var dt = current - previousTime;
            previousTime = current;
            return dt.AsSeconds();
        }

        public void Render()
        {
            window.Clear();
            foreach (var effect in effects)
                window.Draw(effect);
            window.Display();
            FrameNumber++;
        }

        public void Update(float dt)
        {
            foreach (var effect in effects)
            {
                effect.Update(dt);
            }
        }
    }
}
