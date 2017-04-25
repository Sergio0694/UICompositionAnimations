using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas.Effects;
using UICompositionAnimations.Behaviours.Effects;
using UICompositionAnimations.Behaviours.Effects.Base;
using UICompositionAnimations.Helpers;

namespace UICompositionAnimations.Behaviours
{
    /// <summary>
    /// A static class that manages the creation of attached composition effects
    /// </summary>

    public static class AttachedCompositionEffectsFactory
    {
        #region Static effects

        /// <summary>
        /// Creates a new <see cref="AttachedCompositionEffectWithAutoResize{T}"/> instance for the target element
        /// </summary>
        /// <typeparam name="T">The type of element to blur</typeparam>
        /// <param name="element">The target element</param>
        /// <param name="blur">The amount of blur to apply to the element</param>
        /// <param name="ms">The duration of the initial blur animation, in milliseconds</param>
        [MustUseReturnValue, NotNull]
        public static AttachedCompositionEffectWithAutoResize<T> GetAttachedBlur<T>(
            [NotNull] this T element, float blur, int ms) where T : FrameworkElement
        {
            // Get the visual and the compositor
            Visual visual = element.GetVisual();
            Compositor compositor = visual.Compositor;

            // Create the blur effect and the effect factory
            GaussianBlurEffect blurEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BlurAmount = 0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source")
            };
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(blurEffect, new[] { "Blur.BlurAmount" });

            // Setup the rest of the effect
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();
            effectBrush.SetSourceParameter("source", compositor.CreateBackdropBrush());

            // Assign the effect to a brush and display it
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = effectBrush;
            sprite.Size = new Vector2((float)element.ActualWidth, (float)element.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(element, sprite);

            // Animate the blur amount
            effectBrush.StartAnimationAsync("Blur.BlurAmount", blur, TimeSpan.FromMilliseconds(ms));

            // Prepare and return the manager
            return new AttachedCompositionEffectWithAutoResize<T>(element, sprite, effectBrush);
        }

        /// <summary>
        /// Creates a new <see cref="AttachedStaticCompositionEffect{T}"/> instance for the target element
        /// </summary>
        /// <typeparam name="T">The type of element to use to host the effect</typeparam>
        /// <param name="element">The target element</param>
        [MustUseReturnValue, NotNull]
        public static AttachedStaticCompositionEffect<T> GetAttachedHostBackdropBlur<T>(
            [NotNull] this T element) where T : FrameworkElement
        {
            // Setup the host backdrop effect
            Visual visual = ElementCompositionPreview.GetElementVisual(element);
            Compositor compositor = visual.Compositor;
            CompositionBackdropBrush brush = compositor.CreateHostBackdropBrush();
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = brush;
            sprite.Size = new Vector2((float)element.ActualWidth, (float)element.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(element, sprite);
            return new AttachedStaticCompositionEffect<T>(element, sprite, brush);
        }

        #endregion

        #region Animated effects

        /// <summary>
        /// Creates a new <see cref="AttachedAnimatableCompositionEffect{T}"/> instance for the target element
        /// </summary>
        /// <typeparam name="T">The type of element to blur</typeparam>
        /// <param name="element">The target element</param>
        /// <param name="on">The amount of saturation effect to apply</param>
        /// <param name="off">The default amount of saturation effect to apply</param>
        /// <param name="initiallyVisible">Indicates whether or not to apply the effect right away</param>
        [MustUseReturnValue, NotNull]
        public static async Task<AttachedAnimatableCompositionEffect<T>> GetAttachedAnimatableSaturationEffectAsync<T>(
            [NotNull] this T element, float on, float off, bool initiallyVisible) where T : FrameworkElement
        {
            // Get the compositor
            Visual visual = await DispatcherHelper.GetFromUIThreadAsync(element.GetVisual);
            Compositor compositor = visual.Compositor;

            // Create the saturation effect and the effect factory
            SaturationEffect saturationEffect = new SaturationEffect
            {
                Name = "SEffect",
                Saturation = initiallyVisible ? off : on,
                Source = new CompositionEffectSourceParameter("source")
            };
            const String animationPropertyName = "SEffect.Saturation";
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(saturationEffect, new[] { animationPropertyName });

            // Setup the rest of the effect
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();
            effectBrush.SetSourceParameter("source", compositor.CreateBackdropBrush());

            // Assign the effect to a brush and display it
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = effectBrush;
            await DispatcherHelper.RunOnUIThreadAsync(() =>
            {
                // Adjust the sprite size
                sprite.Size = new Vector2((float)element.ActualWidth, (float)element.ActualHeight);

                // Set the child visual
                ElementCompositionPreview.SetElementChildVisual(element, sprite);
                if (initiallyVisible) element.Opacity = 1;
            });
            return new AttachedAnimatableCompositionEffect<T>(element, sprite, effectBrush, Tuple.Create(animationPropertyName, on, off));
        }

        /// <summary>
        /// Creates a new <see cref="AttachedAnimatableCompositionEffect{T}"/> instance for the target element
        /// </summary>
        /// <typeparam name="T">The type of element to blur</typeparam>
        /// <param name="element">The target element</param>
        /// <param name="on">The amount of blur effect to apply</param>
        /// <param name="off">The default amount of blur effect to apply</param>
        /// <param name="initiallyVisible">Indicates whether or not to apply the effect right away</param>
        [MustUseReturnValue, NotNull]
        public static async Task<AttachedAnimatableCompositionEffect<T>> GetAttachedAnimatableBlurEffectAsync<T>(
            [NotNull] this T element, float on, float off, bool initiallyVisible) where T : FrameworkElement
        {
            // Get the compositor
            Visual visual = await DispatcherHelper.GetFromUIThreadAsync(element.GetVisual);
            Compositor compositor = visual.Compositor;

            // Create the blur effect and the effect factory
            GaussianBlurEffect blurEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BlurAmount = 0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source")
            };
            const String animationPropertyName = "Blur.BlurAmount";
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(blurEffect, new[] { animationPropertyName });

            // Setup the rest of the effect
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();
            effectBrush.SetSourceParameter("source", compositor.CreateBackdropBrush());

            // Assign the effect to a brush and display it
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = effectBrush;
            await DispatcherHelper.RunOnUIThreadAsync(() =>
            {
                // Adjust the sprite size
                sprite.Size = new Vector2((float)element.ActualWidth, (float)element.ActualHeight);

                // Set the child visual
                ElementCompositionPreview.SetElementChildVisual(element, sprite);
                if (initiallyVisible) element.Opacity = 1;
            });
            return new AttachedAnimatableCompositionEffect<T>(element, sprite, effectBrush, Tuple.Create(animationPropertyName, on, off));
        }

        /// <summary>
        /// Creates a new <see cref="AttachedCompositeAnimatableCompositionEffect{T}"/> instance for the target element that
        /// applies both a blur and a saturation effect to the visual item
        /// </summary>
        /// <typeparam name="T">The type of element to blur</typeparam>
        /// <param name="element">The target element</param>
        /// <param name="onBlur">The amount of blur effect to apply</param>
        /// <param name="offBlur">The default amount of blur effect to apply</param>
        /// <param name="onSaturation">The amount of saturation effect to apply</param>
        /// <param name="offSaturation">The default amount of saturation effect to apply</param>
        /// <param name="initiallyVisible">Indicates whether or not to apply the effect right away</param>
        [MustUseReturnValue, NotNull]
        public static async Task<AttachedCompositeAnimatableCompositionEffect<T>> GetAttachedAnimatableBlurAndSaturationEffectAsync<T>(
            [NotNull] this T element, float onBlur, float offBlur, float onSaturation, float offSaturation, bool initiallyVisible) where T : FrameworkElement
        {
            // Get the compositor
            Visual visual = await DispatcherHelper.GetFromUIThreadAsync(element.GetVisual);
            Compositor compositor = visual.Compositor;

            // Create the blur effect, the saturation effect and the effect factory
            GaussianBlurEffect blurEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BlurAmount = 0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source")
            };
            SaturationEffect saturationEffect = new SaturationEffect
            {
                Name = "SEffect",
                Saturation = initiallyVisible ? offSaturation : onSaturation,
                Source = blurEffect
            };
            const String blurParameter = "Blur.BlurAmount", saturationParameter = "SEffect.Saturation";
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(saturationEffect, new[]
            {
                blurParameter,
                saturationParameter
            });

            // Setup the rest of the effect
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();
            effectBrush.SetSourceParameter("source", compositor.CreateBackdropBrush());

            // Assign the effect to a brush and display it
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = effectBrush;
            await DispatcherHelper.RunOnUIThreadAsync(() =>
            {
                // Adjust the sprite size
                sprite.Size = new Vector2((float)element.ActualWidth, (float)element.ActualHeight);

                // Set the child visual
                ElementCompositionPreview.SetElementChildVisual(element, sprite);
                if (initiallyVisible) element.Opacity = 1;
            });

            // Prepare and return the wrapped effect
            return new AttachedCompositeAnimatableCompositionEffect<T>(element, sprite, effectBrush,
                new Dictionary<String, Tuple<float, float>>
                {
                    { blurParameter, Tuple.Create(onBlur, offBlur) },
                    { saturationParameter, Tuple.Create(onSaturation, offSaturation) }
                });
        }

        #endregion
    }
}
