using UnityEngine;

public class KartCameraFollow : MonoBehaviour
{
    public Transform target;              // o kart
    public Vector3 offset = new Vector3(0f, 3f, -6f);
    public float followSpeed = 5f;        // quão rápido a câmera acompanha
    public float rotateSpeed = 5f;        // quão rápido gira olhando pro kart

    void LateUpdate()
    {
        if (target == null) return;

        // posição desejada: atrás do kart, com um offset na direção dele
        Vector3 desiredPos = target.position + target.TransformDirection(offset);

        // move suavemente até essa posição
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSpeed * Time.deltaTime
        );

        // rotação desejada: olhar para o kart
        Quaternion desiredRot = Quaternion.LookRotation(
            target.position - transform.position,
            Vector3.up
        );

        // gira suavemente para olhar pro kart
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRot,
            rotateSpeed * Time.deltaTime
        );
    }
}
