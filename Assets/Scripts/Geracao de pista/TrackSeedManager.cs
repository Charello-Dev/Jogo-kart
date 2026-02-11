using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrackSeedManager : MonoBehaviour 
{
    [Header("Seed básica")]
    public bool useSmartSeed = true;      // se o sistema escolhe ou um valor fixo
    public int seed = 12345;              // seed padrão
    public string currentSeed;            // seed atual em uso

    [Header("Segmentos")]
    public List<TrackSegmentSO> segments;   // quais segmentos disponíveis

    [Header("Memória")]
    [Range(1, 5)]
    public int memorySize = 3;              // quantos segmentos lembrar para evitar repetições

    [Header("Ritmo")]
    public int maxCurvesInRow = 2;        // número máximo de curvas consecutivas permitidas
    public float straighBoost = 2f;       // peso extra para segmentos retos

    [Header("Smart Seed")]
    public int maxAttempts = 100;           // número máximo de tentativas para gerar uma seed válida

    private Queue<SegmentType> lastSegments = new(); 

    // chamada pelo TrackGenerator para iniciar a seed
    public void InitializeSeed() {
        if (useSmartSeed) { FindBestSeed(); } // tenta encontrar a melhor seed

        Random.InitState(seed);
        currentSeed = seed.ToString();
        lastSegments.Clear();

        Debug.Log("Usando seed: " + currentSeed);
    }

    // api
    public TrackSegmentSO GetNextSegment() {
        TrackSegmentSO chosen = ChooseWithRules();
        RegisterMemory(chosen.type);
        return chosen;
    }

    // memória
    void RegisterMemory(SegmentType type) {
        lastSegments.Enqueue(type);
        if (lastSegments.Count > memorySize) {
            lastSegments.Dequeue();
        }
    }

    int CountRecentCurves() {
        return lastSegments.Count(t =>
        t.ToString().Contains("Curve")
        );
    }

    // regras de design
    TrackSegmentSO ChooseWithRules() {
        List<TrackSegmentSO> candidates = new();

        int recentCurves = CountRecentCurves();

        foreach (var seg in segments) {
            float dynamicWeight = seg.weight;

            // regra 1: curva demais -> boost para reta
            if (recentCurves >= maxCurvesInRow &&
                seg.type == SegmentType.Straight) {
                    dynamicWeight *= straighBoost;
            }

            // regra 2: bloqueia segmentos sobreusados
            if (recentCurves >= maxCurvesInRow &&
                seg.type.ToString().Contains("Curve")) {
                    continue; // pula este segmento
            }

            if (dynamicWeight > 0f) {
                candidates.Add(new TrackSegmentSO {
                    type = seg.type,
                    prefab = seg.prefab,
                    weight = dynamicWeight
                });
            }

        }

        return ChooseWeighted(candidates);
    }

    TrackSegmentSO ChooseWeighted(List<TrackSegmentSO> list) {
        float total = list.Sum(s => s.weight);
        float roll = Random.Range(0f, total);

        float acc = 0f;
        foreach (var seg in list) {
            acc += seg.weight;
            if (roll <= acc) {
                return seg;
            }
        }
        return list[0]; // fallback
    }

    // testa seeds e escolhe a melhor (n gera pista, só avalia)
    void FindBestSeed() {
        int bestSeed = seed;               // seed inicial
        int bestScore = int.MinValue;      // melhor pontuação inicial

        // testa várias seeds e calcula uma pontuação
        for (int i = 0; i < maxAttempts; i++) {
            int testSeed = Random.Range(100000, 999999);
            int score = EvaluateSeed(testSeed);

            if (score > bestScore) {
                bestScore = score;
                bestSeed = testSeed;
            }
        }

        seed = bestSeed;
    }

    // simulação simples para avaliar uma seed
    int EvaluateSeed(int testSeed) {
        Random.InitState(testSeed);

        int score = 0;
        Queue<SegmentType> tempMemory = new();

        for (int i = 0; i < 10; i++) {
            TrackSegmentSO seg = ChooseWeighted(segments);

            if (seg.type == SegmentType.Straight)
                score += 2;

            if (tempMemory.Count(t => t == seg.type) > 1)
                score -= 2;

            tempMemory.Enqueue(seg.type);
            if (tempMemory.Count > memorySize)
                tempMemory.Dequeue();

        }

        return score;
    }

}