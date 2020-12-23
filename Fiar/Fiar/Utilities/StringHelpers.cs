using System.Globalization;
using System.Text.RegularExpressions;

namespace Fiar
{
    /// <summary>
    /// Helpers for the <see cref="string"/> class
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        /// Try parse double by specific decimal separator
        /// </summary>
        /// <param name="inValue">Value to parse</param>
        /// <param name="decimalSeparator">The decimal separator</param>
        /// <param name="outValue">Output value</param>
        /// <returns>TRUE= success, otherwise FALSE</returns>
        public static bool TryParseToDouble(string inValue, char decimalSeparator, out double outValue)
        {
            var currentCulture = CultureInfo.InstalledUICulture;
            var numberFormat = (NumberFormatInfo)currentCulture.NumberFormat.Clone();
            numberFormat.NumberDecimalSeparator = $"{decimalSeparator}";

            return double.TryParse(inValue, NumberStyles.Any, numberFormat, out outValue);
        }

        /// <summary>
        /// Format the string as a shorten version with possible 3 dots at the end.
        /// </summary>
        /// <param name="value">The string</param>
        /// <param name="length">Allowed length</param>
        /// <returns></returns>
        public static string ShortenWithDots(string value, int length)
        {
            if (value.Length > length)
                return value.Substring(0, length) + "...";
            return value;
        }

        /// <summary>
        /// Format pascal case into readable form - adds spaces between words
        /// </summary>
        /// <param name="value">The input string</param>
        /// <returns>Formatted output string</returns>
        public static string FormatPascalCase(string value)
        {
            return Regex.Replace(value, "(\\B[A-Z])", " $1");
        }
    }
}
