using System;
using System.Xml;

using BeanIO.Internal.Config;
using BeanIO.Internal.Util;

namespace BeanIO.Internal.Compiler.Xml
{
    /// <summary>
    /// Configuration <see cref="Preprocessor"/> for an XML stream format.
    /// </summary>
    internal class XmlPreprocessor : Preprocessor
    {
        public XmlPreprocessor(StreamConfig stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Initializes a group configuration before its children have been processed
        /// </summary>
        /// <param name="config">the group configuration to process</param>
        protected override void InitializeGroup(GroupConfig config)
        {
            base.InitializeGroup(config);

            if (config.XmlName == null)
            {
                config.XmlName = config.Name;
            }

            var type = config.XmlType;
            if (type == null)
            {
                config.XmlType = XmlNodeType.Element;
            }
            else
            {
                if (type != XmlNodeType.Element && type != XmlNodeType.None)
                    throw new BeanIOConfigurationException(string.Format("Invalid xmlType '{0}'", type));
            }

            if (config.XmlNamespace == null)
            {
                var parent = Parent;
                if (parent != null)
                {
                    config.XmlPrefix = parent.XmlPrefix;
                    config.XmlNamespace = parent.XmlNamespace;
                    config.IsXmlNamespaceAware = parent.IsXmlNamespaceAware;
                }
                else
                {
                    config.XmlPrefix = null;
                    config.XmlNamespace = null;
                    config.IsXmlNamespaceAware = false;
                }
            }
            else if (config.XmlNamespace == "*")
            {
                config.XmlPrefix = null;
                config.XmlNamespace = null;
                config.IsXmlNamespaceAware = false;
            }
            else if (config.XmlNamespace == string.Empty)
            {
                config.XmlPrefix = null;
                config.XmlNamespace = null;
                config.IsXmlNamespaceAware = true;
            }
            else
            {
                config.IsXmlNamespaceAware = true;
            }
        }

        /// <summary>
        /// Initializes a segment configuration before its children have been processed
        /// </summary>
        /// <param name="config">the segment configuration to process</param>
        protected override void InitializeSegment(SegmentConfig config)
        {
            base.InitializeSegment(config);

            if (config.XmlName == null)
            {
                config.XmlName = config.Name;
            }

            var type = config.XmlType;
            if (type == null)
            {
                config.XmlType = XmlNodeType.Element;
            }
            else
            {
                if (type != XmlNodeType.Element && type != XmlNodeType.None)
                    throw new BeanIOConfigurationException(string.Format("Invalid xmlType '{0}'", type));
            }

            if (config.XmlPrefix != null)
            {
                if (config.XmlNamespace == null)
                    throw new BeanIOConfigurationException("Missing namespace for configured XML prefix");
            }

            if (config.XmlNamespace == null)
            {
                var parent = Parent;
                config.XmlPrefix = parent.XmlPrefix;
                config.XmlNamespace = parent.XmlNamespace;
                config.IsXmlNamespaceAware = parent.IsXmlNamespaceAware;
            }
            else if (config.XmlNamespace == "*")
            {
                config.XmlPrefix = null;
                config.XmlNamespace = null;
                config.IsXmlNamespaceAware = false;
            }
            else if (config.XmlNamespace == string.Empty)
            {
                config.XmlPrefix = null;
                config.XmlNamespace = null;
                config.IsXmlNamespaceAware = true;
            }
            else
            {
                config.IsXmlNamespaceAware = true;
            }
        }

        /// <summary>
        /// Processes a field configuration
        /// </summary>
        /// <param name="config">the field configuration to process</param>
        protected override void HandleField(FieldConfig config)
        {
            if (config.XmlName == null)
            {
                config.XmlName = config.Name;
            }

            var type = config.XmlType;
            if (type == null)
            {
                var xmlType = Settings.Instance.GetProperty(Settings.DEFAULT_XML_TYPE);
                XmlNodeType newXmlNodeType;
                if (!Enum.TryParse(xmlType, true, out newXmlNodeType))
                    newXmlNodeType = XmlNodeType.Element;
                type = config.XmlType = newXmlNodeType;
            }
            else
            {
                switch (type)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.Attribute:
                    case XmlNodeType.Text:
                        break;
                    default:
                        throw new BeanIOConfigurationException(string.Format("Invalid xmlType '{0}'", type));
                }
            }

            // repeating fields must be of type 'element'
            if (config.IsRepeating && type != XmlNodeType.Element)
            {
                throw new BeanIOConfigurationException("Repeating fields must have xmlType 'element'");
            }

            if (config.XmlNamespace != null
                && type != XmlNodeType.Element
                && type != XmlNodeType.Attribute)
            {
                throw new BeanIOConfigurationException(string.Format("XML namespace is not applicable for xmlType '{0}'", type));
            }

            // if the bean/field/record is nillable, it must be of type 'element'
            if (config.IsNillable && type != XmlNodeType.Element)
            {
                throw new BeanIOConfigurationException(string.Format("xmlType '{0}' is not nillable", type));
            }

            // validate a namespace is set if a prefix is set
            if (config.XmlPrefix != null)
            {
                if (config.XmlNamespace == null)
                    throw new BeanIOConfigurationException("Missing namespace for configured XML prefix");
            }

            var isAttribute = type == XmlNodeType.Attribute;

            if (config.XmlNamespace == null)
            {
                if (isAttribute)
                {
                    config.XmlPrefix = null;
                    config.XmlNamespace = null;
                    config.IsXmlNamespaceAware = false;
                }
                else
                {
                    var parent = Parent;
                    config.XmlPrefix = parent.XmlPrefix;
                    config.XmlNamespace = parent.XmlNamespace;
                    config.IsXmlNamespaceAware = parent.IsXmlNamespaceAware;
                }
            }
            else if (config.XmlNamespace == "*")
            {
                config.XmlPrefix = null;
                config.XmlNamespace = null;
                config.IsXmlNamespaceAware = false;
            }
            else if (config.XmlNamespace == string.Empty)
            {
                config.XmlPrefix = null;
                config.XmlNamespace = null;
                config.IsXmlNamespaceAware = true;
            }
            else
            {
                config.IsXmlNamespaceAware = true;
            }

            // default minOccurs for an attribute is 0
            if (isAttribute)
            {
                if (config.MinOccurs == null)
                {
                    config.MinOccurs = 0;
                }
            }

            base.HandleField(config);
        }

        /// <summary>
        /// This method validates a record identifying field has a literal or regular expression
        /// configured for identifying a record.
        /// </summary>
        /// <param name="field">the record identifying field configuration to validate</param>
        protected override void ValidateRecordIdentifyingCriteria(FieldConfig field)
        {
            if (field.XmlType == XmlNodeType.Text)
                base.ValidateRecordIdentifyingCriteria(field);
        }
    }
}
