using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using UICompositionAnimations.Brushes;
using UICompositionAnimations.Enums;
using UICompositionAnimations.Helpers;
using UICompositionAnimations.Helpers.Cache;

namespace UICompositionAnimations.Behaviours
{
    /// <summary>
    /// A <see langword="delegate"/> that represents a custom effect animation that can be applied to a <see cref="CompositionBrush"/>
    /// </summary>
    /// <param name="brush">The target <see cref="CompositionBrush"/> instance to use to start the animation</param>
    /// <param name="value">The animation target value</param>
    /// <param name="ms">The animation duration, in milliseconds</param>
    public delegate Task EffectAnimation([NotNull] CompositionBrush brush, float value, int ms);

    /// <summary>
    /// A <see langword="class"/> that allows to build custom effects pipelines and create <see cref="CompositionBrush"/> instances from them
    /// </summary>
    [PublicAPI]
    public sealed class PipelineBuilder
    {
        /// <summary>
        /// The <see cref="Func{TResult}"/> instance used to produce the output <see cref="IGraphicsEffectSource"/> for this pipeline
        /// </summary>
        [NotNull]
        private readonly Func<Task<IGraphicsEffectSource>> SourceProducer;

        /// <summary>
        /// The collection of animation properties present in the current pipeline
        /// </summary>
        [NotNull, ItemNotNull]
        private readonly IReadOnlyCollection<string> AnimationProperties;

        /// <summary>
        /// The collection of info on the parameters that need to be initialized after creating the final <see cref="CompositionBrush"/>
        /// </summary>
        [NotNull]
        private readonly IReadOnlyDictionary<string, Func<Task<CompositionBrush>>> LazyParameters;

        #region Constructors

        /// <summary>
        /// Constructor used to initialize a pipeline from a <see cref="CompositionBrush"/>, for example using the <see cref="Compositor.CreateBackdropBrush"/> method
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will return the initial <see cref="CompositionBrush"/></param>
        private PipelineBuilder([NotNull] Func<Task<CompositionBrush>> factory)
        {
            string
                guid = Guid.NewGuid().ToString("N"),
                replaced = Regex.Replace(guid, "[0-9]", "_"),
                id = new string(replaced.ToCharArray().Select((c, i) => c == '_' ? char.ToUpper((char)('a' + i % 26)) : c).ToArray());
            SourceProducer = () => Task.FromResult(new CompositionEffectSourceParameter(id).To<IGraphicsEffectSource>());
            LazyParameters = new Dictionary<string, Func<Task<CompositionBrush>>> { { id, factory } };
            AnimationProperties = new string[0];
        }

        /// <summary>
        /// Base constructor used to create a new instance from scratch
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="animations">The collection of animation properties for the new effect</param>
        /// <param name="lazy">The collection of <see cref="CompositionBrush"/> instances that needs to be initialized for the new effect</param>
        private PipelineBuilder([NotNull] Func<Task<IGraphicsEffectSource>> factory, [NotNull] [ItemNotNull] IReadOnlyCollection<string> animations, [NotNull] IReadOnlyDictionary<string, Func<Task<CompositionBrush>>> lazy)
        {
            SourceProducer = factory;
            AnimationProperties = animations;
            LazyParameters = lazy;
        }

        /// <summary>
        /// Constructor used to initialize a pipeline from a custom <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will return the initial <see cref="IGraphicsEffectSource"/></param>
        private PipelineBuilder([NotNull] Func<Task<IGraphicsEffectSource>> factory)
            : this(factory, new string[0], new Dictionary<string, Func<Task<CompositionBrush>>>())
        { }

        /// <summary>
        /// Constructor used to create a new instance obtained by concatenation between the current pipeline and the input effect info
        /// </summary>
        /// <param name="source">The source pipeline to attach the new effect to</param>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="animations">The collection of animation properties for the new effect</param>
        /// <param name="lazy">The collection of <see cref="CompositionBrush"/> instances that needs to be initialized for the new effect</param>
        private PipelineBuilder([NotNull] PipelineBuilder source,
            [NotNull] Func<Task<IGraphicsEffectSource>> factory,
            [CanBeNull] [ItemNotNull] IReadOnlyCollection<string> animations = null, [CanBeNull] IReadOnlyDictionary<string, Func<Task<CompositionBrush>>> lazy = null)
            : this(factory, animations?.Merge(source.AnimationProperties) ?? source.AnimationProperties, lazy?.Merge(source.LazyParameters) ?? source.LazyParameters)
        { }

        /// <summary>
        /// Constructor used to create a new instance obtained by merging two separate pipelines
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="a">The first pipeline to merge</param>
        /// <param name="b">The second pipeline to merge</param>
        /// <param name="animations">The collection of animation properties for the new effect</param>
        /// <param name="lazy">The collection of <see cref="CompositionBrush"/> instances that needs to be initialized for the new effect</param>
        private PipelineBuilder([NotNull] Func<Task<IGraphicsEffectSource>> factory,
            [NotNull] PipelineBuilder a, [NotNull] PipelineBuilder b,
            [CanBeNull] [ItemNotNull] IReadOnlyCollection<string> animations = null, [CanBeNull] IReadOnlyDictionary<string, Func<Task<CompositionBrush>>> lazy = null)
            : this(factory, animations?.Merge(a.AnimationProperties.Merge(b.AnimationProperties)) ?? a.AnimationProperties.Merge(b.AnimationProperties), lazy?.Merge(a.LazyParameters.Merge(b.LazyParameters)) ?? a.LazyParameters.Merge(b.LazyParameters))
        { }

        #endregion

        #region Initialization

        // The cache manager for backdrop brushes
        [NotNull]
        private static readonly ThreadSafeCompositionCache<CompositionBrush> BackdropBrushCache = new ThreadSafeCompositionCache<CompositionBrush>();

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the <see cref="CompositionBrush"/> returned by <see cref="Compositor.CreateBackdropBrush"/>
        /// </summary>
        [Pure, NotNull]
        public static PipelineBuilder FromBackdropBrush() => new PipelineBuilder(() => BackdropBrushCache.TryGetInstanceAsync(Window.Current.Compositor.CreateBackdropBrush));

        // The cache manager for host backdrop brushes
        [NotNull]
        private static readonly ThreadSafeCompositionCache<CompositionBrush> HostBackdropBrushCache = new ThreadSafeCompositionCache<CompositionBrush>();

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the <see cref="CompositionBrush"/> returned by <see cref="Compositor.CreateHostBackdropBrush"/>
        /// </summary>
        [Pure, NotNull]
        public static PipelineBuilder FromHostBackdropBrush() => new PipelineBuilder(() => HostBackdropBrushCache.TryGetInstanceAsync(Window.Current.Compositor.CreateHostBackdropBrush));

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from a solid <see cref="CompositionBrush"/> with the specified color
        /// </summary>
        /// <param name="color">The desired color for the initial <see cref="CompositionBrush"/></param>
        [Pure, NotNull]
        public static PipelineBuilder FromColor(Color color) => new PipelineBuilder(() => Task.FromResult(new ColorSourceEffect { Color = color }.To<IGraphicsEffectSource>()));

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the input <see cref="CompositionBrush"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that synchronously returns a <see cref="CompositionBrush"/> instance to start the pipeline</param>
        [Pure, NotNull]
        public static PipelineBuilder FromBrush(Func<CompositionBrush> factory) => new PipelineBuilder(() => Task.FromResult(factory()));

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the input <see cref="CompositionBrush"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that asynchronously returns a <see cref="CompositionBrush"/> instance to start the pipeline</param>
        [Pure, NotNull]
        public static PipelineBuilder FromBrush(Func<Task<CompositionBrush>> factory) => new PipelineBuilder(factory);

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the input <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that synchronously returns a <see cref="IGraphicsEffectSource"/> instance to start the pipeline</param>
        [Pure, NotNull]
        public static PipelineBuilder FromEffect(Func<IGraphicsEffectSource> factory) => new PipelineBuilder(() => Task.FromResult(factory()));

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the input <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that asynchronously returns a <see cref="IGraphicsEffectSource"/> instance to start the pipeline</param>
        [Pure, NotNull]
        public static PipelineBuilder FromEffect(Func<Task<IGraphicsEffectSource>> factory) => new PipelineBuilder(factory);

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from a Win2D image
        /// </summary>
        /// <param name="uri">The path for the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure, NotNull]
        public static PipelineBuilder FromImage([NotNull] Uri uri, DpiMode dpiMode = DpiMode.DisplayDpiWith96AsLowerBound, CacheMode cache = CacheMode.Default)
        {
            return new PipelineBuilder(() => Win2DImageHelper.LoadImageAsync(Window.Current.Compositor, uri, dpiMode, cache).ContinueWith(t => t.Result as CompositionBrush));
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from a Win2D image tiled to cover the available space
        /// </summary>
        /// <param name="uri">The path for the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure, NotNull]
        public static PipelineBuilder FromTiles([NotNull] Uri uri, DpiMode dpiMode = DpiMode.DisplayDpiWith96AsLowerBound, CacheMode cache = CacheMode.Default)
        {
            PipelineBuilder image = FromImage(uri, dpiMode, cache);

            async Task<IGraphicsEffectSource> Factory() => new BorderEffect
            {
                ExtendX = CanvasEdgeBehavior.Wrap,
                ExtendY = CanvasEdgeBehavior.Wrap,
                Source = await image.SourceProducer()
            };

            return new PipelineBuilder(image, Factory);
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the <see cref="CompositionBrush"/> returned by <see cref="Compositor.CreateBackdropBrush"/> on the input <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The source <see cref="UIElement"/> to use to create the pipeline</param>
        [Pure, NotNull]
        public static PipelineBuilder FromUIElement([NotNull] UIElement element)
        {
            return new PipelineBuilder(() => Task.FromResult(element.GetVisual().Compositor.CreateBackdropBrush().To<CompositionBrush>()));
        }

        #endregion

        #region Prebuilt pipelines

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the host backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure, NotNull]
        public static PipelineBuilder FromHostBackdropAcrylic(Color tint, float mix, [NotNull] Uri noiseUri, CacheMode cache = CacheMode.Default)
        {
            return FromHostBackdropBrush()
                .Effect(source => new LuminanceToAlphaEffect { Source = source })
                .Opacity(0.4f)
                .Blend(FromHostBackdropBrush(), BlendEffectMode.Multiply)
                .Tint(tint, mix)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the host backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="tintAnimation">The animation to apply on the tint color of the effect</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure, NotNull]
        public static PipelineBuilder FromHostBackdropAcrylic(Color tint, float mix, out EffectAnimation tintAnimation, [NotNull] Uri noiseUri, CacheMode cache = CacheMode.Default)
        {
            return FromHostBackdropBrush()
                .Effect(source => new LuminanceToAlphaEffect { Source = source })
                .Opacity(0.4f)
                .Blend(FromHostBackdropBrush(), BlendEffectMode.Multiply)
                .Tint(tint, mix, out tintAnimation)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure, NotNull]
        public static PipelineBuilder FromBackdropAcrylic(Color tint, float mix, float blur, [NotNull] Uri noiseUri, CacheMode cache = CacheMode.Default)
        {
            return FromBackdropBrush()
                .Tint(tint, mix)
                .Blur(blur)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="tintAnimation">The animation to apply on the tint color of the effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure, NotNull]
        public static PipelineBuilder FromBackdropAcrylic(
            Color tint, float mix, out EffectAnimation tintAnimation,
            float blur,
            [NotNull] Uri noiseUri, CacheMode cache = CacheMode.Default)
        {
            return FromBackdropBrush()
                .Tint(tint, mix, out tintAnimation)
                .Blur(blur)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="blurAnimation">The animation to apply on the blur effect in the pipeline</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure, NotNull]
        public static PipelineBuilder FromBackdropAcrylic(
            Color tint, float mix,
            float blur, out EffectAnimation blurAnimation,
            [NotNull] Uri noiseUri, CacheMode cache = CacheMode.Default)
        {
            return FromBackdropBrush()
                .Tint(tint, mix)
                .Blur(blur, out blurAnimation)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        /// <summary>
        /// Returns a new <see cref="PipelineBuilder"/> instance that implements the in-app backdrop acrylic effect
        /// </summary>
        /// <param name="tint">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        /// <param name="tintAnimation">The animation to apply on the tint color of the effect</param>
        /// <param name="blur">The amount of blur to apply to the acrylic brush</param>
        /// <param name="blurAnimation">The animation to apply on the blur effect in the pipeline</param>
        /// <param name="noiseUri">The <see cref="Uri"/> for the noise texture to load for the acrylic effect</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure, NotNull]
        public static PipelineBuilder FromBackdropAcrylic(
            Color tint, float mix, out EffectAnimation tintAnimation,
            float blur, out EffectAnimation blurAnimation,
            [NotNull] Uri noiseUri, CacheMode cache = CacheMode.Default)
        {
            return FromBackdropBrush()
                .Tint(tint, mix, out tintAnimation)
                .Blur(blur, out blurAnimation)
                .Blend(FromTiles(noiseUri, cache: cache), BlendEffectMode.Overlay, Placement.Background);
        }

        #endregion

        #region Blends

        /// <summary>
        /// Blends two pipelines using a <see cref="BlendEffect"/> instance with the specified mode
        /// </summary>
        /// <param name="pipeline">The second <see cref="PipelineBuilder"/> instance to blend</param>
        /// <param name="mode">The desired <see cref="BlendEffectMode"/> to use to blend the input pipelines</param>
        /// <param name="sorting">The sorting mode to use with the two input pipelines</param>
        [Pure, NotNull]
        public PipelineBuilder Blend([NotNull] PipelineBuilder pipeline, BlendEffectMode mode, Placement sorting = Placement.Foreground)
        {
            var (foreground, background) = sorting == Placement.Foreground ? (this, pipeline) : (pipeline, this);

            async Task<IGraphicsEffectSource> Factory() => new BlendEffect
            {
                Foreground = await foreground.SourceProducer(),
                Background = await background.SourceProducer(),
                Mode = mode
            };

            return new PipelineBuilder(Factory, foreground, background);
        }

        /// <summary>
        /// Blends two pipelines using an <see cref="CrossFadeEffect"/> instance
        /// </summary>
        /// <param name="pipeline">The second <see cref="PipelineBuilder"/> instance to blend</param>
        /// <param name="factor">The cross fade factor to blend the input effects</param>
        /// <param name="sorting">The sorting mode to use with the two input pipelines</param>
        [Pure, NotNull]
        public PipelineBuilder Mix([NotNull] PipelineBuilder pipeline, float factor = 0.5f, Placement sorting = Placement.Foreground)
        {
            if (factor < 0 || factor > 1) throw new ArgumentOutOfRangeException(nameof(factor), "The factor must be in the [0,1] range");
            var (foreground, background) = sorting == Placement.Foreground ? (this, pipeline) : (pipeline, this);

            async Task<IGraphicsEffectSource> Factory() => new CrossFadeEffect
            {
                CrossFade = factor,
                Source1 = await foreground.SourceProducer(),
                Source2 = await background.SourceProducer()
            };

            return new PipelineBuilder(Factory, foreground, background);
        }

        /// <summary>
        /// Blends two pipelines using an <see cref="CrossFadeEffect"/> instance
        /// </summary>
        /// <param name="pipeline">The second <see cref="PipelineBuilder"/> instance to blend</param>
        /// <param name="factor">The cross fade factor to blend the input effects</param>
        /// <param name="animation">The optional blur animation for the effect</param>
        /// <param name="sorting">The sorting mode to use with the two input pipelines</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        [Pure, NotNull]
        public PipelineBuilder Mix([NotNull] PipelineBuilder pipeline, float factor, out EffectAnimation animation, Placement sorting = Placement.Foreground)
        {
            if (factor < 0 || factor > 1) throw new ArgumentOutOfRangeException(nameof(factor), "The factor must be in the [0,1] range");
            var (foreground, background) = sorting == Placement.Foreground ? (this, pipeline) : (pipeline, this);

            async Task<IGraphicsEffectSource> Factory() => new CrossFadeEffect
            {
                CrossFade = factor,
                Source1 = await foreground.SourceProducer(),
                Source2 = await background.SourceProducer(),
                Name = "Fade"
            };

            animation = (brush, value, ms) =>
            {
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException(nameof(value), "The factor must be in the [0,1] range");
                return brush.StartAnimationAsync("Fade.CrossFade", value, TimeSpan.FromMilliseconds(ms));
            };

            return new PipelineBuilder(Factory, foreground, background, new[] { "Fade.CrossFade" });
        }

        /// <summary>
        /// Blends two pipelines using the provided <see cref="Func{T1, T2, TResult}"/> to do so
        /// </summary>
        /// <param name="factory">The blend function to use</param>
        /// <param name="background">The background pipeline to blend with the current instance</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        [Pure, NotNull]
        public PipelineBuilder Merge(
            [NotNull] Func<IGraphicsEffectSource, IGraphicsEffectSource, IGraphicsEffectSource> factory,
            [NotNull] PipelineBuilder background,
            IEnumerable<string> animations = null, IEnumerable<BrushProvider> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => factory(await SourceProducer(), await background.SourceProducer());

            return new PipelineBuilder(Factory, this, background, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }

        /// <summary>
        /// Blends two pipelines using the provided asynchronous <see cref="Func{T1, T2, TResult}"/> to do so
        /// </summary>
        /// <param name="factory">The asynchronous blend function to use</param>
        /// <param name="background">The background pipeline to blend with the current instance</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        [Pure, NotNull]
        public PipelineBuilder Merge(
            [NotNull] Func<IGraphicsEffectSource, IGraphicsEffectSource, Task<IGraphicsEffectSource>> factory,
            [NotNull] PipelineBuilder background,
            IEnumerable<string> animations = null, IEnumerable<BrushProvider> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => await factory(await SourceProducer(), await background.SourceProducer());

            return new PipelineBuilder(Factory, this, background, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }

        #endregion

        #region Built-in effects

        /// <summary>
        /// Adds a new <see cref="GaussianBlurEffect"/> to the current pipeline
        /// </summary>
        /// <param name="blur">The blur amount to apply</param>
        /// <param name="mode">The <see cref="EffectBorderMode"/> parameter for the effect, defaults to <see cref="EffectBorderMode.Hard"/></param>
        /// <param name="optimization">The <see cref="EffectOptimization"/> parameter to use, defaults to <see cref="EffectOptimization.Balanced"/></param>
        [Pure, NotNull]
        public PipelineBuilder Blur(float blur, EffectBorderMode mode = EffectBorderMode.Hard, EffectOptimization optimization = EffectOptimization.Balanced)
        {
            // Blur effect
            async Task<IGraphicsEffectSource> Factory() => new GaussianBlurEffect
            {
                BlurAmount = blur,
                BorderMode = mode,
                Optimization = optimization,
                Source = await SourceProducer()
            };

            return new PipelineBuilder(this, Factory);
        }

        /// <summary>
        /// Adds a new <see cref="GaussianBlurEffect"/> to the current pipeline
        /// </summary>
        /// <param name="blur">The initial blur amount</param>
        /// <param name="animation">The optional blur animation for the effect</param>
        /// <param name="mode">The <see cref="EffectBorderMode"/> parameter for the effect, defaults to <see cref="EffectBorderMode.Hard"/></param>
        /// <param name="optimization">The <see cref="EffectOptimization"/> parameter to use, defaults to <see cref="EffectOptimization.Balanced"/></param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        [Pure, NotNull]
        public PipelineBuilder Blur(float blur, out EffectAnimation animation, EffectBorderMode mode = EffectBorderMode.Hard, EffectOptimization optimization = EffectOptimization.Balanced)
        {
            // Blur effect
            async Task<IGraphicsEffectSource> Factory() => new GaussianBlurEffect
            {
                BlurAmount = blur,
                BorderMode = mode,
                Optimization = optimization,
                Source = await SourceProducer(),
                Name = "Blur"
            };

            animation = (brush, value, ms) => brush.StartAnimationAsync("Blur.BlurAmount", value, TimeSpan.FromMilliseconds(ms));

            return new PipelineBuilder(this, Factory, new[] { "Blur.BlurAmount" });
        }

        /// <summary>
        /// Adds a new <see cref="SaturationEffect"/> to the current pipeline
        /// </summary>
        /// <param name="saturation">The saturation amount for the new effect</param>
        [Pure, NotNull]
        public PipelineBuilder Saturation(float saturation)
        {
            if (saturation < 0 || saturation > 1) throw new ArgumentOutOfRangeException(nameof(saturation), "The saturation must be in the [0,1] range");
            async Task<IGraphicsEffectSource> Factory() => new SaturationEffect
            {
                Saturation = saturation,
                Source = await SourceProducer()
            };

            return new PipelineBuilder(this, Factory);
        }

        /// <summary>
        /// Adds a new <see cref="SaturationEffect"/> to the current pipeline
        /// </summary>
        /// <param name="saturation">The initial saturation amount for the new effect</param>
        /// <param name="animation">The optional saturation animation for the effect</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        [Pure, NotNull]
        public PipelineBuilder Saturation(float saturation, out EffectAnimation animation)
        {
            if (saturation < 0 || saturation > 1) throw new ArgumentOutOfRangeException(nameof(saturation), "The saturation must be in the [0,1] range");
            async Task<IGraphicsEffectSource> Factory() => new SaturationEffect
            {
                Saturation = saturation,
                Source = await SourceProducer(),
                Name = "Saturation"
            };

            animation = (brush, value, ms) =>
            {
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException(nameof(value), "The saturation must be in the [0,1] range");
                return brush.StartAnimationAsync("Saturation.Saturation", value, TimeSpan.FromMilliseconds(ms));
            };

            return new PipelineBuilder(this, Factory, new[] { "Saturation.Saturation" });
        }

        /// <summary>
        /// Adds a new <see cref="OpacityEffect"/> to the current pipeline
        /// </summary>
        /// <param name="opacity">The opacity value to apply to the pipeline</param>
        [Pure, NotNull]
        public PipelineBuilder Opacity(float opacity)
        {
            if (opacity < 0 || opacity > 1) throw new ArgumentOutOfRangeException(nameof(opacity), "The opacity must be in the [0,1] range");
            async Task<IGraphicsEffectSource> Factory() => new OpacityEffect
            {
                Opacity = opacity,
                Source = await SourceProducer()
            };

            return new PipelineBuilder(this, Factory);
        }

        /// <summary>
        /// Adds a new <see cref="OpacityEffect"/> to the current pipeline
        /// </summary>
        /// <param name="opacity">The opacity value to apply to the pipeline</param>
        /// <param name="animation">The optional opacity animation for the effect</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        [Pure, NotNull]
        public PipelineBuilder Opacity(float opacity, out EffectAnimation animation)
        {
            if (opacity < 0 || opacity > 1) throw new ArgumentOutOfRangeException(nameof(opacity), "The opacity must be in the [0,1] range");
            async Task<IGraphicsEffectSource> Factory() => new OpacityEffect
            {
                Opacity = opacity,
                Source = await SourceProducer(),
                Name = "Opacity"
            };

            animation = (brush, value, ms) =>
            {
                if (value < 0 || value > 1) throw new ArgumentOutOfRangeException(nameof(value), "The opacity must be in the [0,1] range");
                return brush.StartAnimationAsync("Opacity.Opacity", value, TimeSpan.FromMilliseconds(ms));
            };

            return new PipelineBuilder(this, Factory, new[] { "Opacity.Opacity" });
        }

        /// <summary>
        /// Applies a tint color on the current pipeline
        /// </summary>
        /// <param name="color">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        [Pure, NotNull]
        public PipelineBuilder Tint(Color color, float mix) => FromColor(color).Mix(this, mix);

        /// <summary>
        /// Applies a tint color on the current pipeline
        /// </summary>
        /// <param name="color">The tint color to use</param>
        /// <param name="mix">The initial amount of tint to apply over the current effect</param>
        /// <param name="animation">The optional tint animation for the effect</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        [Pure, NotNull]
        public PipelineBuilder Tint(Color color, float mix, out EffectAnimation animation) => FromColor(color).Mix(this, mix, out animation);

        #endregion

        #region Custom effects

        /// <summary>
        /// Applies a custom effect to the current pipeline
        /// </summary>
        /// <param name="factory">A <see cref="Func{T, TResult}"/> that takes the current <see cref="IGraphicsEffectSource"/> instance and produces a new effect to display</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        [Pure, NotNull]
        public PipelineBuilder Effect([NotNull] Func<IGraphicsEffectSource, IGraphicsEffectSource> factory, IEnumerable<string> animations = null, IEnumerable<BrushProvider> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => factory(await SourceProducer());

            return new PipelineBuilder(this, Factory, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }

        /// <summary>
        /// Applies a custom effect to the current pipeline
        /// </summary>
        /// <param name="factory">An asynchronous <see cref="Func{T, TResult}"/> that takes the current <see cref="IGraphicsEffectSource"/> instance and produces a new effect to display</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        [Pure, NotNull]
        public PipelineBuilder Effect([NotNull] Func<IGraphicsEffectSource, Task<IGraphicsEffectSource>> factory, IEnumerable<string> animations = null, IEnumerable<BrushProvider> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => await factory(await SourceProducer());

            return new PipelineBuilder(this, Factory, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }

        #endregion

        #region Results

        /// <summary>
        /// Builds a <see cref="CompositionBrush"/> instance from the current effects pipeline
        /// </summary>
        [Pure, NotNull, ItemNotNull]
        public async Task<CompositionBrush> BuildAsync()
        {
            // Validate the pipeline and build the effects factory
            if (!(await SourceProducer() is IGraphicsEffect effect)) throw new InvalidOperationException("The pipeline doesn't contain a valid effects sequence");
            CompositionEffectFactory factory = AnimationProperties.Count > 0
                ? Window.Current.Compositor.CreateEffectFactory(effect, AnimationProperties)
                : Window.Current.Compositor.CreateEffectFactory(effect);

            // Create the effect factory and apply the final effect
            CompositionEffectBrush effectBrush = factory.CreateBrush();
            foreach (KeyValuePair<string, Func<Task<CompositionBrush>>> pair in LazyParameters)
                effectBrush.SetSourceParameter(pair.Key, await pair.Value());

            // Cleanup
            BackdropBrushCache.Cleanup();
            HostBackdropBrushCache.Cleanup();
            return effectBrush;
        }

        /// <summary>
        /// Builds the current pipeline and creates a <see cref="SpriteVisual"/> that is applied to the input <see cref="UIElement"/>
        /// </summary>
        /// <param name="target">The target <see cref="UIElement"/> to apply the brush to</param>
        /// <param name="reference">An optional <see cref="UIElement"/> to use to bind the size of the created brush</param>
        [ItemNotNull]
        public async Task<SpriteVisual> AttachAsync([NotNull] UIElement target, [CanBeNull] UIElement reference = null)
        {
            SpriteVisual visual = Window.Current.Compositor.CreateSpriteVisual();
            visual.Brush = await BuildAsync();
            ElementCompositionPreview.SetElementChildVisual(target, visual);
            if (reference != null) visual.BindSize(reference);
            return visual;
        }

        /// <summary>
        /// Creates a new <see cref="XamlCompositionBrush"/> from the current effects pipeline
        /// </summary>
        [Pure, NotNull]
        public XamlCompositionBrush AsBrush() => new XamlCompositionBrush(this);

        #endregion
    }
}