using System.Collections.Generic;
using System.Linq;
using static Game;

public static class AttackedSquares {
    public static bool IsCheck(int color) {
        Coord[] attackedSquares = GetAttackedSquares(color);

        foreach (Coord square in attackedSquares) {
            if (Board.squares[ToIndex(square)] == Piece.King + color) {
                return true;
            }
        }

        return false;
    }

    public static bool IsAttackedSquare(Coord coord, int color) {
        Coord[] attackedSquares = GetAttackedSquares(color);

        foreach (Coord square in attackedSquares) {
            if (square == coord) {
                return true;
            }
        }

        return false;
    }

    public static Coord[] GetAttackedSquares(int color) {
        List<Coord> attackedSquares = new List<Coord>();

        for (int file = 0; file < 8; file++) {
            for (int rank = 0; rank < 8; rank++) {
                Coord targetCoord = new Coord(file, rank);
                int piece = Board.squares[ToIndex(targetCoord)];
                
                if (Piece.GetColor(piece) != color && piece != Piece.None) {
                    attackedSquares.AddRange(MakeMove.GetMoves(targetCoord, true));
                }
            }
        }

        return attackedSquares.Distinct().ToArray();
    }
}