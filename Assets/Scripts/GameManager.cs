using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Newtonsoft.Json;

public class GameManager : MonoBehaviour
{
    [SerializeField, Range(0, 5)] float TimeSpeed = 1f;
    public GoapSpawner spawner;
    public int maxEnemiesInLevel = 3;
    WaitForSeconds wsCheckIfEnemy = new(3);
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

    public bool Paused = false;
    SaveManager save = new();
    [SerializeField] Player.Rework.Player player;
    #region Upgrade Player event
    public Player.Rework.Events.EventsPlayer.PlayerUpgradeStats AddStats = new();
    public Player.Rework.Events.EventsPlayer.PlayerUpgradeSkill AddSkills = new();
    public Player.Rework.Events.EventsPlayer.PlayerTakeDamage PlayerTakeDamage = new();
    public Player.Rework.Events.EventsPlayer.PlayerGiveEXP PlayerGiveEXP = new();
    public Player.Rework.Events.EventsPlayer.PlayerRemoveQuest PlayerRemoveQuest = new();
    public bool PlayerControlShowed=false;
    public WaitForSeconds delayShowControl=new(5f);
    public Player.Rework.Events.EventsPlayer.PlayerShowControl PlayerShowControl=new();
    #endregion
    public GameObject enemy;
    [SerializeField] GameObject portal;
    string portalKey = "Portal";
    [SerializeField] Generator.Gen3D gen;
    public List<GameObject> enemies = new();
    public List<Vector2Int> emptyTiles = new();
    public int[,] map;
    public EntitiesPlacer entitiesPlacer;
    string bossKey = "Boss";
    [SerializeField] Vector2 offsetSpawnPos=new(10f,10f);
    string dir;
    string savePath;
    public GameSetup gameSetup=new();
    private void Awake()
    {
        dir=Application.dataPath+"/Data/";
        savePath=Application.dataPath+"/Data/Save.json";
        CheckIfSaveExist();
        LoadSave();

        if (player is null)
            player = FindObjectOfType<Player.Rework.Player>();
        AddStats.AddListener(UpgradePlayerStats);
        AddSkills.AddListener(UpgradePlayerSkills);
        PlayerTakeDamage.AddListener(player.TakeDamage);
        PlayerGiveEXP.AddListener(player.GetEvolutionGrade);
        PlayerRemoveQuest.AddListener(player.RemoveQuestActive);
        PlayerShowControl.AddListener(player.ShowControl);
    }

    internal Task ResetSave=>Task.Run(()=>{
        //make a new save here
        //so dleete and reset value
        //or just recreate a new class
        gameSetup=new(); 
        string json=JsonConvert.SerializeObject(gameSetup,Formatting.Indented);
        File.WriteAllText(savePath,json);
    });
    
    void LoadSave(){
        string json=File.ReadAllText(savePath);
        gameSetup=Newtonsoft.Json.JsonConvert.DeserializeObject<GameSetup>(json);
        Debug.Log(gameSetup);
    }

    public void SaveData(){
        if(gameSetup == null){
            Debug.Log("setup ios null");
            gameSetup=new();
            gameSetup.dateTime=DateTime.Now;
        }
        string json=JsonConvert.SerializeObject(gameSetup,Formatting.Indented);
        File.WriteAllText(savePath,json);
    }

    void CheckIfSaveExist(){
        if(!File.Exists(savePath)){
            Debug.Log("Doesnt exist");
            Directory.CreateDirectory(dir);
            File.Create(savePath);
        }
        Debug.Log("found");
    }

    IEnumerator WaitToShowControl(){
        yield return new WaitForSeconds(5);
        if(!gameSetup.PlayerHasMoved)
            PlayerShowControl.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!gameSetup.PlayerHasMoved)
            StartCoroutine(WaitToShowControl());
    }




    #region Upgrade Player
    public bool CanBuySkill(Player.Skill.SkillTemplate skill) => player.SkillPoint >= skill.skillCost;

    void UpgradePlayerStats(Player.Skill.SkillTemplate skill)
    {
        player.UpgradeStats(skill);
        player.SkillQuest();
    }
    void UpgradePlayerSkills(Player.Skill.SkillTemplate skill)
    {
        // player.DonnerCompetence(skill);
        player.SkillQuest();
    }
    #endregion


    #region Sauvegarde
    public void InitialiserSauvegarde()
    {

    }

    public void Sauvegarder()
    {

    }
    #endregion

    #region Portal
    public void SpawnPortal()
    {
        int rnd = UnityEngine.Random.Range(0, gen.emptyTiles.Count);
        var p = Addressables.InstantiateAsync(portalKey, new(gen.emptyTiles[rnd].x, 1, gen.emptyTiles[rnd].y), Quaternion.identity);
        // var p=Instantiate(portal,new(gen.emptyTiles[rnd].x,1,gen.emptyTiles[rnd].y),Quaternion.identity);
        Debug.LogFormat("Portal spawned at {0}:{1}", gen.emptyTiles[rnd][0], gen.emptyTiles[rnd][1]);
    }
    #endregion

    public void PauseGame()
    {
        if (!Paused)
        {
            Time.timeScale = 0;
            Paused = !Paused;
        }
        else
        {
            Time.timeScale = 1;
            Paused = !Paused;
        }
    }

    #region Enemies
    public void StartCheckEnemy()
    {
        StartCoroutine(CheckEnemies());
    }
    IEnumerator CheckEnemies()
    {
        do
        {
            enemies.RemoveAll(item => item == null);
            if (enemies.Count < maxEnemiesInLevel)
            {
                spawner.SpawnNewEnemy();
            }
            yield return wsCheckIfEnemy;
        } while (true);
    }
    #endregion

    #region PlaceEntities
    public bool ValidatePos(int posX, int posY)
    {
        int nbMur = 0;
        //on regarde les murs autour dansun patern de 3X3
        for (int x = posX - 1; x <= posX + 1; x++)
        {
            for (int y = posY - 1; y <= posY + 1; y++)
            {
                //si la case est dans les cases du monde (pas exterieur)
                if (x >= 0 && x < gen.dimension.x && y >= 0 && y < gen.dimension.y)
                {
                    //on ne regarde pas la case cibler 
                    // if (x != posX && y != posY)
                    //alors on ajoute sa valeurs a celle des murs (soit 0 ou 1)
                    nbMur += map[x, y];
                    //sinon le mur est a lexterieur et on renforce le fait de faire apparaitre des murs au niveau exterieur
                }
                else
                    ++nbMur;
            }
        }
        if (nbMur < 3)
            return true;
        else
            return false;
    }
    #endregion

    #region Boss
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if(Keyboard.current.f5Key.wasPressedThisFrame)
            SpawnNewBoss();
        if(Keyboard.current.anyKey.wasPressedThisFrame&&!Keyboard.current.f2Key.wasPressedThisFrame)
            Born();
    }
    /// <summary>
    /// Will spawn the boss near the player using the variable "offsetSpawnPos"
    /// it work using recursive call when validating if the boss is in the bounds of the map
    /// </summary>
    internal void SpawnNewBoss()
    {
        Vector3 posPlayer=player.transform.position;
        float rngX=UnityEngine.Random.Range(posPlayer.x-offsetSpawnPos.x,posPlayer.x+offsetSpawnPos.x+1);
        float rngZ=UnityEngine.Random.Range(posPlayer.z-offsetSpawnPos.y,posPlayer.z+offsetSpawnPos.y+1);
        Vector3 posToSpawn=new(rngX,posPlayer.y,rngZ);
        if(VerifiyIfInBounds(posToSpawn)){
            var boss=Addressables.InstantiateAsync(bossKey,posToSpawn,Quaternion.identity);
        }else{
            SpawnNewBoss();
        }
    }

    /// <summary>
    /// used to verify if the boss is in bounds of the map
    /// </summary>
    /// <param name="posToVerify"></param>
    /// <returns></returns>
    bool VerifiyIfInBounds(Vector3 posToVerify){
        Vector2Int pos=new(Mathf.RoundToInt(posToVerify.x),Mathf.RoundToInt(posToVerify.z));
        if(emptyTiles.Contains(pos))
            return true;
        else 
            return false;
    }
    #endregion

    #region sim
    void Born(){
        FindObjectOfType<GOAPTester>().Duplicate();
    }
    #endregion

    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    void OnValidate()
    {
        Time.timeScale = TimeSpeed;
    }
}

internal enum AccesMat { Sol, Mur };

