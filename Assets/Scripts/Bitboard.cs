using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chess
{
    public class Bitboard
    {
        public ulong whiteKing;
        public ulong whiteQueens;
        public ulong whiteRooks;
        public ulong whiteBishops;
        public ulong whiteKnights;
        public ulong whitePawns;

        public ulong blackKing;
        public ulong blackQueens;
        public ulong blackRooks;
        public ulong blackBishops;
        public ulong blackKnights;
        public ulong blackPawns;

        public bool isWhiteMove;

        //Castle Rights
        public bool whiteKingCastleRights;
        public bool whiteQueenCastleRights;
        public bool blackKingCastleRights;
        public bool blackQueenCastleRights;

        //Enpassant
        public int enpassTargetSquare = -1;

        //Remove when done
        private MoveGenerator moveGen = new MoveGenerator();
        private List<Move> moves;

        public Bitboard(ChessBoard board)
        {
            for (int square = 0; square < board.squares.Length; square++)
            {
                switch (board.squares[square])
                {
                    case Pieces.WhiteKing:
                        whiteKing |= (1UL << square);
                        break;
                    case Pieces.WhiteQueen:
                        whiteQueens |= (1UL << square);
                        break;
                    case Pieces.WhiteRook:
                        whiteRooks |= (1UL << square);
                        break;
                    case Pieces.WhiteBishop:
                        whiteBishops |= (1UL << square);
                        break;
                    case Pieces.WhiteKnight:
                        whiteKnights |= (1UL << square);
                        break;
                    case Pieces.WhitePawn:
                        whitePawns |= (1UL << square);
                        break;
                    case Pieces.BlackKing:
                        blackKing |= (1UL << square);
                        break;
                    case Pieces.BlackQueen:
                        blackQueens |= (1UL << square);
                        break;
                    case Pieces.BlackRook:
                        blackRooks |= (1UL << square);
                        break;
                    case Pieces.BlackBishop:
                        blackBishops |= (1UL << square);
                        break;
                    case Pieces.BlackKnight:
                        blackKnights |= (1UL << square);
                        break;
                    case Pieces.BlackPawn:
                        blackPawns |= (1UL << square);
                        break;
                }
            }
               
            //Set board state
            this.isWhiteMove = board.isWhiteMove;

            this.whiteKingCastleRights = board.whiteKingCastleRights;
            this.whiteQueenCastleRights = board.whiteQueenCastleRights;
            this.blackKingCastleRights = board.blackKingCastleRights;
            this.blackQueenCastleRights = board.blackQueenCastleRights;

            this.enpassTargetSquare = board.enpassTargetSquare; 
            moves = moveGen.GenerateMoves(this);
        }

        public void MakeMove(Move move)
        {
            var moveFound = moves.Where(m => m.startingSquare == move.startingSquare && m.targetSquare == move.targetSquare).FirstOrDefault();

            if (moveFound != default) 
            {
                Debug.Log($"YOOOOOOOOOOOOOO {moveFound.type}");
                if (moveFound.type == MoveType.Quiet)
                {
                    if (isWhiteMove)
                    {
                        if (((whitePawns >> moveFound.startingSquare) & 1UL) == 1) whitePawns = HandleQuietMove(whitePawns, moveFound);
                        else if (((whiteKnights >> moveFound.startingSquare) & 1UL) == 1) whiteKnights = HandleQuietMove(whiteKnights, moveFound);
                        else if (((whiteBishops >> moveFound.startingSquare) & 1UL) == 1) whiteBishops = HandleQuietMove(whiteBishops, moveFound);
                        else if (((whiteRooks >> moveFound.startingSquare) & 1UL) == 1) whiteRooks = HandleQuietMove(whiteRooks, moveFound);
                        else if (((whiteQueens >> moveFound.startingSquare) & 1UL) == 1) whiteQueens = HandleQuietMove(whiteQueens, moveFound);
                        else if (((whiteKing >> moveFound.startingSquare) & 1UL) == 1) whiteKing = HandleQuietMove(whiteKing, moveFound);
                    }
                    else
                    {
                        if (((blackPawns >> moveFound.startingSquare) & 1UL) == 1) blackPawns = HandleQuietMove(blackPawns, moveFound);
                        else if (((blackKnights >> moveFound.startingSquare) & 1UL) == 1) blackKnights = HandleQuietMove(blackKnights, moveFound);
                        else if (((blackBishops >> moveFound.startingSquare) & 1UL) == 1) blackBishops = HandleQuietMove(blackBishops, moveFound);
                        else if (((blackRooks >> moveFound.startingSquare) & 1UL) == 1) blackRooks = HandleQuietMove(blackRooks, moveFound);
                        else if (((blackQueens >> moveFound.startingSquare) & 1UL) == 1) blackQueens = HandleQuietMove(blackQueens, moveFound);
                        else if (((blackKing >> moveFound.startingSquare) & 1UL) == 1) blackKing = HandleQuietMove(blackKing, moveFound);
                    }
                }
                else if (moveFound.type == MoveType.Capture)
                {
                    if (isWhiteMove)
                    {
                        //Move piece
                        if (((whitePawns >> moveFound.startingSquare) & 1UL) == 1) whitePawns = HandleQuietMove(whitePawns, moveFound);
                        else if (((whiteKnights >> moveFound.startingSquare) & 1UL) == 1) whiteKnights = HandleQuietMove(whiteKnights, moveFound);
                        else if (((whiteBishops >> moveFound.startingSquare) & 1UL) == 1) whiteBishops = HandleQuietMove(whiteBishops, moveFound);
                        else if (((whiteRooks >> moveFound.startingSquare) & 1UL) == 1) whiteRooks = HandleQuietMove(whiteRooks, moveFound);
                        else if (((whiteQueens >> moveFound.startingSquare) & 1UL) == 1) whiteQueens = HandleQuietMove(whiteQueens, moveFound);
                        else if (((whiteKing >> moveFound.startingSquare) & 1UL) == 1) whiteKing = HandleQuietMove(whiteKing, moveFound);

                        //RemoveFound captured piece (don't need to check kings because they cannot be captured)
                        if (((blackPawns >> moveFound.targetSquare) & 1UL) == 1) blackPawns = HandleCaptureMove(blackPawns, moveFound);
                        else if (((blackKnights >> moveFound.targetSquare) & 1UL) == 1) blackKnights = HandleCaptureMove(blackKnights, moveFound);
                        else if (((blackBishops >> moveFound.targetSquare) & 1UL) == 1) blackBishops = HandleCaptureMove(blackBishops, moveFound);
                        else if (((blackRooks >> moveFound.targetSquare) & 1UL) == 1) blackRooks = HandleCaptureMove(blackRooks, moveFound);
                        else if (((blackQueens >> moveFound.targetSquare) & 1UL) == 1) blackQueens = HandleCaptureMove(blackQueens, moveFound);
                    }
                    else
                    {
                        //Move piece
                        if (((blackPawns >> moveFound.startingSquare) & 1UL) == 1) blackPawns = HandleQuietMove(blackPawns, moveFound);
                        else if (((blackKnights >> moveFound.startingSquare) & 1UL) == 1) blackKnights = HandleQuietMove(blackKnights, moveFound);
                        else if (((blackBishops >> moveFound.startingSquare) & 1UL) == 1) blackBishops = HandleQuietMove(blackBishops, moveFound);
                        else if (((blackRooks >> moveFound.startingSquare) & 1UL) == 1) blackRooks = HandleQuietMove(blackRooks, moveFound);
                        else if (((blackQueens >> moveFound.startingSquare) & 1UL) == 1) blackQueens = HandleQuietMove(blackQueens, moveFound);
                        else if (((blackKing >> moveFound.startingSquare) & 1UL) == 1) blackKing = HandleQuietMove(blackKing, moveFound);

                        //RemoveFound captured piece (don't need to check kings because they cannot be captured)
                        if (((whitePawns >> moveFound.targetSquare) & 1UL) == 1) whitePawns = HandleCaptureMove(whitePawns, moveFound);
                        else if (((whiteKnights >> moveFound.targetSquare) & 1UL) == 1) whiteKnights = HandleCaptureMove(whiteKnights, moveFound);
                        else if (((whiteBishops >> moveFound.targetSquare) & 1UL) == 1) whiteBishops = HandleCaptureMove(whiteBishops, moveFound);
                        else if (((whiteRooks >> moveFound.targetSquare) & 1UL) == 1) whiteRooks = HandleCaptureMove(whiteRooks, moveFound);
                        else if (((whiteQueens >> moveFound.targetSquare) & 1UL) == 1) whiteQueens = HandleCaptureMove(whiteQueens, moveFound);
                    }
                }
                else if (moveFound.type == MoveType.WhiteKingSideCastle)
                {
                    whiteRooks &= ~(1UL);
                    whiteRooks |= (4UL);

                    whiteKing &= ~(8UL);
                    whiteKing |= (2UL);
                }
                else if (moveFound.type == MoveType.WhiteQueenSideCastle)
                {
                    whiteRooks &= ~(128UL);
                    whiteRooks |= (16UL);

                    whiteKing &= ~(8UL);
                    whiteKing |= (32UL);
                }
                else if (moveFound.type == MoveType.BlackKingSideCastle)
                {
                    blackRooks &= ~(72057594037927936UL);
                    blackRooks |= (288230376151711744UL);

                    blackKing &= ~(576460752303423488UL);
                    blackKing |= (144115188075855872UL);
                }
                else if (moveFound.type == MoveType.BlackQueenSideCastle)
                {
                    blackRooks &= ~(9223372036854775808UL);
                    blackRooks |= (1152921504606846976UL);

                    blackKing &= ~(576460752303423488UL);
                    blackKing |= (2305843009213693952UL);
                }
                else if (moveFound.type == MoveType.EnPassant)
                {
                    //Set enpassTargetSquare to one behind target
                    enpassTargetSquare = isWhiteMove ? moveFound.targetSquare - 8 : moveFound.targetSquare + 8;
                    if (isWhiteMove)
                        whitePawns = HandleQuietMove(whitePawns, moveFound);
                    else
                        blackPawns = HandleQuietMove(blackPawns, moveFound);

                }
                else if (moveFound.type == MoveType.EnPassantCapture)
                {
                    Debug.Log("Yoo we enpassanted frfr!");
                    if (isWhiteMove)
                    {
                        //RemoveFound pawn that got enpassanted (Only need to check black pawns)
                        int pieceToRemSqr = moveFound.targetSquare - 8;
                        blackPawns &= ~(1UL << pieceToRemSqr);

                        //Move pawn (only need to check white pawns)
                        whitePawns = HandleQuietMove(whitePawns, moveFound);
                    }
                    else
                    {
                        //RemoveFound pawn that got enpassanted (Only need to check white pawns)
                        int pieceToRemSqr = moveFound.targetSquare + 8;
                        whitePawns &= ~(1UL << pieceToRemSqr);

                        //Move pawn (only need to check black pawns)
                        blackPawns = HandleQuietMove(blackPawns, moveFound);
                    }
                }
                else if (moveFound.type == MoveType.QueenPromotion || moveFound.type == MoveType.QueenCapturePromotion)
                {
                    if (isWhiteMove)
                    {
                        (whitePawns, whiteQueens) = HandlePromotingMove(whitePawns, whiteQueens, moveFound);

                        if (moveFound.type == MoveType.QueenCapturePromotion)
                        {
                            if (((blackKnights >> moveFound.targetSquare) & 1UL) == 1) blackKnights = HandleCaptureMove(blackKnights, moveFound);
                            else if (((blackBishops >> moveFound.targetSquare) & 1UL) == 1) blackBishops = HandleCaptureMove(blackBishops, moveFound);
                            else if (((blackRooks >> moveFound.targetSquare) & 1UL) == 1) blackRooks = HandleCaptureMove(blackRooks, moveFound);
                            else if (((blackQueens >> moveFound.targetSquare) & 1UL) == 1) blackQueens = HandleCaptureMove(blackQueens, moveFound);
                        }
                    }
                    else
                    {
                        (blackPawns, blackQueens) = HandlePromotingMove(blackPawns, blackQueens, moveFound);

                        if (moveFound.type == MoveType.QueenCapturePromotion)
                        {
                            if (((whiteKnights >> moveFound.targetSquare) & 1UL) == 1) whiteKnights = HandleCaptureMove(whiteKnights, moveFound);
                            else if (((whiteBishops >> moveFound.targetSquare) & 1UL) == 1) whiteBishops = HandleCaptureMove(whiteBishops, moveFound);
                            else if (((whiteRooks >> moveFound.targetSquare) & 1UL) == 1) whiteRooks = HandleCaptureMove(whiteRooks, moveFound);
                            else if (((whiteQueens >> moveFound.targetSquare) & 1UL) == 1) whiteQueens = HandleCaptureMove(whiteQueens, moveFound);
                        }
                    }
                }
                else if (moveFound.type == MoveType.RookPromotion || moveFound.type == MoveType.RookCapturePromotion)
                {
                    if (isWhiteMove)
                    {
                        (whitePawns, whiteRooks) = HandlePromotingMove(whitePawns, whiteRooks, moveFound);

                        if (moveFound.type == MoveType.RookCapturePromotion)
                        {
                            if (((blackKnights >> moveFound.targetSquare) & 1UL) == 1) blackKnights = HandleCaptureMove(blackKnights, moveFound);
                            else if (((blackBishops >> moveFound.targetSquare) & 1UL) == 1) blackBishops = HandleCaptureMove(blackBishops, moveFound);
                            else if (((blackRooks >> moveFound.targetSquare) & 1UL) == 1) blackRooks = HandleCaptureMove(blackRooks, moveFound);
                            else if (((blackQueens >> moveFound.targetSquare) & 1UL) == 1) blackQueens = HandleCaptureMove(blackQueens, moveFound);
                        }
                    }
                    else
                    {
                        (blackPawns, blackRooks) = HandlePromotingMove(blackPawns, blackRooks, moveFound);

                        if (moveFound.type == MoveType.RookCapturePromotion)
                        {
                            if (((whiteKnights >> moveFound.targetSquare) & 1UL) == 1) whiteKnights = HandleCaptureMove(whiteKnights, moveFound);
                            else if (((whiteBishops >> moveFound.targetSquare) & 1UL) == 1) whiteBishops = HandleCaptureMove(whiteBishops, moveFound);
                            else if (((whiteRooks >> moveFound.targetSquare) & 1UL) == 1) whiteRooks = HandleCaptureMove(whiteRooks, moveFound);
                            else if (((whiteQueens >> moveFound.targetSquare) & 1UL) == 1) whiteQueens = HandleCaptureMove(whiteQueens, moveFound);
                        }
                    }
                }
                else if (moveFound.type == MoveType.BishopPromotion || moveFound.type == MoveType.BishopCapturePromotion)
                {
                    if (isWhiteMove)
                    {
                        (whitePawns, whiteBishops) = HandlePromotingMove(whitePawns, whiteBishops, moveFound);

                        if (moveFound.type == MoveType.BishopCapturePromotion)
                        {
                            if (((blackKnights >> moveFound.targetSquare) & 1UL) == 1) blackKnights = HandleCaptureMove(blackKnights, moveFound);
                            else if (((blackBishops >> moveFound.targetSquare) & 1UL) == 1) blackBishops = HandleCaptureMove(blackBishops, moveFound);
                            else if (((blackRooks >> moveFound.targetSquare) & 1UL) == 1) blackRooks = HandleCaptureMove(blackRooks, moveFound);
                            else if (((blackQueens >> moveFound.targetSquare) & 1UL) == 1) blackQueens = HandleCaptureMove(blackQueens, moveFound);
                        }
                    }
                    else
                    {
                        (blackPawns, blackBishops) = HandlePromotingMove(blackPawns, blackBishops, moveFound);

                        if (moveFound.type == MoveType.BishopCapturePromotion)
                        {
                            if (((whiteKnights >> moveFound.targetSquare) & 1UL) == 1) whiteKnights = HandleCaptureMove(whiteKnights, moveFound);
                            else if (((whiteBishops >> moveFound.targetSquare) & 1UL) == 1) whiteBishops = HandleCaptureMove(whiteBishops, moveFound);
                            else if (((whiteRooks >> moveFound.targetSquare) & 1UL) == 1) whiteRooks = HandleCaptureMove(whiteRooks, moveFound);
                            else if (((whiteQueens >> moveFound.targetSquare) & 1UL) == 1) whiteQueens = HandleCaptureMove(whiteQueens, moveFound);
                        }
                    }
                }
                else if (moveFound.type == MoveType.KnightPromotion || moveFound.type == MoveType.KnightCapturePromotion)
                {
                    if (isWhiteMove)
                    {
                        (whitePawns, whiteKnights) = HandlePromotingMove(whitePawns, whiteKnights, moveFound);

                        if (moveFound.type == MoveType.KnightCapturePromotion)
                        {
                            if (((blackKnights >> moveFound.targetSquare) & 1UL) == 1) blackKnights = HandleCaptureMove(blackKnights, moveFound);
                            else if (((blackBishops >> moveFound.targetSquare) & 1UL) == 1) blackBishops = HandleCaptureMove(blackBishops, moveFound);
                            else if (((blackRooks >> moveFound.targetSquare) & 1UL) == 1) blackRooks = HandleCaptureMove(blackRooks, moveFound);
                            else if (((blackQueens >> moveFound.targetSquare) & 1UL) == 1) blackQueens = HandleCaptureMove(blackQueens, moveFound);
                        }
                    }
                    else
                    {
                        (blackPawns, blackKnights) = HandlePromotingMove(blackPawns, blackKnights, moveFound);

                        if (moveFound.type == MoveType.KnightCapturePromotion)
                        {
                            if (((whiteKnights >> moveFound.targetSquare) & 1UL) == 1) whiteKnights = HandleCaptureMove(whiteKnights, moveFound);
                            else if (((whiteBishops >> moveFound.targetSquare) & 1UL) == 1) whiteBishops = HandleCaptureMove(whiteBishops, moveFound);
                            else if (((whiteRooks >> moveFound.targetSquare) & 1UL) == 1) whiteRooks = HandleCaptureMove(whiteRooks, moveFound);
                            else if (((whiteQueens >> moveFound.targetSquare) & 1UL) == 1) whiteQueens = HandleCaptureMove(whiteQueens, moveFound);
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException($"Tried to make moveFound that isn't implemented yet! MoveType: {moveFound.type}");
                }

                if (moveFound.type != MoveType.EnPassant)
                    enpassTargetSquare = -1;

                //Enforce castle rights by checking king and rooks position after each move
                VerifyCastleRights();

                //Change move
                isWhiteMove = !isWhiteMove;
            }
            
        }

        private void VerifyCastleRights()
        {
            if (whiteKingCastleRights || whiteQueenCastleRights)
            {
                //King moved and is not in position to castle anymore
                if ((whiteKing & 8UL) != 8UL)
                {
                    whiteKingCastleRights = false;
                    whiteQueenCastleRights = false;
                }

                //Rooks moved or were captured therefore castle rights were lost
                if ((whiteRooks & 1UL) != 1UL)
                    whiteKingCastleRights = false;
                if ((whiteRooks & 128UL) != 128UL)
                    whiteQueenCastleRights = false;
            }

            if (blackKingCastleRights || blackQueenCastleRights)
            {
                //King moved and is not in position to castle anymore
                if ((blackKing & 576460752303423488UL) != 576460752303423488UL)
                {
                    blackKingCastleRights = false;
                    blackQueenCastleRights = false;
                }

                //Rooks moved or were captured therefore castle rights were lost
                if ((blackRooks & 72057594037927936UL) != 72057594037927936UL)
                    blackKingCastleRights = false;
                if ((blackRooks & 9223372036854775808UL) != 9223372036854775808UL)
                    blackQueenCastleRights = false;
            }
        }

        private ulong HandleQuietMove(ulong board, Move move)
        {
            board &= ~(1UL << move.startingSquare); //Remove from starting pos
            board |= (1UL << move.targetSquare); //Add piece to target

            return board;
        }

        private ulong HandleCaptureMove(ulong cboard, Move move)
        {
            cboard &= ~(1UL << move.targetSquare); //Remove from captured piece

            return cboard;
        }

        private (ulong, ulong) HandlePromotingMove(ulong pawnBoard, ulong promotionBoard, Move move)
        {
            //Remove pawn that is premoting
            pawnBoard &= ~(1UL << move.startingSquare);
            //Create queen that promoted from pawn
            promotionBoard |= (1UL << move.targetSquare);

            return (pawnBoard, promotionBoard);
        }

        public int getKingSquare(bool isWhiteMove)
        {
            ulong kBB = isWhiteMove ? this.whiteKing: this.blackKing;

            for (int square = 0; square < 64; square++)
            {
                if(((kBB >> square) & 1) == 1)
                    return square;
            }

            Debug.Log("Could not find king!");
            return -1;
        }

        public char[] ToArray()
        {
            char[] bbArray = new char[64];
            Array.Fill(bbArray, Pieces.Empty);

            for (int i = 0; i < 64; i++)
            {
                if (((whiteKing >> i) & 1UL) == 1) bbArray[i] = Pieces.WhiteKing;
                if (((whiteQueens >> i) & 1UL) == 1) bbArray[i] = Pieces.WhiteQueen;
                if (((whiteRooks >> i) & 1UL) == 1) bbArray[i] = Pieces.WhiteRook;
                if (((whiteBishops >> i) & 1UL) == 1) bbArray[i] = Pieces.WhiteBishop;
                if (((whiteKnights >> i) & 1UL) == 1) bbArray[i] = Pieces.WhiteKnight;
                if (((whitePawns >> i) & 1UL) == 1) bbArray[i] = Pieces.WhitePawn;

                if (((blackKing >> i) & 1UL) == 1) bbArray[i] = Pieces.BlackKing;
                if (((blackQueens >> i) & 1UL) == 1) bbArray[i] = Pieces.BlackQueen;
                if (((blackRooks >> i) & 1UL) == 1) bbArray[i] = Pieces.BlackRook;
                if (((blackBishops >> i) & 1UL) == 1) bbArray[i] = Pieces.BlackBishop;
                if (((blackKnights >> i) & 1UL) == 1) bbArray[i] = Pieces.BlackKnight;
                if (((blackPawns >> i) & 1UL) == 1) bbArray[i] = Pieces.BlackPawn;
            }

            return bbArray;
        }

        public void Draw()
        {
            throw new NotImplementedException();
        }
    }
}
