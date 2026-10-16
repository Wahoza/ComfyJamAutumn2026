using System.Text;
using UnityEngine;

/// <summary>
/// Names a placed object by its scene and its spot in the hierarchy. The name is the same every time the scene loads,
/// so things that happened to the object can be remembered between room visits.
/// </summary>
public static class HierarchyId
{
    public static string Of(Transform transform)
    {
        StringBuilder path = new();
        for (Transform current = transform; current != null; current = current.parent)
            path.Insert(0, $"/{current.name}#{current.GetSiblingIndex()}");

        return transform.gameObject.scene.path + path;
    }
}
