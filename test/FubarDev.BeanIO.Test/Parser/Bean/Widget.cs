// <copyright file="Widget.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

using BeanIO.Internal.Util;

namespace BeanIO.Parser.Bean
{
    public class Widget
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Model { get; set; }

        public Widget Top { get; set; }

        public Widget Bottom { get; set; }

        public IList<Widget> PartsList { get; set; }

        public IDictionary<string, Widget> PartsMap { get; set; }

        public IList<Widget> GetPartsList()
        {
            return PartsList;
        }

        public void SetPartsList(IList<Widget> partsList)
        {
            PartsList = partsList;
        }

        public Widget GetPart(int index)
        {
            return PartsList[index];
        }

        public Widget GetPart(string key)
        {
            return PartsMap[key];
        }

        public void AddPart(Widget w)
        {
            if (PartsList == null)
                PartsList = new List<Widget>();
            PartsList.Add(w);
        }

        public override string ToString()
        {
            return string.Format(
                "[id={0}, name={1}, model={2}, partsList={3}]",
                Id,
                Name,
                Model,
                PartsList.ToDebug());
        }
    }
}
