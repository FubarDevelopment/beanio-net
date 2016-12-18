// <copyright file="SegmentBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

using BeanIO.Config;
using BeanIO.Internal.Config;

namespace BeanIO.Builder
{
    public static class SegmentBuilderExtensions
    {
        public static InlineMappingLoader<T, TConfig> WithLoader<T, TConfig>(
            this SegmentBuilderSupport<T, TConfig> builder,
            ISettings settings,
            ISchemeProvider schemeProvider)
            where T : SegmentBuilderSupport<T, TConfig>
            where TConfig : SegmentConfig
        {
            return new InlineMappingLoader<T, TConfig>(builder, settings, schemeProvider);
        }
    }
}
