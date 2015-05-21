using System;

using BeanIO.Internal.Config;
using BeanIO.Internal.Config.Annotation;

namespace BeanIO.Builder
{
    /// <summary>
    /// Support for segment configuration builders.
    /// </summary>
    /// <typeparam name="T">The class derived from <see cref="GroupBuilderSupport{T,TConfig}"/>.</typeparam>
    /// <typeparam name="TConfig">The configuration class derived from <see cref="GroupConfig"/>.</typeparam>
    public abstract class GroupBuilderSupport<T, TConfig> : PropertyBuilderSupport<T, TConfig>
        where T : GroupBuilderSupport<T, TConfig>
        where TConfig : GroupConfig
    {
        /// <summary>
        /// Adds a group to this component.
        /// </summary>
        /// <param name="group">the group to add</param>
        /// <returns>The value of <see cref="PropertyBuilderSupport{T,TConfig}.Me"/></returns>
        public T AddGroup(GroupBuilder group)
        {
            Config.Add(group.Build());
            return Me;
        }

        /// <summary>
        /// Adds a group to this component by using the group annotation for the given type.
        /// </summary>
        /// <param name="group">The type that has group annotations</param>
        /// <returns>The value of <see cref="PropertyBuilderSupport{T,TConfig}.Me"/></returns>
        public T AddGroup(Type group)
        {
            var gc = AnnotationParser.CreateGroupConfig(group);
            if (gc == null)
                throw new BeanIOConfigurationException(string.Format("Group annotation not detected on class '{0}'", group));
            Config.Add(gc);
            return Me;
        }
    }
}
