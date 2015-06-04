using System.Collections.Generic;
using System.Linq;
using System.Text;

using BeanIO.Internal.Config;

namespace BeanIO.Internal.Compiler
{
    /// <summary>
    /// A base class for configuration processors
    /// </summary>
    /// <remarks>
    /// The class provides support for traversing a tree of stream
    /// configuration components and generates a "stack trace" if
    /// any overridden method throws a <see cref="BeanIOConfigurationException"/>.
    /// </remarks>
    public abstract class ProcessorSupport
    {
        private readonly Stack<ComponentConfig> _componentConfigurations = new Stack<ComponentConfig>();

        /// <summary>
        /// Gets the parent component for the component being processed
        /// </summary>
        protected virtual ComponentConfig Parent
        {
            get
            {
                if (_componentConfigurations.Count > 1)
                    return _componentConfigurations.Skip(1).First();
                return null;
            }
        }

        /// <summary>
        /// Processes a stream configuration
        /// </summary>
        /// <param name="stream">the <see cref="StreamConfig"/> to process</param>
        public virtual void Process(StreamConfig stream)
        {
            try
            {
                HandleComponent(stream);
            }
            catch (BeanIOConfigurationException ex)
            {
                var message = new StringBuilder();

                message.Append("Invalid ");
                var index = 0;
                foreach (var node in _componentConfigurations)
                {
                    string type;
                    switch (node.ComponentType)
                    {
                        case ComponentType.Constant:
                            type = "property";
                            break;
                        default:
                            type = node.ComponentType.ToString().ToLowerInvariant();
                            break;
                    }
                    ++index;
                    if (index > 1)
                        message.Append(", in ");

                    message.AppendFormat("{0} '{1}'", type, node.Name);
                }

                message.AppendFormat(": {0}", ex.Message);

                throw new BeanIOConfigurationException(message.ToString(), ex);
            }
        }

        /// <summary>
        /// Recursively preprocesses a component and its descendants
        /// </summary>
        /// <param name="component">the component to preprocess</param>
        protected virtual void HandleComponent(ComponentConfig component)
        {
            _componentConfigurations.Push(component);

            switch (component.ComponentType)
            {
                case ComponentType.Stream:
                    InitializeStream((StreamConfig)component);
                    foreach (var child in component)
                        HandleComponent(child);
                    FinalizeStream((StreamConfig)component);
                    break;
                case ComponentType.Group:
                    InitializeGroup((GroupConfig)component);
                    foreach (ComponentConfig child in component)
                        HandleComponent(child);
                    FinalizeGroup((GroupConfig)component);
                    break;
                case ComponentType.Record:
                    InitializeRecord((RecordConfig)component);
                    foreach (ComponentConfig child in component)
                        HandleComponent(child);
                    FinalizeRecord((RecordConfig)component);
                    break;
                case ComponentType.Segment:
                    InitializeSegment((SegmentConfig)component);
                    foreach (ComponentConfig child in component)
                        HandleComponent(child);
                    FinalizeSegment((SegmentConfig)component);
                    break;
                case ComponentType.Field:
                    HandleField((FieldConfig)component);
                    break;
                case ComponentType.Constant:
                    HandleConstant((ConstantConfig)component);
                    break;
            }

            _componentConfigurations.Pop();
        }

        /// <summary>
        /// Initializes a stream configuration before its children have been processed
        /// </summary>
        /// <param name="stream">the stream configuration to process</param>
        protected virtual void InitializeStream(StreamConfig stream)
        {
        }

        /// <summary>
        /// Finalizes a stream configuration after its children have been processed
        /// </summary>
        /// <param name="stream">the stream configuration to finalize</param>
        protected virtual void FinalizeStream(StreamConfig stream)
        {
        }

        /// <summary>
        /// Initializes a group configuration before its children have been processed
        /// </summary>
        /// <param name="group">the group configuration to process</param>
        protected virtual void InitializeGroup(GroupConfig group)
        {
        }

        /// <summary>
        /// Finalizes a group configuration after its children have been processed
        /// </summary>
        /// <param name="group">the group configuration to finalize</param>
        protected virtual void FinalizeGroup(GroupConfig group)
        {
        }

        /// <summary>
        /// Initializes a record configuration before its children have been processed
        /// </summary>
        /// <param name="record">the record configuration to process</param>
        protected virtual void InitializeRecord(RecordConfig record)
        {
        }

        /// <summary>
        /// Finalizes a record configuration after its children have been processed
        /// </summary>
        /// <param name="record">the record configuration to finalize</param>
        protected virtual void FinalizeRecord(RecordConfig record)
        {
        }

        /// <summary>
        /// Initializes a segment configuration before its children have been processed
        /// </summary>
        /// <param name="segment">the segment configuration to process</param>
        protected virtual void InitializeSegment(SegmentConfig segment)
        {
        }

        /// <summary>
        /// Finalizes a segment configuration after its children have been processed
        /// </summary>
        /// <param name="segment">the segment configuration to finalize</param>
        protected virtual void FinalizeSegment(SegmentConfig segment)
        {
        }

        /// <summary>
        /// Processes a field configuration
        /// </summary>
        /// <param name="field">the field configuration to process</param>
        protected virtual void HandleField(FieldConfig field)
        {
        }

        /// <summary>
        /// Processes a constant configuration
        /// </summary>
        /// <param name="constant">the constant configuration to process</param>
        protected virtual void HandleConstant(ConstantConfig constant)
        {
        }
    }
}
