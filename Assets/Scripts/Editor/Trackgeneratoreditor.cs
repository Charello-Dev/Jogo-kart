using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(TrackGenerator))]
public class TrackGeneratorEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        TrackGenerator generator = (TrackGenerator)target;

        EditorGUILayout.Space(12);

        // botão: Gerar Pista
        if (GUILayout.Button("Gerar Pista", GUILayout.Height(40))) {
            // chama o método principal da geração
            generator.GenerateTrack();
       
            // sem isso ele a unity n pede pra salvar aí perde as mudanças
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        EditorGUILayout.Space(4);

        // botão: Seed Aleatória
        if (GUILayout.Button("Nova Seed Aleatória + Gerar", GUILayout.Height(40))) {
            // limpa a pista atual antes de gerar uma nova
            generator.ClearTrack();
            
            // sorteia uma seed nova entre 0 e 99999
            generator.seed = Random.Range(0, 99999);

            // gera a pista com a nova seed
            generator.GenerateTrack();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        // limpa a pista atual
        EditorGUILayout.Space(4);
        if (GUILayout.Button("Limpar Pista", GUILayout.Height(32))) {
            generator.ClearTrack();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        // mostra a seed atual, com um textinho menor
        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(
            $"Seed atual: {generator.seed}  —  anote esta seed para reproduzir esta pista.",
            MessageType.Info
        );

    }
}