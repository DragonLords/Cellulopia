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
    public static GameSetup setup=new();
    public static bool SaveExist()=>File.Exists(pathFile);
    public static GameSetup LoadSave()=>
        JsonConvert.DeserializeObject<GameSetup>(File.ReadAllText(pathFile));

    public static void SaveGame(){
        string json=JsonConvert.SerializeObject(setup,Formatting.Indented);
        File.WriteAllText(pathFile,json);
    }

    public static void ResetSave(){
        using(FileStream fs=File.OpenWrite(pathFile)){
            GameSetup setup=new();
            //reset the save here
            string json=JsonConvert.SerializeObject(setup);
            byte[] bytes=new UTF8Encoding(true).GetBytes(json);
            fs.Write(bytes);
            fs.Flush();
        }
    }
}
