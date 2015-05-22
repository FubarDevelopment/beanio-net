namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// A <see cref="IFieldFormat"/> provides format specific processing for a <see cref="Field"/> parser.
    /// </summary>
    public interface IFieldFormat
    {
        int Size { get; }

        bool IsNillable { get; }

        bool IsLazy { get; }

        string Extract(UnmarshallingContext context, bool reportErrors);

        bool InsertValue(MarshallingContext context, object value);

        void InsertField(MarshallingContext context, string text);
    }
}
