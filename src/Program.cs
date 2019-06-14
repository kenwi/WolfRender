namespace WolfRender
{
    class Program
    {
        static void Main(string[] args)
        {
            Game.Instance.Init(1000, 700);
            Input.Instance.Init();
            Game.Instance.Run();
        }
    }
}