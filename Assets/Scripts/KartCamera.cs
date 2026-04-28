using UnityEngine;

public class KartCamera : MonoBehaviour
{
    [SerializeField] private float speedFollow;
    [SerializeField] private float speedRotate;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    private Vector3 targetPosition;

    private void FixedUpdate() {
        if (target != null) {
            FollowTarget();
        }
    }

    private void FollowTarget() {
        targetPosition = target.TransformPoint(offset);

        transform.position = Vector3.Lerp(transform.position, targetPosition, speedFollow * Time.deltaTime);

        Vector3 distanceTarget = target.position - transform.position;
        Quaternion newRotation = Quaternion.LookRotation(distanceTarget, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, speedRotate * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget) {
        target = newTarget;
    }
}
