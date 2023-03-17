using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess
{
    [System.Serializable]
    public enum MoveType
    {
        Quiet,
        Capture,
        EnPassant,
        EnPassantCapture,
        QueenPromotion,
        RookPromotion,
        BishopPromotion,
        KnightPromotion,
        QueenCapturePromotion,
        RookCapturePromotion,
        BishopCapturePromotion,
        KnightCapturePromotion,
        WhiteKingSideCastle,
        WhiteQueenSideCastle,
        BlackKingSideCastle,
        BlackQueenSideCastle
    }
    [System.Serializable]
    public class Move
    {
        public int startingSquare;
        public int targetSquare;
        public MoveType type;

        public Move(int startingSquare, int targetSquare)
        {
            if (startingSquare < 0 || startingSquare > 63)
                Debug.LogError("Move created with ivalid [startingSquare] Starting square needs to be between 0 <= m <= 63");
            if (targetSquare < 0 || targetSquare > 63)
                Debug.LogError("Move created with ivalid [startingSquare] Starting square needs to be between 0 <= m <= 63");

            this.startingSquare = startingSquare;
            this.targetSquare = targetSquare;
            this.type = MoveType.Quiet;
        }

        public Move(int startingSquare, int targetSquare, MoveType type)
        {
            if (startingSquare < 0 || startingSquare > 63)
                Debug.LogError("Move created with ivalid [startingSquare] Starting square needs to be between 0 <= m <= 63");
            if (targetSquare < 0 || targetSquare > 63)
                Debug.LogError("Move created with ivalid [targetSquare] Target square needs to be between 0 <= m <= 63");

            this.startingSquare = startingSquare;
            this.targetSquare = targetSquare;
            this.type = type;
        }
    }
}
