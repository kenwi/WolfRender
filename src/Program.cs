namespace WolfRender
{
    class Program
    {
        static void Main(string[] args)
        {
            bool limited = false;
            int targetfps = 60;
            float targetUpdateRate = 1f / targetfps;

            Game.Instance.Init(1200, 800);
            InputHandler.Instance.Init();
            while (Game.Instance.Window.IsOpen)
            {
                var dt = Game.Instance.DeltaTime;
                if (limited)
                {
                    while (dt < targetUpdateRate)
                    {
                        dt += Game.Instance.DeltaTime;
                        InputHandler.Instance.Update(dt);
                    }
                }
                else
                {
                    InputHandler.Instance.Update(dt);
                }
                Game.Instance.Update(dt);
                Game.Instance.Render();
            }
        }
    }
}