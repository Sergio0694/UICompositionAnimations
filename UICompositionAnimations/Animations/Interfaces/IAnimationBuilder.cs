using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Animations.Interfaces
{
    public interface IAnimationBuilder
    {
        IAnimationBuilder Opacity(float to, EasingFunctionNames ease);

        IAnimationBuilder Opacity(float from, float to, EasingFunctionNames ease);

        IAnimationBuilder Translation(float to, EasingFunctionNames ease);

        IAnimationBuilder Translation(float from, float to, EasingFunctionNames ease);

        IAnimationBuilder Duration(int ms);

        IAnimationBuilder Duration(TimeSpan duration);

        IAnimationBuilder Delay(int ms);

        IAnimationBuilder Delay(TimeSpan duration);

        void Start();

        void Start([NotNull] Action callback);

        Task StartAsync();
    }
}
