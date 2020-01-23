using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using FluentExtensions.UI.Brushes.Enums;
using FluentExtensions.UI.Brushes.Helpers;
using FluentExtensions.UI.Brushes.Helpers.Cache;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

#nullable enable

namespace FluentExtensions.UI.Brushes.Pipelines
{
    public sealed partial class PipelineBuilder
    {
        /// <summary>
        /// The cache manager for backdrop brushes
        /// </summary>
        private static readonly ThreadSafeCompositionCache<CompositionBrush> BackdropBrushCache = new ThreadSafeCompositionCache<CompositionBrush>();

        /// <summary>
        /// The cache manager for host backdrop brushes
        /// </summary>
        private static readonly ThreadSafeCompositionCache<CompositionBrush> HostBackdropBrushCache = new ThreadSafeCompositionCache<CompositionBrush>();

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the <see cref="CompositionBrush"/> returned by <see cref="Compositor.CreateBackdropBrush"/>
        /// </summary>
        [Pure]
        public static PipelineBuilder FromBackdropBrush()
        {
            return new PipelineBuilder(() => BackdropBrushCache.TryGetInstanceAsync(Window.Current.Compositor.CreateBackdropBrush));
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the <see cref="CompositionBrush"/> returned by <see cref="Compositor.CreateHostBackdropBrush"/>
        /// </summary>
        [Pure]
        public static PipelineBuilder FromHostBackdropBrush()
        {
            return new PipelineBuilder(() => HostBackdropBrushCache.TryGetInstanceAsync(Window.Current.Compositor.CreateHostBackdropBrush));
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from a solid <see cref="CompositionBrush"/> with the specified color
        /// </summary>
        /// <param name="color">The desired color for the initial <see cref="CompositionBrush"/></param>
        [Pure]
        public static PipelineBuilder FromColor(Color color)
        {
            return new PipelineBuilder(() => Task.FromResult<IGraphicsEffectSource>(new ColorSourceEffect { Color = color }));
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the input <see cref="CompositionBrush"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that synchronously returns a <see cref="CompositionBrush"/> instance to start the pipeline</param>
        [Pure]
        public static PipelineBuilder FromBrush(Func<CompositionBrush> factory)
        {
            return new PipelineBuilder(() => Task.FromResult(factory()));
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the input <see cref="CompositionBrush"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that asynchronously returns a <see cref="CompositionBrush"/> instance to start the pipeline</param>
        [Pure]
        public static PipelineBuilder FromBrush(Func<Task<CompositionBrush>> factory)
        {
            return new PipelineBuilder(factory);
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the input <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that synchronously returns a <see cref="IGraphicsEffectSource"/> instance to start the pipeline</param>
        [Pure]
        public static PipelineBuilder FromEffect(Func<IGraphicsEffectSource> factory)
        {
            return new PipelineBuilder(() => Task.FromResult(factory()));
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from the input <see cref="IGraphicsEffectSource"/> instance
        /// </summary>
        /// <param name="factory">A <see cref="Func{TResult}"/> that asynchronously returns a <see cref="IGraphicsEffectSource"/> instance to start the pipeline</param>
        [Pure]
        public static PipelineBuilder FromEffect(Func<Task<IGraphicsEffectSource>> factory)
        {
            return new PipelineBuilder(factory);
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from a Win2D image
        /// </summary>
        /// <param name="relativePath">The relative path for the image to load (eg. "/Assets/image.png")</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromImage(string relativePath, DpiMode dpiMode = DpiMode.DisplayDpiWith96AsLowerBound, CacheMode cache = CacheMode.Default)
        {
            return FromImage(relativePath.ToAppxUri(), dpiMode, cache);
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from a Win2D image
        /// </summary>
        /// <param name="uri">The path for the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromImage(Uri uri, DpiMode dpiMode = DpiMode.DisplayDpiWith96AsLowerBound, CacheMode cache = CacheMode.Default)
        {
            return new PipelineBuilder(async () => (await Win2DImageHelper.LoadImageAsync(Window.Current.Compositor, uri, dpiMode, cache))!);
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from a Win2D image tiled to cover the available space
        /// </summary>
        /// <param name="relativePath">The relative path for the image to load (eg. "/Assets/image.png")</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromTiles(string relativePath, DpiMode dpiMode = DpiMode.DisplayDpiWith96AsLowerBound, CacheMode cache = CacheMode.Default)
        {
            return FromTiles(relativePath.ToAppxUri(), dpiMode, cache);
        }

        /// <summary>
        /// Starts a new <see cref="PipelineBuilder"/> pipeline from a Win2D image tiled to cover the available space
        /// </summary>
        /// <param name="uri">The path for the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">The cache mode to use to load the image</param>
        [Pure]
        public static PipelineBuilder FromTiles(Uri uri, DpiMode dpiMode = DpiMode.DisplayDpiWith96AsLowerBound, CacheMode cache = CacheMode.Default)
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
        [Pure]
        public static PipelineBuilder FromUIElement(UIElement element)
        {
            return new PipelineBuilder(() => Task.FromResult<CompositionBrush>(ElementCompositionPreview.GetElementVisual(element).Compositor.CreateBackdropBrush()));
        }
    }
}
