using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTLOAD : MonoBehaviour
{
    public List<GameObject> cubes=new();
    // Start is called before the first frame update
    void Start()
    {
        SaveManager.setup=SaveManager.LoadSave();
        Debug.Log(SaveManager.setup.worldData.Count);
        this.Load();
    }

    void Load(){
        foreach(var item in SaveManager.setup.worldData){
            var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position=new(item.x,item.y,item.z);
            cube.GetComponent<Renderer>().material.color=item.tileType==0?Color.black:Color.red;
            cubes.Add(cube);
        }
    }
}
