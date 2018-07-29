using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using UICompositionAnimations.Brushes.Cache;
using UICompositionAnimations.Enums;
using UICompositionAnimations.Helpers;

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
    public sealed class CompositionBrushBuilder
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
        private CompositionBrushBuilder([NotNull] Func<Task<CompositionBrush>> factory)
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
        private CompositionBrushBuilder([NotNull] Func<Task<IGraphicsEffectSource>> factory, [NotNull] [ItemNotNull] IReadOnlyCollection<string> animations, [NotNull] IReadOnlyDictionary<string, Func<Task<CompositionBrush>>> lazy)
        {
            SourceProducer = factory;
            AnimationProperties = animations;
            LazyParameters = lazy;
        }

        /// <summary>
        /// Constructor used to initialize a pipeline from a custom <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will return the initial <see cref="IGraphicsEffectSource"/></param>
        private CompositionBrushBuilder([NotNull] Func<Task<IGraphicsEffectSource>> factory)
            : this(factory, new string[0], new Dictionary<string, Func<Task<CompositionBrush>>>())
        { }

        /// <summary>
        /// Constructor used to create a new instance obtained by concatenation between the current pipeline and the input effect info
        /// </summary>
        /// <param name="source">The source pipeline to attach the new effect to</param>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="animations">The collection of animation properties for the new effect</param>
        /// <param name="lazy">The collection of <see cref="CompositionBrush"/> instances that needs to be initialized for the new effect</param>
        private CompositionBrushBuilder([NotNull] CompositionBrushBuilder source,
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
        private CompositionBrushBuilder([NotNull] Func<Task<IGraphicsEffectSource>> factory,
            [NotNull] CompositionBrushBuilder a, [NotNull] CompositionBrushBuilder b,
            [CanBeNull] [ItemNotNull] IReadOnlyCollection<string> animations = null, [CanBeNull] IReadOnlyDictionary<string, Func<Task<CompositionBrush>>> lazy = null)
            : this(factory, animations?.Merge(a.AnimationProperties.Merge(b.AnimationProperties)) ?? a.AnimationProperties.Merge(b.AnimationProperties), lazy?.Merge(a.LazyParameters.Merge(b.LazyParameters)) ?? a.LazyParameters.Merge(b.LazyParameters))
        { }

        #endregion

        #region Initialization

        // The cache manager for backdrop brushes
        [NotNull]
        private static readonly ThreadSafeCompositionCache<CompositionBrush> BackdropBrushCache = new ThreadSafeCompositionCache<CompositionBrush>();

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from the <see cref="CompositionBrush"/> returned by <see cref="Compositor.CreateBackdropBrush"/>
        /// </summary>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromBackdropBrush() => new CompositionBrushBuilder(() => BackdropBrushCache.TryGetInstanceAsync(Window.Current.Compositor.CreateBackdropBrush));

        // The cache manager for host backdrop brushes
        [NotNull]
        private static readonly ThreadSafeCompositionCache<CompositionBrush> HostBackdropBrushCache = new ThreadSafeCompositionCache<CompositionBrush>();

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from the <see cref="CompositionBrush"/> returned by <see cref="Compositor.CreateHostBackdropBrush"/>
        /// </summary>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromHostBackdropBrush() => new CompositionBrushBuilder(() => HostBackdropBrushCache.TryGetInstanceAsync(Window.Current.Compositor.CreateHostBackdropBrush));

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from a solid <see cref="CompositionBrush"/> with the specified color
        /// </summary>
        /// <param name="color">The desired color for the initial <see cref="CompositionBrush"/></param>
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
        public static CompositionBrushBuilder FromImage([NotNull] Uri uri, BitmapCacheMode options = BitmapCacheMode.EnableCaching, BitmapDPIMode dpiMode = BitmapDPIMode.CopyDisplayDPISettingsWith96AsLowerBound)
        {
            return new CompositionBrushBuilder(() => Win2DImageHelper.LoadImageAsync(Window.Current.Compositor, uri, options, dpiMode).ContinueWith(t => t.Result as CompositionBrush));
        }

        /// <summary>
        /// Starts a new <see cref="CompositionBrushBuilder"/> pipeline from a Win2D image tiled to cover the available space
        /// </summary>
        /// <param name="uri">The path for the image to load</param>
        /// <param name="options">Indicates whether or not to force the reload of the Win2D image</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        [Pure, NotNull]
        public static CompositionBrushBuilder FromTiles([NotNull] Uri uri, BitmapCacheMode options = BitmapCacheMode.EnableCaching, BitmapDPIMode dpiMode = BitmapDPIMode.CopyDisplayDPISettingsWith96AsLowerBound)
        {
            CompositionBrushBuilder image = FromImage(uri, options, dpiMode);

            async Task<IGraphicsEffectSource> Factory() => new BorderEffect
            {
                ExtendX = CanvasEdgeBehavior.Wrap,
                ExtendY = CanvasEdgeBehavior.Wrap,
                Source = await image.SourceProducer()
            };

            return new CompositionBrushBuilder(image, Factory);
        }

        #endregion

        #region Blends

        /// <summary>
        /// An <see langword="enum"/> used to modify the default placement of the input <see cref="IGraphicsEffectSource"/> instance in a blend operation
        /// </summary>
        [PublicAPI]
        public enum EffectPlacement
        {
            /// <summary>
            /// The instance used to call the blend method is placed on top of the other
            /// </summary>
            Foreground,

            /// <summary>
            /// The instance used to call the blend method is placed behind the other
            /// </summary>
            Background
        }

        /// <summary>
        /// Blends two pipelines using a <see cref="BlendEffect"/> instance with the specified mode
        /// </summary>
        /// <param name="pipeline">The second <see cref="CompositionBrushBuilder"/> instance to blend</param>
        /// <param name="mode">The desired <see cref="BlendEffectMode"/> to use to blend the input pipelines</param>
        /// <param name="sorting">The sorting mode to use with the two input pipelines</param>
        [Pure, NotNull]
        public CompositionBrushBuilder Blend([NotNull] CompositionBrushBuilder pipeline, BlendEffectMode mode, EffectPlacement sorting = EffectPlacement.Foreground)
        {
            (var foreground, var background) = sorting == EffectPlacement.Foreground ? (this, pipeline) : (pipeline, this);

            async Task<IGraphicsEffectSource> Factory() => new BlendEffect
            {
                Foreground = await foreground.SourceProducer(),
                Background = await background.SourceProducer(),
                Mode = mode
            };

            return new CompositionBrushBuilder(Factory, foreground, background);
        }

        /// <summary>
        /// Blends two pipelines using an <see cref="CrossFadeEffect"/> instance
        /// </summary>
        /// <param name="pipeline">The second <see cref="CompositionBrushBuilder"/> instance to blend</param>
        /// <param name="factor">The cross fade factor to blend the input effects</param>
        /// <param name="sorting">The sorting mode to use with the two input pipelines</param>
        [Pure, NotNull]
        public CompositionBrushBuilder Mix([NotNull] CompositionBrushBuilder pipeline, float factor = 0.5f, EffectPlacement sorting = EffectPlacement.Foreground)
        {
            if (factor < 0 || factor > 1) throw new ArgumentOutOfRangeException(nameof(factor), "The factor must be in the [0,1] range");
            (var foreground, var background) = sorting == EffectPlacement.Foreground ? (this, pipeline) : (pipeline, this);

            async Task<IGraphicsEffectSource> Factory() => new CrossFadeEffect
            {
                CrossFade = factor,
                Source1 = await foreground.SourceProducer(),
                Source2 = await background.SourceProducer()
            };

            return new CompositionBrushBuilder(Factory, foreground, background);
        }

        /// <summary>
        /// Blends two pipelines using an <see cref="CrossFadeEffect"/> instance
        /// </summary>
        /// <param name="pipeline">The second <see cref="CompositionBrushBuilder"/> instance to blend</param>
        /// <param name="factor">The cross fade factor to blend the input effects</param>
        /// <param name="animation">The optional blur animation for the effect</param>
        /// <param name="sorting">The sorting mode to use with the two input pipelines</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        [Pure, NotNull]
        public CompositionBrushBuilder Mix([NotNull] CompositionBrushBuilder pipeline, float factor, out EffectAnimation animation, EffectPlacement sorting = EffectPlacement.Foreground)
        {
            if (factor < 0 || factor > 1) throw new ArgumentOutOfRangeException(nameof(factor), "The factor must be in the [0,1] range");
            (var foreground, var background) = sorting == EffectPlacement.Foreground ? (this, pipeline) : (pipeline, this);

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

            return new CompositionBrushBuilder(Factory, foreground, background, new[] { "Fade.CrossFade" });
        }

        /// <summary>
        /// Blends two pipelines using the provided <see cref="Func{T1, T2, TResult}"/> to do so
        /// </summary>
        /// <param name="factory">The blend function to use</param>
        /// <param name="background">The background pipeline to blend with the current instance</param>
        [Pure, NotNull]
        public CompositionBrushBuilder Merge(
            [NotNull] Func<IGraphicsEffectSource, IGraphicsEffectSource, IGraphicsEffectSource> factory,
            [NotNull] CompositionBrushBuilder background)
        {
            async Task<IGraphicsEffectSource> Factory() => factory(await SourceProducer(), await background.SourceProducer());

            return new CompositionBrushBuilder(Factory, this, background);
        }

        /// <summary>
        /// Blends two pipelines using the provided asynchronous <see cref="Func{T1, T2, TResult}"/> to do so
        /// </summary>
        /// <param name="factory">The asynchronous blend function to use</param>
        /// <param name="background">The background pipeline to blend with the current instance</param>
        [Pure, NotNull]
        public CompositionBrushBuilder Merge(
            [NotNull] Func<IGraphicsEffectSource, IGraphicsEffectSource, Task<IGraphicsEffectSource>> factory,
            [NotNull] CompositionBrushBuilder background)
        {
            async Task<IGraphicsEffectSource> Factory() => await factory(await SourceProducer(), await background.SourceProducer());

            return new CompositionBrushBuilder(Factory, this, background);
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
        public CompositionBrushBuilder Blur(float blur, EffectBorderMode mode = EffectBorderMode.Hard, EffectOptimization optimization = EffectOptimization.Balanced)
        {
            // Blur effect
            async Task<IGraphicsEffectSource> Factory() => new GaussianBlurEffect
            {
                BlurAmount = blur,
                BorderMode = mode,
                Optimization = optimization,
                Source = await SourceProducer()
            };

            return new CompositionBrushBuilder(this, Factory);
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
        public CompositionBrushBuilder Blur(float blur, out EffectAnimation animation, EffectBorderMode mode = EffectBorderMode.Hard, EffectOptimization optimization = EffectOptimization.Balanced)
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

            return new CompositionBrushBuilder(this, Factory, new[] { "Blur.BlurAmount" });
        }

        /// <summary>
        /// Adds a new <see cref="SaturationEffect"/> to the current pipeline
        /// </summary>
        /// <param name="saturation">The saturation amount for the new effect</param>
        [Pure, NotNull]
        public CompositionBrushBuilder Saturation(float saturation)
        {
            if (saturation < 0 || saturation > 1) throw new ArgumentOutOfRangeException(nameof(saturation), "The saturation must be in the [0,1] range");
            async Task<IGraphicsEffectSource> Factory() => new SaturationEffect
            {
                Saturation = saturation,
                Source = await SourceProducer()
            };

            return new CompositionBrushBuilder(this, Factory);
        }

        /// <summary>
        /// Adds a new <see cref="SaturationEffect"/> to the current pipeline
        /// </summary>
        /// <param name="saturation">The initial saturation amount for the new effect</param>
        /// <param name="animation">The optional saturation animation for the effect</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        [Pure, NotNull]
        public CompositionBrushBuilder Saturation(float saturation, out EffectAnimation animation)
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

            return new CompositionBrushBuilder(this, Factory, new[] { "Saturation.Saturation" });
        }

        /// <summary>
        /// Adds a new <see cref="OpacityEffect"/> to the current pipeline
        /// </summary>
        /// <param name="opacity">The opacity value to apply to the pipeline</param>
        [Pure, NotNull]
        public CompositionBrushBuilder Opacity(float opacity)
        {
            if (opacity < 0 || opacity > 1) throw new ArgumentOutOfRangeException(nameof(opacity), "The opacity must be in the [0,1] range");
            async Task<IGraphicsEffectSource> Factory() => new OpacityEffect
            {
                Opacity = opacity,
                Source = await SourceProducer()
            };

            return new CompositionBrushBuilder(this, Factory);
        }

        /// <summary>
        /// Adds a new <see cref="OpacityEffect"/> to the current pipeline
        /// </summary>
        /// <param name="opacity">The opacity value to apply to the pipeline</param>
        /// <param name="animation">The optional opacity animation for the effect</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        [Pure, NotNull]
        public CompositionBrushBuilder Opacity(float opacity, out EffectAnimation animation)
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

            return new CompositionBrushBuilder(this, Factory, new[] { "Opacity.Opacity" });
        }

        /// <summary>
        /// Applies a tint color on the current pipeline
        /// </summary>
        /// <param name="color">The tint color to use</param>
        /// <param name="mix">The amount of tint to apply over the current effect</param>
        [Pure, NotNull]
        public CompositionBrushBuilder Tint(Color color, float mix) => FromColor(color).Mix(this, mix);

        /// <summary>
        /// Applies a tint color on the current pipeline
        /// </summary>
        /// <param name="color">The tint color to use</param>
        /// <param name="mix">The initial amount of tint to apply over the current effect</param>
        /// <param name="animation">The optional tint animation for the effect</param>
        /// <remarks>Note that each pipeline can only contain a single instance of any of the built-in effects with animation support</remarks>
        [Pure, NotNull]
        public CompositionBrushBuilder Tint(Color color, float mix, out EffectAnimation animation) => FromColor(color).Mix(this, mix, out animation);

        #endregion

        #region Custom effects

        /// <summary>
        /// Applies a custom effect to the current pipeline
        /// </summary>
        /// <param name="factory">A <see cref="Func{T, TResult}"/> that takes the current <see cref="IGraphicsEffectSource"/> instance and produces a new effect to display</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        [Pure, NotNull]
        public CompositionBrushBuilder Effect([NotNull] Func<IGraphicsEffectSource, IGraphicsEffectSource> factory, IEnumerable<string> animations = null, IEnumerable<CompositionSourceInitializer> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => factory(await SourceProducer());

            return new CompositionBrushBuilder(this, Factory, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }

        /// <summary>
        /// Applies a custom effect to the current pipeline
        /// </summary>
        /// <param name="factory">An asynchronous <see cref="Func{T, TResult}"/> that takes the current <see cref="IGraphicsEffectSource"/> instance and produces a new effect to display</param>
        /// <param name="animations">The list of optional animatable properties in the returned effect</param>
        /// <param name="initializers">The list of source parameters that require deferred initialization (see <see cref="CompositionEffectSourceParameter"/> for more info)</param>
        [Pure, NotNull]
        public CompositionBrushBuilder Effect([NotNull] Func<IGraphicsEffectSource, Task<IGraphicsEffectSource>> factory, IEnumerable<string> animations = null, IEnumerable<CompositionSourceInitializer> initializers = null)
        {
            async Task<IGraphicsEffectSource> Factory() => await factory(await SourceProducer());

            return new CompositionBrushBuilder(this, Factory, animations?.ToArray(), initializers?.ToDictionary(item => item.Name, item => item.Initializer));
        }

        #endregion

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
    }
}
