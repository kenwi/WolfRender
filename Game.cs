using SFML.System;
using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;
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

        public void Init()
        {
            Random = new Random();
            gameTime = new Clock();
            previousTime = gameTime.ElapsedTime;

            window = new RenderWindow(new VideoMode(512, 512, VideoMode.DesktopMode.BitsPerPixel), "");
            effects = new Effect[]{
                new MapEffect()
                //new DoomFireEffect()
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

        public void Update()
        {
            foreach (var effect in effects)
            {
                effect.Update(gameTime.ElapsedTime.AsSeconds());
            }
        }
    }
}
