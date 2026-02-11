using UnityEngine;

public enum SegmentType
{
    Straight,
    Curve_Left_Open,
    Curve_Left_Medium,
    Curve_Left_Tight,
    Curve_Right_Open,
    Curve_Right_Medium,
    Curve_Right_Tight,
    Hill_Up,
    Hill_Down,
    Bump,
    S_Curve,
    Item_Box_Line,
    StartFinish_Line,
    StartPosition_Line,
}

[CreateAssetMenu(fileName = "TrackSegment", menuName = "Track/Segment")]
public class TrackSegmentSO : ScriptableObject
{
    public SegmentType type;
    public GameObject prefab;

    // quanto maior mais aparece, quanto menor menos aparece
    public float weight = 1f; 
    
}
