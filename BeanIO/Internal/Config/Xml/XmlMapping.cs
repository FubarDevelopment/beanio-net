// <copyright file="XmlMapping.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using BeanIO.Config;

namespace BeanIO.Internal.Config.Xml
{
    /// <summary>
    /// Stores parsing information about an XML mapping file.
    /// </summary>
    internal class XmlMapping
    {
        private const int TYPE_HANDLER_NAMESPACE = 0;

        private readonly List<XmlMapping> _imports = new List<XmlMapping>();

        private readonly Dictionary<string, XElement> _templates = new Dictionary<string, XElement>();

        private Dictionary<string, string> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlMapping"/> class.
        /// </summary>
        public XmlMapping()
        {
            Configuration = new BeanIOConfig();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlMapping"/> class.
        /// </summary>
        /// <param name="name">the mapping file name used for error messages</param>
        /// <param name="location">the location of the mapping (this should be
        /// the absolute URL location of the file so that the same
        /// mapping file will always have the same the location)</param>
        /// <param name="parent">the parent mapping</param>
        public XmlMapping(string name, Uri location, XmlMapping parent)
            : this()
        {
            Name = name;
            Location = location;
            Parent = parent;
        }

        /// <summary>
        /// Gets the name of this mapping file
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the location of this mapping file (in URL format).
        /// </summary>
        public Uri Location { get; }

        /// <summary>
        /// Gets the parent mapping file that imported this mapping file,
        /// or <code>null</code> if this file is the "root" mapping file.
        /// </summary>
        public XmlMapping Parent { get; }

        /// <summary>
        /// Gets the BeanIO configuration for this mapping file.
        /// </summary>
        public BeanIOConfig Configuration { get; }

        /// <summary>
        /// Gets the properties declared in this mapping file
        /// </summary>
        public Properties Properties { get; private set; }

        /// <summary>
        /// Returns whether a given mapping file is being actively loaded
        /// using its location to identify it.
        /// </summary>
        /// <remarks>This is used for detecting circular references.</remarks>
        /// <param name="url">the mapping file location to check</param>
        /// <returns><code>true</code> if the given location is being actively
        /// loaded, and thus the mapping file contains a circular reference</returns>
        public bool IsLoading(Uri url)
        {
            return url == Location
                   || (Parent != null && Parent.IsLoading(url));
        }

        /// <summary>
        /// Adds an imported mapping file to this mapping file.
        /// </summary>
        /// <param name="child">the imported mapping file</param>
        public void AddImport(XmlMapping child)
        {
            _imports.Add(child);
        }

        /// <summary>
        /// Sets a property declared in this mapping file.
        /// </summary>
        /// <param name="name">the property name</param>
        /// <param name="value">the property value</param>
        public void SetProperty(string name, string value)
        {
            if (_properties == null)
                Properties = new Properties(_properties = new Dictionary<string, string>());
            _properties[name] = value;
        }

        /// <summary>
        /// Recursively adds type handlers from all imported mapping files,
        /// and from this mapping file, to a given list.
        /// </summary>
        /// <param name="typeHandlers">the list to add all type handlers to</param>
        public void AddTypeHandlersTo(IList<TypeHandlerConfig> typeHandlers)
        {
            // add children first, so that type handlers declared in
            // a parent mapping file override its children
            foreach (var import in _imports)
                import.AddTypeHandlersTo(typeHandlers);
            foreach (var item in Configuration.TypeHandlerConfigurations)
                typeHandlers.Add(item);
        }

        /// <summary>
        /// Adds a template configuration to this mapping file.
        /// </summary>
        /// <param name="name">the name of the template</param>
        /// <param name="element">the 'template' DOM element</param>
        /// <returns><code>true</code> if the template was successfully added, or
        /// <code>false</code> if the template name already existed</returns>
        public bool AddTemplate(string name, XElement element)
        {
            if (FindTemplate(name) != null)
                return false;
            _templates[name] = element;
            return true;
        }

        /// <summary>
        /// Recursively finds the <code>template</code> DOM element for a given template
        /// name in this mapping file and its parents.
        /// </summary>
        /// <param name="name">the name of the template to retrieve</param>
        /// <returns>the matching template <see cref="XElement"/></returns>
        public XElement FindTemplate(string name)
        {
            XElement template;
            if (!_templates.TryGetValue(name, out template))
            {
                foreach (var import in _imports)
                {
                    template = import.FindTemplate(name);
                    if (template != null)
                        break;
                }
            }

            return template;
        }

        /// <summary>
        ///  whether a global type handler was configured for the
        /// given type handler name.  Recursively checks all imported
        /// mapping files.
        /// </summary>
        /// <param name="name">the type handler name</param>
        /// <returns><code>true</code> if a type handler was declared globally
        /// for the given name</returns>
        public bool IsDeclaredGlobalTypeHandler(string name)
        {
            return IsDeclared(TYPE_HANDLER_NAMESPACE, name);
        }

        public override string ToString()
        {
            return Name;
        }

        private bool IsDeclared(int type, string name)
        {
            switch (type)
            {
                case TYPE_HANDLER_NAMESPACE:
                    if (Configuration.TypeHandlerConfigurations.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
                        return true;
                    break;

                default:
                    throw new BeanIOConfigurationException("Invalid namespace");
            }

            if (_imports.Any(m => m.IsDeclared(type, name)))
                return true;

            return false;
        }
    }
}
