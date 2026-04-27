using UnityEngine;

public class KartRespawn : MonoBehaviour
{
    // referência ao CheckpointSpawner para acessar os pontos de respawn
    public CheckpointSpawner checkpointSpawner;

    // limite de queda para respawnar o kart
    public float fallLimit = -10f;

    void Update()
    {
        if (transform.position.y < fallLimit) {
            Respawn();
        }
    }

    // método para respawnar o kart no último checkpoint alcançado
    void Respawn() {
        int lastCheckpoint = (CheckpointManager.instance.nextCheckpoint - 1 + CheckpointManager.instance.totalCheckpoints) % CheckpointManager.instance.totalCheckpoints;
        
        transform.position = checkpointSpawner.instance.splinePoints[lastCheckpoint * checkpointSpawner.checkpointInterval];
        //transform.rotation = checkpointSpawner.instance.splinePoints[lastCheckpoint * checkpointSpawner.checkpointInterval];
    }
}
