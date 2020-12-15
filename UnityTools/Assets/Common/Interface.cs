using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Interface
{
    public interface IDataNotifier<T>
    {
        IEnumerable<IDataUser<T>> Users { get; }

    }

    public interface IDataUser<T>
    {
        T Data { get; set;}
        void OnDataChanged(T newData);
    }
}