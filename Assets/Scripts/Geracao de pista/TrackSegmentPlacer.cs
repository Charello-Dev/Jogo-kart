using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrackSegmentPlacer : MonoBehaviour
{
    [Header("Layers")]
    public int detectLayer = 9;
    public int groundLayer = 8;

    [Header("Overlap (box)")]
    public Vector3 boundsPadding = Vector3.one * 0.2f;
    public int maxRetries = 5;

    [Header("Referências")]
    public TrackCleaner cleaner;

    private Transform currentSpawnPoint;                // onde o próximo segmento deve nascer
    private readonly List<GameObject> spawnedSegments = new();   // lista de segmentos já colocados
    private readonly List<Bounds> debugBounds = new();

    [Header("Debug Gizmos")]
    public bool drawBounds = true;
    public bool drawSpawnPoints = true;
    public bool drawTrackFlow = true;


    // inicialização do sistema
    public void Initialize(Transform startPoint) {
        currentSpawnPoint = startPoint;
        spawnedSegments.Clear();
        debugBounds.Clear();
    }

    // segmento procedural
    public bool TryPlaceSegment(TrackSegmentSO segment) {
        if (segment == null || segment.prefab == null) { return false; }
        
        for (int i = 0; i < maxRetries; i++) {
            GameObject obj = Instantiate(segment.prefab);
            SetLayerRecursively(obj, detectLayer);      // bota na layer de detecção

            if (!TryAlignSegment(obj, out Transform end)) {
                Destroy(obj);
                continue;
            }

            // força o Unity a atualizar os colliders antes do teste de overlap
            Physics.SyncTransforms();

            // testa se o segmento colide com outro já existente
            if (!HasBoxOverlap(obj)) {
                FinalizeSegment(obj, end);
                return true;
            } 

            Destroy(obj);
        }

        return false;
    }

    public void PlaceFixedSegment(TrackSegmentSO segment) {

        if (segment == null || segment.prefab == null) { return; }

        GameObject obj = Instantiate(segment.prefab);
        SetLayerRecursively(obj, groundLayer);

        if (!TryAlignSegment(obj, out Transform end)) {
            Destroy(obj);
            return;
        }

        FinalizeSegment(obj, end);
    }

    // fechar loop
    public bool TryCloseLoop(Transform targetStartPoint, TrackSegmentSO closingSegment) {
        if (closingSegment == null || closingSegment.prefab == null) { return false; }
    
        for (int i = 0; i < maxRetries; i++) {
            GameObject obj = Instantiate(closingSegment.prefab);
            SetLayerRecursively(obj, detectLayer);

            Transform start = FindPoint(obj.transform, "StartPointTag");
            Transform end   = FindPoint(obj.transform, "EndPointTag");

            if (start == null || end == null)
            {
                Destroy(obj);
                continue;
            }

            // encaixa no fim da pista
            obj.transform.position = currentSpawnPoint.position - start.localPosition;
            obj.transform.rotation = currentSpawnPoint.rotation;

            // corrige rotação para alinhar com o início da pista
            Quaternion delta =
                Quaternion.FromToRotation(end.forward, targetStartPoint.forward);

            obj.transform.rotation = delta * obj.transform.rotation;

            Physics.SyncTransforms();

            if (!HasBoxOverlap(obj))
            {
                FinalizeSegment(obj, end);
                return true;
            }

            Destroy(obj);
        }

        return false;
    }

    bool TryAlignSegment(GameObject obj, out Transform endPoint) {
        endPoint = null;

        Transform start = FindPoint(obj.transform, "StartPointTag");
        Transform end   = FindPoint(obj.transform, "EndPointTag");

        if (start == null || end == null) { return false; }
    
        obj.transform.position = currentSpawnPoint.position - start.localPosition;
        obj.transform.rotation = currentSpawnPoint.rotation;

        endPoint = end;
        return true;
    }

    // overlap mais robusto
    bool HasBoxOverlap(GameObject obj) {
        Bounds bounds = CalculateWorldBounds(obj);
        Vector3 halfExtents = bounds.extents + boundsPadding;

        Collider[] hits = Physics.OverlapBox(
            bounds.center, 
            halfExtents, 
            obj.transform.rotation,
            1 << detectLayer
        );
        return hits.Any(h =>
            h.transform.root.gameObject != obj &&
            spawnedSegments.Contains(h.transform.root.gameObject)
        );
    }

    // finalização
    void FinalizeSegment(GameObject obj, Transform endPoint) {
        SetLayerRecursively(obj, groundLayer);          // bota na layer do chão
        spawnedSegments.Add(obj);                       // registra o segmento
        cleaner.Register(obj);                          // avisa o limpador
        
        currentSpawnPoint = endPoint != null ? endPoint : obj.transform;

        Bounds b = CalculateWorldBounds(obj);
        debugBounds.Add(b);
    }

    // utilitários
    Transform FindPoint(Transform root, string tag) {
        foreach (Transform t in root.GetComponentsInChildren<Transform>()) {
            if (t.CompareTag(tag)) { return t; }
        }

        return null;
    }    

    void SetLayerRecursively(GameObject obj, int layer) {
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerRecursively(t.gameObject, layer);
    }

    Bounds CalculateWorldBounds(GameObject obj) {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.zero);
    
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        return bounds;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // anti-overlap bounds
        if (drawBounds)
        {
            Gizmos.color = Color.red;
            foreach (Bounds b in debugBounds)
            {
                Gizmos.DrawWireCube(b.center, b.size);
            }
        }

        // spawn point
        if (drawSpawnPoints && currentSpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentSpawnPoint.position, 0.3f);
            Gizmos.DrawRay(currentSpawnPoint.position, currentSpawnPoint.forward * 2f);
        }

        if (drawTrackFlow)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < spawnedSegments.Count - 1; i++)
            {
                if (spawnedSegments[i] != null && spawnedSegments[i + 1] != null)
                {
                    Gizmos.DrawLine(
                        spawnedSegments[i].transform.position,
                        spawnedSegments[i + 1].transform.position
                    );
                }
            }
        }
    }

}
