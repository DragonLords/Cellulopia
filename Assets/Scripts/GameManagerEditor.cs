#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameManager manager=(target as GameManager);
        if(GUILayout.Button("Générer carte")){
            Debug.ClearDeveloperConsole();
            manager.GenererCarte();
        }else if(GUILayout.Button("Supprimer texture")){
            manager.debugQuad.sharedMaterial.mainTexture=null;
        }else if(GUILayout.Button("Generer cubes")){
            manager.GenererCubesMap();
        }else if(GUILayout.Button("Supprimer carte")){
            DetruireCarte(manager.mapGenerator.cubes);
        }else if(GUILayout.Button("initialiser sauvegarde")){
            manager.InitialiserSauvegarde();
        }else if(GUILayout.Button("Sauvegarder")){
            manager.Sauvegarder();
        }
        base.OnInspectorGUI();
        
    }

    void DetruireCarte(List<GameObject> cubes){
        for (int i = 0; i < cubes.Count; i++)
        {
            Destroy(cubes[i]);
        }
        cubes.Clear();
    }
}

#endif