using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Algorithms
{
    public static class ListExtensions
    {
        public static void Quicksort<T>(this List<T> array) where T : IComparable<T>
        {
            int left = 0;
            int right = array.Count - 1;

            Quicksort(array, left, right);
        }

        private static void Quicksort<T>(List<T> array, int left, int right) where T : IComparable<T>
        {
            if (left > right || left < 0 || right < 0) return;

            int index = Partition(array, left, right);
            if (index != -1)
            {
                Quicksort(array, left, index - 1);
                Quicksort(array, index + 1, right);
            }
        }

        private static int Partition<T>(List<T> array, int left, int right) where T : IComparable<T>
        {
            if (left > right) return -1;

            int end = left;

            T pivot = array[right];    // choose last one to pivot, easy to code
            for (int i = left; i < right; i++)
            {
                if (array[i].CompareTo(pivot) <= 0)
                {
                    array.Swap(i, end);
                    end++;
                }
            }

            array.Swap(end, right);

            return end;
        }

        public static void Swap<T>(this List<T> array, int indexA, int indexB)
        {
            T temp = array[indexA];
            array[indexA] = array[indexB];
            array[indexB] = temp;
        }
    }

    public static class ArrayExtensions
    {
        public static void Quicksort<T>(this T[] array) where T : IComparable<T>
        {
            int left = 0;
            int right = array.Length - 1;

            Quicksort(array, left, right);
        }

        private static void Quicksort<T>(T[] array, int left, int right) where T : IComparable<T>
        {
            if (left > right || left < 0 || right < 0) return;

            int index = Partition(array, left, right);
            if (index != -1)
            {
                Quicksort(array, left, index - 1);
                Quicksort(array, index + 1, right);
            }
        }

        private static int Partition<T>(T[] array, int left, int right) where T : IComparable<T>
        {
            if (left > right) return -1;

            int end = left;

            T pivot = array[right];    // choose last one to pivot, easy to code
            for (int i = left; i < right; i++)
            {
                if (array[i].CompareTo(pivot) <= 0)
                {
                    Swap(array, i, end);
                    end++;
                }
            }

            Swap(array, end, right);

            return end;
        }

        public static void Swap<T>(this T[] array, int indexA, int indexB)
        {
            T temp = array[indexA];
            array[indexA] = array[indexB];
            array[indexB] = temp;
        }
    }
}