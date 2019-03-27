using System;
using System.Threading.Tasks;
using UICompositionAnimations.Animations.Interfaces;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Animations.Abstract
{
    internal abstract class AnimationBuilderBase : IAnimationBuilder
    {
        public IAnimationBuilder Opacity(float to, EasingFunctionNames ease)
        {
            throw new NotImplementedException();
        }

        public IAnimationBuilder Opacity(float @from, float to, EasingFunctionNames ease)
        {
            throw new NotImplementedException();
        }

        public IAnimationBuilder Translation(float to, EasingFunctionNames ease)
        {
            throw new NotImplementedException();
        }

        public IAnimationBuilder Translation(float @from, float to, EasingFunctionNames ease)
        {
            throw new NotImplementedException();
        }

        public IAnimationBuilder Duration(int ms)
        {
            throw new NotImplementedException();
        }

        public IAnimationBuilder Duration(TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        public IAnimationBuilder Delay(int ms)
        {
            throw new NotImplementedException();
        }

        public IAnimationBuilder Delay(TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Start(Action callback)
        {
            throw new NotImplementedException();
        }

        public Task StartAsync()
        {
            throw new NotImplementedException();
        }
    }
}
