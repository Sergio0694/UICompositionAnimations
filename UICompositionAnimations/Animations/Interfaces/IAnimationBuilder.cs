using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Animations.Interfaces
{
    public interface IAnimationBuilder
    {
        IAnimationBuilder Opacity(float to, EasingFunctionNames ease = EasingFunctionNames.Linear);

        IAnimationBuilder Opacity(float from, float to, EasingFunctionNames ease = EasingFunctionNames.Linear);

        IAnimationBuilder Translation(float to, EasingFunctionNames ease = EasingFunctionNames.Linear);

        IAnimationBuilder Translation(float from, float to, EasingFunctionNames ease = EasingFunctionNames.Linear);

        IAnimationBuilder Duration(int ms);

        IAnimationBuilder Duration(TimeSpan duration);

        IAnimationBuilder Delay(int ms);

        IAnimationBuilder Delay(TimeSpan duration);

        void Start();

        void Start([NotNull] Action callback);

        Task StartAsync();
    }
}
