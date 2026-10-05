using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Handles taking the actual Image and creating relevant Data Sturctures
/// </summary>
public class SnapshotPictureTakingComponent : MonoBehaviour
{
    RenderTexture _currentRT;

    [SerializeField] int _iStoredImageWidth = 300;
    [SerializeField] Camera _snapshotCam;


    [Header("Debug")]
    [SerializeField] RawImage debugOutput;
    bool _bIsMouseButtonDown;

    //CHANGE THIS TO USE ACTUAL INPUT MAPPING
    private void Update()
    {
        if (Mouse.current.leftButton.IsPressed())
        {
            if (!_bIsMouseButtonDown)
            {
                TakeImage();
                _bIsMouseButtonDown = true;
            }
        }
        else
        {
            _bIsMouseButtonDown = false;
        }
    }

    /// <summary>
    /// Handles Picture Taking, and attempts to find capturable objects within cameras frustum
    /// </summary>
    [NaughtyAttributes.Button]
    public void TakeImage()
    {
        if(_currentRT == null)
            return;
        
        RenderTexture imageTexture = new(_currentRT.descriptor);

        int targetWidth = _iStoredImageWidth;
        int targetHeight = (int)((float)_iStoredImageWidth / ((float)_currentRT.width / _currentRT.height));

        imageTexture.width = targetWidth;
        imageTexture.height = targetHeight;

        _snapshotCam.targetTexture = imageTexture;
        _snapshotCam.Render();
        _snapshotCam.targetTexture = _currentRT;

        if (debugOutput)
        {
            debugOutput.texture = imageTexture;
            debugOutput.SetNativeSize();
        }
    }

    public void SetCurrentRenderTexture(RenderTexture currentTexture)
    {
        _currentRT = currentTexture;
    }
}
