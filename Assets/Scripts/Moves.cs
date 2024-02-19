using System;
using System.Collections.Generic;
using System.Linq;
using static Game;

public static class Moves {
    public static List<Coord> KingMove(Coord curCoord) {
        List<Coord> moves = GenerateStepMove(curCoord,
            new (-1, 1), new (0, 1), new (1, 1), new (-1, 0),
            new (1, 0), new (-1, -1), new (0, -1), new (1, -1)
        );

        int color = Piece.GetColor(Board.squares[ToIndex(curCoord)]);

        if (color == Piece.White) {
            if (CastlingManager.whiteQueenSide) moves.Add(new Coord(2, 0));
            if (CastlingManager.whiteKingSide) moves.Add(new Coord(6, 0));
        } else {
            if (CastlingManager.blackQueenSide) moves.Add(new Coord(2, 0));
            if (CastlingManager.blackKingSide) moves.Add(new Coord(6, 0));
        }

        return moves;
    }

    public static List<Coord> PawnMove(Coord curCoord) {
        List<Coord> moves = new List<Coord>();

        // Up
        int up = ToIndex(curCoord) + 8;

        if (!Board.IsOutBoard(up) && Board.squares[up] == Piece.None) {
            moves.Add(ToCoord(up));

            // Two step up
            int twoUp = ToIndex(curCoord) + 16;

            if (curCoord.rank == 1 && Board.squares[twoUp] == Piece.None) {
                moves.Add(ToCoord(twoUp));
            }
        }

        // Left, Right up
        Coord leftUp = new Coord(curCoord.file - 1, curCoord.rank + 1);
        Coord rightUp = new Coord(curCoord.file + 1, curCoord.rank + 1);
        
        if (!Board.IsOutBoard(leftUp) && (Board.squares[ToIndex(leftUp)] != Piece.None || EnpassantManager.CanEnpassant(leftUp))) {
            moves.Add(leftUp);
        }
        if (!Board.IsOutBoard(rightUp) && (Board.squares[ToIndex(rightUp)] != Piece.None || EnpassantManager.CanEnpassant(rightUp))) {
            moves.Add(rightUp);
        }
    
        return moves;
    }

    public static List<Coord> KnightMove(Coord curCoord) {
        return GenerateStepMove(curCoord,
            new (2, 1), new (-2, 1), new (2, -1), new (-2, -1),
            new (1, 2), new (1, -2), new (-1, 2), new (-1, -2)
        );
    }

    public static List<Coord> BishopMove(Coord curCoord) {
        return GenerateSlidingMove(curCoord, 
            new (-1, 1), new (1, 1), new (-1, -1), new (1, -1)
        );
    }

    public static List<Coord> RookMove(Coord curCoord) {
        return GenerateSlidingMove(curCoord,   
            new (0, 1), new (-1, 0), new (1, 0), new (0, -1)
        );
    }

    public static List<Coord> QueenMove(Coord curCoord) {
        return GenerateSlidingMove(curCoord, 
            new (-1, 1), new (0, 1), new (1, 1), new (-1, 0), 
            new (1, 0), new (-1, -1), new (0, -1), new (1, -1)
        );
    }

    static List<Coord> GenerateStepMove(Coord curCoord, params Coord[] moves) {
        for (int i = 0; i < moves.Length; i++) {
            moves[i].file += curCoord.file;
            moves[i].rank += curCoord.rank;
        }

        return moves.ToList();
    }

    static List<Coord> GenerateSlidingMove(Coord curCoord, params Coord[] directions) {
        List<Coord> moves = new List<Coord>();

        foreach (Coord direction in directions) {
            Coord target = curCoord;

            while (true) {
                target.file += direction.file;
                target.rank += direction.rank;

                if (Board.IsOutBoard(target))
                    break;

                moves.Add(target);

                if (Board.squares[ToIndex(target)] != Piece.None)
                    break;
            }
        }

        return moves;
    }
}