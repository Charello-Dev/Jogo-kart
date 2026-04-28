using UnityEngine;
using System.Collections.Generic;

// RequireComponent vai garantir que a unity adicione os componentes num GameObject
// MeshFilter = guarda a geometria (vértices, triângulos)
// MeshRenderer = renderiza essa geometria na tela
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class TrackGenerator : MonoBehaviour
{
    [Header("Aleatoriedade")]
    [Tooltip("Troque o número para ter pistas diferentes")]
    public int seed = 42;

    [Header("Forma da Pista")]
    [Tooltip("Quantos pontos de controle definem o formato")]
    [Range(4, 13)]
    public int numControlPoints = 8;

    [Tooltip("Raio geral da pista")]
    public float trackRadius = 50f;

    [Tooltip("Quanto cada ponto pode se afastar do centro")]
    public float radiusVariation = 0.35f;

    [Header("Qualidade da Pista")]
    [Tooltip("Quantos pontos intermediários são calculados entre os dois pontos de controle")]
    public int segmentsPerControlPoint = 10;

    [Header("Geometria")]
    [Tooltip("Largura da pista")]
    public float trackWidth = 12f;

    [Header("Elevação")]
    [Tooltip("Altura máxima da pista (em metros)")]
    public float maxHeight = 15f;

    [Tooltip("Escala de ruído do Perlin (frequência das subidas e descidas)")]
    [Range(0.1f, 3f)]
    public float noiseScale = 0.5f;

    [Header("Largada")]
    [Tooltip("Quantos pontos de controle formam a reta de largada (sem curvas)")]
    [Range(1, 4)]
    public int startStraightPoints  = 2;

    // pontos que definem a forma geral da pista
    private List<Vector3> controlPoints = new List<Vector3>();

    // pontos que formam a curva suavizada (spline Catmull-Rom)
    public List<Vector3> splinePoints = new List<Vector3>();

    void Start()
    {
        // gera a pista automaticamente quando o jogo começa
        //GenerateTrack();
    }

    public void GenerateTrack()
    {
        // passo 1: gera os pontos de controle aleatórios
        GenerateControlPoints();

        // passo 2: suaviza esses pontos em uma curva contínua
        GenerateSpline();

        // passo 3: cria a geometria 3D da pista
        GenerateMesh();

        // aqui ele chama o método SpawnCheckpoints do CheckpointSpawner para criar os checkpoints na pista gerada
        FindObjectOfType<CheckpointSpawner>().SpawnCheckpoints();

        Debug.Log($"[TrackGenerator] Pista gerada! Seed: {seed} | " +
                  $"Pontos de controle: {controlPoints.Count} | " +
                  $"Pontos na spline: {splinePoints.Count}");
    }

    // passo 1 - Gerar os pontos de controle
    void GenerateControlPoints() {
        controlPoints.Clear();
        Random.InitState(seed);

        for (int i = 0; i < numControlPoints; i++) {
            // ângulo deste ponto
            float angle = (float)i / numControlPoints * Mathf.PI * 2;
            
            // raio com variação aleatória
            float randomFactor = 1f - radiusVariation + Random.value * radiusVariation * 2;
            float radius = trackRadius * randomFactor;

            // conversão de polar para cartesiano
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            
            bool isStartStraight = (i < startStraightPoints) || 
                               (i >= numControlPoints - startStraightPoints);
            
            float y;

            if (isStartStraight) {
                // reta de largada sem variação de altura
                y = 0f;

                radius = trackRadius;
                x = Mathf.Cos(angle) * radius;
                z = Mathf.Sin(angle) * radius;

            } 
            else {
                // variação de altura usando Perlin Noise
                float perlinValue = Mathf.PerlinNoise(seed * 0.01f + i * noiseScale, seed * 0.01f);
                y = (perlinValue * 2f - 1f) * maxHeight;
            }

            controlPoints.Add(new Vector3(x, y, z));
        }
    }

    // passo 2 - Gerar a spline (Catmull-Rom)
    void GenerateSpline() {
        splinePoints.Clear();
        int n = controlPoints.Count;

        for (int i = 0; i < n; i++) {
            // a fórmula Catmull-Rom precisa de 4 pontos consecutivos
            Vector3 p0 = controlPoints[(i - 1 + n) % n];    // ponto anterior ao atual
            Vector3 p1 = controlPoints[i];                  // ponto atual (início do segmento)
            Vector3 p2 = controlPoints[(i + 1) % n];        // próximo ponto (fim do segmento)
            Vector3 p3 = controlPoints[(i + 2) % n];        // ponto após o próximo

            // para cada par de pontos de controle, calculamos vários pontos intermediários ao longo da curva
            for (int j = 0; j < segmentsPerControlPoint; j++) {
                float t = (float)j / segmentsPerControlPoint;
                splinePoints.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }
    }

    // fórmula matemática da Catmull-Rom spline
    /* Dado t entre 0 e 1, ela retorna um ponto na curva.
       t=0 retorna p1, t=1 retorna p2.
       Para t entre 0 e 1, ela interpola com uma curva cúbica
       influenciada por p0 e p3.
    
       Por que cúbica (t³)? Porque uma curva cúbica tem graus
       de liberdade suficientes para controlar posição, inclinação
       e curvatura nos dois extremos — o mínimo para suavidade.
    */

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        float t2 = t * t;   // t¹
        float t3 = t2 * t;  // t²

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    // passo 3 - Gerar a malha (Mesh)
    void GenerateMesh() {
        int n = splinePoints.Count;

        // alocação dos arrays
        // cada ponto da spline gera 2 vértices (esquerda e direita)
        Vector3[] vertices = new Vector3[n * 2];
        Vector2[] uvs      = new Vector2[n * 2];

        // cada segmento entre pontos adjacentes gera 2 triângulos
        int[]     triangles = new int[n * 6];

        float[] cumulativeLengths = CalculateCumulativeLengths();
        float totalLength = cumulativeLengths[n - 1]; // cumprimento total da pista

        // cálculo das vértices e uvs
        for (int i = 0; i < n; i++) {
            // direção do ponto
            // tem como referencia os vizinhos (anterior e próximo) para calcular a direção
            int prev = (i - 1 + n) % n;
            int next = (i + 1) % n;
            Vector3 forward = (splinePoints[next] - splinePoints[prev]).normalized;

            // direção "para direita" (para ir fechando a curva) - até pq é uma curva né
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            // posições dos dois vértices
            vertices[i * 2] = splinePoints[i] - right * (trackWidth * 0.5f);        // borda esquerda
            vertices[i * 2 + 1] = splinePoints[i] + right * (trackWidth * 0.5f);    // borda direita

            // coordenada uv
            float v = cumulativeLengths[i] / totalLength;

            uvs[i * 2] = new Vector2(0f, v);     // borda esquerda
            uvs[i * 2 + 1] = new Vector2(1f, v); // borda direita
        }

        // cálculo dos triângulos
        for (int i = 0; i < n; i++) {
            int nextI = (i + 1) % n;    // índice do próximo ponto
            int baseIdx = i * 6;        // posição inicial deste no array de triângulos

            int iE = i * 2;         // esquerda atual
            int iD = i * 2 + 1;     // direita atual
            int nE = nextI * 2;     // esquerda próxima
            int nD = nextI * 2 + 1; // direita próxima
        
            // triângulo 1 (parte inferior esquerda)
            triangles[baseIdx] = iE;
            triangles[baseIdx + 1] = nE;
            triangles[baseIdx + 2] = iD;

            // triângulo 2 (parte superior direita)
            triangles[baseIdx + 3] = nE;
            triangles[baseIdx + 4] = nD;
            triangles[baseIdx + 5] = iD;
        }

        // criar e aplicar a malha na unity
        Mesh mesh = new Mesh();

        // a unity vem com índices de 16 bits, limitando a 65535 vértices
        // mas se a pista ficar mt grande vamos mudar o padrão
        if (vertices.Length > 65535) { mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; }
    
        mesh.vertices  = vertices;
        mesh.triangles = triangles;
        mesh.uv        = uvs;

        mesh.RecalculateNormals();

        mesh.RecalculateBounds();

        // aplica a malha no MeshFilter
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    // comprimento acumulado - vai servir para a textura avançar uniformemente
    float[] CalculateCumulativeLengths() {
        int     n       = splinePoints.Count;
        float[] lengths = new float[n];
        lengths[0] = 0f; // o primeiro ponto tem distância acumulada = 0
 
        for (int i = 1; i < n; i++)
        {
            // Soma a distância euclideana do ponto anterior até este
            float segmentLength = Vector3.Distance(splinePoints[i], splinePoints[i - 1]);
            lengths[i] = lengths[i - 1] + segmentLength;
        }
 
        return lengths;
    }

    // faz a limpeza da pista atual, atrubuindo uma malha vazia ao MeshFilter.
    // também apaga os dados dos pontos de controle e da spline, 
    // para garantir que a próxima geração comece do zero.
    public void ClearTrack() {
        // cria uma malha vazia e substitui a atual
        GetComponent<MeshFilter>().mesh = new Mesh();

        // limpa os dados dos pontos
        controlPoints.Clear();
        splinePoints.Clear();
    }

    // atalho para limpar e gerar uma nova pista
    public void RegenerateTrack() {
        ClearTrack();
        GenerateTrack();
    }

}
