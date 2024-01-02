// <copyright file="AnnotatedRoom.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    [Record]
    public class AnnotatedRoom
    {
        [Segment(At = 0, Type = typeof(AnnotatedLight), Getter = "GetLightFixture", Setter = "SetLightFixture")]
        private object? _light;

        private AnnotatedFloor? _floor;

        [Field(At = 5)]
        public string? Name { get; set; }

        [Segment(At = 6)]
        public AnnotatedFloor? GetFlooring()
        {
            return _floor;
        }

        public void SetFlooring(AnnotatedFloor? floor)
        {
            _floor = floor;
        }

        public AnnotatedLight? GetLightFixture()
        {
            return (AnnotatedLight?)_light;
        }

        public void SetLightFixture(AnnotatedLight? light)
        {
            _light = light;
        }
    }
}
