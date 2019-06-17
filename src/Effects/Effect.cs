using SFML.Graphics;
using System.Diagnostics;

namespace WolfRender
{
    public abstract class Effect : Drawable
    {
        protected abstract void OnDraw(RenderTarget target, RenderStates states);
        protected abstract void OnUpdate(float time);

        public string Name { get; private set; }
        public Game Instance => Game.Instance;

        protected Effect(string name)
        {
            Name = name;
            Debug.Assert(Shader.IsAvailable);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            OnDraw(target, states);
        }

        public void Update(float time)
        {
            OnUpdate(time);
        }
    }
}
