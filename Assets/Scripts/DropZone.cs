using UnityEngine;

[DisallowMultipleComponent]
public class DropZone : MonoBehaviour
{
    public enum ZoneType { Tower, Hole }
    public ZoneType zoneType = ZoneType.Tower;

    public TowerController tower;
}