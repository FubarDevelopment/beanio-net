namespace BeanIO.Internal.Parser
{
    public static class Value
    {
        /// <summary>
        /// Constant indicating the field was not present in the stream
        /// </summary>
        public const string Missing = "-missing-";

        /// <summary>
        /// Constant indicating the field did not pass validation
        /// </summary>
        public const string Invalid = "-invalid-";

        /// <summary>
        /// Constant indicating the field was nil (XML only)
        /// </summary>
        public const string Nil = "-nil-";
    }
}
