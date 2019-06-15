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
        Effect[] effects;
        Clock gameTime;
        Time previousTime;
        RenderWindow window;
        float deltaTime;
        bool mouseVisible;

        public RenderWindow Window { get => window; private set => window = value; }
        public Vector2f MousePositionNormalized { get => new Vector2f((float)Mouse.GetPosition(window).X / window.Size.X, (float)Mouse.GetPosition(window).Y / window.Size.Y); }
        public Random Random { get; private set; }
        public float DeltaTime { get => deltaTime; set => deltaTime = value; }
        public Clock GameTime { get => gameTime; }
        public Player Player { get => player; set => player = value; }

        public bool HelpMenuVisible { get; set; }
        public bool FramerateLimited { get; set; }
        public bool MouseVisible
        {
            get => mouseVisible;
            set
            {
                Window.SetMouseCursorVisible(value);
                mouseVisible = value;
            }
        }

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
            player.RotationSpeed = Tools.DegToRad(100);
            player.MovementSpeed = 2;

            effects = new Effect[]{
                new MapRenderer(map, player)
                , new HelpScreen()
                , new FpsCounter()
            };
        }

        public void Run()
        {
            while (window.IsOpen)
            {
                deltaTime = gameTime.Restart().AsSeconds();
                window.DispatchEvents();
                Input.Instance.Update(deltaTime);
                Update(deltaTime);
                Render();
            }
        }

        public void Render()
        {
            window.Clear();
            foreach (var effect in effects)
            {
                window.Draw(effect);
            }
            window.Display();
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
