#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    static bool showSave=true;
    static bool GenerationMonde=true;
    public override void OnInspectorGUI()
    {
        GameManager manager=(target as GameManager);
        showSave=EditorGUILayout.Foldout(showSave,"Sauvegarde");
        #region Sauvegarde
        if(showSave){
            if(GUILayout.Button("initialiser sauvegarde")){
                manager.InitialiserSauvegarde();
            }else if(GUILayout.Button("Sauvegarder")){
                manager.Sauvegarder();
            }
        }
        #endregion
        #region Gen monde
        GenerationMonde=EditorGUILayout.Foldout(GenerationMonde,"Generation Monde");
        if(GenerationMonde){
            
        }
        #endregion

        #region save
        if(GUILayout.Button("Reset save")){
            string path=Application.dataPath+"/Data/Save.json";
            if(System.IO.File.Exists(path)){
                GameSetup setup=new();
                string json=Newtonsoft.Json.JsonConvert.SerializeObject(setup,Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(path,json);
            }
            // manager.ResetSave().ConfigureAwait(true);
        }
        #endregion

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