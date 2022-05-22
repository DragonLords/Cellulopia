using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.Serialization;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
using Newtonsoft;
using UnityEditor;
using Newtonsoft.Json.Linq;

/// <summary>
/// Classe qui sert a un paquet dutilite regrouper en tant que gamemanager
/// </summary>
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
    public bool ForcePause = false;
    [SerializeField] Player.Rework.Player player;
    #region Upgrade Player event
    public Player.Rework.Events.EventsPlayer.PlayerUpgradeStats AddStats = new();
    public Player.Rework.Events.EventsPlayer.PlayerUpgradeSkill AddSkills = new();
    public Player.Rework.Events.EventsPlayer.PlayerTakeDamage PlayerTakeDamage = new();
    public Player.Rework.Events.EventsPlayer.PlayerGiveEXP PlayerGiveEXP = new();
    public Player.Rework.Events.EventsPlayer.PlayerRemoveQuest PlayerRemoveQuest = new();
    public bool PlayerControlShowed = false;
    public WaitForSeconds delayShowControl = new(5f);
    public Player.Rework.Events.EventsPlayer.PlayerShowControl PlayerShowControl = new();
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
    [SerializeField] Vector2 offsetSpawnPos = new(10f, 10f);
    string dir;
    string savePath;
    public GameSetup gameSetup = new();

    #region Sound
    [Header("Sound")]
    [SerializeField] AudioSource _source;
    [SerializeField] private AudioClip _hittedSound;
    [SerializeField] private AudioClip _eatSound;
    [SerializeField] private AudioClip _gainLevelSound;
    [SerializeField] private AudioClip _killedSound;
    [SerializeField] private AudioClip _bossMusic;
    internal Dictionary<SoundType, AudioClip> soundStock = new();
    #endregion
    public int limitEnemies = 50;
    private void Awake()
    {
        if (player is null)
            player = FindObjectOfType<Player.Rework.Player>();
        AddStats.AddListener(UpgradePlayerStats);
        AddSkills.AddListener(UpgradePlayerSkills);
        PlayerTakeDamage.AddListener(player.TakeDamage);
        PlayerGiveEXP.AddListener(player.GetEvolutionGrade);
        PlayerRemoveQuest.AddListener(player.RemoveQuestActive);
        PlayerShowControl.AddListener(player.ShowControl);
        FillStockSound();
        LoadSave();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!gameSetup.PlayerHasMoved)
            StartCoroutine(WaitToShowControl());
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            SaveData();
            // UnityEditor.EditorApplication.isPlaying=false;
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !ForcePause)
            PauseGame();
    }

    #region Save
    /// <summary>
    /// sert a charger la sauvegarde
    /// </summary>
    void LoadSave()
    {
        gameSetup = SaveManager.LoadSave();
        player.playerStat.Stack(gameSetup);
    }

    /// <summary>
    /// sert a sauvegarder la progression
    /// </summary>
    public void SaveData()
    {
        player.playerStat.UnStack(SaveManager.setup);
        SaveManager.SaveGame();
    }
    #endregion

    /// <summary>
    /// sert a montrer une animation de souris qui bouge pour montrer les controles
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitToShowControl()
    {
        yield return new WaitForSeconds(5);
        if (!player.playerStat.hasMove)
            PlayerShowControl.Invoke();
    }

    /// <summary>
    /// sert a aller chercher les stats du joueur (utile pour les montrer dans le panneau de competence)
    /// </summary>
    /// <param name="statEffect"></param>
    /// <returns></returns>
    internal List<string> GetPlayerStat(Player.Skill.SkillTemplate.StatEffect statEffect)
    {
        List<string> value = new();
        switch (statEffect)
        {
            case Player.Skill.SkillTemplate.StatEffect.Attack: value.Add($"{player.playerStat.DamageValue}"); value.Add($"{player.playerStat.DelayAttack}"); break;
            case Player.Skill.SkillTemplate.StatEffect.vitesse: value.Add($"{player.playerStat.MoveSpeed}"); break;
            case Player.Skill.SkillTemplate.StatEffect.Life: value.Add($"{player.playerStat.Life}"); value.Add($"{player.playerStat.MaxLife}"); break;
        }
        return value;
    }

    




    #region Upgrade Player
    /// <summary>
    /// dit si le joueur peut acheter uen competence
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    public bool CanBuySkill(Player.Skill.SkillTemplate skill) => player.SkillPoint >= skill.skillCost;

    /// <summary>
    /// augmente la stat du joueur 
    /// </summary>
    /// <param name="skill"></param>
    void UpgradePlayerStats(Player.Skill.SkillTemplate skill)
    {
        player.UpgradeStats(skill);
        player.SkillQuest();
    }

    /// <summary>
    /// augemente la competence du joueur 
    /// </summary>
    /// <param name="skill"></param>
    void UpgradePlayerSkills(Player.Skill.SkillTemplate skill)
    {
        // player.DonnerCompetence(skill);
        player.SkillQuest();
    }
    #endregion

    #region Portal
    public void SpawnPortal()
    {
        int rnd = UnityEngine.Random.Range(0, gen.emptyTiles.Count);
        var p = Addressables.InstantiateAsync(portalKey, new(gen.emptyTiles[rnd].x, 1, gen.emptyTiles[rnd].y), Quaternion.identity);
        // var p=Instantiate(portal,new(gen.emptyTiles[rnd].x,1,gen.emptyTiles[rnd].y),Quaternion.identity);
        // Debug.LogFormat("Portal spawned at {0}:{1}", gen.emptyTiles[rnd][0], gen.emptyTiles[rnd][1]);
    }
    #endregion

    /// <summary>
    /// sert a mettre le jeu en pause
    /// </summary>
    /// <param name="forcingPause">determine si le mode pause est forcer (tableau de competence)</param>
    public void PauseGame(bool forcingPause = false)
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
        if (forcingPause)
        {
            Time.timeScale = 0;
            Paused = true;
            ForcePause = true;
        }
        else
        {
            ForcePause = false;
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
    // void Update()
    // {
    //     if(Keyboard.current.f5Key.wasPressedThisFrame)
    //         SpawnNewBoss();
    //     if(Keyboard.current.anyKey.wasPressedThisFrame&&!Keyboard.current.f2Key.wasPressedThisFrame)
    //         Born();
    // }



    /// <summary>
    /// sert a faire apparaitre le boss
    /// </summary>
    internal void SpawnNewBoss()
    {
        Vector3 posPlayer = player.transform.position;
        float rngX = UnityEngine.Random.Range(posPlayer.x - offsetSpawnPos.x, posPlayer.x + offsetSpawnPos.x + 1);
        float rngZ = UnityEngine.Random.Range(posPlayer.z - offsetSpawnPos.y, posPlayer.z + offsetSpawnPos.y + 1);
        Vector3 posToSpawn = new(rngX, posPlayer.y, rngZ);
        //on verifie si la position est dans larene
        if (VerifiyIfInBounds(posToSpawn))
        {
            var boss = Addressables.InstantiateAsync(bossKey, posToSpawn, Quaternion.identity);
            _source.clip = _bossMusic;
        }
        //sinon on recommence
        else
        {
            SpawnNewBoss();
        }
    }

    /// <summary>
    /// dit si le boss est dans larene 
    /// </summary>
    /// <param name="posToVerify"></param>
    /// <returns></returns>
    bool VerifiyIfInBounds(Vector3 posToVerify)
    {
        Vector2Int pos = new(Mathf.RoundToInt(posToVerify.x), Mathf.RoundToInt(posToVerify.z));
        if (emptyTiles.Contains(pos))
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
        Time.timeScale = TimeSpeed;
    }

    #region Sound
    private void FillStockSound()
    {
        soundStock.Add(SoundType.Eat, _eatSound);
        soundStock.Add(SoundType.LevelUp, _gainLevelSound);
        soundStock.Add(SoundType.Hit, _hittedSound);
        soundStock.Add(SoundType.Killed, _killedSound);
    }
    public void PlaySoundClip(AudioClip clip)
    {
        _source.PlayOneShot(clip);
    }
    #endregion

    internal class SoundEvents : UnityEvent<AudioClip> { }
}
internal enum SoundType { Eat, LevelUp, Hit, Killed }

internal enum AccesMat { Sol, Mur };