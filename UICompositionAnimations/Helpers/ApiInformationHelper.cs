using System;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
using Windows.UI.Composition;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;

namespace UICompositionAnimationsLegacy.Helpers
{
    /// <summary>
    /// A static class that provides useful information on the available APIs
    /// </summary>
    public static class ApiInformationHelper
    {
        #region OS build

        static Version _OSVersion;

        /// <summary>
        /// Gets the current OS version for the device in use
        /// </summary>
        /// <remarks>It is better to check the available APIs directly with the other properties exposed by the class
        /// instead of just checking the OS version and use it to make further decisions.
        /// In particular, users on a Windows Insider ring could already have a given set of APIs even though their OS
        /// build doesn't exactly match the official Windows 10 version that supports a requested API contract.</remarks>
        public static Version OSVersion
        {
            get
            {
                if (_OSVersion == null)
                {
                    string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                    ulong version = ulong.Parse(deviceFamilyVersion);
                    ulong major = (version & 0xFFFF000000000000L) >> 48;
                    ulong minor = (version & 0x0000FFFF00000000L) >> 32;
                    ulong build = (version & 0x00000000FFFF0000L) >> 16;
                    ulong revision = version & 0x000000000000FFFFL;
                    _OSVersion = new Version((int)major, (int)minor, (int)build, (int)revision);
                }
                return _OSVersion;
            }
        }

        /// <summary>
        /// Gets whether or not the current OS version is at least the Anniversary Update (14393)
        /// </summary>
        /// <returns></returns>
        public static bool IsAnniversaryUpdateOrLater => OSVersion.Build > 14393;

        /// <summary>
        /// Gets whether or not the current OS version is at least the Creator's Update (15063)
        /// </summary>
        /// <returns></returns>
        public static bool IsCreatorsUpdateOrLater => OSVersion.Build > 15063;

        /// <summary>
        /// Gets whether or not the current OS version is at least the Fall Creator's Update (16xxx)
        /// </summary>
        /// <returns></returns>
        public static bool IsFallCreatorsUpdateOrLater => OSVersion.Build > 16299;

        #endregion

        #region Available APIs

        /// <summary>
        /// Gets whether or not the connected animations APIs are available
        /// </summary>
        public static bool AreConnectedAnimationsAvailable => ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimationService");

        /// <summary>
        /// Gets whether or not the APIs to find the focusable elements are available
        /// </summary>
        public static bool IsFindFirstFocusableElementAvailable => ApiInformation.IsMethodPresent("Windows.UI.Xaml.Input.FocusManager", nameof(FocusManager.FindFirstFocusableElement));

        /// <summary>
        /// Gets or sets whether the Target property of the composition animation APIs is available
        /// </summary>
        public static bool SupportsCompositionAnimationTarget => ApiInformation.IsPropertyPresent("Windows.UI.Composition.CompositionAnimation", nameof(CompositionAnimation.Target));

        /// <summary>
        /// Gets whether or not the composition animation group type is present
        /// </summary>
        public static bool IsCompositionAnimationGroupAvailable => ApiInformation.IsTypePresent("Windows.UI.Composition.CompositionAnimationGroup");

        /// <summary>
        /// Gets whether or not the implicit animations APIs are available
        /// </summary>
        /// <returns></returns>
        public static bool AreImplicitAnimationsAvailable => ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", nameof(Compositor.CreateImplicitAnimationCollection));

        /// <summary>
        /// Gets whether or not the implicit show/hide animation APIs are available
        /// </summary>
        public static bool AreImplicitShowHideAnimationsAvailable => ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", nameof(ElementCompositionPreview.SetImplicitShowAnimation));

        /// <summary>
        /// Gets whether or not the XAML lights APIs are available
        /// </summary>
        public static bool AreXamlLightsSupported => ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlLight");

        /// <summary>
        /// Gets whether or not the custom XAML brush APIs are available
        /// </summary>
        public static bool AreCustomBrushesSupported => ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase");

        /// <summary>
        /// Gets whether or not the XYFocusUp property is available/>
        /// </summary>
        /// <returns></returns>
        public static bool IsXYFocusAvailable => ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", nameof(Control.XYFocusUp));

        /// <summary>
        /// Gets whether or not the host backdrop effects are available
        /// </summary>
        /// <returns></returns>
        public static bool AreHostBackdropEffectsAvailable => ApiInformation.IsTypePresent("Microsoft.Graphics.Canvas.Effects.GaussianBlurEffect") &&
                                                              ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", nameof(Compositor.CreateHostBackdropBrush));

        /// <summary>
        /// Gets whether or not the backdrop effects APIs are available
        /// </summary>
        /// <returns></returns>
        public static bool AreBackdropEffectsAvailable => ApiInformation.IsTypePresent("Microsoft.Graphics.Canvas.Effects.GaussianBlurEffect") &&
                                                          ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", nameof(Compositor.CreateBackdropBrush));

        /// <summary>
        /// Gets whether or not the drop shadows APIs are available
        /// </summary>
        public static bool AreDropShadowsAvailable => ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", nameof(Compositor.CreateDropShadow));

        /// <summary>
        /// Gets whether or not the staggering method is available
        /// </summary>
        /// <returns></returns>
        public static bool IsRepositionStaggerringAvailable => ApiInformation.IsMethodPresent("Windows.UI.Xaml.Media.Animation.RepositionThemeTransition", "IsStaggeringEnabled");

        /// <summary>
        /// Gets whether or not the RequiresPointer property is available
        /// </summary>
        public static bool IsRequiresPointerAvailable => ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", nameof(Control.RequiresPointer));

        #endregion

        #region Device family

        private static bool? _IsMobileDevice;

        /// <summary>
        /// Gets whether or not the device is a mobile phone
        /// </summary>
        public static bool IsMobileDevice
        {
            get
            {
                if (_IsMobileDevice == null)
                {
                    try
                    {
                        IObservableMap<String, String> qualifiers = ResourceContext.GetForCurrentView().QualifierValues;
                        _IsMobileDevice = qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile";
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // No idea why this should happen
                        return ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
                    }
                }
                return _IsMobileDevice.Value;
            }
        }

        private static bool? _IsDesktop;

        /// <summary>
        /// Gets whether or not the device is running Windows 10 Desktop
        /// </summary>
        public static bool IsDesktop
        {
            get
            {
                if (_IsDesktop == null)
                {
                    try
                    {
                        IObservableMap<String, String> qualifiers = ResourceContext.GetForCurrentView().QualifierValues;
                        _IsDesktop = qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Desktop";
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Weird crash, but still...
                        return false;
                    }
                }
                return _IsDesktop.Value;
            }
        }

        #endregion
    }
}
