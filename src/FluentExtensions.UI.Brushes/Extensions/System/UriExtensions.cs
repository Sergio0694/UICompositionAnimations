using JetBrains.Annotations;

namespace System
{
    /// <summary>
    /// An extension <see langword="class"/> for the <see cref="Uri"/> type
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Returns an <see cref="Uri"/> that starts with the ms-appx:// prefix
        /// </summary>
        /// <param name="uri">The input <see cref="Uri"/> to process</param>
        /// <remarks>This is needed because the XAML converter doesn't use the ms-appx:// prefix</remarks>
        [Pure, NotNull]
        internal static Uri ToAppxUri([NotNull] this Uri uri)
        {
            if (uri.Scheme.Equals("ms-resource"))
            {
                string path = uri.AbsolutePath.StartsWith("/Files")
                    ? uri.AbsolutePath.Replace("/Files", string.Empty)
                    : uri.AbsolutePath;
                return new Uri($"ms-appx://{path}");
            }

            return uri;
        }

        /// <summary>
        /// Returns an <see cref="Uri"/> that starts with the ms-appx:// prefix
        /// </summary>
        /// <param name="path">The input relative path to convert</param>
        [Pure, NotNull]
        public static Uri ToAppxUri([NotNull] this string path)
        {
            string prefix = $"ms-appx://{(path.StartsWith('/') ? string.Empty : "/")}";
            return new Uri($"{prefix}{path}");
        }
    }
}
