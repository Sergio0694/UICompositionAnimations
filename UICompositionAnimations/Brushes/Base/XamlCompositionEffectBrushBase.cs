using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using JetBrains.Annotations;
using UICompositionAnimations.Behaviours;

namespace UICompositionAnimations.Brushes.Base
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

        /// <summary>
        /// Gets or sets an optional path that can be used to mask the <see cref="Windows.UI.Composition.CompositionBrush"/> in the current instance
        /// </summary>
        [CanBeNull]
        public string Path { get; set; }

        /// <inheritdoc/>
        protected override async void OnConnected()
        {
            using (await ConnectedMutex.LockAsync())
            {
                if (CompositionBrush != null) return;
                Windows.UI.Composition.CompositionBrush brush = await OnBrushRequested().BuildAsync();
                CompositionBrush = Path == null ? brush : brush.AsMaskedBrush(Path);
            }
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
