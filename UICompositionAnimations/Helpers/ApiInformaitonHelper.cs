using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.System.Profile;

namespace UICompositionAnimations.Helpers
{
    static class ApiInformationHelper
    {
        static Version osversion = null;
        public static Version OSVersion()
        {
            if (osversion == null)
            {
                string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                ulong version = ulong.Parse(deviceFamilyVersion);
                ulong major = (version & 0xFFFF000000000000L) >> 48;
                ulong minor = (version & 0x0000FFFF00000000L) >> 32;
                ulong build = (version & 0x00000000FFFF0000L) >> 16;
                ulong revision = (version & 0x000000000000FFFFL);
                osversion = new Version((int)major, (int)minor, (int)build, (int)revision);
            }
            return osversion;
        }
        public static bool AreConnectedAnimationsAvailable
        {
            get
            {
                return ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimationService");
            }
        }
        public static bool IsFindFirstFocusableElementAvailable
        {
            get
            {
                return ApiInformation.IsMethodPresent("Windows.UI.Xaml.Input.FocusManager", "FindFirstFocusableElement");
            }
        }
        public static bool SupportsCompositionAnimationTarget
        {
            get
            {
                return ApiInformation.IsPropertyPresent("Windows.UI.Composition.CompositionAnimation", "Target");
            }
        }
        public static bool IsCompositionAnimationGroupAvailable
        {
            get
            {
                return ApiInformation.IsTypePresent("Windows.UI.Composition.CompositionAnimationGroup");
            }
        }
        public static bool SupportsSphericalVideoProjection
        {
            get
            {
                return ApiInformation.IsPropertyPresent("Windows.Media.Playback.MediaPlaybackSession", "SphericalVideoProjection");
            }
        }
        public static bool IsElementSoundAvailable
        {
            get
            {
                return ApiInformation.IsTypePresent("Windows.UI.Xaml.ElementSoundPlayer");
            }
        }

        public static bool AreAppBackgroundEventsAvailable
        {
            get
            {
                return ApiInformation.IsEventPresent("Windows.UI.Xaml.Application", "EnteredBackground");
            }
        }
        public static bool AreImplicitShowHideAnimationsAvailable
        {
            get
            {
                return ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", "SetImplicitShowAnimation");
            }
        }

        public static bool AreXamlLightsSupported
        {
            get
            {
                return ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlLight");
            }
        }

        public static bool AreCustomBrushesSupported
        {
            get
            {
                return ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase");
            }
        }

        public static bool IsCompactOverlayAvailable => ApiInformation.IsEnumNamedValuePresent("Windows.UI.ViewManagement.ApplicationViewMode", "CompactOverlay");
        public static bool IsLightDismissOverlayModeAvailable()
        {
            return ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Primitives.FlyoutBase", "LightDismissOverlayMode");
        }
        public static bool IsAnniversaryUpdate()
        {
            return OSVersion().Build > 14000;
        }
        public static bool IsCreatorsUpdate()
        {
            return OSVersion().Build > 15000;
        }
        public static bool IsFallCreatorsUpdate()
        {
            return OSVersion().Build > 16000;
        }
        public static bool IsStatusBarAvailable()
        {
            return ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
        }
        public static bool IsXYFocusAvailable()
        {
            return ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", "XYFocusUp");
        }

        public static bool IsRemoteSystemAvailable()
        {
            return ApiInformation.IsTypePresent("Windows.System.RemoteSystems.RemoteSystem");
        }
        public static bool AreHostBackdropEffectsAvailable()
        {
            return (ApiInformation.IsTypePresent("Microsoft.Graphics.Canvas.Effects.GaussianBlurEffect") && ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"));
        }
        public static bool AreBackdropEffectsAvailable()
        {
            return (ApiInformation.IsTypePresent("Microsoft.Graphics.Canvas.Effects.GaussianBlurEffect") && ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateBackdropBrush"));
        }

        public static bool AreDropShadowsAvailable
        {
            get
            {
                return ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateDropShadow");
            }
        }

        public static bool AreImplicitAnimationsAvailable()
        {
            return ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateImplicitAnimationCollection");
        }

        public static bool IsRepositionStaggerringAvailable()
        {
            return ApiInformation.IsMethodPresent("Windows.UI.Xaml.Media.Animation.RepositionThemeTransition", "IsStaggeringEnabled");
        }

        public static bool IsRequiresPointerAvailable()
        {
            return ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", "RequiresPointer");
        }


    }
}
