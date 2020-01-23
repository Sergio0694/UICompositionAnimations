using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using FluentExtensions.UI.Brushes.Brushes;

#nullable enable

namespace FluentExtensions.UI.Brushes.Pipelines
{
    /// <summary>
    /// A <see langword="delegate"/> that represents a custom effect animation that can be applied to a <see cref="CompositionBrush"/>
    /// </summary>
    /// <param name="brush">The target <see cref="CompositionBrush"/> instance to use to start the animation</param>
    /// <param name="value">The animation target value</param>
    /// <param name="ms">The animation duration, in milliseconds</param>
    public delegate Task EffectAnimation(CompositionBrush brush, float value, int ms);

    /// <summary>
    /// A <see langword="class"/> that allows to build custom effects pipelines and create <see cref="CompositionBrush"/> instances from them
    /// </summary>
    public sealed partial class PipelineBuilder
    {
        /// <summary>
        /// The <see cref="Func{TResult}"/> instance used to produce the output <see cref="IGraphicsEffectSource"/> for this pipeline
        /// </summary>
        private readonly Func<Task<IGraphicsEffectSource>> SourceProducer;

        /// <summary>
        /// The collection of animation properties present in the current pipeline
        /// </summary>
        private readonly IReadOnlyCollection<string> AnimationProperties;

        /// <summary>
        /// The collection of info on the parameters that need to be initialized after creating the final <see cref="CompositionBrush"/>
        /// </summary>
        private readonly IReadOnlyDictionary<string, Func<Task<CompositionBrush>>> LazyParameters;

        /// <summary>
        /// Constructor used to initialize a pipeline from a <see cref="CompositionBrush"/>, for example using the <see cref="Compositor.CreateBackdropBrush"/> method
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will return the initial <see cref="CompositionBrush"/></param>
        private PipelineBuilder(Func<Task<CompositionBrush>> factory)
        {
            string
                guid = Guid.NewGuid().ToString("N"),
                id = Regex.Replace(guid, @"\d", m => ((char)('g' + m.Value[0] - '0')).ToString());

            SourceProducer = () => Task.FromResult<IGraphicsEffectSource>(new CompositionEffectSourceParameter(id));
            LazyParameters = new Dictionary<string, Func<Task<CompositionBrush>>> { { id, factory } };
            AnimationProperties = new string[0];
        }

        /// <summary>
        /// Base constructor used to create a new instance from scratch
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="animations">The collection of animation properties for the new effect</param>
        /// <param name="lazy">The collection of <see cref="CompositionBrush"/> instances that needs to be initialized for the new effect</param>
        private PipelineBuilder(
            Func<Task<IGraphicsEffectSource>> factory,
            IReadOnlyCollection<string> animations,
            IReadOnlyDictionary<string, Func<Task<CompositionBrush>>> lazy)
        {
            SourceProducer = factory;
            AnimationProperties = animations;
            LazyParameters = lazy;
        }

        /// <summary>
        /// Constructor used to initialize a pipeline from a custom <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will return the initial <see cref="IGraphicsEffectSource"/></param>
        private PipelineBuilder(Func<Task<IGraphicsEffectSource>> factory)
            : this(
                factory,
                new string[0],
                new Dictionary<string, Func<Task<CompositionBrush>>>())
        { }

        /// <summary>
        /// Constructor used to create a new instance obtained by concatenation between the current pipeline and the input effect info
        /// </summary>
        /// <param name="source">The source pipeline to attach the new effect to</param>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="animations">The collection of animation properties for the new effect</param>
        /// <param name="lazy">The collection of <see cref="CompositionBrush"/> instances that needs to be initialized for the new effect</param>
        private PipelineBuilder(
            PipelineBuilder source,
            Func<Task<IGraphicsEffectSource>> factory,
            IReadOnlyCollection<string>? animations = null, IReadOnlyDictionary<string, Func<Task<CompositionBrush>>>? lazy = null)
            : this(
                factory,
                animations?.Merge(source.AnimationProperties) ?? source.AnimationProperties,
                lazy?.Merge(source.LazyParameters) ?? source.LazyParameters)
        { }

        /// <summary>
        /// Constructor used to create a new instance obtained by merging two separate pipelines
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> instance that will produce the new <see cref="IGraphicsEffectSource"/> to add to the pipeline</param>
        /// <param name="a">The first pipeline to merge</param>
        /// <param name="b">The second pipeline to merge</param>
        /// <param name="animations">The collection of animation properties for the new effect</param>
        /// <param name="lazy">The collection of <see cref="CompositionBrush"/> instances that needs to be initialized for the new effect</param>
        private PipelineBuilder(
            Func<Task<IGraphicsEffectSource>> factory,
            PipelineBuilder a,
            PipelineBuilder b,
            IReadOnlyCollection<string>? animations = null,
            IReadOnlyDictionary<string, Func<Task<CompositionBrush>>>? lazy = null)
            : this(
                factory,
                animations?.Merge(a.AnimationProperties.Merge(b.AnimationProperties)) ?? a.AnimationProperties.Merge(b.AnimationProperties),
                lazy?.Merge(a.LazyParameters.Merge(b.LazyParameters)) ?? a.LazyParameters.Merge(b.LazyParameters))
        { }

        /// <summary>
        /// Builds a <see cref="CompositionBrush"/> instance from the current effects pipeline
        /// </summary>
        [Pure]
        public async Task<CompositionBrush> BuildAsync()
        {
            // Validate the pipeline
            if (!(await SourceProducer() is IGraphicsEffect effect)) throw new InvalidOperationException("The pipeline doesn't contain a valid effects sequence");

            // Build the effects factory
            CompositionEffectFactory factory = AnimationProperties.Count > 0
                ? Window.Current.Compositor.CreateEffectFactory(effect, AnimationProperties)
                : Window.Current.Compositor.CreateEffectFactory(effect);

            // Create the effect factory and apply the final effect
            CompositionEffectBrush effectBrush = factory.CreateBrush();
            foreach (KeyValuePair<string, Func<Task<CompositionBrush>>> pair in LazyParameters)
            {
                effectBrush.SetSourceParameter(pair.Key, await pair.Value());
            }

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
        public async Task<SpriteVisual> AttachAsync(UIElement target, UIElement? reference = null)
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
        [Pure]
        public XamlCompositionBrush AsBrush()
        {
            return new XamlCompositionBrush(this);
        }
    }
}
