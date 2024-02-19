using System.Collections.Generic;
using UnityEngine;

public class Game {
    public static int ToIndex(Coord coord) {
        return coord.rank * 8 + coord.file;
    }

    public static Coord ToCoord(int index) {
        return new Coord(index % 8, index / 8);
    }

    public static Coord ToCoord(string coord) {
        Dictionary<char, int> alphabets = new Dictionary<char, int> {
            ['a'] = 0, ['b'] = 1, ['c'] = 2, ['d'] = 3,
            ['e'] = 4, ['f'] = 5, ['g'] = 6, ['h'] = 7,
        };

        return new Coord(alphabets[coord[0]], (int)char.GetNumericValue(coord[1]) - 1);
    }
}

[System.Serializable]
public struct Coord {
    public int file;
    public int rank;

    public readonly static Coord None = new Coord(-1, -1);

    public Coord(int file, int rank) {
        this.file = file;
        this.rank = rank;
    }

    public static bool operator ==(Coord a, Coord b) {
        return a.file == b.file && a.rank == b.rank;
    }

    public static bool operator !=(Coord a, Coord b) {
        return !(a == b);
    }

    public override bool Equals(object obj) {
        if (!(obj is Coord))
            return false;

        Coord other = (Coord)obj;
        return this == other;
    }

    public override int GetHashCode() {
        return file.GetHashCode() ^ rank.GetHashCode();
    }
}