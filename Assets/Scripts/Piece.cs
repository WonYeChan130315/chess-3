public static class Piece {
    public const int None = -1;
    public const int King = 0;
    public const int Pawn = 1;
    public const int Knight = 2;
    public const int Bishop = 3;
    public const int Rook = 4;
    public const int Queen = 5;

    public const int White = 0;
    public const int Black = 6;

    public static bool IsWhite(int piece) {
        return piece < Black;
    }

    public static int GetPieceType(int piece) {
        return IsWhite(piece) ? piece : piece - Black;
    }

    public static int GetColor(int piece) {
        return IsWhite(piece) ? White : Black;
    }

    public static int ReverseColor(int color) {
        return IsWhite(color) ? Black : White;
    }
}