// <copyright file="DefaultSchemeProvider.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using BeanIO.Config.SchemeHandlers;

namespace BeanIO.Config
{
    /// <summary>
    /// The default <see cref="ISchemeProvider"/> implementation
    /// </summary>
    internal class DefaultSchemeProvider : ISchemeProvider
    {
        private readonly Dictionary<string, ISchemeHandler> _schemeHandlers = new Dictionary<string, ISchemeHandler>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSchemeProvider"/> class.
        /// </summary>
        public DefaultSchemeProvider()
        {
            Add(new ResourceSchemeHandler());
        }

        /// <inheritdoc />
        public IEnumerable<string> SupportedSchemes => _schemeHandlers.Keys;

        /// <summary>
        /// Adds a new <see cref="ISchemeHandler"/>
        /// </summary>
        /// <param name="handler">the handler to add</param>
        public void Add(ISchemeHandler handler)
        {
            _schemeHandlers[handler.Scheme] = handler;
        }

        /// <inheritdoc />
        public ISchemeHandler GetSchemeHandler(string scheme, bool throwIfMissing)
        {
            ISchemeHandler handler;
            if (!_schemeHandlers.TryGetValue(scheme, out handler))
            {
                if (!throwIfMissing)
                    return null;
                throw new BeanIOConfigurationException(
                    $"Scheme '{scheme}' must one of: {string.Join(", ", SupportedSchemes.Select(x => $"'{x}'"))}");
            }

            return handler;
        }
    }
}
