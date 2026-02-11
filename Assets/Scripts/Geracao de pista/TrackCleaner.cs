using UnityEngine;
using System.Collections.Generic;

public class TrackCleaner : MonoBehaviour
{
    [Header("Configuração")]
    public Transform trackRoot;

    private readonly List<GameObject> spawnedSegments = new();

    public void Register(GameObject segment)
    {
        if (!spawnedSegments.Contains(segment))
            spawnedSegments.Add(segment);
    }

    public void Clear()
    {
        for (int i = spawnedSegments.Count - 1; i >= 0; i--)
        {
            GameObject seg = spawnedSegments[i];
            if (seg == null) continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(seg);
            else
                Destroy(seg);
#else
            Destroy(seg);
#endif
        }

        spawnedSegments.Clear();
    }
}
