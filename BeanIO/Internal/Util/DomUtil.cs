using System.Xml.Linq;

using JetBrains.Annotations;

namespace BeanIO.Internal.Util
{
    public static class DomUtil
    {
        public static void SetAnnotation<T>([NotNull] this XObject obj, T annotation)
            where T : class
        {
            obj.RemoveAnnotations<T>();
            obj.AddAnnotation(annotation);
        }
    }
}
