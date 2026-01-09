using UnityEngine;

public class CurveTrackSegment : TrackSegment 
{
    [Header("Curva")]
    public float turnAngle = 30f;  // direita + / esquerda -
    
    public override void CalculateConnection() {
        // converte graus para rotação
        connectionRot = Quaternion.Euler(0, turnAngle, 0);
        
        // ponto final (considera curva)
        Vector3 curveOffset = Quaternion.Euler(0, turnAngle * 0.5f, 0) * Vector3.forward * 20f;
        connectionPoint = curveOffset;
        
        heightOffset = 0f;
        segmentType = "Curva_" + turnAngle + "°";
    }
}
