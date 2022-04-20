using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapGenerator
{
    //NOTE: 0 va donner une case vide et 1 un mur
    //Data for test purpose 
    //seed: 5363
    //taux: 36
    //poli: 3
    //force: yup
    // dimension 60X60
    public int[,] carte;
    public Vector2Int dimension = Vector2Int.one;
    public bool seedRandom = true;
    public int seed = 0;
    /// <summary>
    /// Sert a determine le taux de remplissage en % de la carte
    /// </summary>
    [Range(0, 100)] public float _tauxRemplissage = 5;
    [HideInInspector] public List<GameObject> cubes = new();
    [Range(0,10)] public int polissageCarte = 1;
    public bool forceMur=true;
    
    public int[,] GenererCarte()
    {
        RemplirCarte();
        return carte;
    }

    int[,] RemplirCarte()
    {
        carte = new int[dimension.x, dimension.y];
        if (seedRandom)
            seed = Random.Range(-10000, 10000);
        System.Random prng = new(seed);
        for (int x = 0; x < dimension.x; x++)
        {
            for (int y = 0; y < dimension.y; y++)
            {
                //on verifie si la posisiton est une case de contour, si oui on la ferme
                if (x == 0 || x == dimension.x-1 || y == 0 || y == dimension.y-1)
                {
                    // Debug.Log($"wall at {x},{y}");
                    carte[x, y] = 1;
                }
                else
                    carte[x, y] = prng.Next(0, 100) < _tauxRemplissage ? 1 : 0;
            }
        }

        for (int i = 0; i < polissageCarte; i++)
            NormaliserCarte();

        if(forceMur)
            FermerMur();

        return carte;
    }

    void FermerMur(){
        for (int x = 0; x < dimension.x; x++)
        {
            for (int y = 0; y < dimension.y; y++)
            {
                if(x==0||x==dimension.x-1||y==0||y==dimension.y-1)
                    carte[x,y]=1;
            }
        }
    }

    void NormaliserCarte()
    {
        for (int x = 0; x < carte.GetLength(0); x++)
        {
            for (int y = 0; y < carte.GetLength(1); y++)
            {
                int mur = ObtenirMurAutour(x, y);
                //si la case est entourer de plus que 4 mur la position devient un mur
                if (mur > 4)
                    carte[x, y] = 1;
                //si la case a moins que 4 mur alors la position devient vide
                else if(mur < 4)
                    carte[x, y] = 0;
            }
        }
    }
    List<string> walls=new();
    public int ObtenirMurAutour(int posX, int posY)
    {
        int nbMur = 0;
        //on regarde les murs autour dansun patern de 3X3
        for (int x = posX - 1; x <= posX + 1; x++)
        {
            for (int y = posY - 1; y <= posY + 1; y++)
            {
                //si la case est dans les cases du monde (pas exterieur)
                if (x >= 0 && x < dimension.x && y >= 0 && y < dimension.y){
                    //on ne regarde pas la case cibler 
                    // if (x != posX && y != posY)
                        //alors on ajoute sa valeurs a celle des murs (soit 0 ou 1)
                        nbMur += carte[x, y];
                //sinon le mur est a lexterieur et on renforce le fait de faire apparaitre des murs au niveau exterieur
                }else
                    ++nbMur;
            }
        }
        // Debug.Log($"at the position {posX},{posY} there is {nbMur} wall");
        return nbMur;
    }



    void ConsoleDebugCarte()
    {
        System.Text.StringBuilder sb = new();
        for (int x = 0; x < carte.GetLength(0); x++)
        {
            for (int y = 0; y < carte.GetLength(1); y++)
            {
                sb.Append(carte[x, y]);
                sb.Append(' ');
            }
            sb.AppendLine();
        }
        Debug.Log($"{sb}");
    }

    public Dictionary<Vector2,int> GenererPosTuiles(int[,] carte){
        Dictionary<Vector2,int> pos=new();
        for (int x = 0; x < dimension.x; x++)
        {
            for (int y = 0; y < dimension.y; y++)
            {
                pos.Add(new(x,y),carte[x,y]);
            }
        }
        return pos;
    }
}
