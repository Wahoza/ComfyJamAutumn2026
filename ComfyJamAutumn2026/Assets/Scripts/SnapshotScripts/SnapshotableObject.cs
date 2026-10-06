using System.Collections.Generic;
using UnityEngine;

public class SnapshotableObject : MonoBehaviour
{
    [SerializeField] public List<Collider2D> boundings;

    [SerializeField] string _snapshotObjectName;
    [SerializeField] List<string> _descriptors;

    protected void OnEnable()
    {
        SnapshotManager.Instance.RegisterSnapshotableObject(this);
    }

    protected void OnDisable()
    {
        SnapshotManager.NullableInstance?.UnregisterSnapshotableObject(this);
    }

    public void SetDescriptors(List<string> newDescriptors)
    {
        _descriptors = newDescriptors;
    }

    public SnapshotCapturedItemData GetSnapshotData()
    {
        return new SnapshotCapturedItemData() { name = _snapshotObjectName, descriptors = _descriptors };
    }
}
