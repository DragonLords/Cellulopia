using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;

public static class SaveManager
{
    public static String pathFile;
    public static String dirPath;
    public static GameSetup setup=new();
    public static bool SaveExist()=>File.Exists(pathFile);
    public static GameSetup LoadSave()=>
        JsonConvert.DeserializeObject<GameSetup>(File.ReadAllText(pathFile));

    public static void SaveGame(){
        // setup.map=GameManager.Instance.mapData;
        string json=JsonConvert.SerializeObject(setup,Formatting.Indented);
        File.WriteAllText(pathFile,json);
    }

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
