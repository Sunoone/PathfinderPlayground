using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Persistance
{
    // Dictionary that can be serialized.
    [System.Serializable]
    public class SerializableDictionary<T, U> : ISerializationCallbackReceiver
    {
        private Dictionary<T, U> _dictionary = new Dictionary<T, U>();
        private List<T> _keys = new List<T>();
        private List<U> _values = new List<U>();

        public int Count { get { return _dictionary.Count; } }

        public void Add(T key, U value)
        {
            _dictionary.Add(key, value);
        }

        public bool TryGetValue(T key, out U value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public bool Has(T key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Remove(T key)
        {
            _dictionary.Remove(key);
        }

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach (var pair in _dictionary)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            _dictionary = new Dictionary<T, U>();
            int numEntries = Math.Min(_keys.Count, _values.Count);
            for (int index = 0; index < numEntries; index++)
            {
                _dictionary.Add(_keys[index], _values[index]);
            }
        }
    }
}