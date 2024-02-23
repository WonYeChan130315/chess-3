using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using static Game;

public class PieceMoveManager : MonoBehaviour {
    public static int myOrder;
    public static int moveCount = 1;
    public static int curOrder = Piece.White;

    public float pieceMoveSpeed;

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

        // Piece down
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

        // Piece dragging
        if (Input.GetMouseButton(0) && dragging) {
            movingPieceObj.position = mousePosition;
        }
        
        // Piece up
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

        List<Vector2> to = new List<Vector2>();
        List<Vector2> from = new List<Vector2>();

        // Move
        BoardGenerator.Instance.MovePiece(new Move(clickCoord, mouseCoord), curPiece);
                
        from.Add(new Vector2(clickCoord.file, clickCoord.rank));
        to.Add(new Vector2(mouseCoord.file, mouseCoord.rank));
        
        // Check
        CheckPromotion();
        CheckCastling(ref from, ref to);
        CheckEnpassant(targetPiece, ref from, ref to);
        CheckKingAndRookMove();
        CheckCanCastling();
        CheckCanEnpassant();

        if (curOrder == Piece.Black)
            moveCount++;

        curOrder = Piece.ReverseColor(curOrder);
        pv.RPC("SyncCurOrder", RpcTarget.OthersBuffered, curOrder, from.ToArray(), to.ToArray());

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

        if (Piece.GetPieceType(curPiece) == Piece.Pawn && mouseCoord.rank - clickCoord.rank == 2) {
            Coord enpassantCoord = new Coord(clickCoord.file, clickCoord.rank - 1);
            EnpassantManager.enpassantCoord = enpassantCoord;
        }
    }

    void CheckEnpassant(int targetPiece, ref List<Vector2> from, ref List<Vector2> to) {
        if (targetPiece == Piece.None && Piece.GetPieceType(curPiece) == Piece.Pawn && Board.squares[ToIndex(new Coord(mouseCoord.file, mouseCoord.rank - 1))] != Piece.None) {
            Coord target = new Coord(mouseCoord.file, mouseCoord.rank - 1);
            BoardGenerator.Instance.DrawPiece(target, Piece.None);

            from.Add(new Vector2(-1, -1));
            to.Add(new Vector2(target.file, target.rank));
        }
    }

    void CheckCanCastling() {
        CastlingManager.whiteQueenSide = CastlingManager.CanCastling(Piece.White, true);
        CastlingManager.whiteKingSide = CastlingManager.CanCastling(Piece.White, false);

        CastlingManager.blackQueenSide = CastlingManager.CanCastling(Piece.Black, true);
        CastlingManager.blackKingSide = CastlingManager.CanCastling(Piece.Black, false);
    }

    void CheckCastling(ref List<Vector2> from, ref List<Vector2> to) {
        if (Piece.GetPieceType(curPiece) == Piece.King) {
            if (clickCoord.file == 4) {
                if (mouseCoord.file == 2) {
                    BoardGenerator.Instance.MovePiece(new Move(new Coord(0, clickCoord.rank), new Coord(3, clickCoord.rank)), Board.squares[ToIndex(new Coord(0, clickCoord.rank))]);

                    from.Add(new Vector4(0, clickCoord.rank));
                    to.Add(new Vector4(3, clickCoord.rank));
                } else if (mouseCoord.file == 6) {
                    BoardGenerator.Instance.MovePiece(new Move(new Coord(7, clickCoord.rank), new Coord(5, clickCoord.rank)), Board.squares[ToIndex(new Coord(7, clickCoord.rank))]);

                    from.Add(new Vector4(7, clickCoord.rank));
                    to.Add(new Vector4(5, clickCoord.rank));
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
                if (!CastlingManager.blackQueenRookHaveMove && clickCoord == new Coord(0, 0)) CastlingManager.blackQueenRookHaveMove = true;
                else if (!CastlingManager.blackKingRookHaveMove && clickCoord == new Coord(7, 0)) CastlingManager.blackKingRookHaveMove = true;
            }
        }
    }
    
    void CheckPromotion() {
        if (Piece.GetPieceType(curPiece) == Piece.Pawn && mouseCoord.rank == 7) {
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
    void SyncCurOrder(int newCurOrder, Vector2[] from, Vector2[] to) {
        curOrder = newCurOrder;

        for (int i = 0; i < to.Length; i++) {
            Coord correctedFrom = new Coord((int)from[i].x, 7 - (int)from[i].y);
            Coord correctedTo = new Coord((int)to[i].x, 7 - (int)to[i].y);

            Move move = new Move(correctedFrom, correctedTo);
            
            StartCoroutine(PieceMoveRoutine(move));
        }
    }

    IEnumerator PieceMoveRoutine(Move move) {
        Transform piece = BoardGenerator.Instance.pieceRenderers[ToIndex(move.from)].transform;
        Vector3 targetPosition = BoardGenerator.Instance.squareRenderers[ToIndex(move.to)].transform.position;

        while (Vector2.Distance(piece.position, targetPosition) > 0.01f) {
            piece.position = Vector2.Lerp(piece.position, targetPosition, Time.deltaTime * pieceMoveSpeed);
            yield return new WaitForSeconds(0.1f * Time.deltaTime);
        }

        piece.localPosition = Vector2.zero;
        BoardGenerator.Instance.MovePiece(move, Board.squares[ToIndex(move.from)]);
    }
}