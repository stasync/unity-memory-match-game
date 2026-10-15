using System;
using System.Collections;
using Code.GameManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    internal sealed class CardView : MonoBehaviour
    {
        private event Action<BoardLocation> _clicked;

        [SerializeField, Range(0.01f, 0.35f)] private float _animationTime = 0.25f;
        [SerializeField] private Image _image;
        [SerializeField] private Button _button;
        [SerializeField] private AnimationCurve _revealIconAnimation = new();
        [SerializeField] private AnimationCurve _hideIconAnimation = new();

        private Coroutine _currentCoroutine;
        private bool _isVisible;

        private BoardLocation _location;

        private void Awake() =>
            _button.onClick.AddListener(OnClick);

        internal void Init(Sprite sprite, BoardLocation location, Action<BoardLocation> clicked)
        {
            _image.sprite = sprite;
            _image.enabled = true;
            _image.transform.localScale = Vector3.one;
            _isVisible = true;
            _button.interactable = false;

            // Coroutines were stopped when the card was deactivated by the pool.
            _currentCoroutine = null;

            _location = location;
            _clicked = clicked;
        }

        internal void SetInteractable(bool value)
        {
            _button.interactable = value;
        }

        internal void SetVisible(bool value)
        {
            // Don't replay the animation if the card is already in the requested state.
            if (_isVisible == value)
                return;

            _isVisible = value;
            var curve = value ? _revealIconAnimation : _hideIconAnimation;
            if (_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);

            _currentCoroutine = StartCoroutine(AnimateRoutine(curve));
        }

        private void OnClick() =>
            _clicked?.Invoke(_location);

        /// <summary>
        /// Simple animation coroutine, far from perfect, but just to have something to begin with.
        /// </summary>
        private IEnumerator AnimateRoutine(AnimationCurve curve)
        {
            // Curves are authored over a normalized [0, 1] time range.
            var elapsedTime = 0f;
            while (elapsedTime < _animationTime)
            {
                SetIconScale(curve.Evaluate(elapsedTime / _animationTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            SetIconScale(curve.Evaluate(1f));
            _currentCoroutine = null;
        }

        private void SetIconScale(float scaleFactor) =>
            _image.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }
}