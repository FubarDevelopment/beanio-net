using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BeanIO.Internal.Util
{
    /// <summary>
    /// The node for a tree structure.
    /// </summary>
    /// <typeparam name="T">The tree node type</typeparam>
    public class TreeNode<T> : IEnumerable<T> where T : TreeNode<T>
    {
        private readonly List<T> _children;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode{T}"/> class.
        /// </summary>
        public TreeNode()
            : this(-1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode{T}"/> class.
        /// </summary>
        /// <param name="size">The initial size of the node for accommodating children</param>
        public TreeNode(int size)
        {
            if (size <= 0 || size > 10)
                _children = new List<T>();
            else
                _children = new List<T>(size);
        }

        /// <summary>
        /// Gets or sets the name of this node
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the immediate children of this node
        /// </summary>
        public IReadOnlyCollection<T> Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Gets the first child of this node
        /// </summary>
        public T First
        {
            get
            {
                return _children[0];
            }
        }

        /// <summary>
        /// Adds a child to this node
        /// </summary>
        /// <param name="child">The child to add</param>
        public void Add(T child)
        {
            if (!IsSupportedChild(child))
                throw new ArgumentException(string.Format("Child type not supported: {0}", child.GetType()));
            _children.Add(child);
        }

        /// <summary>
        /// Recursively finds the first descendant with the given name
        /// </summary>
        /// <remarks>
        /// All descendants of a child are checked first before moving to the next child.
        /// </remarks>
        /// <param name="name">The name of the node to find</param>
        /// <returns>The matched node, or null if not found</returns>
        public T Find(string name)
        {
            if (string.Equals(Name, name, StringComparison.Ordinal))
                return (T)this;

            foreach (var child in _children)
            {
                var match = child.Find(name);
                if (match != null)
                    return match;
            }

            return null;
        }

        /// <summary>
        /// Returns whether the given node is a descendant of this node or recursively one of its children
        /// </summary>
        /// <param name="node">The TreeNode to test</param>
        /// <returns>true if the given node is a descendant, false otherwise</returns>
        public bool IsDescendant(T node)
        {
            if (ReferenceEquals(node, this))
                return true;

            foreach (var child in _children)
            {
                if (child.IsDescendant(node))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether a node is a supported child of this node.
        /// </summary>
        /// <remarks>
        /// Called by <see cref="Add"/>.
        /// </remarks>
        /// <param name="child">the node to test</param>
        /// <returns>true if the child is allowed</returns>
        public virtual bool IsSupportedChild(T child)
        {
            return true;
        }

        /// <summary>
        /// Sorts all descendants of this node
        /// </summary>
        /// <param name="comparer">The comparer to use for comparing nodes</param>
        public void Sort(IComparer<T> comparer)
        {
            _children.Sort(comparer);
            foreach (var child in _children)
                child.Sort(comparer);
        }

        /// <summary>
        /// Returns an enumerator
        /// </summary>
        /// <returns>The enumerator for the children</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        /// <summary>
        /// Returns a string that represents this object.
        /// </summary>
        /// <returns>
        /// A string that represents this object.
        /// </returns>
        public override string ToString()
        {
            var s = new StringBuilder()
                .Append(GetType().Name)
                .Append("[")
                .AppendFormat("name={0}", Name);
            ToParamString(s);
            s.Append("]");
            return s.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Called by <see cref="ToString()"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected virtual void ToParamString(StringBuilder s)
        {
        }
    }
}
