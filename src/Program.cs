namespace WolfRender
{
    class Program
    {
        static void Main(string[] args)
        {
            Game.Instance.Init(1920, 1080);
            Input.Instance.Init();
            Game.Instance.Run();
        }
    }
}