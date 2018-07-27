using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas.Effects;
using UICompositionAnimations.Helpers;

namespace UICompositionAnimations.Behaviours
{
    public sealed class CompositionBrushBuilder
    {
        private readonly Func<Task<IGraphicsEffectSource>> SourceProducer;

        private readonly IReadOnlyDictionary<String, Func<CompositionBrush>> LazyParameters;

        private readonly IReadOnlyCollection<String> AnimationParameters;

        #region Initialization

        private CompositionBrushBuilder(Func<CompositionBrush> factory)
        {
            SourceProducer = () => Task.FromResult(new CompositionEffectSourceParameter("source").To<IGraphicsEffectSource>());
            LazyParameters = new Dictionary<String, Func<CompositionBrush>> { { "source", factory } };
            AnimationParameters = new string[0];
        }

        private CompositionBrushBuilder(Func<Task<IGraphicsEffectSource>> factory)
        {
            SourceProducer = factory;
            LazyParameters = new Dictionary<String, Func<CompositionBrush>>();
            AnimationParameters = new string[0];
        }

        private CompositionBrushBuilder(CompositionBrushBuilder source, Func<Task<IGraphicsEffectSource>> factory, IReadOnlyDictionary<String, Func<CompositionBrush>> lazy, IReadOnlyCollection<String> animations)
        {
            SourceProducer = factory;

            // Merge the lazy parameters dictionaries
            if (source.LazyParameters.Keys.FirstOrDefault(lazy.ContainsKey) is String key)
                throw new InvalidOperationException($"The key {key} already exists in the current pipeline");
            LazyParameters = new Dictionary<String, Func<CompositionBrush>>(source.LazyParameters.Concat(lazy));

            // Merge the animation parameters
            if (source.AnimationParameters.FirstOrDefault(animations.Contains) is String animation)
                throw new InvalidOperationException($"The animation {animation} already exists in the current pipeline");
            AnimationParameters = source.AnimationParameters.Concat(animations).ToArray();
        }

        [Pure, NotNull]
        public static CompositionBrushBuilder FromBackdropBrush() => new CompositionBrushBuilder(() => Window.Current.Compositor.CreateBackdropBrush());

        [Pure, NotNull]
        public static CompositionBrushBuilder FromHostBackdropBrush() => new CompositionBrushBuilder(() => Window.Current.Compositor.CreateHostBackdropBrush());

        [Pure, NotNull]
        public static CompositionBrushBuilder FromEffect(Func<IGraphicsEffectSource> factory) => new CompositionBrushBuilder(() => Task.FromResult(factory()));

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
            return new CompositionBrushBuilder(this, Factory, new Dictionary<String, Func<CompositionBrush>>(), parameters);
        }

        #endregion

        public async Task<CompositionBrush> BuildAsync()
        {
            // Validate the pipeline and build the effects factory
            if (!(await SourceProducer() is IGraphicsEffect effect)) throw new InvalidOperationException("The pipeline doesn't contain a valid effects sequence");
            CompositionEffectFactory factory = Window.Current.Compositor.CreateEffectFactory(effect, AnimationParameters);

            // Create the effect factory and apply the final effect
            CompositionEffectBrush effectBrush = factory.CreateBrush();
            foreach (KeyValuePair<String, Func<CompositionBrush>> pair in LazyParameters)
                effectBrush.SetSourceParameter(pair.Key, pair.Value());
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
