using Chess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

namespace Chess
{
    //Speedup ideas
    // -Bitboard.trailingbits (should speedup bitscan)
    // -asserting the moves generated are legal seems inefficient
    // -calculating the positions of pieces once and storing it (more overhead but more efficient almost always)?
    public class MoveGenerator
    {
        #region Global MoveGenerator constants

        //Always the same
        private readonly ulong A_FILE = 0x8080808080808080;
        private readonly ulong B_FILE = 0x4040404040404040;
        private readonly ulong G_FILE = 0x202020202020202;
        private readonly ulong H_FILE = 0x0101010101010101;
        private readonly ulong RANK_ONE = 0x00000000000000FF;
        private readonly ulong RANK_EIGHT = 0xFF00000000000000;
        private readonly ulong RANK_FOUR = 0xFF000000;
        private readonly ulong RANK_FIVE = 0xFF00000000;
        private readonly ulong KNIGHT_SPAN = 43234889994UL;
        private readonly ulong KING_SPAN = 460039UL;
        private readonly ulong[] CASTLE_LANES =
        {
            0x70UL, 0x6UL, 0x7000000000000000UL, 0x600000000000000UL //WQS WKS BQS BKS
        };
        private readonly ulong[] RankMasks8 = {
            0xFFUL, 0xFF00UL, 0xFF0000UL, 0xFF000000UL, 0xFF00000000UL, 0xFF0000000000UL, 0xFF000000000000UL, 0xFF00000000000000UL
        };
        private readonly ulong[] FileMask8 = {
            0x8080808080808080UL, 0x4040404040404040UL, 0x2020202020202020UL, 0x1010101010101010UL, 0x808080808080808UL, 0x404040404040404UL, 0x202020202020202UL, 0x101010101010101UL
        };
        private readonly ulong[] DiagnalMask = {
            1UL, 258UL, 66052UL, 16909320UL, 4328785936UL, 1108169199648UL, 283691315109952UL, 72624976668147840UL,
            145249953336295424UL, 290499906672525312UL, 580999813328273408UL, 1161999622361579520UL, 2323998145211531264UL,
            4647714815446351872UL, 9223372036854775808UL
        };
        private readonly ulong[] AntiDiagnalMask = {
            128UL, 32832UL, 8405024UL, 2151686160UL, 550831656968UL, 141012904183812UL, 36099303471055874UL, 9241421688590303745UL,
            4620710844295151872UL, 2310355422147575808UL, 1155177711073755136UL, 577588855528488960UL, 288794425616760832UL,
            144396663052566528UL, 72057594037927936UL
        };

        //Dependant on the board <--- THESE NEED TO BE INITIALIZED AFTER EACH TURN --->
        private ulong EMPTY;
        private ulong NOT_MY_PIECES;
        private ulong OPPONENTS_PIECES;
        private ulong OCCUPIED;
        private ulong OPPONENT_ATTACKS;
        private ulong MY_KING;
        private int KING_SQUARE;
        private ulong CHECKERS;
        private int NUM_CHECKERS;
        private ulong PINNED_PIECES;
        private ulong CAPTURE_MASK = 0xFFFFFFFFFFFFFFFF;
        private ulong PUSH_MASK = 0xFFFFFFFFFFFFFFFF;
        private ulong ENPASS_TARGET = 0UL;
        private int ENPASS_SQUARE;
        private ulong ENPASS_CAPTURE_MASK = 0xFFFFFFFFFFFFFFFF;

        #endregion

        #region Move Generation Functions

        //IDEA: Would it be better to have two of these function? One that takes in a chessboard and one that takes in a bitboard? could save on time converting when it's deep in generating moves
        public List<Move> GenerateMoves(ChessBoard board)
        {
            var stopwatch = Stopwatch.StartNew();
            bool isWhiteMove = board.isWhiteMove;
            var allMoves = new List<Move>();
            Bitboard bb = new Bitboard(board);

            stopwatch.Start();
            //Inititialize state and whatnot
            Initialize(bb, isWhiteMove);

            //Generate moves for all pieces for given turn and add it to the list of total moves
            allMoves.AddRange(isWhiteMove ? GenerateWhitePawnMoves(bb) : GenerateBlackPawnMoves(bb));
            allMoves.AddRange(GenerateKnightMoves(isWhiteMove ? bb.whiteKnights : bb.blackKnights, isWhiteMove));
            allMoves.AddRange(GenerateBishopMoves(isWhiteMove ? bb.whiteBishops : bb.blackBishops, isWhiteMove));
            allMoves.AddRange(GenerateRookMoves(isWhiteMove ? bb.whiteRooks : bb.blackRooks, isWhiteMove));
            allMoves.AddRange(GenerateQueenMoves(isWhiteMove ? bb.whiteQueens : bb.blackQueens, isWhiteMove));
            allMoves.AddRange(GenerateKingMoves(isWhiteMove ? bb.whiteKing : bb.blackKing, isWhiteMove));
            allMoves.AddRange(GenerateCastleMoves(bb));

            stopwatch.Stop();
            UnityEngine.Debug.Log($"CB -- Move Generation took {stopwatch.Elapsed} milliseconds to generate {allMoves.Count} moves!");
            stopwatch.Restart();
            return allMoves;
        }

        public List<Move> GenerateMoves(Bitboard bb)
        {
            var stopwatch = Stopwatch.StartNew();
            bool isWhiteMove = bb.isWhiteMove;
            var allMoves = new List<Move>();

            stopwatch.Start();
            //Inititialize state and whatnot
            Initialize(bb, isWhiteMove);

            //Generate moves for all pieces for given turn and add it to the list of total moves
            allMoves.AddRange(isWhiteMove ? GenerateWhitePawnMoves(bb) : GenerateBlackPawnMoves(bb));
            allMoves.AddRange(GenerateKnightMoves(isWhiteMove ? bb.whiteKnights : bb.blackKnights, isWhiteMove));
            allMoves.AddRange(GenerateBishopMoves(isWhiteMove ? bb.whiteBishops : bb.blackBishops, isWhiteMove));
            allMoves.AddRange(GenerateRookMoves(isWhiteMove ? bb.whiteRooks : bb.blackRooks, isWhiteMove));
            allMoves.AddRange(GenerateQueenMoves(isWhiteMove ? bb.whiteQueens : bb.blackQueens, isWhiteMove));
            allMoves.AddRange(GenerateKingMoves(isWhiteMove ? bb.whiteKing : bb.blackKing, isWhiteMove));
            allMoves.AddRange(GenerateCastleMoves(bb));

            stopwatch.Stop();
            UnityEngine.Debug.Log($"BB -- Move Generation took {stopwatch.Elapsed} milliseconds to generate {allMoves.Count} moves!");
            stopwatch.Restart();
            return allMoves;
        }

        private List<Move> GenerateWhitePawnMoves(Bitboard bb)
        {
            var stopWatch = new Stopwatch();
            var pawnMoves = new List<Move>();
            ulong pawnBB = bb.whitePawns;
            ulong pawnMovesBB = 0UL;

            //Move forward one
            pawnMovesBB = ((((pawnBB & ~(PINNED_PIECES) & ~(RANK_EIGHT)) << 8) & EMPTY)) & (CAPTURE_MASK | PUSH_MASK);

            //stopWatch.Start();
            ////Covert bitboard to moves TODO: make an array of pieces for quicker lookup of piece - Logic chess did something with rooks that worked well
            //for (int square = 0; square < 64; square++)
            //{
            //    //Found move else do nothing
            //    if (((pawnMovesBB >> square) & 1) == 1)
            //    {
            //        if (square >= 56) //Promotion
            //        {
            //            pawnMoves.Add(new Move(square - 8, square, MoveType.QueenPromotion));
            //            pawnMoves.Add(new Move(square - 8, square, MoveType.RookPromotion));
            //            pawnMoves.Add(new Move(square - 8, square, MoveType.BishopPromotion));
            //            pawnMoves.Add(new Move(square - 8, square, MoveType.KnightPromotion));
            //        }
            //        else //Normal
            //        {
            //            pawnMoves.Add(new Move(square - 8, square, MoveType.Quiet));
            //        }
            //    }
            //}
            //stopWatch.Stop();
            //UnityEngine.Debug.Log($"OG PAWN Generation took {stopWatch.Elapsed} milliseconds");

            stopWatch.Reset();
            stopWatch.Start();
            var pawns = BitboardUtility.FindPieces(pawnMovesBB);
            foreach (var piece in pawns)
            {
                if (piece >= 56) //Promotion
                {
                    pawnMoves.Add(new Move(piece - 8, piece, MoveType.QueenPromotion));
                    pawnMoves.Add(new Move(piece - 8, piece, MoveType.RookPromotion));
                    pawnMoves.Add(new Move(piece - 8, piece, MoveType.BishopPromotion));
                    pawnMoves.Add(new Move(piece - 8, piece, MoveType.KnightPromotion));
                }
                else //Normal
                {
                    pawnMoves.Add(new Move(piece - 8, piece, MoveType.Quiet));
                }
            }
            stopWatch.Stop();
            //UnityEngine.Debug.Log($"New PAWN Generation took {stopWatch.Elapsed} milliseconds");

            //Move forward twice
            pawnMovesBB = (((pawnBB & ~(PINNED_PIECES)) << 16) & EMPTY & (EMPTY << 8) & RANK_FOUR) & (CAPTURE_MASK | PUSH_MASK);
            //Covert bitboard to moves
            for (int square = 0; square < 64; square++)
            {
                //Found move else do nothing
                if (((pawnMovesBB >> square) & 1) == 1)
                {
                    pawnMoves.Add(new Move(square - 16, square, MoveType.EnPassant));
                }
            }

            //Capture left
            pawnMovesBB = ((((pawnBB & ~(PINNED_PIECES) & ~(RANK_EIGHT)) << 9)) & (OPPONENTS_PIECES | ENPASS_TARGET) & ~(H_FILE)) & (CAPTURE_MASK | PUSH_MASK | ENPASS_CAPTURE_MASK);
            //Covert bitboard to moves
            for (int square = 0; square < 64; square++)
            {
                //Found move else do nothing
                if (((pawnMovesBB >> square) & 1) == 1)
                {
                    if (square >= 56) //Promotion
                    {
                        pawnMoves.Add(new Move(square - 9, square, MoveType.QueenCapturePromotion));
                        pawnMoves.Add(new Move(square - 9, square, MoveType.RookCapturePromotion));
                        pawnMoves.Add(new Move(square - 9, square, MoveType.BishopCapturePromotion));
                        pawnMoves.Add(new Move(square - 9, square, MoveType.KnightCapturePromotion));
                    }
                    else if(square == ENPASS_SQUARE) //Found Enpass move
                    {
                        var tempMove = new Move(square - 9, square, MoveType.EnPassantCapture);
                        if (TryMakeEnpassMove(tempMove, bb, true))
                            pawnMoves.Add(tempMove);
                    }
                    else //Normal                       
                    {                                   
                        pawnMoves.Add(new Move(square - 9, square, MoveType.Capture));
                    }
                }
            }

            //Capture Right
            pawnMovesBB = ((((pawnBB & ~(PINNED_PIECES) & ~(RANK_EIGHT)) << 7)) & (OPPONENTS_PIECES | ENPASS_TARGET) & ~(A_FILE)) & (CAPTURE_MASK | PUSH_MASK | ENPASS_CAPTURE_MASK);
            //Covert bitboard to moves
            for (int square = 0; square < 64; square++)
            {
                //Found move else do nothing
                if (((pawnMovesBB >> square) & 1) == 1)
                {
                    if (square >= 56) //Promotion
                    {
                        pawnMoves.Add(new Move(square - 7, square, MoveType.QueenCapturePromotion));
                        pawnMoves.Add(new Move(square - 7, square, MoveType.RookCapturePromotion));
                        pawnMoves.Add(new Move(square - 7, square, MoveType.BishopCapturePromotion));
                        pawnMoves.Add(new Move(square - 7, square, MoveType.KnightCapturePromotion));
                    }
                    else if (square == ENPASS_SQUARE) //Found Enpass move
                    {
                        var tempMove = new Move(square - 7, square, MoveType.EnPassantCapture);
                        if (TryMakeEnpassMove(tempMove, bb, true))
                            pawnMoves.Add(tempMove);
                    }
                    else //Normal                       
                    {
                        pawnMoves.Add(new Move(square - 7, square, MoveType.Capture));
                    }
                }
            }

            //Generate Pinned Pawn moves seperate
            ulong pinnedPawnsBB = (pawnBB & PINNED_PIECES);
            if (pinnedPawnsBB != 0)
            {
                //UnityEngine.Debug.Log($"Generating Pinned pawn moves for: {pinnedPawnsBB}");
                pawnMoves.AddRange(GeneratePinnedPawnMoves(bb, pinnedPawnsBB, true));
            }

            return pawnMoves;
        }

        private List<Move> GenerateBlackPawnMoves(Bitboard bb)
        {
            var pawnMoves = new List<Move>();
            ulong pawnBB = bb.blackPawns;
            ulong pawnMovesBB = 0UL;

            //Move forward one
            pawnMovesBB = ((((pawnBB & ~(PINNED_PIECES) & ~(RANK_ONE)) >> 8) & EMPTY)) & (CAPTURE_MASK | PUSH_MASK);
            //Covert bitboard to moves TODO: make an array of pieces for quicker lookup of piece - Logic chess did something with rooks that worked well
            for (int square = 0; square < 64; square++)
            {
                //Found move else do nothing
                if (((pawnMovesBB >> square) & 1) == 1)
                {
                    if (square <= 7) //Promotion
                    {
                        pawnMoves.Add(new Move(square + 8, square, MoveType.QueenPromotion));
                        pawnMoves.Add(new Move(square + 8, square, MoveType.RookPromotion));
                        pawnMoves.Add(new Move(square + 8, square, MoveType.BishopPromotion));
                        pawnMoves.Add(new Move(square + 8, square, MoveType.KnightPromotion));
                    }
                    else //Normal                       
                    {
                        pawnMoves.Add(new Move(square + 8, square, MoveType.Quiet));
                    }
                }
            }

            //Move forward twice
            pawnMovesBB = (((pawnBB & ~(PINNED_PIECES)) >> 16) & EMPTY & (EMPTY >> 8) & RANK_FIVE) & (CAPTURE_MASK | PUSH_MASK);
            //Covert bitboard to moves
            for (int square = 0; square < 64; square++)
            {
                //Found move else do nothing
                if (((pawnMovesBB >> square) & 1) == 1)
                {
                    pawnMoves.Add(new Move(square + 16, square, MoveType.EnPassant)); //Trigger enpass created
                }
            }

            //Capture left
            pawnMovesBB = ((((pawnBB & ~(PINNED_PIECES) & ~(RANK_ONE)) >> 9)) & (OPPONENTS_PIECES | ENPASS_TARGET) & ~(A_FILE)) & (CAPTURE_MASK | PUSH_MASK | ENPASS_CAPTURE_MASK);
            //Covert bitboard to moves
            for (int square = 0; square < 64; square++)
            {
                //Found move else do nothing
                if (((pawnMovesBB >> square) & 1) == 1)
                {
                    if (square <= 7) //Promotion
                    {
                        pawnMoves.Add(new Move(square + 9, square, MoveType.QueenCapturePromotion));
                        pawnMoves.Add(new Move(square + 9, square, MoveType.RookCapturePromotion));
                        pawnMoves.Add(new Move(square + 9, square, MoveType.BishopCapturePromotion));
                        pawnMoves.Add(new Move(square + 9, square, MoveType.KnightCapturePromotion));
                    }
                    else if (square == ENPASS_SQUARE) //Found Enpass move
                    {
                        var tempMove = new Move(square + 9, square, MoveType.EnPassantCapture);
                        if (TryMakeEnpassMove(tempMove, bb, false))
                            pawnMoves.Add(tempMove);
                    }
                    else //Normal                       
                    {
                        pawnMoves.Add(new Move(square + 9, square, MoveType.Capture));
                    }
                }
            }

            //Capture Right
            pawnMovesBB = ((((pawnBB & ~(PINNED_PIECES) & ~(RANK_ONE)) >> 7)) & (OPPONENTS_PIECES | ENPASS_TARGET) & ~(H_FILE)) & (CAPTURE_MASK | PUSH_MASK | ENPASS_CAPTURE_MASK);
            //Covert bitboard to moves
            for (int square = 0; square < 64; square++)
            {
                //Found move else do nothing
                if (((pawnMovesBB >> square) & 1) == 1)
                {
                    if (square <= 7) //Promotion
                    {
                        pawnMoves.Add(new Move(square + 7, square, MoveType.QueenCapturePromotion));
                        pawnMoves.Add(new Move(square + 7, square, MoveType.RookCapturePromotion));
                        pawnMoves.Add(new Move(square + 7, square, MoveType.BishopCapturePromotion));
                        pawnMoves.Add(new Move(square + 7, square, MoveType.KnightCapturePromotion));
                    }
                    else if (square == ENPASS_SQUARE) //Found Enpass move
                    {
                        var tempMove = new Move(square + 7, square, MoveType.EnPassantCapture);
                        if (TryMakeEnpassMove(tempMove, bb, false))
                            pawnMoves.Add(tempMove);
                    }
                    else //Normal                       
                    {
                        pawnMoves.Add(new Move(square + 7, square, MoveType.Capture));
                    }
                }
            }

            //Generate Pinned Pawn moves seperate
            ulong pinnedPawnsBB = (pawnBB & PINNED_PIECES);
            if (pinnedPawnsBB != 0)
                pawnMoves.AddRange(GeneratePinnedPawnMoves(bb, pinnedPawnsBB, false));

            return pawnMoves;
        }

        private List<Move> GenerateKnightMoves(ulong knightBB, bool isWhiteMove)
        {
            ulong knightMovesBB;
            var knightMoves = new List<Move>();

            for (int square = 0; square < 64; square++)
            {
                if(((knightBB >> square) & 1) == 1)
                {
                    if (square > 18)
                    {
                        knightMovesBB = KNIGHT_SPAN << (square - 18);
                    }
                    else
                    {
                        knightMovesBB = KNIGHT_SPAN >> (18 - square);
                    }

                    if (square % 8 < 4)
                    {
                        knightMovesBB &= ~(A_FILE | B_FILE) & NOT_MY_PIECES;
                    }
                    else
                    {
                        knightMovesBB &= ~(G_FILE | H_FILE) & NOT_MY_PIECES;
                    }
                    knightMovesBB &= (CAPTURE_MASK | PUSH_MASK);

                    //Handle pinned pieces before creating moves (can we speed this up to check if any knight are even pinned? if(knightBB & PINNED_PIECES) == 1
                    if (isPinned(square))
                        knightMovesBB &= GeneratePinMask(MY_KING, square, isWhiteMove);

                    for (int i = 0; i < 64; i++)
                    {
                        if (((knightMovesBB >> i) & 1) == 1)
                        {
                            if (isCapture(i))
                                knightMoves.Add(new Move(square, i, MoveType.Capture));
                            else
                                knightMoves.Add(new Move(square, i, MoveType.Quiet));
                        }
                    }
                }
            }

            return knightMoves;
        }

        private List<Move> GenerateBishopMoves(ulong bishopBB, bool isWhiteMove)
        {
            ulong bishopMovesBB;
            var bishopMoves = new List<Move>();

            for (int square = 0; square < 64; square++)
            {
                if (((bishopBB >> square) & 1) == 1)
                {
                    bishopMovesBB = DiagAntiMoves(square) & NOT_MY_PIECES & (CAPTURE_MASK | PUSH_MASK);

                    //Handle pinned pieces before generating moves
                    if (isPinned(square))
                        bishopMovesBB &= GeneratePinMask(MY_KING, square, isWhiteMove);

                    for (int i = 0; i < 64; i++)
                    {
                        if (((bishopMovesBB >> i) & 1) == 1)
                        {
                            if (isCapture(i))
                                bishopMoves.Add(new Move(square, i, MoveType.Capture));
                            else
                                bishopMoves.Add(new Move(square, i, MoveType.Quiet));
                        }
                    }
                }
            }

            return bishopMoves;
        }

        private List<Move> GenerateRookMoves(ulong rookBB, bool isWhiteMove)
        {
            ulong rookMovesBB;
            var rookMoves = new List<Move>();

            for (int square = 0; square < 64; square++)
            {
                if (((rookBB >> square) & 1) == 1)
                {
                    rookMovesBB = HorVerMoves(square) & NOT_MY_PIECES & (CAPTURE_MASK | PUSH_MASK);

                    //Handle pinned pieces before generating moves
                    if (isPinned(square))
                        rookMovesBB &= GeneratePinMask(MY_KING, square, isWhiteMove);

                    for (int i = 0; i < 64; i++)
                    {
                        if (((rookMovesBB >> i) & 1) == 1)
                        {
                            if (isCapture(i))
                                rookMoves.Add(new Move(square, i, MoveType.Capture));
                            else
                                rookMoves.Add(new Move(square, i, MoveType.Quiet));
                        }
                    }
                }
            }

            return rookMoves;
        }

        private List<Move> GenerateQueenMoves(ulong queenBB, bool isWhiteMove)
        {
            ulong queenMovesBB;
            var queenMoves = new List<Move>();

            for (int square = 0; square < 64; square++)
            {
                if (((queenBB >> square) & 1) == 1)
                {
                    queenMovesBB = (HorVerMoves(square) | DiagAntiMoves(square)) & NOT_MY_PIECES & (CAPTURE_MASK | PUSH_MASK);

                    //Handle pinned pieces before generating moves
                    if (isPinned(square))
                        queenMovesBB &= GeneratePinMask(MY_KING, square, isWhiteMove);

                    for (int i = 0; i < 64; i++)
                    {
                        if (((queenMovesBB >> i) & 1) == 1)
                        {
                            if (isCapture(i))
                                queenMoves.Add(new Move(square, i, MoveType.Capture));
                            else
                                queenMoves.Add(new Move(square, i, MoveType.Quiet));
                        }
                    }
                }
            }
            return queenMoves;
        }

        private List<Move> GenerateKingMoves(ulong kingBB, bool isWhiteMove)
        {
            ulong kingMovesBB;
            var kingMoves = new List<Move>();

            for (int square = 0; square < 64; square++)
            {
                if (((kingBB >> square) & 1) == 1)
                {
                    if (square > 9)
                    {
                        kingMovesBB = KING_SPAN << (square - 9);
                    }
                    else
                    {
                        kingMovesBB = KING_SPAN >> (9 - square);
                    }

                    if (square % 8 < 4)
                    {
                        kingMovesBB &= ~(A_FILE | B_FILE) & NOT_MY_PIECES;
                    }
                    else
                    {
                        kingMovesBB &= ~(G_FILE | H_FILE) & NOT_MY_PIECES;
                    }

                    //Remove any generated king moves that land on opponent attacks!
                    kingMovesBB &= ~OPPONENT_ATTACKS;

                    for (int i = 0; i < 64; i++)
                    {
                        if (((kingMovesBB >> i) & 1) == 1)
                        {
                            if (isCapture(i))
                                kingMoves.Add(new Move(square, i, MoveType.Capture));
                            else
                                kingMoves.Add(new Move(square, i, MoveType.Quiet));
                        }
                    }
                }
            }

            return kingMoves;
        }

        //TODO: Can still castle if king and rook move and then go back too original spots
        private List<Move> GenerateCastleMoves(Bitboard bb)
        {
            List<Move> moves = new List<Move>();
            if (CHECKERS != 0) //Cant castle in check
                return moves;

            if (bb.isWhiteMove)
            {
                if (bb.whiteKingCastleRights && (CASTLE_LANES[1] & ~(EMPTY)) == 0 && (CASTLE_LANES[1] & OPPONENT_ATTACKS) == 0) //Has king side castle rights, lanes empty and are not being attacked
                {
                    //King moves to target square and rook moves to starting square
                    moves.Add(new Move(3, 1, MoveType.WhiteKingSideCastle));   
                }
                if (bb.whiteQueenCastleRights && (CASTLE_LANES[0] & ~(EMPTY)) == 0 && (CASTLE_LANES[0] & OPPONENT_ATTACKS) == 0) //Has queen side castle rights, lanes empty and are not being attacked
                {
                    //King moves to target square and rook moves to starting square
                    moves.Add(new Move(3, 5, MoveType.WhiteQueenSideCastle));
                }
            }
            else
            {
                if (bb.blackKingCastleRights && (CASTLE_LANES[3] & ~(EMPTY)) == 0 && (CASTLE_LANES[3] & OPPONENT_ATTACKS) == 0) //Has king side castle rights, lanes empty and are not being attacked
                {
                    //King moves to target square and rook moves to starting square
                    moves.Add(new Move(59, 57, MoveType.BlackKingSideCastle));
                }
                if (bb.blackQueenCastleRights && (CASTLE_LANES[2] & ~(EMPTY)) == 0 && (CASTLE_LANES[2] & OPPONENT_ATTACKS) == 0) //Has queen side castle rights, lanes empty and are not being attacked
                {
                    //King moves to target square and rook moves to starting square
                    moves.Add(new Move(59, 61, MoveType.BlackQueenSideCastle));
                }
            }

            return moves;
        }

        private ulong DiagAntiMoves(int square)
        {
            ulong bSquare = 1UL << square;
            int diagnalIndex = (square / 8) + (square % 8);
            int antiDiagnalIndex = (square / 8) + 7 - (square % 8);

            //Very confusing, somewhat understand how this works... :(
            ulong possibleDiagnal = ((OCCUPIED & DiagnalMask[diagnalIndex]) - 2 * bSquare) ^ (ReverseBits(ReverseBits(OCCUPIED & DiagnalMask[diagnalIndex]) - (2 * ReverseBits(bSquare))));
            ulong possibleAntiDiagnal = ((OCCUPIED & AntiDiagnalMask[antiDiagnalIndex]) - (2 * bSquare)) ^ (ReverseBits(ReverseBits(OCCUPIED & AntiDiagnalMask[antiDiagnalIndex]) - (2 * ReverseBits(bSquare))));

            return (possibleDiagnal & DiagnalMask[diagnalIndex]) | (possibleAntiDiagnal & AntiDiagnalMask[antiDiagnalIndex]);
        }

        private ulong HorVerMoves(int square)
        {
            ulong bSquare = 1UL << square;
            int fileIndex = (8 - (square % 8)) - 1;
            int rankIndex = square / 8;

            //Very confusing, somewhat understand how this works... :(
            ulong possibleHorizontal = (OCCUPIED - 2 * bSquare) ^ (ReverseBits(ReverseBits(OCCUPIED) - (2 * ReverseBits(bSquare))));
            ulong possibleVertical = ((OCCUPIED & FileMask8[fileIndex]) - (2 * bSquare)) ^ (ReverseBits(ReverseBits(OCCUPIED & FileMask8[fileIndex]) - (2 * ReverseBits(bSquare))));

            return (possibleHorizontal & RankMasks8[rankIndex]) | (possibleVertical & FileMask8[fileIndex]);
        }

        #endregion

        #region Attack Vector Functions
        //These set of functions calculate the attack vectors of specific pieces, this can be used to generate
        //attacks from the kings position which will in turn find the checkers position. Can also be used to generate push and capture mask
        //and to figure out which squares are safe for the king

        private ulong GenerateOpponentAttackBB(Bitboard bb, bool isWhiteMove)
        {
            //Local variables to set opponent bitboards
            ulong kBB, qBB, rBB, bBB, nBB, pBB, occupiedWithoutKing;
            if (isWhiteMove)
            {
                kBB = bb.blackKing;
                qBB = bb.blackQueens;
                rBB = bb.blackRooks;
                bBB = bb.blackBishops;
                nBB = bb.blackKnights;
                pBB = bb.blackPawns;
                occupiedWithoutKing = OCCUPIED & ~(bb.whiteKing);
            }
            else
            {
                kBB = bb.whiteKing;
                qBB = bb.whiteQueens;
                rBB = bb.whiteRooks;
                bBB = bb.whiteBishops;
                nBB = bb.whiteKnights;
                pBB = bb.whitePawns;
                occupiedWithoutKing = OCCUPIED & ~(bb.blackKing);
            }

            ulong allAttacks = 0UL;

            //Flipped isWhiteMove to generate opponents pawn moves
            allAttacks |= PawnAttacks(pBB, !isWhiteMove);
            for (int square = 0; square < 64; square++)
            {
                if (((kBB >> square) & 1) == 1) allAttacks |= KingAttacks(square);
                if (((qBB >> square) & 1) == 1) allAttacks |= QueenAttacks(square, occupiedWithoutKing);
                if (((rBB >> square) & 1) == 1) allAttacks |= RookAttacks(square, occupiedWithoutKing);
                if (((bBB >> square) & 1) == 1) allAttacks |= BishopAttacks(square, occupiedWithoutKing);
                if (((nBB >> square) & 1) == 1) allAttacks |= KnightAttacks(square);
            }

            return allAttacks;
        }

        private ulong GeneratePushMask(int checker)
        {
            if ((HorAttacks(KING_SQUARE, OCCUPIED) & HorAttacks(checker, OCCUPIED)) != 0)
            {
                return HorAttacks(KING_SQUARE, OCCUPIED) & HorAttacks(checker, OCCUPIED);
            }
            else if ((VerAttacks(KING_SQUARE, OCCUPIED) & VerAttacks(checker, OCCUPIED)) != 0)
            {
                return VerAttacks(KING_SQUARE, OCCUPIED) & VerAttacks(checker, OCCUPIED);
            }
            else if ((DiagAttacks(KING_SQUARE, OCCUPIED) & DiagAttacks(checker, OCCUPIED)) != 0)
            {
                return DiagAttacks(KING_SQUARE, OCCUPIED) & DiagAttacks(checker, OCCUPIED);
            }
            else if ((AntiDiagAttacks(KING_SQUARE, OCCUPIED) & AntiDiagAttacks(checker, OCCUPIED)) != 0)
            {
                return AntiDiagAttacks(KING_SQUARE, OCCUPIED) & AntiDiagAttacks(checker, OCCUPIED);
            }
            else
            {
                return 0UL;
            }
        }

        private ulong PawnAttacks(ulong pawnBB, bool generateWhite) 
        {
            ulong pawnAttacksBB = 0UL;

            if(generateWhite)
            {
                //Capture left
                pawnAttacksBB = ((((pawnBB & ~(RANK_EIGHT)) << 9)) & ~(H_FILE));
                //Capture Right
                pawnAttacksBB |= ((((pawnBB & ~(RANK_EIGHT)) << 7)) & ~(A_FILE));
            }
            else
            {
                //Capture left
                pawnAttacksBB = ((((pawnBB & ~(RANK_ONE)) >> 9)) & ~(A_FILE));
                //Capture Right
                pawnAttacksBB |= ((((pawnBB & ~(RANK_ONE)) >> 7)) & ~(H_FILE));
            }

            return pawnAttacksBB;
        }

        private ulong KnightAttacks(int square)
        {
            ulong knightAttacksBB = 0UL;
            
            if (square > 18)
            {
                knightAttacksBB = KNIGHT_SPAN << (square - 18);
            }
            else
            {
                knightAttacksBB = KNIGHT_SPAN >> (18 - square);
            }

            if (square % 8 < 4)
            {
                knightAttacksBB &= ~(A_FILE | B_FILE) & NOT_MY_PIECES;
            }
            else
            {
                knightAttacksBB &= ~(G_FILE | H_FILE) & NOT_MY_PIECES;
            }

            return knightAttacksBB;
        }

        private ulong BishopAttacks(int square, ulong occ)
        {
            return DiagAttacks(square, occ) | AntiDiagAttacks(square, occ);
        }

        private ulong RookAttacks(int square, ulong occ)
        {
            return HorAttacks(square, occ) | VerAttacks(square, occ);
        }

        private ulong QueenAttacks(int square, ulong occ)
        {
            return HorAttacks(square, occ) | VerAttacks(square, occ) | DiagAttacks(square, occ) | AntiDiagAttacks(square, occ);
        }

        private ulong KingAttacks(int square)
        {
            ulong kingAttacksBB = 0UL;

            if (square > 9)
            {
                kingAttacksBB = KING_SPAN << (square - 9);
            }
            else
            {
                kingAttacksBB = KING_SPAN >> (9 - square);
            }

            if (square % 8 < 4)
            {
                kingAttacksBB &= ~(A_FILE | B_FILE) & NOT_MY_PIECES;
            }
            else
            {
                kingAttacksBB &= ~(G_FILE | H_FILE) & NOT_MY_PIECES;
            }

            return kingAttacksBB;
        }


        //These Attack calc methods are the same as the move generation versions used above, except they can use a special version of the OCCUPIED bitboard which can include or not include king BB
        //We need these so that we can calculate unsafe squares for the king moves so we have attack vectors that see through the king (ex. [Q---->kxxxx] vs [Q-----k--->] the squares behind the king are unsafe too)
        ulong HorAttacks(int square, ulong occ)
        {
            ulong bbSquare = 1UL << square;
            int rankIndex = square / 8;

            ulong possibleHorizontal = (occ - 2 * bbSquare) ^ (ReverseBits(ReverseBits(occ) - (2 * ReverseBits(bbSquare))));

            return (possibleHorizontal & RankMasks8[rankIndex]);
        }

        ulong VerAttacks(int square, ulong occ)
        {
            ulong bbSquare = 1UL << square;
            int fileIndex = (8 - (square % 8)) - 1;


            ulong possibleVertical = ((occ & FileMask8[fileIndex]) - (2 * bbSquare)) ^ (ReverseBits(ReverseBits(occ & FileMask8[fileIndex]) - (2 * ReverseBits(bbSquare))));
            return (possibleVertical & FileMask8[fileIndex]);
        }

        ulong DiagAttacks(int square, ulong occ)
        {
            ulong bbSquare = 1UL << square;
            int diagnalIndex = (square / 8) + (square % 8);

            ulong possibleDiagnal = ((occ & DiagnalMask[diagnalIndex]) - 2 * bbSquare) ^ (ReverseBits(ReverseBits(occ & DiagnalMask[diagnalIndex]) - (2 * ReverseBits(bbSquare))));

            return (possibleDiagnal & DiagnalMask[diagnalIndex]);
        }

        ulong AntiDiagAttacks(int square, ulong occ)
        {
            ulong bbSquare = 1UL << square;
            int antiDiagnalIndex = (square / 8) + 7 - (square % 8);

            ulong possibleAntiDiagnal = ((occ & AntiDiagnalMask[antiDiagnalIndex]) - (2 * bbSquare)) ^ (ReverseBits(ReverseBits(occ & AntiDiagnalMask[antiDiagnalIndex]) - (2 * ReverseBits(bbSquare))));

            return (possibleAntiDiagnal & AntiDiagnalMask[antiDiagnalIndex]);
        }

        #endregion

        #region Pin Functions
        private bool isPinned(int square)
        {
            if (((PINNED_PIECES >> square) & 1) == 1) return true;
            else return false;
        }

        private ulong FindPinnedPieces(Bitboard bb, bool isWhiteMove)
        {
            ulong pinnedPiecesBB = 0UL;
            ulong k;
            ulong p, n, b, r, q;

            if (isWhiteMove)
            {
                k = bb.whiteKing; p = bb.blackPawns; n = bb.blackKnights; b = bb.blackBishops; r = bb.blackRooks; q = bb.blackQueens;
            }
            else
            {
                k = bb.blackKing; p = bb.whitePawns; n = bb.whiteKnights; b = bb.whiteBishops; r = bb.whiteRooks; q = bb.whiteQueens;
            }

            ulong qbBB = q | b;
            ulong qrBB = q | r;
            for (int square = 0; square < 64; square++)
            {
                if (((qbBB >> square) & 1) == 1)
                {
                    pinnedPiecesBB |= DiagAttacks(KING_SQUARE, OCCUPIED) & (DiagAttacks(square, OCCUPIED) & ~(OPPONENTS_PIECES));
                    pinnedPiecesBB |= AntiDiagAttacks(KING_SQUARE, OCCUPIED) & (AntiDiagAttacks(square, OCCUPIED) & ~(OPPONENTS_PIECES));
                }

                if (((qrBB >> square) & 1) == 1)
                {
                    pinnedPiecesBB |= HorAttacks(KING_SQUARE, OCCUPIED) & (HorAttacks(square, OCCUPIED) & ~(OPPONENTS_PIECES));
                    pinnedPiecesBB |= VerAttacks(KING_SQUARE, OCCUPIED) & (VerAttacks(square, OCCUPIED) & ~(OPPONENTS_PIECES));
                }
            }
            pinnedPiecesBB &= ~EMPTY;
            return pinnedPiecesBB;
        }

        private ulong GeneratePinMask(ulong kingBB, int square, bool isWhiteMove)
        {
            if ((kingBB & HorAttacks(square, OCCUPIED)) != 0)
            {
                return HorAttacks(square, OCCUPIED) & NOT_MY_PIECES;
            }
            else if ((kingBB & VerAttacks(square, OCCUPIED)) != 0)
            {
                return VerAttacks(square, OCCUPIED) & NOT_MY_PIECES;
            }
            else if ((kingBB & DiagAttacks(square, OCCUPIED)) != 0)
            {
                return DiagAttacks(square, OCCUPIED) & NOT_MY_PIECES;
            }
            else if ((kingBB & AntiDiagAttacks(square, OCCUPIED)) != 0)
            {
                return AntiDiagAttacks(square, OCCUPIED) & NOT_MY_PIECES;
            }

            return 0;//Not sure if this should be 0 yet
        }

        private List<Move> GeneratePinnedPawnMoves(Bitboard bb, ulong pinnedPawnsBB, bool isWhiteMove)
        {
            ulong pinnedPawnMovesBB = 0UL;
            List<Move> pinnedPawnMoves = new List<Move>();

            for (int square = 0; square < 64; square++)
            {
                if (((pinnedPawnsBB >> square) & 1) == 1)
                {
                    ulong curPawn = 1UL << square;
                    ulong pinMask = GeneratePinMask(MY_KING, square, isWhiteMove);

                    if (isWhiteMove)
                    {
                        pinnedPawnMovesBB = (curPawn << 9) & (OPPONENTS_PIECES | ENPASS_TARGET) & ~(RANK_EIGHT) & ~(H_FILE) & pinMask & (CAPTURE_MASK | PUSH_MASK | ENPASS_CAPTURE_MASK); //Capture left
                        pinnedPawnMovesBB |= (curPawn << 7) & (OPPONENTS_PIECES | ENPASS_TARGET) & ~(RANK_EIGHT) & ~(A_FILE) & pinMask & (CAPTURE_MASK | PUSH_MASK | ENPASS_CAPTURE_MASK); //Capture right
                        pinnedPawnMovesBB |= (curPawn << 8) & EMPTY & ~(RANK_EIGHT) & pinMask & (CAPTURE_MASK | PUSH_MASK);                    //Move one forward
                        pinnedPawnMovesBB |= (curPawn << 16) & EMPTY & (EMPTY << 8) & (RANK_FOUR) & pinMask & (CAPTURE_MASK | PUSH_MASK);     //Move two forward
                        pinnedPawnMovesBB |= (curPawn << 9) & OPPONENTS_PIECES & (RANK_EIGHT) & ~(H_FILE) & pinMask & (CAPTURE_MASK | PUSH_MASK);  //Capture left Promo
                        pinnedPawnMovesBB |= (curPawn << 7) & OPPONENTS_PIECES & (RANK_EIGHT) & ~(A_FILE) & pinMask & (CAPTURE_MASK | PUSH_MASK);  //Capture right Promo
                        pinnedPawnMovesBB |= (curPawn << 8) & EMPTY & (RANK_EIGHT) & pinMask & (CAPTURE_MASK | PUSH_MASK);						//move forward Promo

                        //Generate list of moves
                        for (int i = 0; i < 64; i++)
                        {
                            if (((pinnedPawnMovesBB >> i) & 1) == 1)
                            {
                                //Pinned pawn promotion (think this is always a capture promotion)
                                if(i >= 56)
                                {
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.QueenCapturePromotion));
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.RookCapturePromotion));
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.BishopCapturePromotion));
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.KnightCapturePromotion));
                                }
                                else if(i == ENPASS_SQUARE)
                                {
                                    var tempMove = new Move(square, i, MoveType.EnPassantCapture);
                                    if (TryMakeEnpassMove(tempMove, bb, true))
                                        pinnedPawnMoves.Add(tempMove);
                                }
                                else if (isCapture(i))
                                {
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.Capture));
                                }
                                else
                                {
                                    if((i - square) == 16) //Move 2 forward
                                        pinnedPawnMoves.Add(new Move(square, i, MoveType.EnPassant));
                                    else // Move 1 forward         
                                        pinnedPawnMoves.Add(new Move(square, i, MoveType.Quiet));
                                }
                            }
                        }
                    }
                    else
                    {
                        pinnedPawnMovesBB = (curPawn >> 9) & (OPPONENTS_PIECES | ENPASS_TARGET) & ~(RANK_ONE) & ~(A_FILE) & pinMask & (CAPTURE_MASK | PUSH_MASK | ENPASS_CAPTURE_MASK); //Capture left
                        pinnedPawnMovesBB |= (curPawn >> 7) & (OPPONENTS_PIECES | ENPASS_TARGET) & ~(RANK_ONE) & ~(H_FILE) & pinMask & (CAPTURE_MASK | PUSH_MASK | ENPASS_CAPTURE_MASK); //Capture right
                        pinnedPawnMovesBB |= (curPawn >> 8) & EMPTY & ~(RANK_ONE) & pinMask & (CAPTURE_MASK | PUSH_MASK);                    //Move one forward
                        pinnedPawnMovesBB |= (curPawn >> 16) & EMPTY & (EMPTY >> 8) & (RANK_FIVE) & pinMask & (CAPTURE_MASK | PUSH_MASK);     //Move two forward
                        pinnedPawnMovesBB |= (curPawn >> 9) & OPPONENTS_PIECES & (RANK_ONE) & ~(A_FILE) & pinMask & (CAPTURE_MASK | PUSH_MASK);  //Capture left Promo
                        pinnedPawnMovesBB |= (curPawn >> 7) & OPPONENTS_PIECES & (RANK_ONE) & ~(H_FILE) & pinMask & (CAPTURE_MASK | PUSH_MASK);  //Capture right Promo
                        pinnedPawnMovesBB |= (curPawn >> 8) & EMPTY & (RANK_ONE) & pinMask & (CAPTURE_MASK | PUSH_MASK);						//move forward Promo

                        //Generate list of moves
                        for (int i = 0; i < 64; i++)
                        {
                            if (((pinnedPawnMovesBB >> i) & 1) == 1)
                            {
                                //Pinned pawn promotion (think this is always a capture promotion)
                                if (i <= 7)
                                {
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.QueenCapturePromotion));
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.RookCapturePromotion));
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.BishopCapturePromotion));
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.KnightCapturePromotion));
                                }
                                else if (i == ENPASS_SQUARE)
                                {
                                    var tempMove = new Move(square, i, MoveType.EnPassantCapture);
                                    if (TryMakeEnpassMove(tempMove, bb, false))
                                        pinnedPawnMoves.Add(tempMove);
                                }
                                else if (isCapture(i))
                                {
                                    pinnedPawnMoves.Add(new Move(square, i, MoveType.Capture));
                                }
                                else
                                {
                                    if((square - i) == 16) //Move 2 forward
                                        pinnedPawnMoves.Add(new Move(square, i, MoveType.EnPassant));
                                    else //Move 1 forward
                                        pinnedPawnMoves.Add(new Move(square, i, MoveType.Quiet));
                                }
                            }
                        }
                    }
                }
            }

            return pinnedPawnMoves;
        }

        #endregion

        #region Helper Functions

        //TODO: Test speed of this - converting tostring and reversing could  be faster (lookup table is an option if this is too slow)
        //Not to self: I had a bug in my implementation of reverseBits(), and chatGPT pointed out my bug and then proceded to write
        //its own implementation without the bug vvvvv :)
        //In my old implementation it only worked for ulongs that were 64 bits in size, anything smaller would break
        private ulong ReverseBits(ulong x)
        {
            x = ((x >> 1) & 0x5555555555555555UL) | ((x & 0x5555555555555555UL) << 1);
            x = ((x >> 2) & 0x3333333333333333UL) | ((x & 0x3333333333333333UL) << 2);
            x = ((x >> 4) & 0x0F0F0F0F0F0F0F0FUL) | ((x & 0x0F0F0F0F0F0F0F0FUL) << 4);
            x = ((x >> 8) & 0x00FF00FF00FF00FFUL) | ((x & 0x00FF00FF00FF00FFUL) << 8);
            x = ((x >> 16) & 0x0000FFFF0000FFFFUL) | ((x & 0x0000FFFF0000FFFFUL) << 16);
            x = ((x >> 32) & 0x00000000FFFFFFFFUL) | ((x & 0x00000000FFFFFFFFUL) << 32);

            return x;
        }

        private bool isCapture(int square)
        {
            if (((OPPONENTS_PIECES >> square) & 1) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private ulong isChecked(Bitboard bb, ulong occ, bool isWhiteMove)
        {
            ulong checkersBB = 0UL;
            ulong k;
            ulong p, n, b, r, q;
            if (isWhiteMove)
            {
                k = bb.whiteKing; p = bb.blackPawns; n = bb.blackKnights; b = bb.blackBishops; r = bb.blackRooks; q = bb.blackQueens;
            }
            else
            {
                k = bb.blackKing; p = bb.whitePawns; n = bb.whiteKnights; b = bb.whiteBishops; r = bb.whiteRooks; q = bb.whiteQueens;
            }

            checkersBB |= (PawnAttacks(k, isWhiteMove) & NOT_MY_PIECES) & p;
            checkersBB |= (KnightAttacks(KING_SQUARE) & NOT_MY_PIECES) & n;
            checkersBB |= (BishopAttacks(KING_SQUARE, occ) & NOT_MY_PIECES) & b;
            checkersBB |= (RookAttacks(KING_SQUARE, occ) & NOT_MY_PIECES) & r;
            checkersBB |= (QueenAttacks(KING_SQUARE, occ) & NOT_MY_PIECES) & q;

            return checkersBB;
        }

        private bool TryMakeEnpassMove(Move enpassMove, Bitboard bb, bool isWhiteMove)
        {
            if (enpassMove.type != MoveType.EnPassantCapture)
            {
                UnityEngine.Debug.LogError($"Attempted to call 'TryMakeEnpassMove' with a move that isn't of type {MoveType.EnPassantCapture}!");
                return false;
            }

            ulong pawnsBB, opponentPawnsBB;
            pawnsBB = isWhiteMove ? bb.whitePawns : bb.blackPawns;
            opponentPawnsBB = isWhiteMove ? bb.blackPawns : bb.whitePawns;

            //make the move
            if(((pawnsBB >> enpassMove.startingSquare) & 1) == 1)
            {
                pawnsBB &= ~(1UL << enpassMove.startingSquare);
                pawnsBB |= (1UL << enpassMove.targetSquare);
                
                opponentPawnsBB &= ~(1UL << (enpassMove.targetSquare + (isWhiteMove ? -8 : 8)));

                bb.whitePawns = isWhiteMove ? pawnsBB : opponentPawnsBB;
                bb.blackPawns = isWhiteMove ? opponentPawnsBB : pawnsBB;

                //need to update occupied with the latest move
                ulong occupied = (bb.whiteKing | bb.whiteQueens | bb.whiteRooks | bb.whiteBishops | bb.whiteKnights | bb.whitePawns | bb.blackKing | bb.blackQueens | bb.blackRooks | bb.blackBishops | bb.blackKnights | bb.blackPawns);
                if (isChecked(bb, occupied, isWhiteMove) == 0)
                    return true; //Legal enpass found
                else
                    return false;
            }
            else
            {
                UnityEngine.Debug.LogError($"Attempted to call 'TryMakeEnpassMove' with an INVALID move! PAWNSBB: {pawnsBB} start: {enpassMove.startingSquare}");
                return false;
            }
        }

        private void Initialize(Bitboard bb, bool isWhiteMove)
        {
            NOT_MY_PIECES = isWhiteMove ? ~(bb.whiteKing | bb.whiteQueens | bb.whiteRooks | bb.whiteBishops | bb.whiteKnights | bb.whitePawns) : ~(bb.blackKing | bb.blackQueens | bb.blackRooks | bb.blackBishops | bb.blackKnights | bb.blackPawns);
            EMPTY = ~(bb.whiteKing | bb.whiteQueens | bb.whiteRooks | bb.whiteBishops | bb.whiteKnights | bb.whitePawns | bb.blackKing | bb.blackQueens | bb.blackRooks | bb.blackBishops | bb.blackKnights | bb.blackPawns);
            OCCUPIED = (bb.whiteKing | bb.whiteQueens | bb.whiteRooks | bb.whiteBishops | bb.whiteKnights | bb.whitePawns | bb.blackKing | bb.blackQueens | bb.blackRooks | bb.blackBishops | bb.blackKnights | bb.blackPawns);
            OPPONENTS_PIECES = isWhiteMove ? (bb.blackKing | bb.blackQueens | bb.blackRooks | bb.blackBishops | bb.blackKnights | bb.blackPawns) : (bb.whiteKing | bb.whiteQueens | bb.whiteRooks | bb.whiteBishops | bb.whiteKnights | bb.whitePawns);
            MY_KING = isWhiteMove ? bb.whiteKing : bb.blackKing;
            KING_SQUARE = bb.getKingSquare(isWhiteMove);
            OPPONENT_ATTACKS = GenerateOpponentAttackBB(bb, isWhiteMove);
            PINNED_PIECES = FindPinnedPieces(bb, isWhiteMove);

            //Set Enpass
            ENPASS_SQUARE = bb.enpassTargetSquare;
            ENPASS_TARGET = 0UL;
            if(ENPASS_SQUARE != -1)
                ENPASS_TARGET = 1UL << bb.enpassTargetSquare;

            //Handle check rules
            CHECKERS = isChecked(bb, OCCUPIED, isWhiteMove);
            NUM_CHECKERS = CHECKERS == 0UL ? 0 : Pieces.CountPieces(CHECKERS);
            CAPTURE_MASK = 0xFFFFFFFFFFFFFFFF;
            PUSH_MASK = 0xFFFFFFFFFFFFFFFF;
            if(NUM_CHECKERS == 1) //Single Check
            {
                //Need this just in case the checker is a pawned that can be enpassanted
                if (isWhiteMove ? (CHECKERS << 8) == ENPASS_TARGET : (CHECKERS >> 8) == ENPASS_TARGET)
                    ENPASS_CAPTURE_MASK = ENPASS_TARGET;
                else
                    ENPASS_CAPTURE_MASK = 0UL;
                CAPTURE_MASK = CHECKERS;
                PUSH_MASK = 0UL;
                int checkerSquare = BitboardHelper.GetSqPos(CHECKERS);
                if (BitboardHelper.IsSlider(bb, checkerSquare))
                    PUSH_MASK = GeneratePushMask(checkerSquare);
            }
            else if(NUM_CHECKERS > 1) //Double Check
            {
                CAPTURE_MASK = 0UL;
                PUSH_MASK = 0UL;
                ENPASS_CAPTURE_MASK = 0UL;
            }
        }
        #endregion
    }
}
