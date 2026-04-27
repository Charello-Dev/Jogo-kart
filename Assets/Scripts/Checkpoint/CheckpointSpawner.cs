using UnityEngine;

public class CheckpointSpawner : MonoBehaviour
{
    public TrackGenerator instance;

    public int checkpointInterval = 20;

    public GameObject CheckpointPrefab;

    public void SpawnCheckpoints() {
        int index = 0;

        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < instance.splinePoints.Count; i+= checkpointInterval) {
            Vector3 position = instance.splinePoints[i];
            Quaternion rotation = Quaternion.identity;
            GameObject checkpoint = Instantiate(CheckpointPrefab, position, rotation, transform) as GameObject;
            checkpoint.GetComponent<Checkpoint>().index = index++;
        }

        CheckpointManager.instance.totalCheckpoints = index;
        Debug.Log($"Total de checkpoints: {CheckpointManager.instance.totalCheckpoints}");

    }
}
