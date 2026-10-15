using UnityEngine;

/// <summary>
/// Edge of a room that a RoomDoor sits on.
/// </summary>
public enum DoorSide
{
    North,
    East,
    South,
    West
}

public static class DoorSideExtensions
{
    /// <summary>
    /// Direction pointing out of the room through this side.
    /// </summary>
    public static Vector2 ToVector(this DoorSide side)
    {
        switch (side)
        {
            case DoorSide.North: return Vector2.up;
            case DoorSide.East: return Vector2.right;
            case DoorSide.South: return Vector2.down;
            default: return Vector2.left;
        }
    }

    /// <summary>
    /// Side on the opposite edge of a room. Leaving through East means arriving through West.
    /// </summary>
    public static DoorSide Opposite(this DoorSide side)
    {
        switch (side)
        {
            case DoorSide.North: return DoorSide.South;
            case DoorSide.East: return DoorSide.West;
            case DoorSide.South: return DoorSide.North;
            default: return DoorSide.East;
        }
    }
}
