using UnityEngine;

public class StraightTrackSegment : TrackSegment
{
    public override void CalculateConnection() {
        connectionPoint = Vector3.forward * 25f;
        connectionRot = Quaternion.identity;
        heightOffset = 0f;
        segmentType = "Reta";
    }
}
