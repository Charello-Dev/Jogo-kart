using UnityEngine;


public class TrackGenerator : MonoBehaviour
{
    [Header("Referências")]
    public TrackSeedManager seedManager;        // quem gerencia as seeds
    public TrackSegmentPlacer segmentPlacer;    // quem constroi a pista
    public TrackCleaner cleaner;                // quem limpa a pista

    [Header("Segmentos Fixos")]
    public TrackSegmentSO startPositionSegment;  // segmento inicial (posição de largada)
    public TrackSegmentSO startFinishSegment;    // segmento inicial/final (linha de chegada)   

    [Header("Config")]
    public int totalSegments = 20;              // até quantos segmentos a pista terá
    public Transform startPoint;                // onde começa a pista

    // gera a pista sem precisar entrar no modo de jogo
    [ContextMenu("Gerar Pista")]                
    public void GenerateTrack() {
        cleaner.Clear();
        seedManager.InitializeSeed();
        
        segmentPlacer.Initialize(startPoint);

        PlaceFixed(startPositionSegment); // coloca o segmento de posição de largada
        PlaceFixed(startFinishSegment);   // coloca o segmento de linha de chegada

        int proceduralCount = totalSegments - 2; // desconta os segmentos fixos

        // gera a pista com base no total de segmentos
        for (int i = 0; i < proceduralCount; i++) { 
            TrackSegmentSO segment = seedManager.GetNextSegment();
            segmentPlacer.TryPlaceSegment(segment);
        }
    }

    void PlaceFixed(TrackSegmentSO segment) {
        if (segment == null) return;
        segmentPlacer.PlaceFixedSegment(segment);
    }

    // limpa a pista sem precisar entrar no modo de jogo
    [ContextMenu("Limpar Pista")]
    public void ClearTrack() {
        cleaner.Clear();
    }

}
