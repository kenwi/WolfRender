using System;
using SFML.System;

namespace WolfRender
{
    class Program
    {
        static void Main(string[] args)
        {
            int targetfps = 60;
            float targetUpdateRate = 1f / targetfps;

            Game.Instance.Init();
            InputHandler.Instance.Init();
            while (Game.Instance.Window.IsOpen)
            {
                var dt = Game.Instance.DeltaTime;
                while (dt < targetUpdateRate)
                {
                    dt += Game.Instance.DeltaTime;
                    InputHandler.Instance.Update();
                }
                Game.Instance.Update(dt);
                Game.Instance.Render();
            }
        }
    }
}