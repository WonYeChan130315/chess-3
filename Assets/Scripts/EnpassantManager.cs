using UnityEngine;
using static Game;

public static class EnpassantManager {
    public static Coord enpassantCoord;

    public static bool CanEnpassant(Coord targetCoord) {
        return targetCoord == enpassantCoord;
    }
}