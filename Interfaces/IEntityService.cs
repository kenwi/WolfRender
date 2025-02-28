using System.Collections.Generic;

namespace WolfRender.Interfaces
{
    public interface IEntityService
    {
        List<IEntity> Entities { get; }
        void Init();
        void Update(float dt);
    }
} 