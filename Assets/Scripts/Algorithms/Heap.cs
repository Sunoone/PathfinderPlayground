using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IHeapItem<T> : IComparable<T> { int HeapIndex { get; set; } }

namespace Algorithms
{
    public class Heap<T> where T : IHeapItem<T>
    {
        private T[] _items;
        public int CurrentItemCount { get; private set; }

        public Heap(int maxHeapSize)
        {
            _items = new T[maxHeapSize];
        }

        public void Add(T item)
        {
            item.HeapIndex = CurrentItemCount;
            _items[CurrentItemCount] = item;
            SortUp(item);
            CurrentItemCount++;
        }

        public T RemoveFirst()
        {
            T firstItem = _items[0];
            CurrentItemCount--;
            _items[0] = _items[CurrentItemCount];
            _items[0].HeapIndex = 0;
            SortDown(_items[0]);
            return firstItem;
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public bool Contains(T item)
        {
            return Equals(_items[item.HeapIndex], item);
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int childIndexLeft = item.HeapIndex * 2 + 1;
                int childIndexRight = item.HeapIndex * 2 + 2;
                if (childIndexLeft >= CurrentItemCount)
                    return;

                // Sets the swapindex to the heapindex of the highest priority child.
                int swapIndex = (childIndexRight < CurrentItemCount && _items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0) ? childIndexRight : childIndexLeft;
                if (item.CompareTo(_items[swapIndex]) < 0)
                    Swap(item, _items[swapIndex]);
                else
                    return;
            }
        }

        private void SortUp(T item)
        {
            SetParentIndexAndParentItem(item, out int parentIndex, out T parentItem);
            while (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
                SetParentIndexAndParentItem(item, out parentIndex, out parentItem);
            }
        }

        private void SetParentIndexAndParentItem(T item, out int parentIndex, out T parentItem)
        {
            parentIndex = (item.HeapIndex - 1) / 2;
            parentItem = _items[parentIndex];
        }

        private void Swap(T itemA, T itemB)
        {
            _items[itemA.HeapIndex] = itemB;
            _items[itemB.HeapIndex] = itemA;
            int itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }
    }
}

