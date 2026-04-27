using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int index;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Kart")) {
            CheckpointManager.instance.CheckpointReached(index);
        }
    }
}
