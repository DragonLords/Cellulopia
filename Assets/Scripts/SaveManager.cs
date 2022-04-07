using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Newtonsoft;

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

    internal void CreerFichier(string fileLocation){
        File.Create(fileLocation);
    }

    internal void SaveDebug(GameManager.DataWorld data,string filePath){
        // File.Create(filePath);
        FileStream fs=new(filePath,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite);
        List<int> mapData=new();
        for (int x = 0; x < data.carte.GetLength(0); x++)
        {
            for (int y = 0; y < data.carte.GetLength(1); y++)
            {
                mapData.Add(data.carte[x,y]);
            }
        }
        fs.Close();
        data.mapData.carteFinal=mapData.ToArray();
        string json=JsonUtility.ToJson(data,true);
        File.WriteAllText(filePath,json,System.Text.Encoding.UTF8);
    }

    internal void LoadDataDebug(GameManager.DataWorld data,string path){
        //get pos of tile is: y*dimension.x+x
    }
}
