using System.Text;

namespace BeanIO.Internal.Parser.Format.Json
{
    internal static class JsonNodeUtil
    {
        /// <summary>
        /// Returns a description for given JSON node type.
        /// </summary>
        /// <param name="node">the JSON node to return the type description for</param>
        /// <returns>the description</returns>
        public static string GetTypeDescription(this IJsonNode node)
        {
            return string.Format("{0}{1}", node.JsonType, node.IsJsonArray ? "[]" : string.Empty);
        }
    }
}
