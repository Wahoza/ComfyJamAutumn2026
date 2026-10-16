using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that handles Snapshotable Objects and Camera.
/// </summary>
public class SnapshotManager : MonoBehaviour
{
    private static SnapshotManager m_instance;

    private List<PageComponentWithConditions> _pageWithConditionListeners;

    /// <summary>
    /// Instance of SnapshotManager. If SnapshotManager is not instanced, creates a new SnapshotManager.
    /// </summary>
    public static SnapshotManager Instance {

        get
        {
            if (m_instance == null)
            {
                GameObject holder = new();
                holder.name = "SnapshotManager";

                m_instance = holder.AddComponent<SnapshotManager>();
                m_instance.Initialize();
                
                DontDestroyOnLoad(holder);
            }
            return m_instance;
        }
    }

    /// <summary>
    /// Instance of SnapshotManager. If SnapshotManager is not instanced, returns null.
    /// </summary>
    public static SnapshotManager NullableInstance 
    {
        get
        {
            return m_instance;
        }
    }

    private List<SnapshotableObject> _activeSnapshotableObjects;

    public void Initialize()
    {
        _activeSnapshotableObjects = new List<SnapshotableObject>();
        _pageWithConditionListeners = new();
    }

    private void Awake()
    {
        if (m_instance != null) {

            if (m_instance != this)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            m_instance = this;
        }
    }
    void OnDestroy()
    {
        if (m_instance != null)
        {
            Destroy(gameObject);
        }
    }

    public void RegisterSnapshotableObject(SnapshotableObject newSnapshotObject)
    {
        if(newSnapshotObject)
            _activeSnapshotableObjects.Add(newSnapshotObject);
    }

    public void UnregisterSnapshotableObject(SnapshotableObject snapshotObject) 
    {
        if(snapshotObject != null && _activeSnapshotableObjects.Contains(snapshotObject))
            _activeSnapshotableObjects.Remove(snapshotObject);
    }

    public List<SnapshotableObject> GetSnapshotableObjects()
    {
        return _activeSnapshotableObjects;
    }

    public bool TakeSnapshotAndAttemptForJournal(SnapshotPictureData data)
    {
        bool success = false;

        foreach(var item in _pageWithConditionListeners)
        {
            if (item.DoPictureInsertion(data))
            {
                success = true;
            }
        }

        return success;
    }

    public void RegisterPageWithCondition(PageComponentWithConditions page)
    {
        if (!_pageWithConditionListeners.Contains(page))
            _pageWithConditionListeners.Add(page);
    }

    public void UnregisterPageWithCondition(PageComponentWithConditions page)
    {
        if (_pageWithConditionListeners.Contains(page))
            _pageWithConditionListeners.Remove(page);
    }
}
