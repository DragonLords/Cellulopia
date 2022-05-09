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
    [SerializeField,Range(0,5)] float TimeSpeed=1f;
    public GoapSpawner spawner;
    public int maxEnemiesInLevel=3;
    WaitForSeconds wsCheckIfEnemy=new(3);
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

    public bool Paused=false;
    SaveManager save = new();
    [SerializeField] Player.Rework.Player player;
    #region Upgrade Player event
    public Player.Rework.Events.EventsPlayer.PlayerUpgradeStats AddStats = new();
    public Player.Rework.Events.EventsPlayer.PlayerUpgradeSkill AddSkills = new();
    public Player.Rework.Events.EventsPlayer.PlayerTakeDamage PlayerTakeDamage=new();
    public Player.Rework.Events.EventsPlayer.PlayerGiveEXP PlayerGiveEXP=new();
    public Player.Rework.Events.EventsPlayer.PlayerRemoveQuest PlayerRemoveQuest=new();
    #endregion
    public GameObject enemy;
    [SerializeField] GameObject portal;
    string portalKey="Portal";
    [SerializeField] Generator.Gen3D gen;
    public List<GameObject> enemies=new();
    public List<Vector2Int> emptyTiles=new();
    public int[,] map;
    public EntitiesPlacer entitiesPlacer;
    private void Awake()
    {
        if(player is null)
            player = FindObjectOfType<Player.Rework.Player>();
        AddStats.AddListener(UpgradePlayerStats);
        AddSkills.AddListener(UpgradePlayerSkills);
        PlayerTakeDamage.AddListener(player.TakeDamage);
        PlayerGiveEXP.AddListener(player.GetEvolutionGrade);
        PlayerRemoveQuest.AddListener(player.RemoveQuestActive);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    #region Upgrade Player
    public bool CanBuySkill(Player.Skill.SkillTemplate skill)=>player.SkillPoint >= skill.skillCost;

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
    public void SpawnPortal(){
        int rnd=UnityEngine.Random.Range(0,gen.emptyTiles.Count);
        var p=Addressables.InstantiateAsync(portalKey,new(gen.emptyTiles[rnd].x,1,gen.emptyTiles[rnd].y),Quaternion.identity);
        // var p=Instantiate(portal,new(gen.emptyTiles[rnd].x,1,gen.emptyTiles[rnd].y),Quaternion.identity);
        Debug.LogFormat("Portal spawned at {0}:{1}",gen.emptyTiles[rnd][0],gen.emptyTiles[rnd][1]);
    }
    #endregion

    public void PauseGame(){
        if(!Paused){
            Time.timeScale=0;
            Paused=!Paused;
        }else{
            Time.timeScale=1;
            Paused=!Paused;
        }
    }

    #region Enemies
    public void StartCheckEnemy(){
        StartCoroutine(CheckEnemies());
    }
    IEnumerator CheckEnemies(){
        do
        {
            enemies.RemoveAll(item=>item==null);
            if(enemies.Count<maxEnemiesInLevel){
                spawner.SpawnNewEnemy();
            }
            yield return wsCheckIfEnemy;
        } while (true);
    }
    #endregion

    #region PlaceEntities
    public bool ValidatePos(int posX, int posY){
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
            if(nbMur<3)
                return true;
            else
                 return false;
    }
    #endregion

    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    void OnValidate()
    {
        Time.timeScale=TimeSpeed;
    }
}

internal enum AccesMat { Sol, Mur };

