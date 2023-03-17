using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess
{
    public static class BitboardUtility
    {
        private static readonly int[] BitScanTable = new int[]
        {
            0, 47,  1, 56, 48, 27,  2, 60,
           57, 49, 41, 37, 28, 16,  3, 61,
           54, 58, 35, 52, 50, 42, 21, 44,
           38, 32, 29, 23, 17, 11,  4, 62,
           46, 55, 26, 59, 40, 36, 15, 53,
           34, 51, 20, 43, 31, 22, 10, 45,
           25, 39, 14, 33, 19, 30,  9, 24,
           13, 18,  8, 12,  7,  6,  5, 63
        };

        public static int BitScanLookup(ulong bitboard)
        {
            return BitScanTable[((bitboard ^ (bitboard - 1)) * 0x03f79d71b4cb0a89UL) >> 58];
        }
        public static IEnumerable<int> FindPieces(ulong bitboard)
        {
            while (bitboard != 0)
            {
                int index = BitScanLookup(bitboard);
                yield return index;
                bitboard ^= (1UL << index);
            }
        }
    }

}
