using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileMap
{

    public class TileMapGenerator : MonoBehaviour
    {
        public Tilemap tileGround;
        public Tilemap tileWall;
        public TileBase ground;
        public TileBase wall;
        public MapGenerator mapGenerator = new();
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        internal void Generate()
        {
            Empty();
            int[,] carte=mapGenerator.GenererCarte();
            for (int x = 0; x < carte.GetLength(0); x++)
            {
                for (int y = 0; y < carte.GetLength(1); y++)
                {
                    if (carte[x, y] == 0)
                        tileGround.SetTile(new(x, y), ground);
                    else
                        tileWall.SetTile(new(x, y), wall);
                }
            }
        }
        
        internal void Empty()
        {
            tileWall.ClearAllTiles();
            tileGround.ClearAllTiles();
            
        }

    }

    [CustomEditor(typeof(TileMapGenerator))]
    public class TileMapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TileMapGenerator gen=(TileMapGenerator)target;
            if (GUILayout.Button("Gen"))
            {
                gen.Generate();
            }else if (GUILayout.Button("Off"))
            {
                gen.Empty();
            }
            base.OnInspectorGUI();
        }
    }


}