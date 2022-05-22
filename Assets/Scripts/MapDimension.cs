using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDimension : MonoBehaviour
{
    public static MapDimension Instance;
    public Dictionary<MapSize,Vector2Int> mapSizeReference=new(){
        {MapSize.Small,new(50,50)},
        {MapSize.Medium,new(100,100)}
    };
    public MapSize mapSize;
    internal Vector2Int mapDimension=new(50,50);
    private void Awake()
    {
        Instance=this;
        DontDestroyOnLoad(gameObject);
        mapDimension=mapSizeReference[mapSize];
        SaveManager.setup.worldSize=mapSize;
    }

}


public enum MapSize{Small,Medium,Large}