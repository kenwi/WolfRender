using SFML.Graphics;
using SFML.System;
using System.Threading.Tasks;
using System.Threading;

namespace WolfRender.Interfaces
{
    public interface IWindowService
    {
        bool IsMouseVisible { get; set; }
        Vector2i WindowCenter { get; }
        RenderWindow Window { get; }
        void CreateWindow();
        Task StopAsync(CancellationToken cancellationToken);
    }
}