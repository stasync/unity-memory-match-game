using Code.GameManagement;
using UnityEngine;

namespace Code.Presentation
{
    /// <summary>
    /// NOTE: this is temporary code to test the basic logic, and board initialization. Will be removed later.
    /// </summary>
    internal sealed class PrototypePresentationDrawer
    {
        private readonly GameManager _gameManager;

        public PrototypePresentationDrawer(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        internal void OnGUI()
        {
            if (_gameManager.CurrentGameSession != null)
            {
                if (GUILayout.Button("Save"))
                    _gameManager.TrySaveCurrentSession();
            }

            if (GUILayout.Button("Load"))
                _gameManager.TryLoadSessionFromSaveData();

            if (_gameManager.CurrentGameSession == null)
            {
                if (GUILayout.Button("Start"))
                    _gameManager.StartOrRestartGame(rows: 5, columns: 6);
            }
            else
            {
                if (GUILayout.Button("Stop"))
                {
                    _gameManager.TryStopCurrentSession();
                    return;
                }

                GUILayout.Label($"Turns: {_gameManager.CurrentGameSession.Turns}, Matches: {_gameManager.CurrentGameSession.Matches}");

                GUILayout.BeginHorizontal();
                for (var column = 0; column < _gameManager.CurrentGameSession.Columns; column++)
                {
                    GUILayout.BeginVertical();
                    for (var row = 0; row < _gameManager.CurrentGameSession.Rows; row++)
                    {
                        var state = _gameManager.CurrentGameSession.GetState(row, column);
                        if (state.IsResolved)
                            GUILayout.Button("#");
                        else
                        {
                            if (GUILayout.Button(state.Type.ToString()))
                                _gameManager.CurrentGameSession.OnInput(new BoardLocation(row, column));
                        }
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}