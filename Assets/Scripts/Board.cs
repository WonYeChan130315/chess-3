using System.Linq;
using static Game;

public static class Board {
    public static int[] squares = Enumerable.Repeat(-1, 64).ToArray();

    public static bool IsOutBoard(Coord coord) {
        return coord.file < 0 || coord.file > 7 || coord.rank < 0 || coord.rank > 7;
    }

    public static bool IsOutBoard(int index) {
        return index < 0 || index > 63;
    }

    public static bool IsLightSquare(Coord coord) {
        return (coord.file + coord.rank) % 2 != 0;
    }
}