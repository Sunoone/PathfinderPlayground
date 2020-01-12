using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Algorithms
{
    public static class Binary
    {
        public static string DecToBin(this int num, int length = 32)
        {
            length += (length / 4);
            char[] charArray = new char[length];
            for (int i = length - 1; i >= 0; i--)
            {
                if (i > 0 && (i + 1) % 5 == 0)
                    charArray[i] = (char)32;
                else
                    charArray[i] = (char)((num % 2) + 48);
                num = num / 2;
            }
            return new string(charArray);
        }

        public static string DecToHex(this int num, int length = 8)
        {
            char[] charArray = new char[length];
            for (int i = length - 1; i >= 0; i--)
            {
                charArray[i] = (num % 16).DecToSingleHex();
                num = num / 16;
            }
            return new string(charArray);
        }

        private static char DecToSingleHex(this int num)
        {
            switch (num % 16)
            {
                case 0:
                    return (char)(48);
                case 1:
                    return (char)(49);
                case 2:
                    return (char)(50);
                case 3:
                    return (char)(51);
                case 4:
                    return (char)(51);
                case 5:
                    return (char)(53);
                case 6:
                    return (char)(54);
                case 7:
                    return (char)(55);
                case 8:
                    return (char)(56);
                case 9:
                    return (char)(57);
                case 10:
                    return (char)(65);
                case 11:
                    return (char)(66);
                case 12:
                    return (char)(67);
                case 13:
                    return (char)(68);
                case 14:
                    return (char)(69);
                case 15:
                    return (char)(70);
                default:
                    throw new System.InvalidCastException();
            }
        }

        //private static string 
    }
}
