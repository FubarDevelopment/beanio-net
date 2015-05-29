using System;

namespace BeanIO.Internal.Parser
{
    /// <summary>
    /// Used to create a reference to a variable held by a <see cref="ParsingContext"/>.
    /// </summary>
    /// <typeparam name="T">the variable type</typeparam>
    public class ParserLocal<T> : IParserLocal
    {
        private readonly Func<T> _createFunc;

        private int _index = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserLocal{T}"/> class.
        /// </summary>
        public ParserLocal()
            : this(default(T))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserLocal{T}"/> class.
        /// </summary>
        /// <param name="defaultValue">the default value</param>
        public ParserLocal(T defaultValue)
            : this(() => defaultValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserLocal{T}"/> class.
        /// </summary>
        /// <param name="createFunc">the function to create a default value</param>
        public ParserLocal(Func<T> createFunc)
        {
            _createFunc = createFunc;
        }

        /// <summary>
        /// Initializes the variable.
        /// </summary>
        /// <param name="index">the index of the variable in the <see cref="ParsingContext.LocalHeap"/></param>
        /// <param name="context">the <see cref="ParsingContext"/> being initialized</param>
        public void Init(int index, ParsingContext context)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            _index = index;
            Set(context, CreateDefaultValue());
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/> to get the value from</param>
        /// <returns>the value</returns>
        public T Get(ParsingContext context)
        {
            return (T)context.LocalHeap[_index];
        }

        /// <summary>
        /// Sets the value
        /// </summary>
        /// <param name="context">the <see cref="ParsingContext"/> to set the value on</param>
        /// <param name="value">the new value</param>
        public void Set(ParsingContext context, T value)
        {
            context.LocalHeap[_index] = value;
        }

        /// <summary>
        /// Called when initialized to return a default value.  If not overridden,
        /// it returns the default value passed via the constructor.
        /// </summary>
        /// <returns>the default value</returns>
        protected virtual T CreateDefaultValue()
        {
            return _createFunc();
        }
    }
}
