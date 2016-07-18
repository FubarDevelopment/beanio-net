// <copyright file="ResourceSchemeHandler.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;

namespace BeanIO.Config.SchemeHandlers
{
    /// <summary>
    /// Handles loading from a <code>resource:</code> URI.
    /// </summary>
    public class ResourceSchemeHandler : ISchemeHandler
    {
        /// <summary>
        /// Gets the schema this handler supports (e.g. file)
        /// </summary>
        public string Scheme => "resource";

        /// <summary>
        /// This functions opens a stream for the given <paramref name="resource"/> <see cref="Uri"/>
        /// </summary>
        /// <param name="resource">The resource to load the mapping from</param>
        /// <returns>the stream to read the mapping from</returns>
        public System.IO.Stream Open(Uri resource)
        {
            if (resource.Scheme != Scheme)
                throw new ArgumentOutOfRangeException($"Only '{Scheme}' URLs are allowed");

            var resName = resource.LocalPath;
            var commaIndex = resName.IndexOf(',');
            if (commaIndex == -1)
                throw new BeanIOConfigurationException($"No assembly specified for resource name {resName}");

            var asmName = resName.Substring(commaIndex + 1).Trim();
            resName = resName.Substring(0, commaIndex).TrimEnd();
            var resAssembly = Assembly.Load(new AssemblyName(asmName));
            return resAssembly.GetManifestResourceStream(resName);
        }
    }
}
