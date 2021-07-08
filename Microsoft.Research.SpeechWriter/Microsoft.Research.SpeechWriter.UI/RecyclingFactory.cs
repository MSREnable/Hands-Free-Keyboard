using System;
using System.Collections.Generic;

namespace Microsoft.Research.SpeechWriter.UI
{
    public static class RecyclingFactory<T>
        where T : class, new()
    {
        private static readonly Stack<WeakReference<T>> _recyclables = new Stack<WeakReference<T>>();

        public static T Create()
        {
            T value = null;
            while (value == null && _recyclables.Count != 0 && !_recyclables.Pop().TryGetTarget(out value))
            {
            }

            if (value == null)
            {
                //Debug.WriteLine($"Created {typeof(T).Name}");
                value = new T();
            }
            else
            {
                //Debug.WriteLine($"Recycled {typeof(T).Name}");
            }


            return value;
        }

        public static void Recycle(T value)
        {
            var reference = new WeakReference<T>(value);
            _recyclables.Push(reference);
        }
    }

    public static class RecyclingFactory
    {
        public static T Create<T>()
            where T : class, new()
        {
            var value = RecyclingFactory<T>.Create();
            return value;
        }

        public static void Recycle<T>(T value)
            where T : class, new()
        {
            RecyclingFactory<T>.Recycle(value);
        }
    }
}
