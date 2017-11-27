// <copyright file="Bean.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections;
using System.Diagnostics.CodeAnalysis;

using NodaTime;

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
        public LocalDate? date;
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
