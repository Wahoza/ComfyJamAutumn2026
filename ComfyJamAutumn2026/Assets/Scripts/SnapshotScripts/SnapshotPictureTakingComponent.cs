using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevelPhysics2D;
using UnityEngine.UI;

/// <summary>
/// Handles taking the actual Image and creating relevant Data Sturctures
/// </summary>

[RequireComponent(typeof(SnapshotCameraComponent))]
public class SnapshotPictureTakingComponent : MonoBehaviour
{
    RenderTexture _currentRT;

    [SerializeField] Camera _snapshotCam;
    [SerializeField] ShowTakenSnapshotComponent _showTakenSnapshot;

    [Header("Picture Settings")]
    [SerializeField] int _iStoredImageWidth = 300;
    [SerializeField] FilterMode _imageFilterMode = FilterMode.Point;

    bool _bIsMouseButtonDown;

    //CHANGE THIS TO USE ACTUAL INPUT MAPPING
    private void Start()
    {
        InputManager.InputActions.CameraControll.TakePicture.performed += OnTakeImageInput;
    }

    /// <summary>
    /// Handles Picture Taking, and attempts to find capturable objects within cameras frustum
    /// </summary>
    [NaughtyAttributes.Button]
    public SnapshotPictureData TakeImage()
    {
        //Capture Render Texture
        if(_currentRT == null)
            return new SnapshotPictureData{ };
        
        RenderTexture pictureTexture = new(_currentRT.descriptor);

        int targetWidth = _iStoredImageWidth;
        int targetHeight = (int)((float)_iStoredImageWidth / ((float)_currentRT.width / _currentRT.height));

        pictureTexture.width = targetWidth;
        pictureTexture.height = targetHeight;
        pictureTexture.filterMode = _imageFilterMode;
        _snapshotCam.targetTexture = pictureTexture;
        _snapshotCam.Render();
        _snapshotCam.targetTexture = _currentRT;

        var worldCameraRect = GetComponent<SnapshotCameraComponent>().GetWorldRect();




        List<SnapshotableObject> potentialObjects = SnapshotManager.Instance.GetSnapshotableObjects();

        List<SnapshotCapturedItemData> foundObjects = new();

        //float3 minWorldRect = new (worldCameraRect.xMin, worldCameraRect.yMin, 1);
        //float3 maxWorldRect = new(worldCameraRect.xMax, worldCameraRect.yMax, 1);
        //MinMaxAABB minMaxAABB = new MinMaxAABB(minWorldRect, maxWorldRect);//{Min = new float2() {, ), }, ;
        Bounds cameraWorldBounds = new();
        cameraWorldBounds.min = worldCameraRect.min;
        cameraWorldBounds.max = worldCameraRect.max;

        foreach (SnapshotableObject obj in potentialObjects)
        {
            foreach(var collider in obj.boundings)
            {
                if (collider.bounds.Intersects(cameraWorldBounds))
                {
                    foundObjects.Add(obj.GetSnapshotData());
                    break;
                }
            }
        }

        SnapshotPictureData output = new SnapshotPictureData();
        output.picture = pictureTexture;
        output.capturedItems = foundObjects;

        return output;
    }

    void OnTakeImageInput(InputAction.CallbackContext callbackContext)
    {
        _showTakenSnapshot.StartShowingImage(TakeImage());
    }
    public void SetCurrentRenderTexture(RenderTexture currentTexture)
    {
        _currentRT = currentTexture;
    }

}
