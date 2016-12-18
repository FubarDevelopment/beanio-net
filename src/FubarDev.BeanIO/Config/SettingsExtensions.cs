// <copyright file="SettingsExtensions.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace BeanIO.Config
{
    public static class SettingsExtensions
    {
        /// <summary>
        /// Returns the boolean value of a BeanIO configuration setting
        /// </summary>
        /// <param name="settings">The settings to read the value from</param>
        /// <param name="key">the property key</param>
        /// <returns>true if the property value is "1" or "true" (case insensitive),
        /// or false if the property is any other value</returns>
        public static bool GetBoolean(this ISettings settings, string key)
        {
            var temp = settings[key];
            if (string.IsNullOrEmpty(temp))
                return false;
            temp = temp.Trim();
            if (string.Equals("true", temp, StringComparison.OrdinalIgnoreCase))
                return true;
            return temp == "1";
        }

        /// <summary>
        /// Returns a BeanIO configuration setting as an integer
        /// </summary>
        /// <param name="settings">The settings to read the value from</param>
        /// <param name="key">the property key</param>
        /// <param name="defaultValue">the default value if the setting wasn't configured or invalid</param>
        /// <returns>the <code>int</code> property value or <paramref name="defaultValue"/></returns>
        public static int GetInt(this ISettings settings, string key, int defaultValue)
        {
            var temp = settings[key];
            if (string.IsNullOrWhiteSpace(temp))
                return defaultValue;
            int result;
            if (int.TryParse(temp, out result))
                return result;

            return defaultValue;
        }
    }
}
