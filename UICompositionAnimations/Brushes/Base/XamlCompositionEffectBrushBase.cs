using Windows.UI.Xaml.Media;
using JetBrains.Annotations;
using UICompositionAnimationsLegacy.Behaviours;
using UICompositionAnimationsLegacy.Helpers;

namespace UICompositionAnimationsLegacy.Brushes.Base
{
    /// <summary>
    /// A custom <see cref="XamlCompositionBrushBase"/> <see langword="class"/> that's ready to be used with a custom <see cref="CompositionBrushBuilder"/> pipeline.
    /// </summary>
    [PublicAPI]
    public abstract class XamlCompositionEffectBrushBase : XamlCompositionBrushBase
    {
        // Initialization mutex
        [NotNull]
        private readonly AsyncMutex ConnectedMutex = new AsyncMutex();

        /// <summary>
        /// A method that builds and returns the <see cref="CompositionBrushBuilder"/> pipeline to use in the current instance.<para/>
        /// This method can also be used to store any needed <see cref="EffectAnimation"/> instances in local fields, for later use (they will need to be called upon <see cref="XamlCompositionBrushBase.CompositionBrush"/>).
        /// </summary>
        [MustUseReturnValue, NotNull]
        protected abstract CompositionBrushBuilder OnBrushRequested();

        /// <inheritdoc/>
        protected override async void OnConnected()
        {
            using (await ConnectedMutex.LockAsync())
                if (CompositionBrush == null)
                    CompositionBrush = await OnBrushRequested().BuildAsync();
            base.OnConnected();
        }

        /// <inheritdoc/>
        protected override async void OnDisconnected()
        {
            using (await ConnectedMutex.LockAsync())
            {
                if (CompositionBrush != null)
                {
                    CompositionBrush.Dispose();
                    CompositionBrush = null;
                }
            }
            base.OnDisconnected();
        }
    }
}