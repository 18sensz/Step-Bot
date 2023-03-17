using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Chess
{
    public static class Pieces
    {
        public const char WhiteKing = 'K';
        public const char WhiteQueen = 'Q';
        public const char WhiteRook = 'R';
        public const char WhiteBishop = 'B';
        public const char WhiteKnight = 'N';
        public const char WhitePawn = 'P';
               
        public const char BlackKing = 'k';
        public const char BlackQueen = 'q';
        public const char BlackRook = 'r';
        public const char BlackBishop = 'b';
        public const char BlackKnight = 'n';
        public const char BlackPawn = 'p';

        public const char Empty = 'x';

        public static int CountPieces(ulong bb)
        {
            int n = 0;
            for (int i = 0; i < 64; i++)
            {
                if (((bb >> i) & 1) == 1)
                {
                    n++;
                }
            }
            return n;
        }
    }
}
