using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;

/// <summary>
/// Classe qui sert au action de sauvegarde
/// </summary>
public static class SaveManager
{
    public static String pathFile;
    public static String dirPath;
    public static GameSetup setup=new();
    /// <summary>
    /// determine si la suavegarde existe
    /// </summary>
    /// <returns></returns>
    public static bool SaveExist()=>File.Exists(pathFile);
    /// <summary>
    /// retourne sous forme de gamesetup les donnees de la sauvegarde
    /// </summary>
    /// <returns></returns>
    public static GameSetup LoadSave()=>
        JsonConvert.DeserializeObject<GameSetup>(File.ReadAllText(pathFile));

    /// <summary>
    /// sert a sauvegarder la partie
    /// </summary>
    public static void SaveGame(){
        // setup.map=GameManager.Instance.mapData;
        string json=JsonConvert.SerializeObject(setup,Formatting.Indented);
        File.WriteAllText(pathFile,json);
    }

    /// <summary>
    /// sert a reset la sauvegarde
    /// </summary>
    public static void ResetSave(){
        if(!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        using(FileStream fs=new(pathFile,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite,500,true)){
            //reset the save
            GameSetup setup=new();
            //convert it to json and then to bytes for the save
            byte[] bytes=new UTF8Encoding(true).GetBytes(JsonConvert.SerializeObject(setup,Formatting.Indented));
            fs.Write(bytes);
            fs.Close();
        }
    }
}
