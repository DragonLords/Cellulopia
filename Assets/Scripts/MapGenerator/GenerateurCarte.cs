using System.Collections;
using UnityEngine;

namespace Gen
{
    public class GenerateurCarte : MonoBehaviour
    {
        public Vector2Int dimension=Vector2Int.one;
        public int seed = 0;
        public bool seedAleatoire = true;
        [Range(0,100)]public int tauxRemplissage;
        public int nombreDeFoisDePollisageDeCarte = 3;
        int[,] carte;
        private void Start()
        {
            GenererCarte();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                GenererCarte();
        }

        public void GenererCarte()
        {
            carte = new int[dimension.x, dimension.y];
            RemplirCarte();
            for (int i = 0; i < nombreDeFoisDePollisageDeCarte; i++)
                PolissageCarte();
        }

        void RemplirCarte()
        {
            if(seedAleatoire)
                seed=Random.Range(int.MinValue,int.MaxValue);
            System.Random prng = new System.Random(seed);
            for (int x = 0; x < dimension.x; x++)
            {
                for (int y = 0; y < dimension.y; y++)
                {
                    if (x == 0 || x == dimension.x-1 || y == 0 || y == dimension.y-1)
                        carte[x, y] = 1;
                    else
                        carte[x, y] = prng.Next(0, 100)<tauxRemplissage?1:0;
                }
            }
        }

        void PolissageCarte()
        {
            for (int x = 0; x < dimension.x; x++)
            {
                for (int y = 0; y < dimension.y; y++)
                {
                    int nbMur = ObtenirnNombreMurAutour(x, y);
                    if (nbMur > 4)
                        carte[x, y] = 1;
                    else
                        carte[x, y] = 0;
                }
            }
        }

        int ObtenirnNombreMurAutour(int posX,int posY)
        {
            int nbMur = 0;
            for (int x = posX-1; x <= posX+1; x++)
            {
                for (int y = posY-1; y <= posY+1; y++)
                {
                    if (x >= 0 && x < dimension.x && y >= 0 && y < dimension.y)
                    {
                        if (x != posX || y != posY)
                            nbMur += carte[x, y];
                    }else
                        ++nbMur;
                }
            }
            return nbMur;
        }

        private void OnDrawGizmos()
        {
            if (carte != null)
            {
                for (int x = 0; x < dimension.x; x++)
                {
                    for (int y = 0; y < dimension.y; y++)
                    {
                        Gizmos.color = carte[x, y] == 0 ? Color.white : Color.black;
                        Vector3 vs = new(-dimension.x / 2 + x + .5f, -dimension.y / 2 + y + .5f);
                        Gizmos.DrawCube(vs,Vector3.one);
                    }
                }
            }
        }
    }
}