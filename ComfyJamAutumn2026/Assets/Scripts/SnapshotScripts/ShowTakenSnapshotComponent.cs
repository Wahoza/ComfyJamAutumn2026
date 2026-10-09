using NaughtyAttributes;
using Sequences;
using UnityEngine;
using UnityEngine.UI;

public class ShowTakenSnapshotComponent : MonoBehaviour
{
    private enum Adjustment
    {
        AdjustToHeight,
        AdjustToWidth,
        Average,
    }
    [Header("Image Holder")]
    [Required][SerializeField] RawImage _imageHolder;
    [Required][SerializeField] RectTransform _imageTargetSizeRefrence;
    [SerializeField] Adjustment _imageAdjustment;
    [Header("Sequence Data")]
    [SerializeField] SequenceComponent _startingComponent;
    [SerializeField] SequenceBlackboardComponent _blackboardComponent;
    [SerializeField] string _objectSaveableSequenceString;

    [Header("Gamestate Data")]
    [SerializeField] IGameState sequenceGameState;


    public void StartShowingImage(SnapshotPictureData data)
    {
        GameStateManager.Instance?.RequestGameStateSwitch(sequenceGameState);

        AdjustImageHolder(data.picture);

        _blackboardComponent.AddToDictionary(_objectSaveableSequenceString, GetIsToBeSaved(data), true);

        _startingComponent.PlaySequence();
    }

    private void AdjustImageHolder(RenderTexture tex)
    {
        var deltaTexture = new Vector2(tex.width, tex.height);


        var deltaTarget = _imageTargetSizeRefrence.sizeDelta;


        //if anchors are set to stretch

        Debug.Log(deltaTarget);
        if(deltaTarget.x == 0)
            deltaTarget.x = _imageTargetSizeRefrence.rect.xMax - _imageTargetSizeRefrence.rect.xMin;

        if (deltaTarget.y == 0)
            deltaTarget.y = _imageTargetSizeRefrence.rect.yMax - _imageTargetSizeRefrence.rect.yMin;

        Debug.Log(deltaTarget);

        float heightDevider = deltaTarget.y / deltaTexture.y;
        float widthDevider = deltaTarget.x / deltaTexture.x;

        float usedDevider = 0;

        switch (_imageAdjustment)
        {
            case Adjustment.AdjustToHeight:
                usedDevider = heightDevider;
                break;
            case Adjustment.AdjustToWidth:
                usedDevider = widthDevider;
                break;
            case Adjustment.Average:
                usedDevider = (heightDevider + widthDevider) / 2;
                break;
        }
        
        _imageHolder.rectTransform.sizeDelta = deltaTexture * usedDevider;
        _imageHolder.texture = tex;
    }

    private bool GetIsToBeSaved(SnapshotPictureData data)
    {
        return false;
    }
}
