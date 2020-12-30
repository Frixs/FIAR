using System;
using System.Linq;
using System.Reflection;

namespace Fiar
{
    /// <summary>
    /// Enum attribute extensions
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Get <see cref="System.ComponentModel.DescriptionAttribute"/> from the enum if exists.
        /// </summary>
        /// <param name="genericEnum">The enum</param>
        /// <returns>Description string or enum.ToString if does not exist</returns>
        public static string GetDescription(this Enum genericEnum)
        {
            // Get enum type
            Type type = genericEnum.GetType();
            // Get member info
            MemberInfo[] memberInfo = type.GetMember(genericEnum.ToString());
            // Check if member info exists and it contains any information...
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                // Get all attributes
                var attributes = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                // Check if any attribute exists...
                if ((attributes != null && attributes.Count() > 0))
                    // Return Description attribute value
                    return ((System.ComponentModel.DescriptionAttribute)attributes.ElementAt(0)).Description;
            }

            // No Description attribute exists....
            return genericEnum.ToString();
        }

        /// <summary>
        /// General purpose to get attribute of enum value
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type to get</typeparam>
        /// <param name="value">Enum value</param>
        /// <returns>Attribute instance</returns>
        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            return type.GetField(name) // I prefer to get attributes this way
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .FirstOrDefault();
        }
    }
}
