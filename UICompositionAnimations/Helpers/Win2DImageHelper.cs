using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas.UI.Xaml;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Helpers
{
    /// <summary>
    /// A helper classe that loads Win2D images and manages an internal cache of <see cref="CompositionBrush"/> objects with the loaded images
    /// </summary>
    public static class Win2DImageHelper
    {
        /// <summary>
        /// Gets the maximum time to wait for the Win2D device to be restored in case of initial failure
        /// </summary>
        private const int DeviceLostRecoveryThreshold = 1000;

        /// <summary>
        /// Gets a shared semaphore to avoid loading multiple Win2D resources at the same time
        /// </summary>
        private static readonly SemaphoreSlim Win2DSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Gets the local cache mapping for previously loaded Win2D images
        /// </summary>
        private static readonly IDictionary<Uri, CompositionSurfaceBrush> SurfacesCache = new Dictionary<Uri, CompositionSurfaceBrush>();

        #region Public APIs

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image from the shared <see cref="CanvasDevice"/> instance
        /// </summary>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        [PublicAPI]
        [Pure, ItemCanBeNull]
        public static Task<CompositionSurfaceBrush> LoadImageAsync([NotNull] Uri uri, BitmapDPIMode dpiMode)
        {
            return LoadImageAsync(Window.Current.Compositor, uri, BitmapCacheMode.DisableCaching, dpiMode);
        }

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image
        /// </summary>
        /// <param name="canvas">The <see cref="CanvasControl"/> to use to load the target image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        [PublicAPI]
        [Pure, ItemCanBeNull]
        public static Task<CompositionSurfaceBrush> LoadImageAsync([NotNull] this CanvasControl canvas, [NotNull] Uri uri, BitmapDPIMode dpiMode)
        {
            return LoadImageAsync(Window.Current.Compositor, canvas, uri, BitmapCacheMode.DisableCaching, dpiMode);
        }

        /// <summary>
        /// Clears the internal cache of Win2D images
        /// </summary>
        /// <returns>A sequence of the <see cref="CompositionBrush"/> objects that were present in the cache</returns>
        /// <remarks>The returned items should be manually disposed once checked that they are no longer being used in other effect pipelines</remarks>
        [PublicAPI]
        [MustUseReturnValue, ItemNotNull]
        public static async Task<IEnumerable<CompositionBrush>> ClearCacheAsync()
        {
            await Win2DSemaphore.WaitAsync();
            IEnumerable<CompositionSurfaceBrush> surfaces = SurfacesCache.Values;
            SurfacesCache.Clear();
            Win2DSemaphore.Release();
            return surfaces;
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
        [ItemNotNull]
        private static async Task<CompositionSurfaceBrush> LoadSurfaceBrushAsync([NotNull] ICanvasResourceCreator creator,
            [NotNull] Compositor compositor, [NotNull] CanvasDevice canvasDevice, [NotNull] Uri uri, BitmapDPIMode dpiMode)
        {
            CanvasBitmap bitmap = null;
            try
            {
                // Load the bitmap with the appropriate settings
                DisplayInformation display = DisplayInformation.GetForCurrentView();
                float dpi = display.LogicalDpi;
                switch (dpiMode)
                {
                    case BitmapDPIMode.UseSourceDPI:
                        bitmap = await CanvasBitmap.LoadAsync(creator, uri);
                        break;
                    case BitmapDPIMode.Default96DPI:
                        bitmap = await CanvasBitmap.LoadAsync(creator, uri, 96);
                        break;
                    case BitmapDPIMode.CopyDisplayDPISetting:
                        bitmap = await CanvasBitmap.LoadAsync(creator, uri, dpi);
                        break;
                    case BitmapDPIMode.CopyDisplayDPISettingsWith96AsLowerBound:
                        bitmap = await CanvasBitmap.LoadAsync(creator, uri, dpi >= 96 ? dpi : 96);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dpiMode), "Unsupported DPI mode");
                }

                // Get the device and the target surface
                CompositionGraphicsDevice device = CanvasComposition.CreateCompositionGraphicsDevice(compositor, canvasDevice);
                CompositionDrawingSurface surface = device.CreateDrawingSurface(default(Size),
                    DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

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
            }
        }

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image
        /// </summary>
        /// <param name="compositor">The compositor to use to render the Win2D image</param>
        /// <param name="canvas">The <see cref="CanvasControl"/> to use to load the target image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="options">Indicates whether or not to force the reload of the Win2D image</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        [ItemCanBeNull]
        internal static async Task<CompositionSurfaceBrush> LoadImageAsync(
            [NotNull] Compositor compositor, [NotNull] CanvasControl canvas, [NotNull] Uri uri, BitmapCacheMode options, BitmapDPIMode dpiMode)
        {
            TaskCompletionSource<CompositionSurfaceBrush> tcs = new TaskCompletionSource<CompositionSurfaceBrush>();
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
            await Win2DSemaphore.WaitAsync();
            if (options == BitmapCacheMode.EnableCaching && SurfacesCache.TryGetValue(uri, out CompositionSurfaceBrush cached))
            {
                Win2DSemaphore.Release();
                return cached;
            }

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
            CompositionSurfaceBrush instance = tcs.Task.Result;

            // Cache when needed and return the result
            if (instance != null &&
                options != BitmapCacheMode.DisableCaching &&
                !SurfacesCache.ContainsKey(uri)) SurfacesCache.Add(uri, instance);
            Win2DSemaphore.Release();
            return instance;
        }

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image from the shared <see cref="CanvasDevice"/> instance
        /// </summary>
        /// <param name="compositor">The compositor to use to render the Win2D image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="options">Indicates whether or not to force the reload of the Win2D image</param>
        /// <param name="dpiMode">Indicates the desired DPI mode to use when loading the image</param>
        [ItemCanBeNull]
        internal static async Task<CompositionSurfaceBrush> LoadImageAsync(
            [NotNull] Compositor compositor, [NotNull] Uri uri, BitmapCacheMode options, BitmapDPIMode dpiMode)
        {
            // Fix the Uri if it has been generated by the XAML designer
            if (uri.Scheme.Equals("ms-resource"))
            {
                String path = uri.AbsolutePath.StartsWith("/Files")
                    ? uri.AbsolutePath.Replace("/Files", String.Empty)
                    : uri.AbsolutePath;
                uri = new Uri($"ms-appx://{path}");
            }

            // Lock the semaphore and check the cache first
            await Win2DSemaphore.WaitAsync();
            if (options == BitmapCacheMode.EnableCaching && SurfacesCache.TryGetValue(uri, out CompositionSurfaceBrush cached))
            {
                Win2DSemaphore.Release();
                return cached;
            }

            // Load the image
            CompositionSurfaceBrush brush;
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
            if (brush != null &&
                options != BitmapCacheMode.DisableCaching &&
                !SurfacesCache.ContainsKey(uri)) SurfacesCache.Add(uri, brush);
            Win2DSemaphore.Release();
            return brush;
        }

        #endregion
    }
}
