﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
#pragma warning disable 169

namespace BeanIO.Beans
{
    /// <summary>
    /// A common bean object used by Groovy test cases.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "directly ported from the Java source code")]
    public class Bean
    {
        #region simple properties
        public string type;
        public string text;
        public string field1;
        private string field2;
        public string field3;
        #endregion

        #region collection properties
        public Hashtable map;
        public ArrayList list;
        #endregion

        #region bean properties
        public Bean group;
        public Bean record;
        public Bean segment;
        #endregion
    }
}