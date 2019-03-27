using Windows.UI.Xaml;
using JetBrains.Annotations;
using UICompositionAnimations.Animations;
using UICompositionAnimations.Animations.Interfaces;

namespace UICompositionAnimations.Extensions.Windows.UI.Xaml
{
    public static class AnimationExtensions
    {
        public static IAnimationBuilder Animate([NotNull] this UIElement target)
        {
            return new CompositionAnimationBuilder(target);
        }
    }
}
