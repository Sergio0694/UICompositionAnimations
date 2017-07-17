using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using JetBrains.Annotations;
using UICompositionAnimations.Helpers;
using UICompositionAnimations.Helpers.PointerEvents;

namespace UICompositionAnimations.Lights
{
    /// <summary>
    /// A class that contains an attached property to register a target <see cref="UIElement"/> as a lights container
    /// </summary>
    public static class LightsSourceHelper
    {
        // The list of light generators
        private static Func<PointerPositionSpotLight>[] LightGenerators;

        /// <summary>
        /// Initializes the helper with a series of light generators used to create the lights in each target <see cref="UIElement"/>
        /// </summary>
        /// <param name="generators"><para>The sequence of light generators to store and use when needed.</para>
        /// <para>Each generator can be as simple as () => new PointerPositionSpotLight(), or it can assign custom
        /// properties to each light when needed, like () => new PointerPositionSpotLight { IdAppendage = "MyID", Shade = 0x10 }</para></param>
        public static void Initialize([NotNull] params Func<PointerPositionSpotLight>[] generators)
        {
            if (LightGenerators != null) throw new InvalidOperationException("The helper has already been initialized");
            LightGenerators = generators;
        }

        /// <summary>
        /// Gets the <see cref="IsLightsContainerProperty"/> value for the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The target element to inspect</param>
        public static String GetIsLightsContainer(UIElement element)
        {
            return element.GetValue(IsLightsContainerProperty).To<String>();
        }

        /// <summary>
        /// Sets the <see cref="IsLightsContainerProperty"/> value for the target <see cref="UIElement"/>
        /// </summary>
        /// <param name="element">The target element to edit</param>
        /// <param name="value">The new value for the property</param>
        public static void SetIsLightsContainer(UIElement element, bool value)
        {
            element?.SetValue(IsLightsContainerProperty, value);
        }

        /// <summary>
        /// Identifies the attached <see cref="DependencyProperty"/> to set a target <see cref="UIElement"/> as a lights container
        /// </summary>
        public static readonly DependencyProperty IsLightsContainerProperty =
            DependencyProperty.RegisterAttached("IsLightsContainer", typeof(bool), typeof(LightsSourceHelper),
                new PropertyMetadata(default(bool), OnIsLightsContainerPropertyChanged));

        private static readonly IDictionary<UIElement, ControlAttachedHandlersInfo> HandlersMap = new Dictionary<UIElement, ControlAttachedHandlersInfo>();

        private static void OnIsLightsContainerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Platform test
            if (ApiInformationHelper.IsMobileDevice) return;

            // Unpack
            UIElement @this = d.To<UIElement>();
            bool value = e.NewValue.To<bool>();

            // Lights setup
            if (value)
            {
                // Lights setup
                PointerPositionSpotLight[] lights = LightGenerators.Select(factory =>
                {
                    PointerPositionSpotLight light = factory();
                    light.Active = false;
                    @this.Lights.Add(light);
                    return light;
                }).ToArray();

                // Animate the lights when the pointer exits and leaves the area
                bool lightsEnabled = false;
                ControlAttachedHandlersInfo info = @this.ManageHostPointerStates((type, state) =>
                {
                    bool lightsVisible = type == PointerDeviceType.Mouse && state;
                    if (lightsEnabled == lightsVisible) return;
                    lightsEnabled = lightsVisible;
                    foreach (PointerPositionSpotLight light in lights)
                        light.Active = lightsEnabled;
                });
                HandlersMap.Add(@this, info);
            }
            else
            {
                if (!HandlersMap.TryGetValue(@this, out ControlAttachedHandlersInfo info))
                    throw new InvalidOperationException("Error retrieving the pointer handlers info on the current control");
                HandlersMap.Remove(@this);
                info.TryRemoveHandlers();
                @this.Lights.Clear();
            }
        }
    }
}
