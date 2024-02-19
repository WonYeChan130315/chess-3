using System.Collections.Generic;
using UnityEngine;
using static Game;

public static class MakeMove {
    public static bool IsPossibleMove(Coord targetCoord, Coord[] moves) {
        foreach (Coord move in moves) {
            if (move == targetCoord) {
                return true;
            }
        }

        return false;
    }
    
    public static Coord[] GetMoves(Coord curCoord, bool checkIgnore = false) {
        int piece = Piece.GetPieceType(Board.squares[ToIndex(curCoord)]);

        List<Coord> moves = new List<Coord>();

        switch (piece) {
            case Piece.King:
                moves = Moves.KingMove(curCoord);
                break;
            case Piece.Pawn:
                moves = Moves.PawnMove(curCoord);
                break;
            case Piece.Knight:
                moves = Moves.KnightMove(curCoord);
                break;
            case Piece.Bishop:
                moves = Moves.BishopMove(curCoord);
                break;
            case Piece.Rook:
                moves = Moves.RookMove(curCoord);
                break;
            case Piece.Queen:
                moves = Moves.QueenMove(curCoord);
                break;
        }

        bool moveEmpty = moves == null || moves.Count <= 0;

        if (!moveEmpty) {
            int i = 0;

            while (i < moves.Count) {
                bool isOutBoard = Board.IsOutBoard(moves[i]);

                bool sameColor = !isOutBoard && Piece.GetColor(Board.squares[ToIndex(moves[i])]) == Piece.GetColor(Board.squares[ToIndex(curCoord)]);
                bool notEmpty = !isOutBoard && Board.squares[ToIndex(moves[i])] != Piece.None;
                bool isCheck = !isOutBoard && !checkIgnore && IsCheckMove(curCoord, moves[i]);

                if (isOutBoard || (sameColor && notEmpty) || isCheck) {
                    moves.RemoveAt(i);
                    i--;
                }
                i++;
            }
        }

        return !moveEmpty ? moves.ToArray() : new Coord[] {};
    }

    public static bool IsCheckMove(Coord curCoord, Coord targetCoord) {
        int curPiece = Board.squares[ToIndex(curCoord)];
        int targetPiece = Board.squares[ToIndex(targetCoord)];

        Board.squares[ToIndex(targetCoord)] = curPiece;
        Board.squares[ToIndex(curCoord)] = Piece.None;

        bool isCheck = AttackedSquares.IsCheck(PieceMoveManager.curOrder);

        Board.squares[ToIndex(targetCoord)] = targetPiece;
        Board.squares[ToIndex(curCoord)] = curPiece;

        return isCheck;
    }
}

// [5] [11] cur = 5 tar = 11       target coord = curPiece [11] -> [5] [5] = 11
// 

//  [0] [1] [2] [4] [5] [6] [7] [8]
//  *** *** *** 

// i = 3