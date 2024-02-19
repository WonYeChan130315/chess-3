using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using static Game;

public class PieceMoveManager : MonoBehaviour {
    public static int myOrder;
    public static int moveCount = 1;
    public static int curOrder = Piece.White;

    Transform movingPieceObj;
    Coord clickCoord;
    Coord mouseCoord;
    Coord[] legalMoves;
    PhotonView pv;

    int curPiece;

    bool dragging = false;

    void Awake() {
        pv = GetComponent<PhotonView>();

        myOrder = pv.IsMine ? curOrder : Piece.ReverseColor(curOrder);
        BoardGenerator.Instance.applyFEN = pv.IsMine ? BoardGenerator.Instance.originFEN : BoardGenerator.Instance.blackBottomFEN;
    }

    void Update() {
        PlayMove();
    }

    void PlayMove() {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -3;

        mouseCoord = PositionToCoord(mousePosition);

        if (Input.GetMouseButtonDown(0) && !Board.IsOutBoard(mouseCoord)) {
            clickCoord = mouseCoord;

            if (Board.squares[ToIndex(clickCoord)] == Piece.None)
                return;

            dragging = true;

            movingPieceObj = BoardGenerator.Instance.pieceRenderers[ToIndex(clickCoord)].transform;
            curPiece = Board.squares[ToIndex(clickCoord)];

            if (myOrder == curOrder && Piece.GetColor(Board.squares[ToIndex(clickCoord)]) == curOrder) {
                legalMoves = MakeMove.GetMoves(clickCoord);
                SelectLegalMoves();
            }
        }

        if (Input.GetMouseButton(0) && dragging) {
            movingPieceObj.position = mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && dragging) {
            dragging = false;

            movingPieceObj.localPosition = Vector2.zero;

            BoardGenerator.Instance.ClearBoard();

            if (!Board.IsOutBoard(mouseCoord) && legalMoves != null && MakeMove.IsPossibleMove(mouseCoord, legalMoves) && myOrder == curOrder && Piece.GetColor(Board.squares[ToIndex(clickCoord)]) == curOrder) {
                PieceMove();
            }
        }
    }

    void PieceMove() {
        int targetPiece = Board.squares[ToIndex(mouseCoord)];

        // Move
        BoardGenerator.Instance.MovePiece(clickCoord, mouseCoord, curPiece);
        
        // Check
        CheckPromotion();
        CheckCastling();
        CheckEnpassant(targetPiece);
        CheckKingAndRookMove();
        CheckCanCastling();
        CheckCanEnpassant();

        if (curOrder == Piece.Black)
            moveCount++;

        curOrder = Piece.ReverseColor(curOrder);
        pv.RPC("SyncCurOrder", RpcTarget.OthersBuffered, Board.squares);

        // Check the checkmate
        List<Coord> attackList = new List<Coord>();

        for(int file = 0; file < 8; file++) {
            for(int rank = 0; rank < 8; rank++) {
                Coord target = new Coord(file, rank);

                if(Piece.GetColor(Board.squares[ToIndex(target)]) == curOrder) {
                    attackList.AddRange(MakeMove.GetMoves(target));
                }
            }
        }

        if (attackList.Count == 0 && AttackedSquares.IsCheck(curOrder)) {
            print(Piece.ReverseColor(curOrder) + " win (checkmate)");
        } else if (attackList.Count == 0 && !AttackedSquares.IsCheck(curOrder)) {
            print("draw (stalemate)");
        }

        // Check the 50 moves
        if (moveCount > 50) {
            print("draw (50 moves)");
        }
    }

    void CheckCanEnpassant() {
        EnpassantManager.enpassantCoord = Coord.None;

        if (Piece.GetPieceType(curPiece) == Piece.Pawn && Mathf.Abs(clickCoord.rank - mouseCoord.rank) == 2) {
            int direction = Piece.IsWhite(curOrder) ? 1 : -1;
            Coord enpassantCoord = new Coord(clickCoord.file, clickCoord.rank + direction);

            EnpassantManager.enpassantCoord = enpassantCoord;
        }
    }

    void CheckEnpassant(int targetPiece) {
        int direction = Piece.IsWhite(curOrder) ? 1 : -1;

        if (targetPiece == Piece.None && Piece.GetPieceType(curPiece) == Piece.Pawn && Board.squares[ToIndex(mouseCoord) - ToIndex(new Coord(0, 1 * direction))] != Piece.None) {
            BoardGenerator.Instance.DrawPiece(ToCoord(ToIndex(mouseCoord) - ToIndex(new Coord(0, 1 * direction))), Piece.None);
        }
    }

    void CheckCanCastling() {
        CastlingManager.whiteQueenSide = CastlingManager.CanCastling(Piece.White, true);
        CastlingManager.whiteKingSide = CastlingManager.CanCastling(Piece.White, false);

        CastlingManager.blackQueenSide = CastlingManager.CanCastling(Piece.Black, true);
        CastlingManager.blackKingSide = CastlingManager.CanCastling(Piece.Black, false);
    }

    void CheckCastling() {
        if (Piece.GetPieceType(curPiece) == Piece.King) {
            if (clickCoord.file == 4) {
                if (mouseCoord.file == 2) {
                    BoardGenerator.Instance.MovePiece(new Coord(0, clickCoord.rank), new Coord(3, clickCoord.rank), Board.squares[ToIndex(new Coord(0, clickCoord.rank))]);
                } else if (mouseCoord.file == 6) {
                    BoardGenerator.Instance.MovePiece(new Coord(7, clickCoord.rank), new Coord(5, clickCoord.rank), Board.squares[ToIndex(new Coord(7, clickCoord.rank))]);
                }
            }
        }
    }

    void CheckKingAndRookMove() {
        // Check the king
        if (Piece.GetPieceType(curPiece) == Piece.King) {
            if (!CastlingManager.whiteKingHaveMove && Piece.IsWhite(Piece.GetColor(curPiece))) CastlingManager.whiteKingHaveMove = true;
            else if (!CastlingManager.blackKingHaveMove && !Piece.IsWhite(Piece.GetColor(curPiece))) CastlingManager.blackKingHaveMove = true;
        }

        // Check the rook
        if (Piece.GetPieceType(curPiece) == Piece.Rook) {
            if (Piece.IsWhite(Piece.GetColor(curPiece))) {
                // White
                if (!CastlingManager.whiteQueenRookHaveMove && clickCoord == new Coord(0, 0)) CastlingManager.whiteQueenRookHaveMove = true;
                else if (!CastlingManager.whiteKingRookHaveMove && clickCoord == new Coord(7, 0)) CastlingManager.whiteKingRookHaveMove = true;
            } else {
                // Black
                if (!CastlingManager.blackQueenRookHaveMove && clickCoord == new Coord(0, 7)) CastlingManager.blackQueenRookHaveMove = true;
                else if (!CastlingManager.blackKingRookHaveMove && clickCoord == new Coord(7, 7)) CastlingManager.blackKingRookHaveMove = true;
            }
        }
    }
    
    void CheckPromotion() {
        int promotionRank = Piece.IsWhite(curOrder) ? 7 : 0;

        if (Piece.GetPieceType(curPiece) == Piece.Pawn && mouseCoord.rank == promotionRank) {
            BoardGenerator.Instance.DrawPiece(mouseCoord, Piece.Queen);
        }
    }

    void SelectLegalMoves() {
        foreach (Coord move in legalMoves) {
            BoardGenerator.Instance.SelectSquare(move);
        }
    }

    Coord PositionToCoord(Vector2 position) {
        int file = Mathf.RoundToInt(position.x + 3.5f);
        int rank = Mathf.RoundToInt(position.y + 3.5f);

        return new Coord(file, rank);
    }

    [PunRPC]
    void SyncCurOrder(int newCurOrder, Vector4[] moves) {
        foreach (Vector4 move in moves) {
            BoardGenerator.Instance.MovePiece(new Coord(7 - (int)move.x, 7 - (int)move.y), new Coord(7 - (int)move.z, 7 - (int)move.w), Board.squares[ToIndex(new Coord(7 - (int)move.x, 7 - (int)move.y))]);
        }

        curOrder = newCurOrder;
    }
}