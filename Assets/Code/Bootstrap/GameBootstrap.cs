using Code.GameManagement;
using Code.Presentation;
using UnityEngine;

namespace Code.Bootstrap
{
    /// <summary>
    /// Will be responsible for wiring all the initial services (like game manager and presentation).
    /// Object has to be in the bootstrap scene.
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameViewController _gameViewControllerPrefab;
        private GameManager _gameManager;

        private void Start()
        {
            // Create game manager.
            _gameManager = new GameManager();

            // Initialize view.
            var gameViewController = Instantiate(_gameViewControllerPrefab);
            gameViewController.Initialize(_gameManager);
        }

        private void Update()
        {
            _gameManager.Update(Time.deltaTime);
        }
    }
}