using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class CapperEntities
{

    public static List<GOAPAgent> allEn=new();

    public static void Start(){
        MaintainTheCap().ConfigureAwait(false);
    }

    static async Task MaintainTheCap(){
        do
        {
            allEn=new(GameObject.FindObjectsOfType<GOAPAgent>());
            await Task.Yield();
        } while (Application.isPlaying);
    }

    public static bool CanSpawn(){
        if(allEn.Count<=20){
            return true;
        }else{
            return false;
        }
    }
}
