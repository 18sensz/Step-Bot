using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess
{
    public static class BitboardHelper
    {
        public static bool IsSlider(Bitboard bb, int square)
        {
            if((((bb.whiteKing >> square) & 1) == 1) || (((bb.whiteKnights >> square) & 1) == 1) || (((bb.whitePawns >> square) & 1) == 1)
                || (((bb.blackKing >> square) & 1) == 1) || (((bb.blackKnights >> square) & 1) == 1) || (((bb.blackPawns >> square) & 1) == 1))
                return false;

            return true;
        }

        //Only works with BB with one piece
        public static int GetSqPos(ulong bb)
        {
            for (int i = 0; i < 64; i++)
            {
                if (((bb >> i) & 1) == 1)
                {
                    return i;
                }
            }
            return -1;//Didnt find any pieces
        }
    }
}
