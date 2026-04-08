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
    [Range(4, 16)]
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
    [Tooltip("Altura máxima da pista")]
    public float maxHeight = 15f;

    [Tooltip("Escala das pistas. Menor = mais longo e suave, Maior = variação mais frequente")]
    [Range(0.1f, 3f)]
    public float noiseScale = 0.5f;

    // pontos que definem a forma geral da pista
    private List<Vector3> controlPoints = new List<Vector3>();

    // pontos que formam a curva suavizada (spline Catmull-Rom)
    private List<Vector3> splinePoints = new List<Vector3>();

    void Start()
    {
        // gera a pista automaticamente quando o jogo começa
        GenerateTrack();
    }

    public void GenerateTrack()
    {
        // passo 1: gera os pontos de controle aleatórios
        GenerateControlPoints();

        // passo 2: suaviza esses pontos em uma curva contínua
        GenerateSpline();

        // passo 3: cria a geometria 3D da pista
        GenerateMesh();

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
            
            // para o eixo Y
            float perlinValue = Mathf.PerlinNoise(seed * 0.01f + i * noiseScale, seed * 0.01f);
            float y = (perlinValue * 2f - 1f) * maxHeight;

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
        Vector3[] rights = new Vector3[n];

        Vector3 initialForward = (splinePoints[1] - splinePoints[n - 1]).normalized;

        rights[0] = Vector3.Cross(Vector3.up, initialForward).normalized;


        // cálculo das vértices e uvs
        for (int i = 1; i < n; i++) {
            // direção do ponto
            // tem como referencia os vizinhos (anterior e próximo) para calcular a direção
            int prev = (i - 1 + n) % n;
            int next = (i + 1) % n;
            Vector3 forward = (splinePoints[next] - splinePoints[prev]).normalized;

            rights[i] = rights[i - 1] - Vector3.Dot(rights[i - 1], forward) * forward;
            rights[i] = rights[i].normalized;

            if (Vector3.Dot(rights[i], rights[i - 1]) < 0)
                rights[i] = -rights[i];
        }

        Vector3 fwd0 = (splinePoints[1] - splinePoints[n - 1]).normalized;
        Vector3 seam = (rights[0] + rights[n - 1]).normalized;
        seam = (seam - Vector3.Dot(seam, fwd0) * fwd0).normalized;
        rights[0]     = seam;
        rights[n - 1] = seam;

        // suavização
        for (int i = 0; i < 5; i++) {
            Vector3[] smoothed = new Vector3[n];
            for (int j = 0; j < n; j++)
            {
                int prev = (j - 1 + n) % n;
                int next = (j + 1) % n;
                Vector3 fwd = (splinePoints[next] - splinePoints[prev]).normalized;

                // média ponderada: vizinhos valem 1x, ponto atual vale 2x
                // dar mais peso ao atual preserva a forma geral da orientação
                Vector3 avg = rights[prev] + rights[j] * 2f + rights[next];

                avg = avg - Vector3.Dot(avg, fwd) * fwd;

                smoothed[j] = avg.normalized;
            }
            rights = smoothed;
        }

        for (int i = 0; i < n; i++) {       
            // posições dos dois vértices
            vertices[i * 2] = splinePoints[i] - rights[i] * (trackWidth * 0.5f);        // borda esquerda
            vertices[i * 2 + 1] = splinePoints[i] + rights[i] * (trackWidth * 0.5f);    // borda direita

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
