using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private CanvasScaler UICameraCanvasScaler;
    [SerializeField] private RectTransform UICameraPanel;
    [SerializeField] private RawImage UICameraTextureHolder;
    [SerializeField] private RenderTexture templateRenderTexture;
    [SerializeField] private Camera renderCamera;

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


        float aspectScreen = 0;
        float aspectRect = 0; 
        float aspectScaler = 0; 

        float widthInPixels = 0; 

        //Adjust in case width < height
        switch (usingHeight)
        {
            case true:
                aspectScreen = (float)Screen.width / (float)Screen.height;
                aspectRect = cameraVisualRect.size.x / cameraVisualRect.size.y;
                aspectScaler = UICameraCanvasScaler.referenceResolution.x / UICameraCanvasScaler.referenceResolution.y;

                float heightInPixels = cameraVisualRect.height / (UICameraCanvasScaler.referenceResolution.y / Screen.height);
                widthInPixels = heightInPixels * aspectRect;
                break;

            case false:
                aspectScreen = (float)Screen.width / (float)Screen.height;
                aspectRect = cameraVisualRect.size.x / cameraVisualRect.size.y;
                aspectScaler = UICameraCanvasScaler.referenceResolution.x / UICameraCanvasScaler.referenceResolution.y;
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
