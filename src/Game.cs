using SFML.System;
using SFML.Graphics;
using SFML.Window;
using System;

namespace WolfRender
{
    public class Game : Singleton<Game>
    {
        Map map;
        Player player;
        static Effect[] effects;
        Clock gameTime;
        RenderWindow window;
        Time previousTime;

        public RenderWindow Window { get => window; private set => window = value; }
        public Vector2f MousePositionNormalized { get => new Vector2f((float)Mouse.GetPosition(window).X / window.Size.X, (float)Mouse.GetPosition(window).Y / window.Size.Y); }
        public Random Random { get; private set; }
        public Clock GameTime { get => gameTime; }
        public bool ShowHelpScreen { get; set; }
        public int FrameNumber { get; set; }
        public float DeltaTime { get => calculateDt(); }
        public Player Player => player;

        public void Init(uint width, uint height)
        {
            Random = new Random();
            gameTime = new Clock();
            previousTime = gameTime.ElapsedTime;

            window = new RenderWindow(new VideoMode(width, height, VideoMode.DesktopMode.BitsPerPixel), "WolfRender");
            map = new Map();
            player = new Player();
            player.Fov = MathF.PI * 0.5f;
            player.Position = new Vector2f(map.Size.X / 4, map.Size.Y / 4);
            effects = new Effect[]{
                new MapRenderer(map, player)
                , new HelpScreen()
                , new FpsCounter()
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
