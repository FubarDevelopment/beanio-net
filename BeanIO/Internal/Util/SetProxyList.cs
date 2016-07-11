// <copyright file="SetProxyList.cs" company="Fubar Development Junker">
// Copyright (c) 2016 Fubar Development Junker. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace BeanIO.Internal.Util
{
    internal class SetProxyList : IList
    {
        private readonly IEnumerable _instance;

        private readonly MethodInfo _addMethod;

        private readonly PropertyInfo _countProperty;

        public SetProxyList(IEnumerable instance)
        {
            _instance = instance;
            Type collectionType = instance.GetType();
            TypeInfo collectionTypeInfo = collectionType.GetTypeInfo();
            _addMethod = collectionTypeInfo.DeclaredMethods.FirstOrDefault(x => x.IsPublic && !x.IsStatic && x.Name == "Add" && x.GetParameters().Length == 1);
            _countProperty = collectionTypeInfo.DeclaredProperties.FirstOrDefault(x => x.CanRead && x.GetMethod.IsPublic && !x.GetMethod.IsStatic && x.Name == "Count");
        }

        public IEnumerable Instance => _instance;

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public int Count => (int)_countProperty.GetMethod.Invoke(_instance, null);

        public bool IsSynchronized
        {
            get { throw new NotSupportedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        public object this[int index]
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public int Add(object value)
        {
            _addMethod.Invoke(_instance, new[] { value });
            return 0;
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
        {
            throw new NotSupportedException();
        }

        public int IndexOf(object value)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public IEnumerator GetEnumerator()
        {
            return _instance.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }
    }
}
