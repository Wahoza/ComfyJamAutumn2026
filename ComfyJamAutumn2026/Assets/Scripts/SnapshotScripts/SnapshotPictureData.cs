using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SnapshotPictureData
{
    public SnapshotPictureData()
    {
        picture = null;
        capturedItems = new();
    }
    public RenderTexture picture;

    public List<SnapshotCapturedItemData> capturedItems;
}

public struct SnapshotCapturedItemData
{
    public string name;
    public List<string> descriptors;
}
