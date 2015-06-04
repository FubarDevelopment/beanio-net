using System;
using System.Reflection;

namespace BeanIO.Config.SchemeHandlers
{
    public class ResourceSchemeHandler : ISchemeHandler
    {
        /// <summary>
        /// Gets the schema this handler supports (e.g. file)
        /// </summary>
        public string Scheme
        {
            get { return "resource"; }
        }

        /// <summary>
        /// This functions opens a stream for the given <paramref name="resource"/> <see cref="Uri"/>
        /// </summary>
        /// <param name="resource">The resource to load the mapping from</param>
        /// <returns>the stream to read the mapping from</returns>
        public System.IO.Stream Open(Uri resource)
        {
            if (resource.Scheme != Scheme)
                throw new ArgumentOutOfRangeException(string.Format("Only '{0}' URLs are allowed", Scheme));

            var resName = resource.LocalPath;
            var commaIndex = resName.IndexOf(',');
            if (commaIndex == -1)
                throw new BeanIOConfigurationException(string.Format("No assembly specified for resource name {0}", resName));

            var asmName = resName.Substring(commaIndex + 1).Trim();
            resName = resName.Substring(0, commaIndex).TrimEnd();
            var resAssembly = Assembly.Load(new AssemblyName(asmName));
            return resAssembly.GetManifestResourceStream(resName);
        }
    }
}
