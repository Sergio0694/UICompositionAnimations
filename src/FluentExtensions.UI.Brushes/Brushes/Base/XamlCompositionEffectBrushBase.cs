using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using FluentExtensions.UI.Brushes.Pipelines;

#nullable enable

namespace FluentExtensions.UI.Brushes.Brushes.Base
{
    /// <summary>
    /// A custom <see cref="XamlCompositionBrushBase"/> <see langword="class"/> that's ready to be used with a custom <see cref="PipelineBuilder"/> pipeline
    /// </summary>
    public abstract class XamlCompositionEffectBrushBase : XamlCompositionBrushBase
    {
        /// <summary>
        /// The initialization <see cref="AsyncMutex"/> instance
        /// </summary>
        private readonly AsyncMutex ConnectedMutex = new AsyncMutex();

        /// <summary>
        /// A method that builds and returns the <see cref="PipelineBuilder"/> pipeline to use in the current instance.<para/>
        /// This method can also be used to store any needed <see cref="EffectAnimation"/> instances in local fields, for later use (they will need to be called upon <see cref="XamlCompositionBrushBase.CompositionBrush"/>).
        /// </summary>
        protected abstract PipelineBuilder OnBrushRequested();

        private bool _IsEnabled = true;

        /// <summary>
        /// Gest or sets whether or not the current brush is using the provided pipeline, or the fallback color
        /// </summary>
        public bool IsEnabled
        {
            get => _IsEnabled;
            set => OnEnabledToggled(value);
        }

        /// <inheritdoc/>
        protected override async void OnConnected()
        {
            using (await ConnectedMutex.LockAsync())
            {
                if (CompositionBrush == null)
                {
                    if (_IsEnabled) CompositionBrush = await OnBrushRequested().BuildAsync();
                    else CompositionBrush = await PipelineBuilder.FromColor(FallbackColor).BuildAsync();
                }
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

        /// <summary>
        /// Updates the <see cref="XamlCompositionBrushBase.CompositionBrush"/> property depending on the input value
        /// </summary>
        /// <param name="value">The new value being set to the <see cref="IsEnabled"/> property</param>
        protected async void OnEnabledToggled(bool value)
        {
            using (await ConnectedMutex.LockAsync())
            {
                if (_IsEnabled == value) return;
                _IsEnabled = value;

                if (CompositionBrush != null)
                {
                    if (_IsEnabled) CompositionBrush = await OnBrushRequested().BuildAsync();
                    else CompositionBrush = await PipelineBuilder.FromColor(FallbackColor).BuildAsync();
                }
            }
        }
    }
}
