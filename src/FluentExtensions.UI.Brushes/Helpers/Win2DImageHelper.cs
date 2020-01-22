using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas.UI.Xaml;
using UICompositionAnimations.Enums;
using UICompositionAnimations.Helpers.Cache;

#nullable enable

namespace UICompositionAnimations.Helpers
{
    /// <summary>
    /// A helper <see langword="class"/> that loads Win2D images and manages an internal cache of <see cref="CompositionBrush"/> instances with the loaded images
    /// </summary>
    public static class Win2DImageHelper
    {
        /// <summary>
        /// Gets the maximum time to wait for the Win2D device to be restored in case of initial failure
        /// </summary>
        private const int DeviceLostRecoveryThreshold = 1000;

        /// <summary>
        /// Synchronization mutex to access the cache and load Win2D images concurrently
        /// </summary>
        private static readonly AsyncMutex Win2DMutex = new AsyncMutex();

        /// <summary>
        /// Gets the local cache mapping for previously loaded Win2D images
        /// </summary>
        private static readonly ThreadSafeCompositionMapCache<Uri, CompositionSurfaceBrush> Cache = new ThreadSafeCompositionMapCache<Uri, CompositionSurfaceBrush>();

        #region Public APIs

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image from the shared <see cref="CanvasDevice"/> instance
        /// </summary>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">Indicates the cache option to use to load the image</param>
        [Pure]
        public static Task<CompositionSurfaceBrush?> LoadImageAsync(Uri uri, DpiMode dpiMode, CacheMode cache = CacheMode.Default)
        {
            return LoadImageAsync(Window.Current.Compositor, uri, dpiMode, cache);
        }

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image
        /// </summary>
        /// <param name="canvas">The <see cref="CanvasControl"/> to use to load the target image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">Indicates the cache option to use to load the image</param>
        [Pure]
        public static Task<CompositionSurfaceBrush?> LoadImageAsync(this CanvasControl canvas, Uri uri, DpiMode dpiMode, CacheMode cache = CacheMode.Default)
        {
            return LoadImageAsync(Window.Current.Compositor, canvas, uri, dpiMode, cache);
        }

        /// <summary>
        /// Clears the internal cache of Win2D images
        /// </summary>
        /// <returns>A sequence of the <see cref="CompositionBrush"/> objects that were present in the cache</returns>
        /// <remarks>The returned items should be manually disposed after checking that they are no longer in use in other effect pipelines</remarks>
        public static async Task<IReadOnlyList<CompositionBrush>> ClearCacheAsync()
        {
            using (await Win2DMutex.LockAsync())
                return Cache.Clear();
        }

        #endregion

        #region Library APIs

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> from the input <see cref="System.Uri"/>, and prepares it to be used in a tile effect
        /// </summary>
        /// <param name="creator">The resource creator to use to load the image bitmap (it can be the same <see cref="CanvasDevice"/> used later)</param>
        /// <param name="compositor">The compositor instance to use to create the final brush</param>
        /// <param name="canvasDevice">The device to use to process the Win2D image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        private static async Task<CompositionSurfaceBrush> LoadSurfaceBrushAsync(
            ICanvasResourceCreator creator,
            Compositor compositor,
            CanvasDevice canvasDevice,
            Uri uri,
            DpiMode dpiMode)
        {
            CanvasBitmap bitmap = null;
            try
            {
                // Load the bitmap with the appropriate settings
                DisplayInformation display = DisplayInformation.GetForCurrentView();
                float dpi = display.LogicalDpi;
                switch (dpiMode)
                {
                    case DpiMode.UseSourceDpi:
                        bitmap = await CanvasBitmap.LoadAsync(creator, uri);
                        break;
                    case DpiMode.Default96Dpi:
                        bitmap = await CanvasBitmap.LoadAsync(creator, uri, 96);
                        break;
                    case DpiMode.DisplayDpi:
                        bitmap = await CanvasBitmap.LoadAsync(creator, uri, dpi);
                        break;
                    case DpiMode.DisplayDpiWith96AsLowerBound:
                        bitmap = await CanvasBitmap.LoadAsync(creator, uri, dpi >= 96 ? dpi : 96);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dpiMode), "Unsupported DPI mode");
                }

                // Get the device and the target surface
                CompositionGraphicsDevice device = CanvasComposition.CreateCompositionGraphicsDevice(compositor, canvasDevice);
                CompositionDrawingSurface surface = device.CreateDrawingSurface(default, DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

                // Calculate the surface size
                Size
                    size = bitmap.Size,
                    sizeInPixels = new Size(bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height);
                CanvasComposition.Resize(surface, sizeInPixels);

                // Draw the image on the surface and get the resulting brush
                using (CanvasDrawingSession session = CanvasComposition.CreateDrawingSession(surface, new Rect(0, 0, sizeInPixels.Width, sizeInPixels.Height), dpi))
                {
                    // Fill the target surface
                    session.Clear(Color.FromArgb(0, 0, 0, 0));
                    session.DrawImage(bitmap, new Rect(0, 0, size.Width, size.Height), new Rect(0, 0, size.Width, size.Height));
                    session.EffectTileSize = new BitmapSize { Width = (uint)size.Width, Height = (uint)size.Height };

                    // Setup the effect brush to use
                    CompositionSurfaceBrush brush = surface.Compositor.CreateSurfaceBrush(surface);
                    brush.Stretch = CompositionStretch.None;
                    double pixels = display.RawPixelsPerViewPixel;
                    if (pixels > 1)
                    {
                        brush.Scale = new Vector2((float)(1 / pixels));
                        brush.BitmapInterpolationMode = CompositionBitmapInterpolationMode.NearestNeighbor;
                    }
                    return brush;
                }
            }
            finally
            {
                // Manual using block to allow the initial switch statement
                bitmap?.Dispose();
                Cache.Cleanup();
            }
        }

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image
        /// </summary>
        /// <param name="compositor">The compositor to use to render the Win2D image</param>
        /// <param name="canvas">The <see cref="CanvasControl"/> to use to load the target image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">Indicates the cache option to use to load the image</param>
        internal static async Task<CompositionSurfaceBrush?> LoadImageAsync(
            Compositor compositor,
            CanvasControl canvas,
            Uri uri,
            DpiMode dpiMode,
            CacheMode cache)
        {
            TaskCompletionSource<CompositionSurfaceBrush> tcs = new TaskCompletionSource<CompositionSurfaceBrush>();

            // Loads an image using the input CanvasDevice instance
            async Task<CompositionSurfaceBrush> LoadImageAsync(bool shouldThrow)
            {
                // Load the image - this will only succeed when there's an available Win2D device
                try
                {
                    return await LoadSurfaceBrushAsync(canvas, compositor, canvas.Device, uri, dpiMode);
                }
                catch when (!shouldThrow)
                {
                    // Win2D error, just ignore and continue
                    return null;
                }
            }

            // Handler to create the Win2D image from the input CanvasControl
            async void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
            {
                // Cancel previous actions
                args.GetTrackedAction()?.Cancel();

                // Load the image and notify the canvas
                Task<CompositionSurfaceBrush> task = LoadImageAsync(false);
                IAsyncAction action = task.AsAsyncAction();
                try
                {
                    args.TrackAsyncAction(action);
                    CompositionSurfaceBrush brush = await task;
                    action.Cancel();
                    tcs.TrySetResult(brush);
                }
                catch (COMException)
                {
                    // Somehow another action was still being tracked
                    tcs.TrySetResult(null);
                }
            }

            // Lock the semaphore and check the cache first
            using (await Win2DMutex.LockAsync())
            {
                if (cache == CacheMode.Default && Cache.TryGetInstance(uri, out CompositionSurfaceBrush cached)) return cached;

                // Load the image
                canvas.CreateResources += Canvas_CreateResources;
                try
                {
                    // This will throw and the canvas will re-initialize the Win2D device if needed
                    await LoadImageAsync(true);
                }
                catch (ArgumentException)
                {
                    // Just ignore here
                }
                catch
                {
                    // Win2D messed up big time
                    tcs.TrySetResult(null);
                }
                await Task.WhenAny(tcs.Task, Task.Delay(DeviceLostRecoveryThreshold).ContinueWith(t => tcs.TrySetResult(null)));
                canvas.CreateResources -= Canvas_CreateResources;
                CompositionSurfaceBrush brush = tcs.Task.Result;

                // Cache when needed and return the result
                if (brush != null)
                {
                    if (cache == CacheMode.Default) Cache.Add(uri, brush);
                    else if (cache == CacheMode.Overwrite) Cache.Overwrite(uri, brush);
                }
                return brush;
            }
        }

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image from the shared <see cref="CanvasDevice"/> instance
        /// </summary>
        /// <param name="compositor">The compositor to use to render the Win2D image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        /// <param name="cache">Indicates the cache option to use to load the image</param>
        internal static async Task<CompositionSurfaceBrush?> LoadImageAsync(
            Compositor compositor,
            Uri uri,
            DpiMode dpiMode,
            CacheMode cache)
        {
            // Lock and check the cache first
            using (await Win2DMutex.LockAsync())
            {
                uri = uri.ToAppxUri();
                if (cache == CacheMode.Default && Cache.TryGetInstance(uri, out CompositionSurfaceBrush cached)) return cached;

                // Load the image
                CompositionSurfaceBrush? brush;
                try
                {
                    // This will throw and the canvas will re-initialize the Win2D device if needed
                    CanvasDevice sharedDevice = CanvasDevice.GetSharedDevice();
                    brush = await LoadSurfaceBrushAsync(sharedDevice, compositor, sharedDevice, uri, dpiMode);
                }
                catch
                {
                    // Device error
                    brush = null;
                }

                // Cache when needed and return the result
                if (brush != null)
                {
                    if (cache == CacheMode.Default) Cache.Add(uri, brush);
                    else if (cache == CacheMode.Overwrite) Cache.Overwrite(uri, brush);
                }
                return brush;
            }
        }

        #endregion
    }
}