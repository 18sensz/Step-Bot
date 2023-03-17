using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chess
{
    [System.Serializable]
    public class ChessBoard
    {
        public char[] squares = new char[64];

        private List<Move> moves = new List<Move>();
        public List<Move> Moves { get { return moves; } }

        public bool isWhiteMove;

        public event Action<ChessBoard> OnPieceMoved;
        //public event Action<GameResult> GameOver; Game result would be some enum to signify how the game ended? checkmate if king is in check and has no moves, stalemate if king is not in check and has no moves, do we care about time (dont think so)
        private MoveGenerator moveGen = new MoveGenerator();

        //Castle Rights
        public bool whiteKingCastleRights;
        public bool whiteQueenCastleRights;
        public bool blackKingCastleRights;
        public bool blackQueenCastleRights;

        //Enpassant
        public int enpassTargetSquare = -1;

        public ChessBoard(Position pos)
        {
            Array.Fill(squares, Pieces.Empty);

            //Set board of squares
            squares = pos.squares;

            //Set Turn
            isWhiteMove = pos.isWhiteMove;

            //Set castle rights
            whiteKingCastleRights = pos.whiteKingCastleRights;
            blackKingCastleRights = pos.blackKingCastleRights;
            whiteQueenCastleRights = pos.whiteQueenCastleRights;
            blackQueenCastleRights = pos.blackQueenCastleRights;

            //Set enpass
            enpassTargetSquare = pos.enpassTargetSquare;

            moves = moveGen.GenerateMoves(this);
        }
        public ChessBoard(Bitboard bb)
        {
            Array.Fill(squares, Pieces.Empty);

            squares = bb.ToArray();

            isWhiteMove = bb.isWhiteMove;
            whiteKingCastleRights = bb.whiteKingCastleRights;
            blackKingCastleRights = bb.blackKingCastleRights;
            whiteQueenCastleRights = bb.whiteQueenCastleRights;
            blackQueenCastleRights = bb.blackQueenCastleRights;

            enpassTargetSquare = bb.enpassTargetSquare;
                 
            moves = moveGen.GenerateMoves(this);
        }


        //Note: Attempts to make the given move; returns true if it is a legal move; returns false if it is not legal
        public bool TryMakeMove(Move move)
        {
            ////Check if it's a legal move
            //var moveFound = moves.Where(m => m.startingSquare == move.startingSquare && m.targetSquare == move.targetSquare).FirstOrDefault();
            //if (moveFound != default)
            //{

            //    if (moveFound.type == MoveType.Quiet || moveFound.type == MoveType.Capture)
            //    {
            //        char pieceToMove = this.squares[moveFound.startingSquare];
            //        this.squares[moveFound.startingSquare] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = pieceToMove;
            //    }
            //    else if (moveFound.type == MoveType.WhiteKingSideCastle)
            //    {
            //        this.squares[3] = Pieces.Empty;
            //        this.squares[0] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = Pieces.WhiteKing;
            //        this.squares[2] = Pieces.WhiteRook;
            //        this.whiteQueenCastleRights = false;
            //        this.whiteKingCastleRights = false;
            //    }
            //    else if (moveFound.type == MoveType.WhiteQueenSideCastle)
            //    {
            //        this.squares[3] = Pieces.Empty;
            //        this.squares[7] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = Pieces.WhiteKing;
            //        this.squares[4] = Pieces.WhiteRook;
            //        this.whiteKingCastleRights = false;
            //        this.whiteQueenCastleRights = false;
            //    }
            //    else if (moveFound.type == MoveType.BlackKingSideCastle)
            //    {
            //        this.squares[59] = Pieces.Empty;
            //        this.squares[56] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = Pieces.BlackKing;
            //        this.squares[58] = Pieces.BlackRook;
            //        this.blackKingCastleRights = false;
            //        this.blackQueenCastleRights = false;
            //    }
            //    else if (moveFound.type == MoveType.BlackQueenSideCastle)
            //    {
            //        this.squares[59] = Pieces.Empty;
            //        this.squares[63] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = Pieces.BlackKing;
            //        this.squares[60] = Pieces.BlackRook;
            //        this.blackKingCastleRights = false;
            //        this.blackQueenCastleRights = false;
            //    }
            //    else if (moveFound.type == MoveType.EnPassant)
            //    {
            //        //Set enpassTargetSquare to one behind target
            //        enpassTargetSquare = isWhiteMove ? moveFound.targetSquare - 8 : moveFound.targetSquare + 8;

            //        char pieceToMove = this.squares[moveFound.startingSquare];
            //        this.squares[moveFound.startingSquare] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = pieceToMove;
            //    }
            //    else if (moveFound.type == MoveType.EnPassantCapture)
            //    {
            //        //Remove pawn that got enpassanted
            //        this.squares[isWhiteMove ? moveFound.targetSquare - 8 : moveFound.targetSquare + 8] = Pieces.Empty;

            //        char pieceToMove = this.squares[moveFound.startingSquare];
            //        this.squares[moveFound.startingSquare] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = pieceToMove;
            //    }
            //    else if (moveFound.type == MoveType.QueenPromotion || moveFound.type == MoveType.QueenCapturePromotion)
            //    {
            //        this.squares[moveFound.startingSquare] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = isWhiteMove ? Pieces.WhiteQueen : Pieces.BlackQueen;
            //    }
            //    else if (moveFound.type == MoveType.RookPromotion || moveFound.type == MoveType.RookCapturePromotion)
            //    {
            //        this.squares[moveFound.startingSquare] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = isWhiteMove ? Pieces.WhiteRook : Pieces.BlackRook;
            //    }
            //    else if (moveFound.type == MoveType.BishopPromotion || moveFound.type == MoveType.BishopCapturePromotion)
            //    {
            //        this.squares[moveFound.startingSquare] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = isWhiteMove ? Pieces.WhiteBishop : Pieces.WhiteBishop;
            //    }
            //    else if (moveFound.type == MoveType.KnightPromotion || moveFound.type == MoveType.KnightCapturePromotion)
            //    {
            //        this.squares[moveFound.startingSquare] = Pieces.Empty;
            //        this.squares[moveFound.targetSquare] = isWhiteMove ? Pieces.WhiteKnight : Pieces.BlackKnight;
            //    }
            //    else
            //    {
            //        throw new NotImplementedException($"Tried to make move that isn't implemented yet! MoveType: {moveFound.type}");
            //    }

            //    //reset enpass always unless enpassant was triggered
            //    if(moveFound.type != MoveType.EnPassant)
            //        enpassTargetSquare = -1;

            //    //Enforce castle rights by checking king and rooks position after each move
            //    VerifyCastleRights();

            //    isWhiteMove = !isWhiteMove;
            //    moves = moveGen.GenerateMoves(this);
            //    OnPieceMoved?.Invoke(this);

            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            moves = moveGen.GenerateMoves(this);
            OnPieceMoved?.Invoke(this);
            return true;
        }

        public void VerifyCastleRights()
        {
            if(whiteKingCastleRights || whiteQueenCastleRights)
            {
                //King moved and is not in position to castle anymore
                if (squares[3] != Pieces.WhiteKing)
                {
                    whiteKingCastleRights = false;
                    whiteQueenCastleRights = false;
                }

                //Rooks moved or were captured therefore castle rights were lost
                if (squares[0] != Pieces.WhiteRook)
                    whiteKingCastleRights = false;
                if (squares[7] != Pieces.WhiteRook)
                    whiteQueenCastleRights = false;
            }

            if(blackKingCastleRights || blackQueenCastleRights)
            {
                //King moved and is not in position to castle anymore
                if (squares[59] != Pieces.BlackKing)
                {
                    blackKingCastleRights = false;
                    blackQueenCastleRights = false;
                }

                //Rooks moved or were captured therefore castle rights were lost
                if (squares[56] != Pieces.BlackRook)
                    blackKingCastleRights = false;
                if (squares[63] != Pieces.BlackRook)
                    blackQueenCastleRights = false;
            }
        }
    }
}
