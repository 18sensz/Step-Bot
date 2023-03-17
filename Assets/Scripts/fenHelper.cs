using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Chess
{
    public static class fenHelper
    {
        public const string startingFenPostion = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static Dictionary<string, int> squareToIndex = new Dictionary<string, int>()
        {
            {"h1", 0},  {"g1", 1},  {"f1", 2},  {"e1", 3},  {"d1", 4},  {"c1", 5},  {"b1", 6},  {"a1", 7},
            {"h2", 8},  {"g2", 9},  {"f2", 10}, {"e2", 11}, {"d2", 12}, {"c2", 13}, {"b2", 14}, {"a2", 15},
            {"h3", 16}, {"g3", 17}, {"f3", 18}, {"e3", 19}, {"d3", 20}, {"c3", 21}, {"b3", 22}, {"a3", 23},
            {"h4", 24}, {"g4", 25}, {"f4", 26}, {"e4", 27}, {"d4", 28}, {"c4", 29}, {"b4", 30}, {"a4", 31},
            {"h5", 32}, {"g5", 33}, {"f5", 34}, {"e5", 35}, {"d5", 36}, {"c5", 37}, {"b5", 38}, {"a5", 39},
            {"h6", 40}, {"g6", 41}, {"f6", 42}, {"e6", 43}, {"d6", 44}, {"c6", 45}, {"b6", 46}, {"a6", 47},
            {"h7", 48}, {"g7", 49}, {"f7", 50}, {"e7", 51}, {"d7", 52}, {"c7", 53}, {"b7", 54}, {"a7", 55},
            {"h8", 56}, {"g8", 57}, {"f8", 58}, {"e8", 59}, {"d8", 60}, {"c8", 61}, {"b8", 62}, {"a8", 63}
        };

        public static Position getPosition(string fenString = startingFenPostion)
        {
            Position position = new Position();
            var fenSections = fenString.Split(' ');

            //Position parsing
            var positionSection = fenSections[0];
            int offset = 0;
            for (int i = 0; i < positionSection.Length; i++)
            {
                var cur = positionSection[i];
                if (cur == '/')
                {
                    offset--;
                }
                else if (char.IsDigit(cur))
                {
                    offset += int.Parse(cur.ToString()) - 1;
                }
                else
                {
                    position.squares[63 - (i + offset)] = cur;
                }
            }

            //Set player turn
            position.isWhiteMove = fenSections[1][0] == 'w' ? true : false;

            //Set Castle Rights
            position.whiteKingCastleRights = fenSections[2].Contains('K');
            position.whiteQueenCastleRights = fenSections[2].Contains('Q');
            position.blackKingCastleRights = fenSections[2].Contains('k');
            position.blackQueenCastleRights = fenSections[2].Contains('q');

            //Set EnPassant
            position.enpassTargetSquare = fenSections[3] == "-" ? -1 : squareToIndex[fenSections[3]];

            //Set Halfmove clock : The number of halfmoves since the last capture or pawn advance, used for the fifty-move rule.
            //--not implemented yet--

            //Set Fullmove number : The number of the full moves. It starts at 1 and is incremented after Black's move.
            //--not implemented yet--

            return position;
        }
    }
}
