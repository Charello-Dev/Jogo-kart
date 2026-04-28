using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class TrackMenuUI : MonoBehaviour
{
    [Header("Gerador")]
    public TrackGenerator trackGenerator;

    [Header("Textos")]
    public TextMeshProUGUI seedTxt;
    public TextMeshProUGUI pcTxt;

    [Header("Botões")]
    public Button gerarNovaBtn;
    public Button gerarSeedBtn;
    public Button playBtn;

    [Header("Controles")]
    public Slider pontosSlider;
    public Toggle relevoToggle;
    public TMP_InputField seedInput;

    public GameObject kartPrefab;
    public GameObject menuPanel;
    
    void Start()
    {
        // gerar psita ocm uma seed nova
        gerarNovaBtn.onClick.AddListener(GerarNova);

        // pista baseada em pista q o player deu a seed
        gerarSeedBtn.onClick.AddListener(GerarPorSeed);

        // slider q define os pontos de controle
        pontosSlider.value = 13;
        pcTxt.text = pontosSlider.value.ToString();
        pontosSlider.onValueChanged.AddListener(PontosDeControle);
        
        // toggle de relevo
        trackGenerator.maxHeight = 12;
        relevoToggle.onValueChanged.AddListener(Relevo);

        // botão de play
        playBtn.onClick.AddListener(StartGame);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void GerarNova() {
        // limpa a pista atual antes de gerar uma nova
        trackGenerator.ClearTrack();
            
        // sorteia uma seed nova entre 0 e 99999
        int seed = Random.Range(0, 99999);
        trackGenerator.seed = seed;
        seedTxt.text = $"Seed: {seed}";

        // gera a pista com a nova seed
        trackGenerator.GenerateTrack();
    }

    void GerarPorSeed() {
        if (int.TryParse(seedInput.text, out int seed)) {
            trackGenerator.seed = seed;
            seedTxt.text = $"Seed: {seed}";
            trackGenerator.GenerateTrack();
        }
    }

    void PontosDeControle(float valor) {
        int pc = (int)valor;
        trackGenerator.numControlPoints = pc;
        pcTxt.text = $"{pc}";
    }

    void Relevo(bool ativo) {
        if (ativo) {
            trackGenerator.maxHeight = 12;
        }
        else {
            trackGenerator.maxHeight = 0;
        }
    }

    void StartGame() {
        menuPanel.SetActive(false);

        GameObject kart = Instantiate(kartPrefab, trackGenerator.splinePoints[0], Quaternion.identity);
        kart.GetComponent<KartRespawn>().checkpointSpawner = FindObjectOfType<CheckpointSpawner>();
        FindObjectOfType<KartCamera>().SetTarget(kart.transform);
    }



}
