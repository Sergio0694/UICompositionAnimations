using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas.Effects;
using UICompositionAnimations.Enums;
using UICompositionAnimations.Helpers;

namespace UICompositionAnimations.Behaviours
{
    /// <summary>
    /// A <see langword="class"/> that allows to build custom effects pipelines and create <see cref="CompositionBrush"/> instances from them
    /// </summary>
    [PublicAPI]
    public sealed class CompositionBrushBuilder
    {
        /// <summary>
        /// The <see cref="Func{TResult}"/> instance used to produce the output <see cref="IGraphicsEffectSource"/> for this pipeline
        /// </summary>
        [NotNull]
        private readonly Func<Task<IGraphicsEffectSource>> SourceProducer;

        /// <summary>
        /// The collection of info on the parameters that need to be initialized after creating the final <see cref="CompositionBrush"/>
        /// </summary>
        [NotNull]
        private readonly IReadOnlyDictionary<String, Func<Task<CompositionBrush>>> LazyParameters;

        /// <summary>
        /// The collection of animation parameters present in the current pipeline
        /// </summary>
        [NotNull, ItemNotNull]
        private readonly IReadOnlyCollection<String> AnimationParameters;

        #region Constructors

        /// <summary>
        /// Constructor used to initialize a pipeline from a <see cref="CompositionBrush"/>, for example using the <see cref="Compositor.CreateBackdropBrush"/> method
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will return the initial <see cref="CompositionBrush"/></param>
        private CompositionBrushBuilder([NotNull] Func<Task<CompositionBrush>> factory)
        {
            SourceProducer = () => Task.FromResult(new CompositionEffectSourceParameter("source").To<IGraphicsEffectSource>());
            LazyParameters = new Dictionary<String, Func<Task<CompositionBrush>>> { { "source", factory } };
            AnimationParameters = new string[0];
        }

        /// <summary>
        /// Constructor used to initialize a pipeline from a custom <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will return the initial <see cref="IGraphicsEffectSource"/></param>
        private CompositionBrushBuilder([NotNull] Func<Task<IGraphicsEffectSource>> factory)
        {
            SourceProducer = factory;
            LazyParameters = new Dictionary<String, Func<Task<CompositionBrush>>>();
            AnimationParameters = new string[0];
        }

        /// <summary>
        /// Constructor used to create a new instance obtained by concatenation between the current pipeline and the input effect info
        /// </summary>
        /// <param name="source">The source pipeline to attach the new effect to</param>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="lazy">The collection of <see cref="CompositionBrush"/> instances that needs to be initialized for the new effect</param>
        /// <param name="animations">The collection of animation properties for the new effect</param>
        private CompositionBrushBuilder(
            [NotNull] CompositionBrushBuilder source, 
            [NotNull] Func<Task<IGraphicsEffectSource>> factory, [NotNull] IReadOnlyDictionary<String, Func<Task<CompositionBrush>>> lazy, [NotNull, ItemNotNull] IReadOnlyCollection<String> animations)
        {
            SourceProducer = factory;

            // Merge the lazy parameters dictionaries
            if (source.LazyParameters.Keys.FirstOrDefault(lazy.ContainsKey) is String key)
                throw new InvalidOperationException($"The key {key} already exists in the current pipeline");
            LazyParameters = new Dictionary<String, Func<Task<CompositionBrush>>>(source.LazyParameters.Concat(lazy));

            // Merge the animation parameters
            if (source.AnimationParameters.FirstOrDefault(animations.Contains) is String animation)
                throw new InvalidOperationException($"The animation {animation} already exists in the current pipeline");
            AnimationParameters = source.AnimationParameters.Concat(animations).ToArray();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from the <see cref="CompositionBrush"/> returned by <see cref="Compositor.CreateBackdropBrush"/>
        /// </summary>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromBackdropBrush() => new CompositionBrushBuilder(() => Task.FromResult<CompositionBrush>(Window.Current.Compositor.CreateBackdropBrush()));

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from the <see cref="CompositionBrush"/> returned by <see cref="Compositor.CreateHostBackdropBrush"/>
        /// </summary>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromHostBackdropBrush() => new CompositionBrushBuilder(() => Task.FromResult<CompositionBrush>(Window.Current.Compositor.CreateHostBackdropBrush()));

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from a solid <see cref="CompositionBrush"/> with the specified color
        /// </summary>
        /// <param name="color">The desired <see cref="Color"/> for the initial <see cref="CompositionBrush"/></param>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromColor(Color color) => new CompositionBrushBuilder(() => Task.FromResult(new ColorSourceEffect { Color = color }.To<IGraphicsEffectSource>()));

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from the input <see cref="CompositionBrush"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that synchronously returns a <see cref="CompositionBrush"/> instance to start the pipeline</param>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromBrush(Func<CompositionBrush> factory) => new CompositionBrushBuilder(() => Task.FromResult(factory()));

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from the input <see cref="CompositionBrush"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that asynchronously returns a <see cref="CompositionBrush"/> instance to start the pipeline</param>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromBrush(Func<Task<CompositionBrush>> factory) => new CompositionBrushBuilder(factory);

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from the input <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that synchronously returns a <see cref="IGraphicsEffectSource"/> instance to start the pipeline</param>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromEffect(Func<IGraphicsEffectSource> factory) => new CompositionBrushBuilder(() => Task.FromResult(factory()));

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from the input <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that asynchronously returns a <see cref="IGraphicsEffectSource"/> instance to start the pipeline</param>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromEffect(Func<Task<IGraphicsEffectSource>> factory) => new CompositionBrushBuilder(factory);

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from a Win2D image
        /// </summary>
        /// <param name="uri">The path for the image to load</param>
        /// <param name="options">Indicates whether or not to force the reload of the Win2D image</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromImage([NotNull] Uri uri, BitmapCacheMode options, BitmapDPIMode dpiMode)
        {
            return new CompositionBrushBuilder(() => Win2DImageHelper.LoadImageAsync(Window.Current.Compositor, uri, options, dpiMode).ContinueWith(t => t.Result as CompositionBrush));
        }

        #endregion

        #region Effects

        public CompositionBrushBuilder Blur(float blur, EffectBorderMode mode = EffectBorderMode.Hard, EffectOptimization optimization = EffectOptimization.Balanced)
        {
            // Blur effect
            async Task<IGraphicsEffectSource> Factory() => new GaussianBlurEffect
            {
                Name = "Blur",
                BlurAmount = blur,
                BorderMode = mode,
                Optimization = optimization,
                Source = await SourceProducer()
            };

            // Info
            String[] parameters = { "Blur.BlurAmount" };
            return new CompositionBrushBuilder(this, Factory, new Dictionary<String, Func<Task<CompositionBrush>>>(), parameters);
        }

        #endregion

        public async Task<CompositionBrush> BuildAsync()
        {
            // Validate the pipeline and build the effects factory
            if (!(await SourceProducer() is IGraphicsEffect effect)) throw new InvalidOperationException("The pipeline doesn't contain a valid effects sequence");
            CompositionEffectFactory factory = Window.Current.Compositor.CreateEffectFactory(effect, AnimationParameters);

            // Create the effect factory and apply the final effect
            CompositionEffectBrush effectBrush = factory.CreateBrush();
            foreach (KeyValuePair<String, Func<Task<CompositionBrush>>> pair in LazyParameters)
                effectBrush.SetSourceParameter(pair.Key, await pair.Value());
            return effectBrush;
        }
    }

    public static class CompositionBrushExtensions
    {
        /// <summary>
        /// Adds a <see cref="Visual"/> object on top of the target <see cref="UIElement"/> and binds the size of the two items with an expression animation
        /// </summary>
        /// <param name="host">The <see cref="Visual"/> object that will host the effect</param>
        /// <param name="element">The target <see cref="UIElement"/> (bound to the given visual) that will host the effect</param>
        /// <param name="visual">The source <see cref="Visual"/> object to display</param>
        public static void AttachToElement([NotNull] this CompositionBrush brush, [NotNull] FrameworkElement target)
        {
            // Add the brush to a sprite and attach it to the target element
            SpriteVisual sprite = Window.Current.Compositor.CreateSpriteVisual();
            sprite.Brush = brush;
            sprite.Size = new Vector2((float)target.ActualWidth, (float)target.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(target, sprite);

            // Keep the sprite size in sync
            Visual visual = target.GetVisual();
            ExpressionAnimation bindSizeAnimation = Window.Current.Compositor.CreateExpressionAnimation($"{nameof(visual)}.Size");
            bindSizeAnimation.SetReferenceParameter(nameof(visual), visual);

            // Start the animation
            sprite.StartAnimation("Size", bindSizeAnimation);
        }
    }
}
