using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

public class SaveManager
{
    string fichierPath;
    public void Save(){
        ChercheFichier();
    }

    public void Sauvegarder(GameManager.DataWorld data){
        string json=JsonUtility.ToJson(data,true);
        fichierPath=$"{Application.dataPath}\\SaveData\\save.json";
        File.WriteAllText(fichierPath,json);
    }

    void ChercheFichier(){
        string path=Application.dataPath;
        string folder=$"{path}\\SaveData";
        string fichier=$"{folder}\\save.json";
        bool fileExist=File.Exists(fichier);
        bool folderExist=Directory.Exists(folder);
        if(!folderExist)
            CreerDossier(folder);
        if(!fileExist){
            CreerFichier(fichier);
        }
    }

    private void CreerDossier(string folderPath)
    {
        Directory.CreateDirectory(folderPath);
    }

    void CreerFichier(string fileLocation){
        File.Create(fileLocation);
    }
}
