using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess
{
    public class GameManager : MonoBehaviour
    {
        public ChessBoard board;
        
        private ChessBoardVisuals chessboardVisuals;
        private BoardInputActions boardInputActions;
        private bool selectedPiece = false;
        private int selectedPieceIndex;

        private

        void Start()
        {
            //Find ChessBoard UI
            chessboardVisuals = GameObject.FindObjectOfType<ChessBoardVisuals>();

            //Get position and initialize a ChessBoard with that position
            var position = fenHelper.getPosition();
            board = new ChessBoard(position);


            //Generate ChessBoard UI
            chessboardVisuals.GenerateBoard(board);

            boardInputActions = new BoardInputActions();
            //boardInputActions.Board.Enable();
            //boardInputActions.Board.Click.performed += makeMove;

            //Subscribe to events
            board.OnPieceMoved += onPieceMoved;
            board.OnPieceMoved += chessboardVisuals.GenerateBoard;
            chessboardVisuals.OnPieceSelected += onPieceSelected; //TODO: Move controls to it's own script?


        }

        private void OnDestroy()
        {
            //Unsubscribe before the object is destroyed
            board.OnPieceMoved -= onPieceMoved;
            board.OnPieceMoved -= chessboardVisuals.GenerateBoard;
            chessboardVisuals.OnPieceSelected -= onPieceSelected;
        }

        void Update()
        {
            
        }

        private void makeMove(InputAction.CallbackContext c)
        {
            board.TryMakeMove(board.Moves[Random.Range(0, board.Moves.Count)]);
        }

        //Called every time a piece is moved on Chessboard
        private void onPieceMoved(ChessBoard b)
        {
            chessboardVisuals.GenerateBoard(b);//Just have the visual script sub to the same event this function is doing... maybe?
        }

        //Called anytime a piece is selected on the chessboard
        private void onPieceSelected(int index)
        {
            //Already have a piece selected-- 
            if (selectedPiece)
            {
                //Remove this when done testing bb move (or comment out)
                var bbTest = new Bitboard(board);
                bbTest.MakeMove(new Move(selectedPieceIndex, index));
                board.squares = bbTest.ToArray();
                board.isWhiteMove = bbTest.isWhiteMove;
                board.whiteKingCastleRights = bbTest.whiteKingCastleRights;
                board.whiteQueenCastleRights = bbTest.whiteQueenCastleRights;
                board.blackKingCastleRights = bbTest.blackKingCastleRights;
                board.blackQueenCastleRights = bbTest.blackQueenCastleRights;
                board.enpassTargetSquare = bbTest.enpassTargetSquare;
                board.TryMakeMove(null);
                //

                //var movemade = board.TryMakeMove(new Move(selectedPieceIndex, index));

                chessboardVisuals.HideMoves();

                //Reset selected piece regardless if move was successful
                selectedPiece = false;
                selectedPieceIndex = -1;

                //if(movemade && !board.isWhiteMove)
                //{
                //    board.TryMakeMove(board.Moves[Random.Range(0, board.Moves.Count)]);
                //}
                
            }
            else if (board.squares[index] != Pieces.Empty)//No piece selected yet - set selectedPieceIndex and selectedPiece
            {
                selectedPieceIndex = index;
                selectedPiece = true;

                chessboardVisuals.ShowMoves(index, board.Moves);
            }
        }
    }
}
