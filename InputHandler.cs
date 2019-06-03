namespace WolfRender
{
    public class InputHandler : Singleton<InputHandler>
    {
        public void Init()
        {
            Game.Instance.Window.Closed += (s, e) => Game.Instance.Window.Close();
        }

        public void Update()
        {
            Game.Instance.Window.DispatchEvents();
        }
    }
}
