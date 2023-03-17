using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess
{
    public class Position
    {
        //Board squares
        public char[] squares = new char[64];

        //WhosMove
        public bool isWhiteMove;

        //Castle Rights
        public bool whiteKingCastleRights;
        public bool whiteQueenCastleRights;
        public bool blackKingCastleRights;
        public bool blackQueenCastleRights;

        //Enpass
        public int enpassTargetSquare = -1;

        public Position()
        {
            Array.Fill(squares, Pieces.Empty);
        }

    }
}
