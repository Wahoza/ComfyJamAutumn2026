using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// <summary>
/// This Component is in charge of setting up the Render Texture, and moving the camera & visuals.
/// </summary>
public class SnapshotCameraComponent : MonoBehaviour
{
    [Header("Required Elements")]
    [Required][SerializeField] private CanvasScaler UICameraCanvasScaler;
    [Required][SerializeField] private RectTransform UICameraPanel;
    [Required][SerializeField] private RawImage UICameraTextureHolder;
    [Required][SerializeField] private RenderTexture templateRenderTexture;
    [Required][SerializeField] private Camera renderCamera;

    [Header("Parameters")]
    [SerializeField] private float _fTextureResolutionScaling = 1;
    [SerializeField] private float _fCameraZoom = 1.2f;
    [SerializeField] private Vector2 _vCameraWorldOffset;
    
    private Vector2 _fPrevMousePosition = new Vector2( float.MaxValue, float.MaxValue );
    private RenderTexture _rtInstancedRenderTexture;

    void OnEnable()
    { 
        StartCoroutine(SetUpCameraFromEnable());
    }
    IEnumerator SetUpCameraFromEnable() { 
        yield return null;
        SetUpCamera();
    }

    /// <summary>
    /// Set Up the Snapshot Camera, Including Render Texture and Camera, and assign these items
    /// </summary>
    void SetUpCamera()
    {
        //Get Values
        Rect cameraVisualRect = UICameraPanel.rect;

        bool usingHeight = Screen.width < Screen.height;

        float aspectRect = cameraVisualRect.size.x / cameraVisualRect.size.y;
        float widthInPixels = 0; 

        //Adjust in case width < height (required because of Canvas scaler)
        switch (usingHeight)
        {
            case true:
                float heightInPixels = cameraVisualRect.height / (UICameraCanvasScaler.referenceResolution.y / Screen.height);
                widthInPixels = heightInPixels * aspectRect;
                break;

            case false:
                widthInPixels = cameraVisualRect.width / (UICameraCanvasScaler.referenceResolution.x / Screen.width);
                break;
        }

        //Setup Render Texture

        if (_rtInstancedRenderTexture != null)
            Destroy(_rtInstancedRenderTexture);

        _rtInstancedRenderTexture = null;

        _rtInstancedRenderTexture = new(templateRenderTexture.descriptor);

        _rtInstancedRenderTexture.width = Mathf.CeilToInt(widthInPixels * _fTextureResolutionScaling);
        _rtInstancedRenderTexture.height = Mathf.CeilToInt(widthInPixels / aspectRect * _fTextureResolutionScaling);


        renderCamera.targetTexture = _rtInstancedRenderTexture;
        UICameraTextureHolder.texture = _rtInstancedRenderTexture;


        //Rescale Camera Orthographic Size

        float finalCamOrthoSizeHeight = widthInPixels / aspectRect / (float)Screen.height * Camera.main.orthographicSize;
        renderCamera.orthographicSize = finalCamOrthoSizeHeight * 1 / _fCameraZoom;

        GetComponent<SnapshotPictureTakingComponent>()?.SetCurrentRenderTexture(_rtInstancedRenderTexture);
    }

    public Rect GetWorldRect()
    {
        Rect cameraVisualRect = UICameraPanel.rect;

        float screenPositionY = cameraVisualRect.position.y / (UICameraCanvasScaler.referenceResolution.y / (float)Screen.height);

        Vector2 position = Camera.main.ScreenToWorldPoint(UICameraPanel.position);

        bool usingHeight = Screen.width < Screen.height;

        float aspectRect = cameraVisualRect.size.x / cameraVisualRect.size.y;
        float widthInPixels = 0;
        float heightInPixels = 0;

        //Adjust in case width < height
        Debug.Log(usingHeight);
        switch (usingHeight)
        {
            case true:
                heightInPixels = cameraVisualRect.height / (UICameraCanvasScaler.referenceResolution.y / Screen.height);
                widthInPixels = heightInPixels * aspectRect;
                break;

            case false:
                widthInPixels = cameraVisualRect.width / (UICameraCanvasScaler.referenceResolution.x / Screen.width);
                heightInPixels = widthInPixels / aspectRect;
                break;
        }

        Vector2 size = Camera.main.ScreenToWorldPoint(new Vector2(widthInPixels, heightInPixels)) - Camera.main.ScreenToWorldPoint(Vector2.zero);
        size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y)) * 1 / _fCameraZoom;

        return new Rect(position - size / 2, size);
    }

    public void OnMousePositionChange(Vector2 newMousePosition)
    {
        if(_fPrevMousePosition == newMousePosition)
            return;

        _fPrevMousePosition = newMousePosition;

        UICameraPanel.position = newMousePosition;

        var pos = Camera.main.ScreenToWorldPoint(newMousePosition);
        renderCamera.transform.position = pos + (Vector3)_vCameraWorldOffset;
    }

    private void Update()
    {
        OnMousePositionChange(Mouse.current.position.value);
    }
}
