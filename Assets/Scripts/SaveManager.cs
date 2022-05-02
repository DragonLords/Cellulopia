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

}
