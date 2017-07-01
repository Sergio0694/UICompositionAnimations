using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using JetBrains.Annotations;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace UICompositionAnimations.Behaviours
{
    /// <summary>
    /// An helper class that manages the tint and noise layers on the custom acrylic brush effect
    /// </summary>
    internal static class AcrylicEffectHelper
    {
        /// <summary>
        /// Gets a shared semaphore to avoid loading multiple Win2D resources at the same time
        /// </summary>
        private static readonly SemaphoreSlim Win2DSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Gets the local cache mapping for previously loaded Win2D images
        /// </summary>
        private static readonly IDictionary<String, CompositionSurfaceBrush> SurfacesCache = new Dictionary<String, CompositionSurfaceBrush>();

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image
        /// </summary>
        /// <param name="compositor">The compositor to use to render the Win2D image</param>
        /// <param name="canvas">The <see cref="CanvasControl"/> to use to load the target image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="timeThreshold">The maximum time to wait for the Win2D device to be restored in case of initial failure/></param>
        /// <param name="reload">Indicates whether or not to force the reload of the Win2D image</param>
        [ItemCanBeNull]
        private static async Task<CompositionSurfaceBrush> LoadWin2DSurfaceBrushFromImageAsync(
            [NotNull] Compositor compositor, [NotNull] CanvasControl canvas, [NotNull] Uri uri, int timeThreshold = 1000, bool reload = false)
        {
            TaskCompletionSource<CompositionSurfaceBrush> tcs = new TaskCompletionSource<CompositionSurfaceBrush>();
            async Task<CompositionSurfaceBrush> LoadImageAsync(bool shouldThrow)
            {
                // Load the image - this will only succeed when there's an available Win2D device
                try
                {
                    using (CanvasBitmap bitmap = await CanvasBitmap.LoadAsync(canvas, uri))
                    {
                        // Get the device and the target surface
                        CompositionGraphicsDevice device = CanvasComposition.CreateCompositionGraphicsDevice(compositor, canvas.Device);
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
                            CompositionSurfaceBrush brush = surface.Compositor.CreateSurfaceBrush(surface);
                            return brush;
                        }
                    }
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
            if (!reload && SurfacesCache.TryGetValue(uri.ToString(), out CompositionSurfaceBrush cached))
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
            await Task.WhenAny(tcs.Task, Task.Delay(timeThreshold).ContinueWith(t => tcs.TrySetResult(null)));
            canvas.CreateResources -= Canvas_CreateResources;
            CompositionSurfaceBrush instance = tcs.Task.Result;
            String key = uri.ToString();
            if (instance != null && !SurfacesCache.ContainsKey(key)) SurfacesCache.Add(key, instance);
            Win2DSemaphore.Release();
            return instance;
        }

        /// <summary>
        /// Loads a <see cref="CompositionSurfaceBrush"/> instance with the target image from the shared <see cref="CanvasDevice"/> instance
        /// </summary>
        /// <param name="compositor">The compositor to use to render the Win2D image</param>
        /// <param name="uri">The path to the image to load</param>
        /// <param name="reload">Indicates whether or not to force the reload of the Win2D image</param>
        [ItemCanBeNull]
        private static async Task<CompositionSurfaceBrush> LoadWin2DSurfaceBrushFromImageAsync(
            [NotNull] Compositor compositor, [NotNull] Uri uri, bool reload = false)
        {
            // Lock the semaphore and check the cache first
            await Win2DSemaphore.WaitAsync();
            if (!reload && SurfacesCache.TryGetValue(uri.ToString(), out CompositionSurfaceBrush cached))
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
                using (CanvasBitmap bitmap = await CanvasBitmap.LoadAsync(sharedDevice, uri))
                {
                    // Get the device and the target surface
                    CompositionGraphicsDevice device = CanvasComposition.CreateCompositionGraphicsDevice(compositor, sharedDevice);
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
                        brush = surface.Compositor.CreateSurfaceBrush(surface);
                    }
                }
            }
            catch
            {
                // Device error
                brush = null;
            }
            String key = uri.ToString();
            if (brush != null && !SurfacesCache.ContainsKey(key)) SurfacesCache.Add(key, brush);
            Win2DSemaphore.Release();
            return brush;
        }

        /// <summary>
        /// Concatenates the source effect with a tint overlay and a border effect
        /// </summary>
        /// <param name="compositor">The current <see cref="Compositor"/> object to use</param>
        /// <param name="source">The source effect to insert in the pipeline</param>
        /// <param name="parameters">A dictionary to use to keep track of reference parameters to add when creating a <see cref="CompositionEffectFactory"/></param>
        /// <param name="color">The tint color</param>
        /// <param name="colorMix">The amount of tint color to apply</param>
        /// <param name="canvas">The optional <see cref="CanvasControl"/> to use to generate the image for the <see cref="BorderEffect"/></param>
        /// <param name="uri">The path to the source image to use for the <see cref="BorderEffect"/></param>
        /// <param name="timeThreshold">The maximum time to wait for the Win2D device to be restored in case of initial failure/></param>
        /// <param name="reload">Indicates whether or not to force the reload of the Win2D image</param>
        /// <returns>The resulting effect through the pipeline</returns>
        /// <remarks>The method does side effect on the <paramref name="parameters"/> variable</remarks>
        [MustUseReturnValue, ItemNotNull]
        public static async Task<IGraphicsEffect> ConcatenateEffectWithTintAndBorderAsync(
            [NotNull] Compositor compositor,
            [NotNull] IGraphicsEffectSource source, [NotNull] IDictionary<String, CompositionBrush> parameters,
            Color color, float colorMix,
            [CanBeNull] CanvasControl canvas, [NotNull] Uri uri, int timeThreshold = 1000, bool reload = false)
        {
            // Setup the tint effect
            ArithmeticCompositeEffect tint = new ArithmeticCompositeEffect
            {
                MultiplyAmount = 0,
                Source1Amount = 1 - colorMix,
                Source2Amount = colorMix, // Mix the background with the desired tint color
                Source1 = source,
                Source2 = new ColorSourceEffect { Color = color }
            };

            // Get the noise brush using Win2D
            CompositionSurfaceBrush noiseBitmap = canvas == null
                ? await LoadWin2DSurfaceBrushFromImageAsync(compositor, uri, reload)
                : await LoadWin2DSurfaceBrushFromImageAsync(compositor, canvas, uri, timeThreshold, reload);

            // Make sure the Win2D brush was loaded correctly
            if (noiseBitmap != null)
            {
                // Layer 4 - Noise effect
                BorderEffect borderEffect = new BorderEffect
                {
                    ExtendX = CanvasEdgeBehavior.Wrap,
                    ExtendY = CanvasEdgeBehavior.Wrap,
                    Source = new CompositionEffectSourceParameter(nameof(noiseBitmap))
                };
                BlendEffect blendEffect = new BlendEffect
                {
                    Background = tint,
                    Foreground = borderEffect,
                    Mode = BlendEffectMode.Overlay
                };
                parameters.Add(nameof(noiseBitmap), noiseBitmap);
                return blendEffect;
            }
            return tint;
        }
    }
}
