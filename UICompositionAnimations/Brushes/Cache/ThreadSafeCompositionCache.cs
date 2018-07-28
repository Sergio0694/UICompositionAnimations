using System;
using System.Collections.Generic;
using Windows.UI.Composition;
using JetBrains.Annotations;

namespace UICompositionAnimations.Brushes.Cache
{
    internal sealed class ThreadSafeCompositionCache<T> where T : CompositionObject
    {
        private readonly List<WeakReference<T>> Cache = new List<WeakReference<T>>();

        public T TryGetInstance()
        {
            foreach (WeakReference<T> value in Cache)
                if (value.TryGetTarget(out T instance) && instance.Dispatcher.HasThreadAccess)
                    return instance;
            return null;
        }

        public void Add([NotNull] T value) => Cache.Add(new WeakReference<T>(value));

        public void Cleanup() => Cache.RemoveAll(reference => !reference.TryGetTarget(out _));
    }
}
