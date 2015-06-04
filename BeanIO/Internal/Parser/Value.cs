namespace BeanIO.Internal.Parser
{
    public static class Value
    {
        /// <summary>
        /// Constant indicating the field was not present in the stream
        /// </summary>
        public static readonly string Missing = new string("-missing-".ToCharArray());

        /// <summary>
        /// Constant indicating the field did not pass validation
        /// </summary>
        public static readonly string Invalid = new string("-invalid-".ToCharArray());

        /// <summary>
        /// Constant indicating the field was nil (XML only)
        /// </summary>
        public static readonly string Nil = new string("-nil-".ToCharArray());
    }
}
