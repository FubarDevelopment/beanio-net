using BeanIO.Internal.Config;

namespace BeanIO.Builder
{
    /// <summary>
    /// Support for segment configuration builders.
    /// </summary>
    /// <typeparam name="T">The class derived from <see cref="SegmentBuilderSupport{T,TConfig}"/>.</typeparam>
    /// <typeparam name="TConfig">The configuration class derived from <see cref="SegmentConfig"/>.</typeparam>
    public abstract class SegmentBuilderSupport<T, TConfig> : PropertyBuilderSupport<T, TConfig>
        where T : SegmentBuilderSupport<T, TConfig>
        where TConfig : SegmentConfig
    {
        /// <summary>
        /// Sets the name of a child component to use as the key for an
        /// inline map bound to this record or segment.
        /// </summary>
        /// <param name="name">the component name</param>
        /// <returns>The value of <see cref="SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T Key(string name)
        {
            Config.SetKey(name);
            return Me;
        }

        /// <summary>
        /// Sets the name of a child component to return as the value for this
        /// record or segment in lieu of a bound class.
        /// </summary>
        /// <param name="name">the component name</param>
        /// <returns>The value of <see cref="SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T Value(string name)
        {
            Config.Target = name;
            return Me;
        }

        /// <summary>
        /// Adds a segment to this component.
        /// </summary>
        /// <param name="segment">the segment to add</param>
        /// <returns>The value of <see cref="SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T AddSegment(SegmentBuilder segment)
        {
            Config.Add(segment.Build());
            return Me;
        }

        /// <summary>
        /// Adds a field to this component.
        /// </summary>
        /// <param name="field">the field to add</param>
        /// <returns>The value of <see cref="SegmentBuilderSupport{T,TConfig}.Me"/></returns>
        public T AddField(FieldBuilder field)
        {
            Config.Add(field.Build());
            return Me;
        }
    }
}
