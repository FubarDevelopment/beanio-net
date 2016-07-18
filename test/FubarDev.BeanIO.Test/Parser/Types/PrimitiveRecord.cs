// <copyright file="PrimitiveRecord.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BeanIO.Parser.Types
{
    public class PrimitiveRecord
    {
        public byte ByteValue { get; set; }

        public sbyte SignedByteValue { get; set; }

        public short ShortValue { get; set; }

        public ushort UnsignedShortValue { get; set; }

        public int IntegerValue { get; set; }

        public uint UnsignedIntegerValue { get; set; }

        public long LongValue { get; set; }

        public ulong UnsignedLongValue { get; set; }

        public float FloatValue { get; set; }

        public double DoubleValue { get; set; }

        public char CharacterValue { get; set; }

        public bool BooleanValue { get; set; }

        public decimal DecimalValue { get; set; }
    }
}
