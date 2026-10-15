using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

public class PageComponentWithConditions : PageComponent
{
    [SerializedDictionary("Game Object to show", "Picture name")]
    [SerializeField] SerializedDictionary<GameObject, ConditionObject> contentConditionDictionary;

    [System.Serializable]
    struct ConditionObject
    {
        public string conditionName;
        public RawImage imageHolder;
        public RectTransform targetSize;
        public ImageAdjustment adjustment;
    }

    public enum ImageAdjustment
    {
        AdjustToHeight = 2, AdjustToWidth = 1, Average = 0
    }
    public override void Initalize()
    {
        base.Initalize();

        SnapshotManager.Instance.RegisterPageWithCondition(this);

        foreach (var condition in contentConditionDictionary)
        {
            if(condition.Value.conditionName != "")
            {
                condition.Key.gameObject.SetActive(false);
            }
        }
    }

    public bool DoPictureInsertion(SnapshotPictureData pictureData)
    {
        foreach (var condition in contentConditionDictionary)
        {
            if(pictureData.capturedItems.Exists((item) => item.name == condition.Value.conditionName))
            {
                condition.Key.gameObject.SetActive(true);

                AdjustImageHolder(pictureData.picture, condition.Value.targetSize, condition.Value.imageHolder, condition.Value.adjustment);

                return true;
            }
        }

        return false;
    }

    private void AdjustImageHolder(RenderTexture tex, RectTransform imageSizeRefrence, RawImage holder, ImageAdjustment adjustment)
    {
        var deltaTexture = new Vector2(tex.width, tex.height);


        var deltaTarget = imageSizeRefrence.sizeDelta;


        //if anchors are set to stretch

        Debug.Log(deltaTarget);
        if (deltaTarget.x == 0)
            deltaTarget.x = imageSizeRefrence.rect.xMax - imageSizeRefrence.rect.xMin;

        if (deltaTarget.y == 0)
            deltaTarget.y = imageSizeRefrence.rect.yMax - imageSizeRefrence.rect.yMin;

        Debug.Log(deltaTarget);

        float heightDevider = deltaTarget.y / deltaTexture.y;
        float widthDevider = deltaTarget.x / deltaTexture.x;

        float usedDevider = 0;

        switch (adjustment)
        {
            case ImageAdjustment.AdjustToHeight:
                usedDevider = heightDevider;
                break;
            case ImageAdjustment.AdjustToWidth:
                usedDevider = widthDevider;
                break;
            case ImageAdjustment.Average:
                usedDevider = (heightDevider + widthDevider) / 2;
                break;
        }

        holder.rectTransform.sizeDelta = deltaTexture * usedDevider;
        holder.texture = tex;
    }
}
