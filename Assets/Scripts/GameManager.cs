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
    [SerializeField] Generator.Gen3D gen;
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
        // SpawnEnemy().ConfigureAwait(false);
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
        var p=Instantiate(portal,new(gen.emptyTiles[rnd].x,1,gen.emptyTiles[rnd].y),Quaternion.identity);
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

}

internal enum AccesMat { Sol, Mur };

