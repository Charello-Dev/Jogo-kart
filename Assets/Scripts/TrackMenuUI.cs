using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TrackMenuUI : MonoBehaviour
{
    [Header("Gerador")]
    public TrackGenerator trackGenerator;

    [Header("Textos")]
    public TextMeshProUGUI seedTxt;

    [Header("Botões")]
    public Button gerarNovaBtn;
    public Button gerarSeedBtn;

    [Header("Controles")]
    public Slider pontosSlider;
    public Toggle relevoToggle;
    public TMP_InputField seedInput;

    void Start()
    {
        gerarNovaBtn.onClick.AddListener(GerarNova);
        gerarSeedBtn.onClick.AddListener(GerarPorSeed);
    }

    void GerarNova() {
        // limpa a pista atual antes de gerar uma nova
        trackGenerator.ClearTrack();
            
        // sorteia uma seed nova entre 0 e 99999
        int seed = Random.Range(0, 99999);
        trackGenerator.seed = seed;
        seedTxt.text = $"Seed: {seed.ToString()}";

        // gera a pista com a nova seed
        trackGenerator.GenerateTrack();
    }

    void GerarPorSeed() {
        
    }

}
