using static Game;

public static class CastlingManager {
    static readonly Coord[] whiteQueenSideBetween = new Coord[] { new (2, 0), new (3, 0) };
    static readonly Coord[] whiteKingSideBetween = new Coord[] { new (5, 0), new (6, 0) };

    static readonly Coord[] blackQueenSideBetween = new Coord[] { new (2, 7), new (3, 7) };
    static readonly Coord[] blackKingSideBetween = new Coord[] { new (5, 7), new (6, 7) };

    public static bool whiteKingHaveMove = false;
    public static bool blackKingHaveMove = false;

    public static bool whiteQueenRookHaveMove = true;
    public static bool whiteKingRookHaveMove = true;
    public static bool blackQueenRookHaveMove = true;
    public static bool blackKingRookHaveMove = true;

    public static bool whiteQueenSide;
    public static bool whiteKingSide;
    public static bool blackQueenSide;
    public static bool blackKingSide;

    public static bool CanCastling(int color, bool isQueenSide) {
        bool emptyBetween = true;
        Coord[] betweenCoords;

        if (Piece.IsWhite(color)) {
            if (isQueenSide) betweenCoords = whiteQueenSideBetween;
            else betweenCoords = whiteKingSideBetween;
        } else {
            if (isQueenSide) betweenCoords = blackQueenSideBetween;
            else betweenCoords = blackKingSideBetween;
        }

        foreach (Coord between in betweenCoords) {
            // Check the between the king and the rook is empty
            if (Board.squares[ToIndex(between)] != Piece.None) {
                emptyBetween = false;
            }

            // Check the between the king and rook is attacked
            if (AttackedSquares.IsAttackedSquare(between, color)) {
                emptyBetween = false;
            }
        }

        // Check the king is check
        bool isCheck = AttackedSquares.IsCheck(color);

        // Check the king and rook have not move
        bool rookHaveMove;
        bool kingHaveMove;

        if (Piece.IsWhite(color)) {
            if (isQueenSide) rookHaveMove = whiteQueenRookHaveMove;
            else rookHaveMove = whiteKingRookHaveMove;

            kingHaveMove = whiteKingHaveMove;
        } else {
            if (isQueenSide) rookHaveMove = blackQueenRookHaveMove;
            else rookHaveMove = blackKingRookHaveMove;

            kingHaveMove = blackKingHaveMove;
        }

        // Check whether rook survives
        bool rookSurvives = Board.squares[ToIndex(new Coord(isQueenSide ? 0 : 7, 0))] == Piece.Rook + color;

        return emptyBetween && !rookHaveMove && !kingHaveMove && !isCheck && rookSurvives;
    }
}