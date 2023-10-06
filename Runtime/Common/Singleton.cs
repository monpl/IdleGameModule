using System;

namespace IdleGameModule.Common
{
    public class Singleton<T> where T : class, new()
    {
        private static readonly Lazy<T> _instance;

        public static T Instance => _instance.Value;

        static Singleton()
        {
            _instance = new Lazy<T>(() => new T());
        }
    }
}