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
        int[,] carte;
        public void Init(int[,] carte,TileBase ground,TileBase wall,Tilemap mapWall,Tilemap mapGround){
            this.carte=carte;
            this.tileGround=mapGround;
            this.tileWall=mapWall;
            this.ground=ground;
            this.wall=wall;
            Generate();
        }

        internal void Generate()
        {
            Empty();
            for (int x = 0; x < carte.GetLength(0); x++)
            {
                for (int y = 0; y < carte.GetLength(1); y++)
                {
                    if (carte[x, y] == 0){
                        tileGround.SetTile(new(x, y), ground);
                        GameManager.Instance.emptyTiles.Add(new(x,y));
                    }
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

#if UNITY_EDITOR
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
#endif

}