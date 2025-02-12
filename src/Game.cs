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
        Time totalGameTime;
        RenderWindow window;
        float deltaTime;
        uint targetFps;
        bool mouseVisible;
        bool limited;
        MapRenderer mapRenderer;

        public RenderWindow Window { get => window; set => window = value; }
        public Vector2i WindowCenter { get; set; }//=> window.Position + new Vector2i((int)window.Size.X / 2, (int)window.Size.Y / 2);
        public Time TotalGameTime { get => totalGameTime; }
        public Player Player { get => player; set => player = value; }
        public MapRenderer MapRenderer { get => mapRenderer; }
        public Random Random { get; private set; }
        public float DeltaTime { get => deltaTime; set => deltaTime = value; }
        public bool IsHelpMenuVisible { get; set; }
        public bool IsFullScreen { get; set; }

        public bool IsFramerateLimited
        {
            get => limited;
            set
            {
                uint fps = 0;
                if(value)
                {
                    fps = targetFps;
                }
                window.SetFramerateLimit(fps);
                limited = value;
            }
        }

        public bool MouseVisible
        {
            get => mouseVisible;
            set
            {
                Window.SetMouseCursorVisible(value);
                Window.SetMouseCursorGrabbed(!value);
                mouseVisible = value;
            }
        }

        public void Init(uint width, uint height, uint targetFps = 60, bool limitFrameRate = true)
        {
            Random = new Random();
            gameTime = new Clock();
            window = new RenderWindow(new VideoMode(width, height, VideoMode.DesktopMode.BitsPerPixel), "WolfRender");

            View view = new View(new FloatRect(0, 0, 1024, 768));
            window.SetView(view);
            WindowCenter = window.Position + new Vector2i((int)view.Size.X / 2, (int)view.Size.Y / 2);

            this.targetFps = targetFps;
            this.IsFramerateLimited = limitFrameRate;
            map = new Map();
            
            player = new Player(
                new Vector2f(map.Size.X / 4, map.Size.Y / 4),
                0.0f
            );
            
            player.Fov = MathF.PI * 0.5f;
            player.MovementSpeed = 2;
            
            mapRenderer = new MapRenderer(map, player);
            effects = new Effect[]{
                mapRenderer,
                new HelpScreen(),
                new FpsCounter()
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
                totalGameTime += Time.FromSeconds(deltaTime);
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
