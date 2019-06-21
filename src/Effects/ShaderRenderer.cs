using System;
using SFML.Graphics;

namespace WolfRender
{
    public class ShaderRenderer : Effect
    {
        Shader shader;
        Sprite sprite;
        Texture texture;

        public ShaderRenderer() : base("ShaderRenderer")
        {
            texture = new Texture(Instance.Window.Size.X, Instance.Window.Size.Y);
            sprite = new Sprite(texture);
            shader = new Shader(null, null, "shaders/shader.frag");
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            states = new RenderStates(states) {
                Shader = shader
            };
            target.Draw(sprite, states);
        }

        protected override void OnUpdate(float time)
        {

        }
    }
}
