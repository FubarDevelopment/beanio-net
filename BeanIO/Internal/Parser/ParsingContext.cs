﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// Base class for the parsing context - marshalling or unmarshalling.
    /// </summary>
    public abstract class ParsingContext
    {
        private readonly Stack<IIteration> _iterations = new Stack<IIteration>();

        private int _fieldOffset;

        /// <summary>
        /// Gets the parsing mode.
        /// </summary>
        public abstract ParsingMode Mode { get; }

        /// <summary>
        /// Gets the local heap
        /// </summary>
        public object[] LocalHeap { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a repeating segment or field is being parsed.
        /// </summary>
        public bool IsRepeating
        {
            get { return _iterations.Count != 0; }
        }

        /// <summary>
        /// Pushes an <see cref="IIteration"/> onto a stack for adjusting
        /// field positions and indices.
        /// </summary>
        /// <param name="iteration">the <see cref="IIteration"/> to push</param>
        public void PushIteration(IIteration iteration)
        {
            _iterations.Push(iteration);
        }

        /// <summary>
        /// Pops the last <see cref="IIteration"/> pushed onto the stack.
        /// </summary>
        /// <returns>the top most <see cref="IIteration"/></returns>
        public IIteration PopIteration()
        {
            var iter = _iterations.Pop();
            if (iter.IsDynamicIteration)
                _fieldOffset += iter.IterationSize * iter.GetIterationIndex(this);
            return iter;
        }

        /// <summary>
        /// Calculates a field position by adjusting for any applied iterations.
        /// </summary>
        /// <param name="position">the field position to adjust (i.e. the position of the first occurrence of the field)</param>
        /// <returns>the adjusted field position</returns>
        public int GetAdjustedFieldPosition(int position)
        {
            return _fieldOffset + position + _iterations.Sum(x => x.IterationSize * x.GetIterationIndex(this));
        }

        /// <summary>
        /// Returns the current field index relative to any current iteration.
        /// </summary>
        /// <returns>the field index</returns>
        public int GetRelativeFieldIndex()
        {
            if (_iterations.Count == 0)
                return 0;
            return _iterations.Peek().GetIterationIndex(this);
        }

        /// <summary>
        /// Initializes the local heap with the given size.
        /// </summary>
        /// <param name="size">The size of the local heap</param>
        public void CreateHeap(int size)
        {
            LocalHeap = new object[size];
        }

        /// <summary>
        /// Clear is invoked after each bean object (record or group) is marshalled
        /// </summary>
        protected virtual void ClearOffset()
        {
            _fieldOffset = 0;
        }
    }
}