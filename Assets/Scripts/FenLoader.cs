using System.Collections.Generic;
using UnityEngine;
using static Game;

public static class FenLoader {
    public static void ReadPositionFromFen(string fen) {
        int file = 0;
        int rank = 7;

        bool setPosition = true;
        string enpassant = "";

        bool checkEnpassant = false;
        bool checkOrder = false;
        bool checkCastling = false;
        bool checkDummy = false;

        foreach(char w in fen) {
            if (w == '-' && checkCastling) {
                checkEnpassant = true;
            } else if (char.IsDigit(w)) { // Digit
                if (setPosition) {
                    file += (int)char.GetNumericValue(w);
                } else if (!checkEnpassant) {
                    enpassant += w;
                    checkEnpassant = true;
                }

                if (checkEnpassant && !checkDummy) {
                    checkDummy = true;
                } else if (checkEnpassant && checkDummy) {
                    PieceMoveManager.moveCount = (int)char.GetNumericValue(w);
                }
            } else if (w == '/') { // '/'
                file = 0;
                rank--;
            } else if (w == ' ') {
                setPosition = false;
            } else if (char.IsLetter(w)) {
                if (setPosition) {
                    Dictionary<char, int> pieceFromSymbol = new Dictionary<char, int> { // Piece
                        ['k'] = Piece.King, ['p'] = Piece.Pawn, ['n'] = Piece.Knight,
                        ['b'] = Piece.Bishop, ['r'] = Piece.Rook, ['q'] = Piece.Queen,
                    };

                    int pieceType = pieceFromSymbol[char.ToLower(w)];
                    int pieceColor = char.IsUpper(w) ? Piece.White : Piece.Black;

                    BoardGenerator.Instance.DrawPiece(new Coord(file, rank), pieceType + pieceColor);

                    file++;
                } else {
                    if ((w == 'w' || w == 'b') && !checkOrder) {
                        PieceMoveManager.curOrder = w == 'w' ? Piece.White : w == 'b' ? Piece.Black : PieceMoveManager.curOrder;
                        checkOrder = true;
                    } else if (w == 'a' || w == 'b' || w == 'c' || w == 'd' || w == 'e' || w == 'f' || w == 'g' || w == 'h') {
                        enpassant += w;
                    } else {
                        if (CastlingManager.whiteQueenRookHaveMove) CastlingManager.whiteQueenRookHaveMove = w != 'Q';
                        if (CastlingManager.whiteKingRookHaveMove) CastlingManager.whiteKingRookHaveMove = w != 'K';
                        if (CastlingManager.blackQueenRookHaveMove) CastlingManager.blackQueenRookHaveMove = w != 'q';
                        if (CastlingManager.blackKingRookHaveMove) CastlingManager.blackKingRookHaveMove = w != 'k';

                        checkCastling = true;
                    }
                }
            }
        }

        if (enpassant != "") {
            Debug.Log(enpassant);
            EnpassantManager.enpassantCoord = ToCoord(enpassant);
        } else {
            EnpassantManager.enpassantCoord = Coord.None;
        }
    }
}