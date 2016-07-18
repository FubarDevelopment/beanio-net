// <copyright file="JsonWrapper.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text;

using BeanIO.Internal.Util;

using Newtonsoft.Json.Linq;

namespace BeanIO.Internal.Parser.Format.Json
{
    /// <summary>
    /// A <see cref="JsonWrapper"/> is used to handle nested JSON objects.
    /// </summary>
    internal class JsonWrapper : DelegatingParser, IJsonNode
    {
        private bool _isOptional;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonWrapper"/> class.
        /// </summary>
        public JsonWrapper()
        {
            JsonArrayIndex = -1;
        }

        /// <summary>
        /// Gets or sets the JSON field name.
        /// </summary>
        public string JsonName { get; set; }

        /// <summary>
        /// Gets or sets the type of node.
        /// </summary>
        /// <remarks>
        /// If <see cref="IJsonNode.IsJsonArray"/> is true, this method returns the component type of the array.
        /// </remarks>
        public JTokenType JsonType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node is a JSON array.
        /// </summary>
        public bool IsJsonArray { get; set; }

        /// <summary>
        /// Gets or sets the index of this node in its parent array, or -1 if not applicable
        /// (i.e. its parent is an object).
        /// </summary>
        public int JsonArrayIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether this node may be explicitly set to <code>null</code>.
        /// </summary>
        public bool IsNillable { get; set; }

        /// <summary>
        /// Gets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        public override bool IsOptional => _isOptional;

        /// <summary>
        /// Sets a value indicating whether this node must exist during unmarshalling.
        /// </summary>
        /// <param name="optional">true when the value doesn't need to exist during unmarshalling</param>
        public void SetOptional(bool optional)
        {
            _isOptional = optional;
        }

        /// <summary>
        /// Returns whether this parser and its children match a record being unmarshalled.
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if matched, false otherwise</returns>
        public override bool Matches(UnmarshallingContext context)
        {
            if (!IsIdentifier)
                return true;

            var ctx = (JsonUnmarshallingContext)context;
            if (ctx.Push(this, false) == null)
                return false;

            try
            {
                return base.Matches(context);
            }
            finally
            {
                ctx.Pop();
            }
        }

        /// <summary>
        /// Unmarshals a record
        /// </summary>
        /// <param name="context">The <see cref="UnmarshallingContext"/></param>
        /// <returns>true if this component was present in the unmarshalled record, or false otherwise</returns>
        public override bool Unmarshal(UnmarshallingContext context)
        {
            var ctx = (JsonUnmarshallingContext)context;
            if (ctx.Push(this, true) == null)
                return false;

            try
            {
                base.Unmarshal(context);
                return true;
            }
            finally
            {
                ctx.Pop();
            }
        }

        /// <summary>
        /// Marshals a record
        /// </summary>
        /// <param name="context">The <see cref="MarshallingContext"/></param>
        /// <returns>whether a value was marshalled</returns>
        public override bool Marshal(MarshallingContext context)
        {
            var contentChecked = false;

            if (IsOptional)
            {
                if (!HasContent(context))
                    return false;
                contentChecked = true;
            }

            var ctx = (JsonMarshallingContext)context;

            // if nillable and there is no descendant with content, mark the element nil
            if (IsNillable && !contentChecked && !HasContent(context))
            {
                ctx.Put(this, null);
            }
            else
            {
                ctx.Push(this);
                try
                {
                    base.Marshal(context);
                }
                finally
                {
                    ctx.Pop();
                }
            }

            return true;
        }

        /// <summary>
        /// Called by <see cref="TreeNode{T}.ToString"/> to append node parameters to the output
        /// </summary>
        /// <param name="s">The output to append</param>
        protected override void ToParamString(StringBuilder s)
        {
            base.ToParamString(s);

            s.AppendFormat(", jsonName={0}", JsonName)
             .AppendFormat(", jsonType={0}", JsonType);
            if (IsJsonArray)
                s.Append("[]");
            if (JsonArrayIndex >= 0)
                s.AppendFormat(", jsonArrayIndex={0}", JsonArrayIndex);
            s.AppendFormat(", {0}", DebugUtil.FormatOption("optional", IsOptional))
             .AppendFormat(", {0}", DebugUtil.FormatOption("nillable", IsNillable));
        }
    }
}
