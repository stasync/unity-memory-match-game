using System;
using System.Collections.Generic;
using Code.GameManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GameBoardView : GridLayoutGroup
    {
        private struct CardDisableRequest
        {
            internal BoardLocation Location;
            internal float Time;
        }

        public event Action<BoardLocation> CardClicked = delegate { };

        [SerializeField] private int _layoutColumns = 1;

        [SerializeField] private RectTransform _rectTransform;

        private GameSession _gameSession;
        private CardViewPool _cardViewPool;
        private CellsVisualData _visualData;
        private CardView[,] _cardInstances;

        private readonly Queue<BoardLocation> _currentTurnClickedCards = new();
        private readonly List<CardDisableRequest> _delayedDisabledCards = new();

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
        }

        internal void Initialize(CardView cardViewPrefab, CellsVisualData cellsVisualData)
        {
            _cardViewPool ??= new CardViewPool(cardViewPrefab, transform);
            _visualData = cellsVisualData;
        }

        internal void InitializeSession(GameSession session)
        {
            if (_gameSession != null)
                throw new InvalidOperationException("View is already initialized. Call 'Release' before initializing it again.");

            _layoutColumns = session.Columns;
            constraint = Constraint.FixedRowCount;
            constraintCount = session.Rows;

            _gameSession = session;
            _cardInstances = new CardView[session.Rows, session.Columns];

            for (var row = 0; row < session.Rows; row++)
            {
                for (var column = 0; column < session.Columns; column++)
                {
                    var state = session.GetState(row, column);
                    var cardViewInstance = _cardViewPool.Get();
                    cardViewInstance.Init(_visualData.CellVisualData[state.Type], new BoardLocation(row, column), OnCardClicked);
                    _cardInstances[row, column] = cardViewInstance;
                }
            }
        }

        internal void OnSessionStarted()
        {
            for (var row = 0; row < _gameSession.Rows; row++)
            {
                for (var column = 0; column < _gameSession.Columns; column++)
                {
                    var state = _gameSession.GetState(row, column);
                    var cardViewInstance = _cardInstances[row, column];

                    cardViewInstance.SetVisible(state.IsResolved);
                    cardViewInstance.SetInteractable(!state.IsResolved);
                }
            }
        }

        internal void HandleInput(BoardLocation boardLocation)
        {
            for (var i = _delayedDisabledCards.Count - 1; i >= 0; i--)
            {
                var delayedDisabledCard = _delayedDisabledCards[i];
                if (delayedDisabledCard.Location.Equals(boardLocation))
                    _delayedDisabledCards.RemoveAt(i);
            }
            
            _currentTurnClickedCards.Enqueue(boardLocation);
            SetCardVisible(boardLocation, value: true);

            // A face-up card can't be picked again within the same turn.
            SetCardInteractable(boardLocation, value: false);
        }

        private void OnCardClicked(BoardLocation boardLocation) =>
            CardClicked(boardLocation);

        internal void OnTurnStarted()
        {
            foreach (var delayedDisabledCard in _delayedDisabledCards)
                SetCardVisible(delayedDisabledCard.Location, false);

            _delayedDisabledCards.Clear();
        }

        internal void OnTurnFinished(bool isMatch)
        {
            if (isMatch)
            {
                _currentTurnClickedCards.Clear();
                return;
            }

            while (_currentTurnClickedCards.TryDequeue(out var boardLocation))
            {
                // Mismatched cards can be picked again while they are still face-up.
                SetCardInteractable(boardLocation, value: true);
                _delayedDisabledCards.Add(new CardDisableRequest
                {
                    Location = boardLocation,
                    Time = 1f
                });
            }
        }

        internal void TryReleaseSession()
        {
            if (_gameSession == null)
                return;

            // Drop pending turn state so it doesn't leak into the next session.
            _currentTurnClickedCards.Clear();
            _delayedDisabledCards.Clear();

            _gameSession = null;
            foreach (var card in _cardInstances)
                _cardViewPool.Release(card);
            _cardInstances = null;
        }

        internal void SetCardVisible(BoardLocation boardLocation, bool value)
        {
            ref var cardView = ref _cardInstances[boardLocation.Row, boardLocation.Column];
            cardView.SetVisible(value);
        }

        internal void SetCardInteractable(BoardLocation boardLocation, bool value)
        {
            ref var cardView = ref _cardInstances[boardLocation.Row, boardLocation.Column];
            cardView.SetInteractable(value);
        }

        private void Update()
        {
            for (var i = _delayedDisabledCards.Count - 1; i >= 0; i--)
            {
                var delayedDisabledCard = _delayedDisabledCards[i];
                delayedDisabledCard.Time -= Time.deltaTime;
                if (delayedDisabledCard.Time <= 0)
                {
                    SetCardVisible(delayedDisabledCard.Location, false);
                    _delayedDisabledCards.RemoveAt(i);
                }
                else
                    _delayedDisabledCards[i] = delayedDisabledCard;
            }
        }

        public override void SetLayoutVertical()
        {
            var width = _rectTransform.rect.width - padding.horizontal - (_layoutColumns - 1) * spacing.x;
            var cellWidth = width / _layoutColumns;
            cellSize = new Vector2(cellWidth, cellWidth);

            base.SetLayoutVertical();
        }
    }
}