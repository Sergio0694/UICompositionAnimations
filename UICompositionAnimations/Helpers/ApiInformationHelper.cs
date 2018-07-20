using System;
using Windows.System.Profile;
using JetBrains.Annotations;

namespace UICompositionAnimations.Helpers
{
    /// <summary>
    /// A static class that provides useful information on the available APIs
    /// </summary>
    [PublicAPI]
    public static class ApiInformationHelper
    {
        private static Version _OSVersion;

        /// <summary>
        /// Gets the current OS version for the device in use
        /// </summary>
        /// <remarks>It is better to check the available APIs directly with the other properties exposed by the class
        /// instead of just checking the OS version and use it to make further decisions.
        /// In particular, users on a Windows Insider ring could already have a given set of APIs even though their OS
        /// build doesn't exactly match the official Windows 10 version that supports a requested API contract.</remarks>
        [NotNull]
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
    }
}
