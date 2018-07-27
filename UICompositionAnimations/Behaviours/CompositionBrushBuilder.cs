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
            String
                guid = Guid.NewGuid().ToString("N"),
                replaced = Regex.Replace(guid, "[0-9]", "_"),
                id = new String(replaced.ToCharArray().Select((c, i) => c == '_' ? char.ToUpper((char)('a' + i % 26)) : c).ToArray());
            SourceProducer = () => Task.FromResult(new CompositionEffectSourceParameter(id).To<IGraphicsEffectSource>());
            LazyParameters = new Dictionary<String, Func<Task<CompositionBrush>>> { { id, factory } };
            AnimationParameters = new string[0];
        }

        /// <summary>
        /// Base constructor used to create a new instance from scratch
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="lazy">The collection of <see cref="CompositionBrush"/> instances that needs to be initialized for the new effect</param>
        /// <param name="animations">The collection of animation properties for the new effect</param>
        private CompositionBrushBuilder([NotNull] Func<Task<IGraphicsEffectSource>> factory, [NotNull] IReadOnlyDictionary<String, Func<Task<CompositionBrush>>> lazy, [NotNull, ItemNotNull] IReadOnlyCollection<String> animations)
        {
            SourceProducer = factory;
            LazyParameters = lazy;
            AnimationParameters = animations;
        }

        /// <summary>
        /// Constructor used to initialize a pipeline from a custom <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will return the initial <see cref="IGraphicsEffectSource"/></param>
        private CompositionBrushBuilder([NotNull] Func<Task<IGraphicsEffectSource>> factory)
            : this(factory, new Dictionary<String, Func<Task<CompositionBrush>>>(), new string[0])
        { }

        /// <summary>
        /// Constructor used to create a new instance obtained by concatenation between the current pipeline and the input effect info
        /// </summary>
        /// <param name="source">The source pipeline to attach the new effect to</param>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        private CompositionBrushBuilder([NotNull] CompositionBrushBuilder source, [NotNull] Func<Task<IGraphicsEffectSource>> factory)
            : this(factory, source.LazyParameters, source.AnimationParameters)
        { }

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
            : this(factory, source.LazyParameters.Merge(lazy), source.AnimationParameters.Merge(animations))
        { }

        /// <summary>
        /// Constructor used to create a new instance obtained by merging two separate pipelines
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="a">The first pipeline to merge</param>
        /// <param name="b">The second pipeline to merge</param>
        private CompositionBrushBuilder(
            [NotNull] Func<Task<IGraphicsEffectSource>> factory,
            [NotNull] CompositionBrushBuilder a, [NotNull] CompositionBrushBuilder b)
            : this(factory, a.LazyParameters.Merge(b.LazyParameters), a.AnimationParameters.Merge(b.AnimationParameters))
        { }

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

        public static CompositionBrushBuilder Blend([NotNull] CompositionBrushBuilder foreground, [NotNull] CompositionBrushBuilder background, BlendEffectMode mode)
        {
            async Task<IGraphicsEffectSource> Factory() => new BlendEffect
            {
                Foreground = await foreground.SourceProducer(),
                Background = await background.SourceProducer(),
                Mode = mode
            };

            return new CompositionBrushBuilder(Factory, foreground, background);
        }

        public static CompositionBrushBuilder Mix([NotNull] CompositionBrushBuilder foreground, [NotNull] CompositionBrushBuilder background, float mix)
        {
            if (mix <= 0 || mix >= 1) throw new ArgumentOutOfRangeException(nameof(mix), "The mix value must be in the (0,1) range");
            async Task<IGraphicsEffectSource> Factory() => new ArithmeticCompositeEffect
            {
                MultiplyAmount = 0,
                Source1Amount = 1 - mix,
                Source2Amount = mix,
                Source1 = await background.SourceProducer(),
                Source2 = await foreground.SourceProducer()
            };

            return new CompositionBrushBuilder(Factory, foreground, background);
        }

        public static CompositionBrushBuilder Merge(
            [NotNull] Func<IGraphicsEffectSource, IGraphicsEffectSource, IGraphicsEffectSource> factory,
            [NotNull] CompositionBrushBuilder foreground, [NotNull] CompositionBrushBuilder background)
        {
            async Task<IGraphicsEffectSource> Factory() => factory(await foreground.SourceProducer(), await background.SourceProducer());

            return new CompositionBrushBuilder(Factory, foreground, background);
        }

        public static CompositionBrushBuilder Merge(
            [NotNull] Func<IGraphicsEffectSource, IGraphicsEffectSource, Task<IGraphicsEffectSource>> factory,
            [NotNull] CompositionBrushBuilder foreground, [NotNull] CompositionBrushBuilder background)
        {
            async Task<IGraphicsEffectSource> Factory() => await factory(await foreground.SourceProducer(), await background.SourceProducer());

            return new CompositionBrushBuilder(Factory, foreground, background);
        }

        #endregion

        #region Effects

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

        public CompositionBrushBuilder Tint(Color color, float mix) => Mix(FromColor(color), this, mix);

        public CompositionBrushBuilder Effect([NotNull] Func<IGraphicsEffectSource, IGraphicsEffectSource> factory)
        {
            async Task<IGraphicsEffectSource> Factory() => factory(await SourceProducer());

            return new CompositionBrushBuilder(this, Factory);
        }

        public CompositionBrushBuilder Effect([NotNull] Func<IGraphicsEffectSource, Task<IGraphicsEffectSource>> factory)
        {
            async Task<IGraphicsEffectSource> Factory() => await factory(await SourceProducer());

            return new CompositionBrushBuilder(this, Factory);
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
            CompositionEffectFactory factory = AnimationParameters.Count > 0
                ? Window.Current.Compositor.CreateEffectFactory(effect, AnimationParameters)
                : Window.Current.Compositor.CreateEffectFactory(effect);

            // Create the effect factory and apply the final effect
            CompositionEffectBrush effectBrush = factory.CreateBrush();
            foreach (KeyValuePair<String, Func<Task<CompositionBrush>>> pair in LazyParameters)
                effectBrush.SetSourceParameter(pair.Key, await pair.Value());
            return effectBrush;
        }
    }
}
