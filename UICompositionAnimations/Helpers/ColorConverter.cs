using System;
using System.Linq;
using Windows.UI;

namespace UICompositionAnimations.Helpers
{
    /// <summary>
    /// A simple class that contains some color managment methods
    /// </summary>
    internal static class ColorConverter
    {
        /// <summary>
        /// Returns the Color represented by the hex String
        /// </summary>
        /// <param name="color">If it contains just the RGB values {RRBBGG} the Alpha channel is automatically set to FF</param>
        public static Color String2Color(String color)
        {
            //Cancels the # symbol from the String, if present
            if (color.Contains('#')) color = color.Substring(1);
            byte alpha;
            String RGB;
            if (color.Length == 6)
            {
                alpha = 255;
                RGB = color;
            }
            else
            {
                alpha = byte.Parse(color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                RGB = color.Substring(2);
            }
            return ColorHelper.FromArgb(alpha,
                    byte.Parse(RGB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(RGB.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(RGB.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
        }
    }
}
