using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    DataWorld data=new();
    [SerializeField] public MapGenerator mapGenerator;
    SaveManager save=new();
    public Renderer debugQuad;
    // Start is called before the first frame update
    void Start()
    {
        Generation();
    }

    private void Generation()
    {
        GenererCarte();
        GenererCubesMap();
        Sauvegarder();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)){
            DetruireCubes();
        }
    }

    async void DetruireCubes(){
        foreach(var cube in mapGenerator.cubes)
            Destroy(cube);
        mapGenerator.cubes.Clear();
        await Task.Delay(1000*2);
        Generation();
    }

    private void Vider()
    {
        foreach(var cube in mapGenerator.cubes){
            Destroy(cube);
        }
        mapGenerator.cubes.Clear();
    }

    public void GenererCarte(){
        data.carte=mapGenerator.GenererCarte();
        ShowText(data.carte);
    }

    void ShowText(int[,] carte){
        List<Color> colors=new();
        for (int x = 0; x < mapGenerator.dimension.x; x++)
        {
            for (int y = 0; y < mapGenerator.dimension.y; y++)
            {
                colors.Add(carte[x,y]==0?Color.white:Color.black);
            }
        }
        Texture2D tex=new(mapGenerator.dimension.x,mapGenerator.dimension.y);
        tex.SetPixels(colors.ToArray());
        tex.Apply();
        debugQuad.sharedMaterial.mainTexture=tex;
    }

    public void InitialiserSauvegarde(){
        save.Save();
    }

    public void Sauvegarder(){
        data.time=Time.time.ToString();
        ConsoleDebugCarte();
        save.Sauvegarder(data);
    }   

    void ConsoleDebugCarte()
    {
        System.Text.StringBuilder sb = new();
        for (int x = 0; x < mapGenerator.carte.GetLength(0); x++)
        {
            for (int y = 0; y < mapGenerator.carte.GetLength(1); y++)
            {
                sb.Append(mapGenerator.carte[x, y]);
                sb.Append(' ');
            }
            sb.AppendLine();
        }
        data.ArrayMap=sb.ToString();
    }

    public void GenererCubesMap(){
        for (int x = 0; x < mapGenerator.dimension.x; x++)
        {
            for (int y = 0; y < mapGenerator.dimension.y; y++)
            {
                var cube=Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube),new(x,y),Quaternion.identity,transform);
                cube.GetComponent<Renderer>().material.color=mapGenerator.carte[x,y]==0?Color.white:Color.black;
                mapGenerator.cubes.Add(cube);
            }
        }
    }

    [System.Serializable]
    public class DataWorld{
        public int[,] carte;
        public string time;
        public string ArrayMap;
    }

}


