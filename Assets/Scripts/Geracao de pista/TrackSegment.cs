using UnityEngine;

public class TrackSegment : MonoBehaviour
{
    [Header("Conexaõ com próximo segmento")]
    public Vector3 connectionPoint = Vector3.forward * 20f;     // onde encaixa próximo
    public Quaternion connectionRot = Quaternion.identity;      // como fica virado
    public float heightOffset = 0f;                             // ajuste de altura

    [Header ("Informações do segmento")]
    public string segmentType = "Reta";          // tipo do segmento
    public float segmentLength = 20f;            // comprimento do segmento

    [Header("Debug Visual")]
    public bool showConnectionGizmo = true;     // mostrar gizmo de conexão

    [HideInInspector] public bool isPlaced = false; // se o segmento já foi colocado na pista

    void Update() {
        // mostra linha verde até o connection point (só no editor)
        if (showConnectionGizmo && !Application.isPlaying) {
            Debug.DrawLine(transform.position, 
                           transform.TransformPoint(connectionPoint), 
                           Color.green, 0.1f);
        }
    }

    void OnDrawGizmos() {
        if (!showConnectionGizmo) return;

        // mostra esfera no ponto de conexão
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.TransformPoint(connectionPoint), 1f);
    
        // linha até connectionPoint
        Gizmos.DrawLine(transform.position, 
                           transform.TransformPoint(connectionPoint));
    }

    public virtual void CalculateConnection() {
        // filhos vão estar completando!! 
    }

}
