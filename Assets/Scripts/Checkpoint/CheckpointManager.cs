using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    public int totalCheckpoints;

    public int nextCheckpoint;

    void Awake() {
        instance = this;
    }

    public void CheckpointReached(int index) {
        if (index == nextCheckpoint)
        {
            nextCheckpoint++;

            if (nextCheckpoint >= totalCheckpoints) {
                nextCheckpoint = 0;
            }
            
            Debug.Log($"Checkpoint {index} passou! Próximo: {nextCheckpoint}");
        }
    }
}
