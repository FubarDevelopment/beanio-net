// <copyright file="Job.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections;

namespace BeanIO.Parser.InlineMaps
{
    public class Job
    {
        public string Id { get; set; }

        public IDictionary Codes { get; set; }
    }
}
