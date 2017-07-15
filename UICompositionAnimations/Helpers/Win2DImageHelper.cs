using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
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
        [PublicAPI]
        [Pure, ItemCanBeNull]
        public static Task<CompositionSurfaceBrush> LoadImageAsync([NotNull] Uri uri)
        {
            return LoadImageAsync(Window.Current.Compositor, uri, CacheLoadingMode.DisableCaching);
        }

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image
        /// </summary>
        /// <param name="canvas">The <see cref="CanvasControl"/> to use to load the target image</param>
        /// <param name="uri">The path to the image to load</param>
        [PublicAPI]
        [Pure, ItemCanBeNull]
        internal static Task<CompositionSurfaceBrush> LoadImageAsync([NotNull] this CanvasControl canvas, [NotNull] Uri uri)
        {
            return LoadImageAsync(Window.Current.Compositor, canvas, uri, CacheLoadingMode.DisableCaching);
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
        [ItemNotNull]
        private static async Task<CompositionSurfaceBrush> LoadSurfaceBrushAsync([NotNull] ICanvasResourceCreator creator,
            [NotNull] Compositor compositor, [NotNull] CanvasDevice canvasDevice, [NotNull] Uri uri)
        {
            using (CanvasBitmap bitmap = await CanvasBitmap.LoadAsync(creator, uri))
            {
                // Get the device and the target surface
                CompositionGraphicsDevice device = CanvasComposition.CreateCompositionGraphicsDevice(compositor, canvasDevice);
                CompositionDrawingSurface surface = device.CreateDrawingSurface(default(Size),
                    DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);

                // Calculate the surface size
                Size size = bitmap.Size;
                CanvasComposition.Resize(surface, size);

                // Draw the image on the surface and get the resulting brush
                using (CanvasDrawingSession session = CanvasComposition.CreateDrawingSession(surface))
                {
                    session.Clear(Color.FromArgb(0, 0, 0, 0));
                    session.DrawImage(bitmap, new Rect(0, 0, size.Width, size.Height), new Rect(0, 0, size.Width, size.Height));
                    session.EffectTileSize = new BitmapSize { Width = (uint)size.Width, Height = (uint)size.Height };
                    CompositionSurfaceBrush brush = surface.Compositor.CreateSurfaceBrush(surface);
                    brush.Stretch = CompositionStretch.None;
                    return brush;
                }
            }
        }

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image
        /// </summary>
        /// <param name="compositor">The compositor to use to render the Win2D image</param>
        /// <param name="canvas">The <see cref="CanvasControl"/> to use to load the target image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="options">Indicates whether or not to force the reload of the Win2D image</param>
        [ItemCanBeNull]
        internal static async Task<CompositionSurfaceBrush> LoadImageAsync(
            [NotNull] Compositor compositor, [NotNull] CanvasControl canvas, [NotNull] Uri uri, CacheLoadingMode options)
        {
            TaskCompletionSource<CompositionSurfaceBrush> tcs = new TaskCompletionSource<CompositionSurfaceBrush>();
            async Task<CompositionSurfaceBrush> LoadImageAsync(bool shouldThrow)
            {
                // Load the image - this will only succeed when there's an available Win2D device
                try
                {
                    return await LoadSurfaceBrushAsync(canvas, compositor, canvas.Device, uri);
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
            if (options == CacheLoadingMode.EnableCaching && SurfacesCache.TryGetValue(uri, out CompositionSurfaceBrush cached))
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
                options != CacheLoadingMode.DisableCaching &&
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
        [ItemCanBeNull]
        internal static async Task<CompositionSurfaceBrush> LoadImageAsync(
            [NotNull] Compositor compositor, [NotNull] Uri uri, CacheLoadingMode options)
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
            if (options == CacheLoadingMode.EnableCaching && SurfacesCache.TryGetValue(uri, out CompositionSurfaceBrush cached))
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
                brush = await LoadSurfaceBrushAsync(sharedDevice, compositor, sharedDevice, uri);
            }
            catch
            {
                // Device error
                brush = null;
            }

            // Cache when needed and return the result
            if (brush != null &&
                options != CacheLoadingMode.DisableCaching &&
                !SurfacesCache.ContainsKey(uri)) SurfacesCache.Add(uri, brush);
            Win2DSemaphore.Release();
            return brush;
        }

        #endregion
    }
}
