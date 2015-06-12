using System;

using BeanIO.Internal.Config;

namespace BeanIO.Internal.Compiler.Flat
{
    internal abstract class FlatParserFactory : ParserFactorySupport
    {
        /// <summary>
        /// Creates a stream configuration pre-processor
        /// </summary>
        /// <remarks>May be overridden to return a format specific version</remarks>
        /// <param name="config">the stream configuration to pre-process</param>
        /// <returns>the new <see cref="Preprocessor"/></returns>
        protected override Preprocessor CreatePreprocessor(StreamConfig config)
        {
            return new FlatPreprocessor(config);
        }

        /// <summary>
        /// Finalizes a record configuration after its children have been processed
        /// </summary>
        /// <param name="config">the record configuration to finalize</param>
        protected override void FinalizeRecord(RecordConfig config)
        {
            base.FinalizeRecord(config);

            // sort nodes according to their position in the record
            ////record.sort(new NodeComparator());
        }

        /*
        private class NodeComparer : IComparer<Component>
        {
            private Dictionary<Component, int> _cache = new Dictionary<Component, int>();

            public int Compare(Component x, Component y)
            {
                return GetPosition(x).CompareTo(GetPosition(y));
            }

            private int GetPosition(Component component)
            {
            }
        }
         */
    }
}
