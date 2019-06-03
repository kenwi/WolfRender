using System;

namespace WolfRender
{
    public class Singleton<T>
    {
        static readonly Lazy<T> instance = new Lazy<T>();
        public static T Instance => instance.Value;
    }
}
