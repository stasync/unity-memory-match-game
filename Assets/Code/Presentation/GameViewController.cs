using System;
using Code.GameManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GameViewController : MonoBehaviour
    {
        [SerializeField, Range(GameSession.MIN_ROWS, GameSession.MAX_ROWS)]
        private byte _desiredRows = 5;

        [SerializeField, Range(GameSession.MIN_COLUMNS, GameSession.MAX_COLUMNS)]
        private byte _desiredColumns = 6;

        [SerializeField] private Button _startStopButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;

        [SerializeField] private GameBoardView _gameBoardView;
        [SerializeField] private ScoreView _scoreView;

        [SerializeField] private CellsVisualData _visualData;

        [SerializeField] private CardView _cardViewPrefab;

        [Tooltip("Draws the prototype IMGUI overlay. It reveals every card's type, so keep it off outside of debugging.")]
        [SerializeField] private bool _showDebugOverlay;

        private GameManager _gameManager;
        private PrototypePresentationDrawer _presentationDrawer;

        /// <summary>
        /// Will be called from bootstrap
        /// </summary>
        public void Initialize(GameManager gameManager)
        {
            if (_gameManager != null)
                throw new InvalidOperationException($"{nameof(GameViewController)} is already initialized.");

            _gameManager = gameManager;
            _gameManager.SessionInitialized += OnSessionInitialized;
            _gameManager.SessionReleased += OnSessionReleased;
            _gameManager.SessionSaved += OnSessionSaved;

            _gameBoardView.Initialize(_cardViewPrefab, _visualData);
            _gameBoardView.CardClicked += OnCardClicked;

            _startStopButton.onClick.AddListener(OnStartStopButtonPressed);
            _saveButton.onClick.AddListener(OnSaveButtonPressed);
            _saveButton.interactable = false;

            _loadButton.onClick.AddListener(OnLoadButtonPressed);
            _loadButton.interactable = false;

            _presentationDrawer = new PrototypePresentationDrawer(_gameManager);

            UpdateStartStopButtonText();
            UpdateSaveButtonState();
            UpdateScoreViewVisibility();
        }

        private void OnSessionInitialized()
        {
            UpdateStartStopButtonText();
            UpdateSaveButtonState();
            UpdateScoreViewVisibility();

            var session = _gameManager.CurrentGameSession;
            session.Started += OnSessionStarted;
            session.TurnStarted += OnTurnStarted;
            session.TurnFinished += OnTurnFinished;
            session.CellResolved += OnCurrentSessionCellResolved;
            session.InputAccepted += OnInputAccepted;

            _gameBoardView.InitializeSession(session);
            UpdateScoreView();
        }

        private void OnSessionReleased()
        {
            UpdateStartStopButtonText();
            UpdateSaveButtonState();
            UpdateScoreViewVisibility();

            _gameBoardView.TryReleaseSession();
        }

        private void OnSessionSaved() =>
            _loadButton.interactable = true;

        private void OnCardClicked(BoardLocation boardLocation) =>
            _gameManager.CurrentGameSession.OnInput(boardLocation);

        private void OnStartStopButtonPressed()
        {
            if (_gameManager.CurrentGameSession == null)
            {
                var boardSizeError = GetBoardSizeError();
                if (boardSizeError != null)
                {
                    Debug.LogError(boardSizeError, this);
                    return;
                }

                _gameManager.StartOrRestartGame(rows: _desiredRows, columns: _desiredColumns);
            }
            else
                _gameManager.TryStopCurrentSession();
        }

        /// <summary>
        /// Returns a description of why the desired board can't be built, or null if it can.
        /// </summary>
        private string GetBoardSizeError()
        {
            var cellCount = _desiredRows * _desiredColumns;
            if (cellCount % 2 != 0)
                return $"Board {_desiredRows}x{_desiredColumns} has an odd number of cells, so cards can't be paired.";

            var pairCount = cellCount / 2;
            if (_visualData != null && _visualData.CellVisualData.Length < pairCount)
                return $"Board {_desiredRows}x{_desiredColumns} needs {pairCount} card sprites, but '{_visualData.name}' has only {_visualData.CellVisualData.Length}.";

            return null;
        }

        private void OnValidate()
        {
            var boardSizeError = GetBoardSizeError();
            if (boardSizeError != null)
                Debug.LogWarning(boardSizeError, this);
        }

        private void OnSaveButtonPressed() =>
            _gameManager.TrySaveCurrentSession();

        private void OnLoadButtonPressed() =>
            _gameManager.TryLoadSessionFromSaveData();

        private void OnSessionStarted()
        {
            _gameBoardView.OnSessionStarted();
        }

        private void OnTurnFinished(bool isMatch)
        {
            UpdateScoreView();
            _gameBoardView.OnTurnFinished(isMatch);
        }

        private void OnTurnStarted() =>
            _gameBoardView.OnTurnStarted();

        private void OnCurrentSessionCellResolved(BoardLocation boardLocation)
        {
            _gameBoardView.SetCardVisible(boardLocation, true);
            _gameBoardView.SetCardInteractable(boardLocation, false);
        }

        private void OnInputAccepted(BoardLocation boardLocation) =>
            _gameBoardView.HandleInput(boardLocation);

        private void UpdateStartStopButtonText() =>
            _startStopButton.GetComponentInChildren<Text>().text =
                _gameManager.CurrentGameSession == null ? "Start" : "Stop";

        private void UpdateSaveButtonState() =>
            _saveButton.interactable = _gameManager.CurrentGameSession != null;

        private void UpdateScoreViewVisibility() =>
            _scoreView.gameObject.SetActive(_gameManager.CurrentGameSession != null);

        private void UpdateScoreView()
        {
            _scoreView.UpdateTurnsCount(_gameManager.CurrentGameSession.Turns);
            _scoreView.UpdateMatchesCount(_gameManager.CurrentGameSession.Matches);
            // Time should be updated via reactive property.
            _scoreView.UpdateTime(_gameManager.CurrentGameSession.Time);
        }

        private void OnGUI()
        {
            if (_showDebugOverlay)
                _presentationDrawer?.OnGUI();
        }
    }
}