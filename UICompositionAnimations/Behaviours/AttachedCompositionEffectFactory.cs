using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using UICompositionAnimations.Behaviours.Effects;
using UICompositionAnimations.Behaviours.Misc;
using UICompositionAnimations.Composition;
using UICompositionAnimations.Enums;
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
        /// Creates a new <see cref="AttachedStaticCompositionEffect{T}"/> instance for the target element
        /// </summary>
        /// <typeparam name="T">The type of element to blur</typeparam>
        /// <param name="element">The target element</param>
        /// <param name="blur">The amount of blur to apply to the element</param>
        /// <param name="ms">The duration of the initial blur animation, in milliseconds</param>
        /// <param name="disposeOnUnload">Indicates whether or not to automatically dispose and remove the effect when the target element is unloaded</param>
        /// <remarks>This method returns a <see cref="ValueTask{TResult}"/> instance and runs synchronously if called on the UI thread</remarks>
        [ItemNotNull]
        public static async ValueTask<AttachedStaticCompositionEffect<T>> AttachCompositionBlurEffect<T>(
            [NotNull] this T element, float blur, int ms, bool disposeOnUnload = false) where T : FrameworkElement
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
            await AddToTreeAndBindSizeAsync(visual, element, sprite);

            // Animate the blur amount
            effectBrush.StartAnimationAsync("Blur.BlurAmount", blur, TimeSpan.FromMilliseconds(ms)).Forget();

            // Prepare and return the manager
            return new AttachedStaticCompositionEffect<T>(element, sprite, disposeOnUnload);
        }

        /// <summary>
        /// Creates an effect brush that's similar to the official Acrylic brush in the Fall Creator's Update.
        /// The pipeline uses the following effects: BackdropBrush > <see cref="GaussianBlurEffect"/> >
        /// <see cref="ColorSourceEffect"/> > <see cref="BorderEffect"/> with customizable blend factors for each couple of layers
        /// </summary>
        /// <typeparam name="TSource">The type of the element that will be the source for the composition effect</typeparam>
        /// <typeparam name="T">The type of the target element that will host the resulting <see cref="SpriteVisual"/></typeparam>
        /// <param name="element">The <see cref="FrameworkElement"/> that will be the source of the effect</param>
        /// <param name="target">The target host for the resulting effect</param>
        /// <param name="blur">The amount of blur to apply to the element</param>
        /// <param name="ms">The duration of the initial blur animation, in milliseconds</param>
        /// <param name="color">The tint color for the effect</param>
        /// <param name="colorMix">The opacity of the color over the blurred background</param>
        /// <param name="saturation">An optional parameter to set the overall saturation of the effect (if null, it will default to 1)</param>
        /// <param name="canvas">The optional source <see cref="CanvasControl"/> to generate the noise image using Win2D</param>
        /// <param name="uri">The path of the noise image to use</param>
        /// <param name="timeThreshold">The maximum time to wait for the Win2D device to be restored in case of initial failure</param>
        /// <param name="reload">Indicates whether or not to force the reload of the Win2D image</param>
        /// <param name="fadeIn">Indicates whether or not to fade the effect in</param>
        /// <param name="disposeOnUnload">Indicates whether or not to automatically dispose and remove the effect when the target element is unloaded</param>
        [ItemNotNull]
        public static async Task<AttachedStaticCompositionEffect<T>> AttachCompositionInAppCustomAcrylicEffectAsync<TSource, T>(
            [NotNull] this TSource element, [NotNull] T target, float blur, int ms, Color color, float colorMix, float? saturation,
            [CanBeNull] CanvasControl canvas, [NotNull] Uri uri, int timeThreshold = 1000, bool reload = false, bool fadeIn = false, bool disposeOnUnload = false)
            where TSource : FrameworkElement
            where T : FrameworkElement
        {
            // Percentage check
            if (saturation < 0 || saturation > 1) throw new ArgumentOutOfRangeException("The input saturation value must be in the [0,1] range");
            if (colorMix <= 0 || colorMix >= 1) throw new ArgumentOutOfRangeException("The mix factors must be in the [0,1] range");
            if (timeThreshold <= 0) throw new ArgumentOutOfRangeException("The time threshold must be a positive number");

            // Setup the compositor
            Visual visual = ElementCompositionPreview.GetElementVisual(element);
            Compositor compositor = visual.Compositor;

            // Prepare a luminosity to alpha effect to adjust the background contrast
            CompositionBackdropBrush backdropBrush = compositor.CreateBackdropBrush();
            const String
                blurName = "Blur",
                blurParameterName = "Blur.BlurAmount";
            GaussianBlurEffect blurEffect = new GaussianBlurEffect
            {
                Name = blurName,
                BlurAmount = 0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter(nameof(backdropBrush))
            };

            // Background with blur and tint overlay
            IDictionary<String, CompositionBrush> sourceParameters = new Dictionary<String, CompositionBrush>
            {
                { nameof(backdropBrush), backdropBrush }
            };

            // Get the noise brush using Win2D
            IGraphicsEffect source = await AcrylicEffectHelper.ConcatenateEffectWithTintAndBorderAsync(compositor,
                blurEffect, sourceParameters, color, colorMix, canvas, uri, timeThreshold, reload);

            // Add the final saturation effect if needed
            if (saturation != null)
            {
                SaturationEffect saturationEffect = new SaturationEffect
                {
                    Saturation = saturation.Value,
                    Source = source
                };
                source = saturationEffect;
            }

            // Make sure the Win2D brush was loaded correctly
            CompositionEffectFactory factory = compositor.CreateEffectFactory(source, new[] { blurParameterName });

            // Create the effect factory and apply the final effect
            CompositionEffectBrush effectBrush = factory.CreateBrush();
            foreach (KeyValuePair<String, CompositionBrush> pair in sourceParameters)
            {
                effectBrush.SetSourceParameter(pair.Key, pair.Value);
            }

            // Create the sprite to display and add it to the visual tree
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = effectBrush;

            // Assign the visual
            if (fadeIn)
            {
                sprite.StopAnimation("Opacity");
                sprite.Opacity = 0;
            }
            await AddToTreeAndBindSizeAsync(target.GetVisual(), target, sprite);
            if (fadeIn)
            {
                // Fade the effect in
                ScalarKeyFrameAnimation opacityAnimation = sprite.Compositor.CreateScalarKeyFrameAnimation(1, 0,
                    TimeSpan.FromMilliseconds(ms), null, sprite.GetEasingFunction(EasingFunctionNames.Linear));
                sprite.StartAnimation("Opacity", opacityAnimation);
            }

            // Animate the blur and return the result
            effectBrush.StartAnimationAsync(blurParameterName, blur, TimeSpan.FromMilliseconds(ms)).Forget();
            return new AttachedStaticCompositionEffect<T>(target, sprite, disposeOnUnload);
        }

        /// <summary>
        /// Creates a new <see cref="AttachedStaticCompositionEffect{T}"/> instance for the target element
        /// </summary>
        /// <typeparam name="T">The type of element to use to host the effect</typeparam>
        /// <param name="element">The target element</param>
        /// <param name="disposeOnUnload">Indicates whether or not to automatically dispose and remove the effect when the target element is unloaded</param>
        /// <remarks>This method returns a <see cref="ValueTask{TResult}"/> instance and runs synchronously if called on the UI thread</remarks>
        [ItemNotNull]
        public static async ValueTask<AttachedStaticCompositionEffect<T>> AttachCompositionHostBackdropBlurEffect<T>(
            [NotNull] this T element, bool disposeOnUnload = false) where T : FrameworkElement
        {
            // Setup the host backdrop effect
            Visual visual = ElementCompositionPreview.GetElementVisual(element);
            Compositor compositor = visual.Compositor;
            CompositionBackdropBrush brush = compositor.CreateHostBackdropBrush();
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = brush;
            await AddToTreeAndBindSizeAsync(visual, element, sprite);
            return new AttachedStaticCompositionEffect<T>(element, sprite, disposeOnUnload);
        }

        /// <summary>
        /// Creates an effect brush that's similar to the official Acrylic brush in the Fall Creator's Update.
        /// The pipeline uses the following effects: HostBackdropBrush > <see cref="LuminanceToAlphaEffect"/> >
        /// <see cref="OpacityEffect"/> > <see cref="BlendEffect"/> > <see cref="ArithmeticCompositeEffect"/> >
        /// <see cref="ColorSourceEffect"/> > <see cref="BorderEffect"/> with customizable blend factors for each couple of layers
        /// </summary>
        /// <typeparam name="T">The type of the target element that will host the resulting <see cref="SpriteVisual"/></typeparam>
        /// <param name="element">The target element that will host the effect</param>
        /// <param name="color">The tint color for the effect</param>
        /// <param name="colorMix">The opacity of the color over the blurred background</param>
        /// <param name="canvas">The optional source <see cref="CanvasControl"/> to generate the noise image using Win2D</param>
        /// <param name="uri">The path of the noise image to use</param>
        /// <param name="timeThreshold">The maximum time to wait for the Win2D device to be restored in case of initial failure/></param>
        /// <param name="reload">Indicates whether or not to force the reload of the Win2D image</param>
        /// <param name="disposeOnUnload">Indicates whether or not to automatically dispose and remove the effect when the target element is unloaded</param>
        [ItemNotNull]
        public static async Task<AttachedStaticCompositionEffect<T>> AttachCompositionCustomAcrylicEffectAsync<T>(
            [NotNull] this T element, Color color, float colorMix,
            [CanBeNull] CanvasControl canvas, [NotNull] Uri uri, int timeThreshold = 1000, bool reload = false, bool disposeOnUnload = false) 
            where T : FrameworkElement
        {
            // Percentage check
            if (colorMix <= 0 || colorMix >= 1) throw new ArgumentOutOfRangeException("The mix factors must be in the [0,1] range");
            if (timeThreshold <= 0) throw new ArgumentOutOfRangeException("The time threshold must be a positive number");

            // Setup the compositor
            Visual visual = ElementCompositionPreview.GetElementVisual(element);
            Compositor compositor = visual.Compositor;

            // Prepare a luminosity to alpha effect to adjust the background contrast
            CompositionBackdropBrush hostBackdropBrush = compositor.CreateHostBackdropBrush();
            CompositionEffectSourceParameter backgroundParameter = new CompositionEffectSourceParameter(nameof(hostBackdropBrush));
            LuminanceToAlphaEffect alphaEffect = new LuminanceToAlphaEffect { Source = backgroundParameter };
            OpacityEffect opacityEffect = new OpacityEffect
            {
                Source = alphaEffect,
                Opacity = 0.4f // Reduce the amount of the effect to avoid making bright areas completely black
            };

            // Layer [0,1,3] - Desktop background with blur and tint overlay
            BlendEffect alphaBlend = new BlendEffect
            {
                Background = backgroundParameter,
                Foreground = opacityEffect,
                Mode = BlendEffectMode.Overlay
            };
            IDictionary<String, CompositionBrush> sourceParameters = new Dictionary<String, CompositionBrush>
            {
                { nameof(hostBackdropBrush), hostBackdropBrush }
            };

            // Get the noise brush using Win2D
            IGraphicsEffect source = await AcrylicEffectHelper.ConcatenateEffectWithTintAndBorderAsync(compositor,
                alphaBlend, sourceParameters, color, colorMix, canvas, uri, timeThreshold, reload);

            // Make sure the Win2D brush was loaded correctly
            CompositionEffectFactory factory = compositor.CreateEffectFactory(source);

            // Create the effect factory and apply the final effect
            CompositionEffectBrush effectBrush = factory.CreateBrush();
            foreach (KeyValuePair<String, CompositionBrush> pair in sourceParameters)
            {
                effectBrush.SetSourceParameter(pair.Key, pair.Value);
            }

            // Create the sprite to display and add it to the visual tree
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = effectBrush;
            await AddToTreeAndBindSizeAsync(visual, element, sprite);
            return new AttachedStaticCompositionEffect<T>(element, sprite, disposeOnUnload);
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
        /// <param name="disposeOnUnload">Indicates whether or not to automatically dispose and remove the effect when the target element is unloaded</param>
        [ItemNotNull]
        public static async Task<AttachedAnimatableCompositionEffect<T>> AttachCompositionAnimatableSaturationEffectAsync<T>(
            [NotNull] this T element, float on, float off, bool initiallyVisible, bool disposeOnUnload = false) 
            where T : FrameworkElement
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
            await AddToTreeAndBindSizeAsync(visual, element, sprite);
            if (initiallyVisible) await DispatcherHelper.RunOnUIThreadAsync(() => element.Opacity = 1);
            return new AttachedAnimatableCompositionEffect<T>(element, sprite, new CompositionAnimationParameters(animationPropertyName, on, off), disposeOnUnload);
        }

        /// <summary>
        /// Creates a new <see cref="AttachedAnimatableCompositionEffect{T}"/> instance for the target element
        /// </summary>
        /// <typeparam name="T">The type of element to blur</typeparam>
        /// <param name="element">The target element</param>
        /// <param name="on">The amount of blur effect to apply</param>
        /// <param name="off">The default amount of blur effect to apply</param>
        /// <param name="initiallyVisible">Indicates whether or not to apply the effect right away</param>
        /// <param name="disposeOnUnload">Indicates whether or not to automatically dispose and remove the effect when the target element is unloaded</param>
        [ItemNotNull]
        public static async Task<AttachedAnimatableCompositionEffect<T>> AttachCompositionAnimatableBlurEffectAsync<T>(
            [NotNull] this T element, float on, float off, bool initiallyVisible, bool disposeOnUnload = false) where T : FrameworkElement
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
            await AddToTreeAndBindSizeAsync(visual, element, sprite);
            if (initiallyVisible) await DispatcherHelper.RunOnUIThreadAsync(() => element.Opacity = 1);
            return new AttachedAnimatableCompositionEffect<T>(element, sprite, new CompositionAnimationParameters(animationPropertyName, on, off), disposeOnUnload);
        }

        /// <summary>
        /// Creates a new <see cref="AttachedAnimatableCompositionEffect{T}"/> instance with blur, tint and noise effects
        /// </summary>
        /// <typeparam name="TSource">The type of the element that will be the source for the composition effect</typeparam>
        /// <typeparam name="T">The type of the target element that will host the resulting <see cref="SpriteVisual"/></typeparam>
        /// <param name="element">The <see cref="FrameworkElement"/> that will be the source of the effect</param>
        /// <param name="target">The target host for the resulting effect</param>
        /// <param name="on">The amount of blur effect to apply</param>
        /// <param name="off">The default amount of blur effect to apply</param>
        /// <param name="initiallyVisible">Indicates whether or not to apply the effect right away</param>
        /// <param name="color">The tint color for the effect</param>
        /// <param name="colorMix">The opacity of the color over the blurred background</param>
        /// <param name="canvas">The optional source <see cref="CanvasControl"/> to generate the noise image using Win2D</param>
        /// <param name="uri">The path of the noise image to use</param>
        /// <param name="timeThreshold">The maximum time to wait for the Win2D device to be restored in case of initial failure</param>
        /// <param name="reload">Indicates whether or not to force the reload of the Win2D image</param>
        /// <param name="disposeOnUnload">Indicates whether or not to automatically dispose and remove the effect when the target element is unloaded</param>
        [ItemNotNull]
        public static async Task<AttachedAnimatableCompositionEffect<T>> AttachCompositionAnimatableInAppCustomAcrylicEffectAsync<TSource, T>(
            [NotNull] this TSource element, [NotNull] T target,
            float on, float off, bool initiallyVisible,
            Color color, float colorMix, [CanBeNull] CanvasControl canvas, [NotNull] Uri uri,
            int timeThreshold = 1000, bool reload = false, bool disposeOnUnload = false) 
            where TSource : FrameworkElement
            where T : FrameworkElement
        {
            // Get the compositor
            Visual visual = await DispatcherHelper.GetFromUIThreadAsync(element.GetVisual);
            Compositor compositor = visual.Compositor;

            // Create the blur effect and the effect factory
            CompositionBackdropBrush backdropBrush = compositor.CreateBackdropBrush();
            GaussianBlurEffect blurEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BlurAmount = 0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter(nameof(backdropBrush))
            };
            const String animationPropertyName = "Blur.BlurAmount";

            // Prepare the dictionary with the parameters to add
            IDictionary<String, CompositionBrush> sourceParameters = new Dictionary<String, CompositionBrush>
            {
                { nameof(backdropBrush), backdropBrush }
            };

            // Get the noise brush using Win2D
            IGraphicsEffect source = await AcrylicEffectHelper.ConcatenateEffectWithTintAndBorderAsync(compositor,
                blurEffect, sourceParameters, color, colorMix, canvas, uri, timeThreshold, reload);

            // Make sure the Win2D brush was loaded correctly
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(source, new[] { animationPropertyName });

            // Create the effect factory and apply the final effect
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();
            foreach (KeyValuePair<String, CompositionBrush> pair in sourceParameters)
            {
                effectBrush.SetSourceParameter(pair.Key, pair.Value);
            }

            // Assign the effect to a brush and display it
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = effectBrush;
            await AddToTreeAndBindSizeAsync(target.GetVisual(), target, sprite);
            if (initiallyVisible) await DispatcherHelper.RunOnUIThreadAsync(() => element.Opacity = 1);
            return new AttachedAnimatableCompositionEffect<T>(target, sprite, new CompositionAnimationParameters(animationPropertyName, on, off), disposeOnUnload);
        }

        /// <summary>
        /// Creates a new <see cref="AttachedAnimatableCompositionEffect{T}"/> instance with blur, tint and noise effects
        /// </summary>
        /// <typeparam name="TSource">The type of the element that will be the source for the composition effect</typeparam>
        /// <typeparam name="T">The type of the target element that will host the resulting <see cref="SpriteVisual"/></typeparam>
        /// <param name="element">The <see cref="FrameworkElement"/> that will be the source of the effect</param>
        /// <param name="target">The target host for the resulting effect</param>
        /// <param name="onBlur">The amount of blur effect to apply</param>
        /// <param name="offBlur">The default amount of blur effect to apply</param>
        /// <param name="onSaturation">The amount of saturation effect to apply</param>
        /// <param name="offSaturation">The default amount of saturation effect to apply</param>
        /// <param name="initiallyVisible">Indicates whether or not to apply the effect right away</param>
        /// <param name="color">The tint color for the effect</param>
        /// <param name="colorMix">The opacity of the color over the blurred background</param>
        /// <param name="canvas">The optional source <see cref="CanvasControl"/> to generate the noise image using Win2D</param>
        /// <param name="uri">The path of the noise image to use</param>
        /// <param name="timeThreshold">The maximum time to wait for the Win2D device to be restored in case of initial failure</param>
        /// <param name="reload">Indicates whether or not to force the reload of the Win2D image</param>
        /// <param name="disposeOnUnload">Indicates whether or not to automatically dispose and remove the effect when the target element is unloaded</param>
        [ItemNotNull]
        public static async Task<AttachedCompositeAnimatableCompositionEffect<T>> AttachCompositionAnimatableInAppCustomAcrylicAndSaturationEffectAsync<TSource, T>(
            [NotNull] this TSource element, [NotNull] T target,
            float onBlur, float offBlur,
            float onSaturation, float offSaturation,
            bool initiallyVisible,
            Color color, float colorMix, [CanBeNull] CanvasControl canvas, [NotNull] Uri uri,
            int timeThreshold = 1000, bool reload = false, bool disposeOnUnload = false)
            where TSource : FrameworkElement
            where T : FrameworkElement
        {
            // Get the compositor
            Visual visual = await DispatcherHelper.GetFromUIThreadAsync(element.GetVisual);
            Compositor compositor = visual.Compositor;

            // Create the blur effect and the effect factory
            CompositionBackdropBrush backdropBrush = compositor.CreateBackdropBrush();
            GaussianBlurEffect blurEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BlurAmount = 0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter(nameof(backdropBrush))
            };
            const String animationPropertyName = "Blur.BlurAmount";

            // Prepare the dictionary with the parameters to add
            IDictionary<String, CompositionBrush> sourceParameters = new Dictionary<String, CompositionBrush>
            {
                { nameof(backdropBrush), backdropBrush }
            };

            // Get the noise brush using Win2D
            IGraphicsEffect source = await AcrylicEffectHelper.ConcatenateEffectWithTintAndBorderAsync(compositor,
                blurEffect, sourceParameters, color, colorMix, canvas, uri, timeThreshold, reload);

            // Add the final saturation effect
            SaturationEffect saturationEffect = new SaturationEffect
            {
                Name = "SEffect",
                Saturation = initiallyVisible ? offSaturation : onSaturation,
                Source = source
            };
            const String saturationParameter = "SEffect.Saturation";

            // Make sure the Win2D brush was loaded correctly
            CompositionEffectFactory effectFactory = compositor.CreateEffectFactory(saturationEffect, new[]
            {
                animationPropertyName,
                saturationParameter
            });

            // Create the effect factory and apply the final effect
            CompositionEffectBrush effectBrush = effectFactory.CreateBrush();
            foreach (KeyValuePair<String, CompositionBrush> pair in sourceParameters)
            {
                effectBrush.SetSourceParameter(pair.Key, pair.Value);
            }

            // Assign the effect to a brush and display it
            SpriteVisual sprite = compositor.CreateSpriteVisual();
            sprite.Brush = effectBrush;
            await AddToTreeAndBindSizeAsync(target.GetVisual(), target, sprite);
            if (initiallyVisible) await DispatcherHelper.RunOnUIThreadAsync(() => element.Opacity = 1);
            return new AttachedCompositeAnimatableCompositionEffect<T>(target, sprite,
                new Dictionary<String, CompositionAnimationValueParameters>
                {
                    { animationPropertyName, new CompositionAnimationValueParameters(onBlur, offBlur) },
                    { saturationParameter, new CompositionAnimationValueParameters(onSaturation, offSaturation) }
                }, disposeOnUnload);
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
        /// <param name="disposeOnUnload">Indicates whether or not to automatically dispose and remove the effect when the target element is unloaded</param>
        [ItemNotNull]
        public static async Task<AttachedCompositeAnimatableCompositionEffect<T>> AttachCompositionAnimatableBlurAndSaturationEffectAsync<T>(
            [NotNull] this T element, float onBlur, float offBlur, float onSaturation, float offSaturation, bool initiallyVisible, bool disposeOnUnload = false) 
            where T : FrameworkElement
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
            await AddToTreeAndBindSizeAsync(visual, element, sprite);
            if (initiallyVisible) await DispatcherHelper.RunOnUIThreadAsync(() => element.Opacity = 1);

            // Prepare and return the wrapped effect
            return new AttachedCompositeAnimatableCompositionEffect<T>(element, sprite,
                new Dictionary<String, CompositionAnimationValueParameters>
                {
                    { blurParameter, new CompositionAnimationValueParameters(onBlur, offBlur) },
                    { saturationParameter, new CompositionAnimationValueParameters(onSaturation, offSaturation) }
                }, disposeOnUnload);
        }

        #endregion

        #region Tools

        /// <summary>
        /// Adds a <see cref="Visual"/> object on top of the target <see cref="UIElement"/> and binds the size of the two items with an expression animation
        /// </summary>
        /// <param name="host">The <see cref="Visual"/> object that will host the effect</param>
        /// <param name="element">The target <see cref="UIElement"/> (bound to the given visual) that will host the effect</param>
        /// <param name="visual">The source <see cref="Visual"/> object to display</param>
        private static async Task AddToTreeAndBindSizeAsync([NotNull] Visual host, [NotNull] UIElement element, [NotNull] Visual visual)
        {
            // Add the shadow as a child of the host in the visual tree
            await DispatcherHelper.RunOnUIThreadAsync(() => ElementCompositionPreview.SetElementChildVisual(element, visual));

            // Make sure size of shadow host and shadow visual always stay in sync
            ExpressionAnimation bindSizeAnimation = host.Compositor.CreateExpressionAnimation($"{nameof(host)}.Size");
            bindSizeAnimation.SetReferenceParameter(nameof(host), host);

            // Start the animation
            visual.StartAnimation("Size", bindSizeAnimation);
        }

        #endregion
    }
}