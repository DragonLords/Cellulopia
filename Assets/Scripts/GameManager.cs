using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = (GameManager)FindObjectOfType(typeof(GameManager));
            return instance;
        }
    }
    #endregion
    DataWorld data = new();
    SaveManager save = new();
    #region Generation Monde
    [SerializeField] public MapGenerator mapGenerator;
    [SerializeField] TileMap.TileMapGenerator tileMapGenerator;
    AccesMat accesMat;
    GameObject _carte;
    [SerializeField] Tilemap tileMapGround;
    [SerializeField] Tilemap tileMapWall;
    [SerializeField] TileBase tileGround;
    [SerializeField] TileBase tileWall;
    public List<Vector2Int> emptyTiles=new();
    #endregion
    [SerializeField] SpawnerFood spawnerFood;

    [SerializeField] Player.Player player;
    #region Upgrade Player event
    public Player.Events.PlayerUpgradeStats AddStats = new();
    public Player.Events.PlayerUpgradeSkill AddSkills = new();
    public Player.Events.PlayerTakeDamage PlayerTakeDamage=new();
    public Player.Events.PlayerGiveEXP PlayerGiveEXP=new();
    public Player.Events.PlayerRemoveQuest PlayerRemoveQuest=new();
    #endregion
    public GameObject enemy;
    private void Awake()
    {
        if(player is null)
            player = FindObjectOfType<Player.Player>();
        AddStats.AddListener(UpgradePlayerStats);
        AddSkills.AddListener(UpgradePlayerSkills);
        PlayerTakeDamage.AddListener(player.TakeDamage);
        PlayerGiveEXP.AddListener(player.GetEvolutionGrade);
        PlayerRemoveQuest.AddListener(player.RemoveQuestActive);
    }

    // Start is called before the first frame update
    void Start()
    {
        GenererTotaliteMonde();
        spawnerFood.StartSpawn(player.transform);
        // SpawnEnemy().ConfigureAwait(false);
    }

    async Task SpawnEnemy()
    {
        GameObject enContianer=new("EnemyContianer");
        for (int i = 0; i < 100; i++)
        {
            int rnd=UnityEngine.Random.Range(0,emptyTiles.Count);
            Instantiate(enemy,new(emptyTiles[rnd].x,emptyTiles[rnd].y),Quaternion.identity,enContianer.transform);
            await Task.Yield();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Upgrade Player
    void UpgradePlayerStats(Player.Skill.SkillTemplate skill)
    {
        player.UpgradeStats(skill);
    }
    void UpgradePlayerSkills(Player.Skill.SkillTemplate skill)
    {
        player.DonnerCompetence(skill);
    }
    #endregion


    #region Sauvegarde
    public void InitialiserSauvegarde()
    {
        save.Save();
    }

    public void Sauvegarder()
    {
        data.time = Time.time.ToString();
        ConsoleDebugCarte();
        save.Sauvegarder(data);
    }
    #endregion

    #region Generation Monde
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
        data.ArrayMap = sb.ToString();
    }

    internal Transform GenererMeshes(AccesMat terme)
    {
        GameObject sol = new() { name = terme.ToString() };
        foreach (KeyValuePair<Vector2, int> kvp in data.tuilesPos)
        {
            if (kvp.Value == ((int)terme))
            {
                GameObject objet = new();
                objet.transform.SetParent(sol.transform);
                objet.transform.position = kvp.Key;
                objet.AddComponent<MeshGenerator>().Init(data.carte, this);
            }
        }
        FusionerMeshes(sol.transform, terme);
        // AddNavData(sol);
        return sol.transform;
    }

    void FusionerMeshes(Transform cible, AccesMat terme)
    {
        MeshFilter[] filters = cible.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[filters.Length];
        for (int i = 0; i < filters.Length; i++)
        {
            combine[i].mesh = filters[i].sharedMesh;
            combine[i].transform = filters[i].transform.localToWorldMatrix;
            filters[i].gameObject.SetActive(false);
        }
        MeshFilter filter = cible.gameObject.AddComponent<MeshFilter>();
        filter.mesh = new() { name = $"Mesh finale {cible.name}" };
        filter.sharedMesh.CombineMeshes(combine, true);
        MeshCollider collider = cible.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = filter.sharedMesh;
        MeshRenderer renderer = cible.gameObject.AddComponent<MeshRenderer>();
        renderer.material = Addressables.LoadAssetAsync<Material>(terme.ToString()).WaitForCompletion();
        Debug.Log(terme.ToString());
        for (int i = 0; i < filters.Length; i++)
        {
            DestroyImmediate(filters[i].gameObject);
        }
        renderer.enabled=false;
    }

    internal void GenererTotaliteMonde()
    {
        if (_carte is not null)
            SupprimerMonde();
        System.Diagnostics.Stopwatch TimerGenMonde = new();
        TimerGenMonde.Start();
        GameObject carte = new() { name = "Carte_Mesh" };
        this._carte = carte;
        data.carte = mapGenerator.GenererCarte();
        data.tuilesPos = mapGenerator.GenererPosTuiles(data.carte);
        tileMapGenerator.Init(data.carte,tileGround,tileWall,tileMapWall,tileMapGround);
        GenererMeshes(AccesMat.Sol).SetParent(carte.transform);
        // GenererMeshes(AccesMat.Mur).SetParent(carte.transform);
        string pathSave = $"{Application.dataPath}/SaveData/save.json";
        // save.CreerFichier(pathSave);
        data.dimension = mapGenerator.dimension;
        save.SaveDebug(data, pathSave);
        for(int i=0;i<emptyTiles.Count;++i){
            if(mapGenerator.ObtenirMurAutour(emptyTiles[i].x,emptyTiles[i].y)==0){
                player.gameObject.transform.parent.position=new(emptyTiles[i].x,emptyTiles[i].y);
                break;
            }
            continue;
        }
        for(int i=emptyTiles.Count-1;i>0;--i){
            if(mapGenerator.ObtenirMurAutour(emptyTiles[i].x,emptyTiles[i].y)==0){
                enemy.transform.position=new(emptyTiles[i].x,emptyTiles[i].y);
                break;
            }
            continue;
        }

        // Debug.Log(ObtenirMurAutour(emptyTiles[15].x,emptyTiles[15].y));
        carte.AddComponent<NavMeshSurface>().BuildNavMesh();
        // Destroy(tileMapGenerator.gameObject);
        TimerGenMonde.Stop();
        Debug.LogFormat("World generated in {0} ms", TimerGenMonde.ElapsedMilliseconds);
    }

    internal void AddNavData(GameObject ground){
        var surface=ground.AddComponent<NavMeshSurface>();
        surface.BuildNavMesh();
    }

    internal void SupprimerMonde()
    {
        tileMapGround.ClearAllTiles();
        tileMapWall.ClearAllTiles();
        emptyTiles.Clear();
        // if (_carte is not null)
        // {
        //     DestroyImmediate(_carte);
        // }
    }
    #endregion

    [System.Serializable]
    public class DataWorld
    {
        public int[,] carte;
        public string time;
        public string ArrayMap;
        public Vector2Int dimension;
        public int[] mapContract;
        public Dictionary<Vector2, int> tuilesPos = new();
        public MapData mapData = new();
        [System.Serializable]
        public class MapData
        {
            public int[] carteFinal;
        }
    }

}

internal enum AccesMat { Sol, Mur };

