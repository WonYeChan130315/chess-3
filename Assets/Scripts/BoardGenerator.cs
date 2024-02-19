using UnityEngine;
using TMPro;
using static Game;

# pragma warning disable CS0108

public class BoardGenerator : MonoBehaviour {
    private static BoardGenerator instance;

    public static BoardGenerator Instance {
        get {
            if (!instance) {
                instance = FindObjectOfType<BoardGenerator>();
            }
            
            return instance;
        }
    }

    // Board
    public string originFEN;
    public string blackBottomFEN;

    public string applyFEN;

    [HideInInspector] public MeshRenderer[] squareRenderers = new MeshRenderer[64];
    [HideInInspector] public SpriteRenderer[] pieceRenderers = new SpriteRenderer[64];

    [Space(5)]

    public BoardTheme light;        
    public BoardTheme dark;

    [System.Serializable]
    public struct BoardTheme {
        public Color normal;
        public Color select;
    }

    [Space(10)]

    // Piece
    public Sprite[] whitePieces;
    public Sprite[] blackPieces;
   
    void Awake() {
        if (Instance != this) {
            Destroy(gameObject);
            return;
        }
    }

    void Start() {
        GenerateBoard();
        FenLoader.ReadPositionFromFen(applyFEN);
    }

    void GenerateBoard() {
        for (int file = 0; file < 8; file++) {
            for (int rank = 0; rank < 8; rank++) {
                Shader squareShader = Shader.Find("Unlit/Color");

                GenerateSquare(new Coord(file, rank), squareShader);
            }
        }
    }

    void GenerateSquare(Coord coord, Shader squareShader) {
        // Create square
        Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;

        // string alphabets = "abcdefgh";

        square.name = coord.file + "" + coord.rank;//alphabets[coord.rank] + "" + (coord.file + 1);
        square.parent = transform;
        square.position = CoordToPosition(coord);

        MeshRenderer squareRenderer = square.GetComponent<MeshRenderer>();

        squareRenderer.material.shader = squareShader;
        squareRenderer.material.color = Board.IsLightSquare(coord) ? light.normal : dark.normal;

        squareRenderers[ToIndex(coord)] = squareRenderer;

        // Create piece for square
        Transform piece = new GameObject("Piece").transform;
        piece.parent = square;
        piece.localPosition = Vector2.zero;

        pieceRenderers[ToIndex(coord)] = piece.gameObject.AddComponent<SpriteRenderer>();
    }

    Vector2 CoordToPosition(Coord coord) {
        float x = coord.file - 3.5f;
        float y = coord.rank - 3.5f;

        return new Vector2(x, y);
    }

    public void MovePiece(Coord from, Coord to, int piece) {
        DrawPiece(from, Piece.None);
        DrawPiece(to, piece);
    }

    public void DrawPiece(Coord coord, int piece) {
        int index = ToIndex(coord);

        Board.squares[index] = piece;

        if (piece != Piece.None) {
            pieceRenderers[index].sprite = Piece.IsWhite(piece) ? whitePieces[Piece.GetPieceType(piece)] : blackPieces[Piece.GetPieceType(piece)];
        } else {
            pieceRenderers[index].sprite = null;
        }
    }

    public void ClearBoard() {
        for (int file = 0; file < 8; file++) {
            for (int rank = 0; rank < 8; rank++) {
                Coord target = new Coord(file, rank);

                MeshRenderer square = squareRenderers[ToIndex(target)];
                Color color = Board.IsLightSquare(target) ? light.normal : dark.normal;

                square.material.color = color;
            }
        }
    }

    public void SelectSquare(Coord coord) {
        MeshRenderer square = squareRenderers[ToIndex(coord)];
        Color color = Board.IsLightSquare(coord) ? light.select : dark.select;

        square.material.color = color;
    }
}
