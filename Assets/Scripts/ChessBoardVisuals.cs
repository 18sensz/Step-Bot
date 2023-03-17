using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess
{
    public class ChessBoardVisuals : MonoBehaviour
    {
        [Header("Chess Board Customization")]
        public GameObject squareSpacePrefab;
        public Color lightSquareColor;
        public Color darkSquareColor;

        [Header("Chess Piece Prefabs")]
        public GameObject whiteKing;
        public GameObject whiteQueen;
        public GameObject whiteRook;
        public GameObject whiteBishop;
        public GameObject whiteKnight;
        public GameObject whitePawn;

        public GameObject blackKing;
        public GameObject blackQueen;
        public GameObject blackRook;
        public GameObject blackBishop;
        public GameObject blackKnight;
        public GameObject blackPawn;

        [Header("Indicator Prefabs")]
        public GameObject moveIndicator;
        public GameObject captureIndicator;
        public GameObject selectedIndicator;
        private List<GameObject> moveIndicatorList = new List<GameObject>();


        [Header("Misc.")]
        private Camera mainCamera;
        private BoardInputActions boardInputActions;

        public event Action<int> OnPieceSelected;

        public void Start()
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            boardInputActions = new BoardInputActions();
            boardInputActions.Board.Enable();
            boardInputActions.Board.Click.performed += onClick;
        }

        private void OnDestroy()
        {
            boardInputActions.Board.Click.performed -= onClick;
            boardInputActions.Board.Disable();
        }

        private void onClick(InputAction.CallbackContext context)
        {
            Ray ray = mainCamera.ScreenPointToRay(boardInputActions.Board.Mouse.ReadValue<Vector2>());
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            if(hit.collider != null && int.TryParse(hit.collider.name, out int clickedSquareIndex))
            {  
               OnPieceSelected.Invoke(clickedSquareIndex);             
            }  
        }

        public void GenerateBoard(ChessBoard board)
        {
            if(transform.childCount != 0)
            {
                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }

            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var generatedSquare = Instantiate(squareSpacePrefab, new Vector2(-j, i), Quaternion.identity);

                    generatedSquare.transform.SetParent(this.transform);
                    generatedSquare.name = $"{count}";
                    generatedSquare.GetComponent<SpriteRenderer>().color = ((i + j) % 2 == 0) ? lightSquareColor : darkSquareColor;

                    if (board.squares[count] != Pieces.Empty)
                    {
                        var generatedPiece = Instantiate(DeterminePiece(board.squares[count]), generatedSquare.transform.position, Quaternion.identity);
                        generatedPiece.transform.SetParent(this.transform);
                    }
                    count++;
                }
            }
        }

        public void ShowMoves(int pieceIndex, List<Move> moves)
        {
            //Show selected indicator
            GameObject selectedSquare = GameObject.Find(pieceIndex.ToString());
            GameObject _selectedIndicator = Instantiate(selectedIndicator, selectedSquare.transform.position, Quaternion.identity);
            moveIndicatorList.Add(_selectedIndicator);

            //Show capture/move indicators
            var selectedPieceMoves = moves.Where(m => m.startingSquare == pieceIndex).ToList();
            foreach (var move in selectedPieceMoves)
            {
                //Find square of move
                GameObject moveSquare = GameObject.Find(move.targetSquare.ToString());
                GameObject indicator;
                //Instantiate move indicator prefab at target square and add to list to delete later
                switch (move.type)
                {
                    case MoveType.Capture:
                    case MoveType.QueenCapturePromotion:
                    case MoveType.RookCapturePromotion:
                    case MoveType.BishopCapturePromotion:
                    case MoveType.KnightCapturePromotion:
                    case MoveType.EnPassantCapture:
                        indicator  = Instantiate(captureIndicator, moveSquare.transform.position, Quaternion.identity);
                        moveIndicatorList.Add(indicator);
                        break;
                    default:
                        indicator = Instantiate(moveIndicator, moveSquare.transform.position, Quaternion.identity);
                        moveIndicatorList.Add(indicator);
                        break;
                }
            }
        }

        public void HideMoves()
        {
            foreach (var indicator in moveIndicatorList)
            {
                Destroy(indicator);
            }
        }

        private GameObject DeterminePiece(char pieceCode)
        {
            switch(pieceCode)
            {
                case Pieces.WhiteKing: return whiteKing;
                case Pieces.WhiteQueen: return whiteQueen;
                case Pieces.WhiteRook: return whiteRook;
                case Pieces.WhiteBishop: return whiteBishop;
                case Pieces.WhiteKnight: return whiteKnight;
                case Pieces.WhitePawn: return whitePawn;
                case Pieces.BlackKing: return blackKing;
                case Pieces.BlackQueen: return blackQueen;
                case Pieces.BlackRook: return blackRook;
                case Pieces.BlackBishop: return blackBishop;
                case Pieces.BlackKnight: return blackKnight;
                case Pieces.BlackPawn: return blackPawn;
                default: return null;
            }
        }
    }
}
