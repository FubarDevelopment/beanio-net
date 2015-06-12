using System.Collections.Generic;
using System.Xml.Linq;

using BeanIO.Internal.Parser.Format.Xml.Annotations;

namespace BeanIO.Internal.Util
{
    internal class NamespaceAwareElementComparer : IComparer<XElement>
    {
        static NamespaceAwareElementComparer()
        {
            Default = new NamespaceAwareElementComparer();
        }

        public static NamespaceAwareElementComparer Default { get; private set; }

        public static int Compare(NamespaceHandlingMode handlingMode1, XName x, NamespaceHandlingMode handlingMode2, XName y)
        {
            XName name1, name2;
            if (!IsNamespaceAware(handlingMode1) || !IsNamespaceAware(handlingMode2))
            {
                name1 = XNamespace.None + x.LocalName;
                name2 = XNamespace.None + y.LocalName;
            }
            else
            {
                name1 = handlingMode1 == NamespaceHandlingMode.DefaultNamespace ? XNamespace.None + x.LocalName : x;
                name2 = handlingMode2 == NamespaceHandlingMode.DefaultNamespace ? XNamespace.None + y.LocalName : y;
            }

            return string.CompareOrdinal(name1.ToString(), name2.ToString());
        }

        public static NamespaceHandlingMode GetHandlingModeFor(XElement element)
        {
            var nsHandlingModeAttr = element.Annotation<NamespaceModeAnnotation>();
            return nsHandlingModeAttr != null ? nsHandlingModeAttr.HandlingMode : NamespaceHandlingMode.UseNamespace;
        }

        public int Compare(XElement x, XElement y)
        {
            var hm1 = GetHandlingModeFor(x);
            var hm2 = GetHandlingModeFor(y);
            return Compare(hm1, x.Name, hm2, y.Name);
        }

        private static bool IsNamespaceAware(NamespaceHandlingMode handlingMode)
        {
            return handlingMode != NamespaceHandlingMode.IgnoreNamespace;
        }
    }
}
