using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SnapshotOccluderComponent : MonoBehaviour
{
    [SerializeField] Image _occlusionHolder;
    [SerializeField] Material _occlusionMat;
    [SerializeField] CanvasScaler _occlisionCanvas;
    [SerializeField] float _fOcclisionDuration;
    [SerializeField] float _fOcclisionOutDuration;
    [SerializeField] AnimationCurve _occlusionCurve;
    [SerializeField] AnimationCurve _occlusionOutCurve;

    private Coroutine _occlusionRoutine;
    private float _startingVal;
    private Material _instancedOcclusionMat;
    private string _sRenderHeight = "_Renderheight";

    float _empty
    {
        get
        {
            return -_occlisionCanvas.referenceResolution.y / 2 - 1;
        }
    }
    float _filled
    {
        get
        {
            return _occlisionCanvas.referenceResolution.y / 2 + 1;
        }
    }

    private void Start()
    {
        _instancedOcclusionMat = Instantiate(_occlusionMat);
        _occlusionHolder.material = _instancedOcclusionMat;

        _instancedOcclusionMat.SetFloat(_sRenderHeight, _empty);
    }

    [NaughtyAttributes.Button]
    public void Occlude()
    {
        if(_occlusionRoutine != null )
            StopCoroutine(_occlusionRoutine);

        _occlusionRoutine = StartCoroutine(OcclusionRoutine());
    }

    private IEnumerator OcclusionRoutine()
    {
        float evaluatedFill = _filled;
        float evaluatedEmpty = _empty;
        float elapsed = 0;
        float direction = evaluatedFill - evaluatedEmpty;
        yield return null;

        while (elapsed < _fOcclisionDuration) { 
            
            float fillVal = direction * _occlusionCurve.Evaluate(elapsed / _fOcclisionDuration) + evaluatedEmpty;

            elapsed += Time.deltaTime;

            _instancedOcclusionMat.SetFloat(_sRenderHeight, fillVal);

            yield return null;
        }

        _instancedOcclusionMat.SetFloat(_sRenderHeight, _filled);

    }

    [NaughtyAttributes.Button]
    public void DeOcclude()
    {
        if (_occlusionRoutine != null)
            StopCoroutine(_occlusionRoutine);

        StartCoroutine(OcclusionOutRoutine());
    }

    private IEnumerator OcclusionOutRoutine()
    {
        float evaluatedFill = _filled;
        float evaluatedEmpty = _empty;
        float elapsed = 0;
        float direction = evaluatedFill - evaluatedEmpty;
        yield return null;

        while (elapsed < _fOcclisionOutDuration)
        {

            float fillVal = direction * _occlusionOutCurve.Evaluate(_fOcclisionOutDuration - (elapsed / _fOcclisionOutDuration)) + evaluatedEmpty;

            _instancedOcclusionMat.SetFloat(_sRenderHeight, fillVal);

            yield return null;

            elapsed += Time.deltaTime;

        }

        _instancedOcclusionMat.SetFloat(_sRenderHeight, _empty);

    }
}
