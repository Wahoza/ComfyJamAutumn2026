using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnapshotBackgroundEffectComponent : MonoBehaviour, IGameStateListener
{
    [SerializeField] List<IGameState> _cameraStates;
    [SerializeField] Image _matHolder;
    [SerializeField] Material _mat;
    [SerializeField] float _fFillDuration;
    [SerializeField] float _fEmptyDuration;
    [SerializeField] AnimationCurve _fillCurve;
    [SerializeField] AnimationCurve _emptyCurve;

    private Coroutine _coroutine;
    private Material _instancedMat;
    private void Start()
    {
        _instancedMat = Instantiate(_mat);
        _matHolder.material = _instancedMat;

        (this as IGameStateListener).Subscribe();
    }

    void OnDestroy()
    {
        (this as IGameStateListener).Unsubscribe();
    }

    private void DoPopup() 
    { 
        if(_coroutine != null)
            StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(PopupRoutine());
    }
    private void DoEmpty()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(PopupCloseRoutine());

    }

    private IEnumerator PopupRoutine()
    {
        float elapsed = 0;

        while (elapsed < _fFillDuration)
        {
            float distance = elapsed / _fFillDuration;
            float evaluated = _fillCurve.Evaluate(distance);

            _instancedMat.SetFloat("_FillAmount", evaluated);

            yield return null;
            elapsed += Time.deltaTime;
        }

        _instancedMat.SetFloat("_FillAmount", 1);

    }

    private IEnumerator PopupCloseRoutine() {
        float elapsed = 0;

        while (elapsed < _fEmptyDuration)
        {
            float distance = 1 - (elapsed / _fEmptyDuration);
            float evaluated = _emptyCurve.Evaluate(distance);

            _instancedMat.SetFloat("_FillAmount", evaluated);

            yield return null;
            elapsed += Time.deltaTime;
        }

        _instancedMat.SetFloat("_FillAmount", 0);

    }

    public void NotifyStateChange(IGameState newState, IGameState oldState)
    {
        if(_cameraStates.Contains(newState) && !_cameraStates.Contains(oldState))
        {
            DoPopup();
        }
        else if (_cameraStates.Contains(oldState) && !_cameraStates.Contains(newState))
        {
            DoEmpty();
        }
    }
}
