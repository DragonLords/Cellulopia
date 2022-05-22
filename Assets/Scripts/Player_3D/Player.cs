using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Player.Rework
{
    /// <summary>
    /// classe qui sert a linteraction du joueur 
    /// </summary>
    public class Player : MonoBehaviour
    {
        [SerializeField] Animator _anim;
        int _eatAnimID=0;
        int eatingSpeedParam=0;
        GameObject mouseAnim;
        internal bool canAttack=true;
        internal WaitForSeconds DelayBetweenAttack=new(1.5f);
        [SerializeField] GameObject GreatestParent;
        public int DamageValue = 1;
        [SerializeField, TagSelector] internal string portalTag;
        public Quest.QuestTemplate activeQuest;
        bool alive = true;
        [SerializeField] Rigidbody _rb;
        InputAction _movePress;
        PlayerInput playerInput;
        [SerializeField] internal float _moveSpeed = 1f;
        float _speed;
        Camera _mainCamera;
        public Vector2 moveInput;
        public Vector2 mousePos;
        public Vector3 mousePosWorld;
        public Vector3 force;
        public bool mousePress = false;
        public bool KeyPress = false;
        [SerializeField] Danger.PlayerDanger danger;
        public Events.EventsPlayer.PlayerGiveFood PlayerGiveFood = new();

        #region UI
        [SerializeField] GameObject _uiSkill;
        [SerializeField] Slider _sliderFaim;
        [SerializeField] Slider _sliderVie;
        [SerializeField] Slider _sliderXP;
        #endregion
        #region Evolution
        int _evolutionPoints = 0;
        public int EvolutionPoints { get => _evolutionPoints; set => _evolutionPoints = Mathf.Clamp(value, 0, int.MaxValue); }
        int _skillPoint = 0;
        public int SkillPoint { get => _skillPoint; set => _skillPoint = Mathf.Clamp(value, 0, 99); }
        public int level = 0;
        public int MaxLevel { get => levelSettings.levelRequirement.Count - 1; }
        public LevelSettings levelSettings;
        [SerializeField] TextMeshProUGUI _textPoint;
        /// <summary>
        /// Dictionnary for the table of level
        /// the key is the level 
        /// the value number of xp to have
        /// </summary>
        /// <returns></returns>
        Dictionary<int, int> tableEvolution = new()
        {
            { 1, 50 },
            { 2, 100 },
            { 3, 200 }
        };
        #endregion
        #region food finder
        int _delayTickFaim = 5;
        public int DelayTickFaim { get => _delayTickFaim = _delayTickFaim * 1000; set => _delayTickFaim = value; }
        float _faim = 0;
        float MaxValeurFaim = 50;
        public float Faim { get => _faim; set => _faim = Mathf.Clamp(value, 0, MaxValeurFaim); }
        [SerializeField] float radiusDetectFood = 20f;
        List<Vector3> foodProx = new();
        bool hungry = false;
        /// <summary>
        /// time in second to wait between damage due to hunger
        /// </summary>
        int _tickHungry = 15;
        public int TickHungry { get => _tickHungry = Mathf.RoundToInt(_tickHungry * 1000); set => _tickHungry = value; }
        int penalityHunger = -1;
        #endregion
        float _life = 100f;
        internal float _maxLife = 100f;
        [property: SerializeField] public float Life { get => _life; set => _life = Mathf.Clamp(value, 0, _maxLife); }
        internal bool hasQuest = false;
        public List<Quest.QuestTemplate> questActive = new();
        public Quest.OrderQuest questOrder;
        public Quest.QuestHolder questHolder;
        public PlayerStat playerStat;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            if(_anim == null)
                _anim=GetComponent<Animator>();
            _eatAnimID=Animator.StringToHash("Eat");
            eatingSpeedParam=Animator.StringToHash("EatingSpeed");
            mouseAnim=transform.root.GetComponentInChildren<Mouse_anim>(true).gameObject;
            mouseAnim.SetActive(false);
            Faim = 10f;
            if (_sliderFaim is not null)
            {
                _sliderFaim.interactable = false;
                UpdateSlider();
            }
            if (_sliderVie is not null)
                _sliderVie.interactable = false;
            InitEvent();
            InitializeValue();
            GetEvolutionGrade(0);
        }

        /// <summary>
        /// sert a initialiser les evenements
        /// </summary>
        private void InitEvent()
        {
            PlayerGiveFood.AddListener(GiveFood);
        }

        /// <summary>
        /// sert a redonner de la nourriture au joueur 
        /// </summary>
        /// <param name="value">la valeur a donner</param>
        internal void GiveFood(float value)
        {
            Faim += value;
            UpdateSlider();
        }

        void InitializeValue()
        {
            // Faim = 5;
            // _moveSpeed = 15f;
            levelSettings = ScriptableObject.CreateInstance<LevelSettings>();
            
            DamageValue=playerStat.DamageValue;
            _moveSpeed=playerStat.MoveSpeed;
            _skillPoint=playerStat.SkillPoint;
            _life=playerStat.Life;
            _maxLife=playerStat.MaxLife;


            levelSettings.InitTable().Wait();
            UpdateSlider();
            UpdateSliderXP();
        }


        /// <summary>
        /// sert a donner des points de competence et xp au joueur 
        /// </summary>
        /// <param name="value"></param>
        internal void GetEvolutionGrade(int value)
        {
            if (level == MaxLevel)
            {
                // Debug.Log("max level reaced");
                return;
            }
            EvolutionPoints += value;
            int[] xp = levelSettings.AddExperience(EvolutionPoints, level, SkillPoint).Result;
            EvolutionPoints = xp[0];
            level = xp[1];
            SkillPoint = xp[2];
            playerStat.Level=level;
            playerStat.SkillPoint=SkillPoint;
            playerStat.XP=EvolutionPoints;
            string pts=SkillPoint<=1?"Point":"Points";
            _textPoint.text = $"{SkillPoint} {pts}";
            GameManager.Instance.PlaySoundClip(GameManager.Instance.soundStock[SoundType.LevelUp]);
            playerStat.nextLevelXP=levelSettings.levelRequirement[level];
            // EvolutionCheck();
            UpdateSliderXP();
        }

        /// <summary>
        /// sert a augmenter une stat du joueur
        /// </summary>
        /// <param name="skillTemplate"></param>
        internal void UpgradeStats(Skill.SkillTemplate skillTemplate)
        {
            switch (skillTemplate.statEffect)
            {
                case Skill.SkillTemplate.StatEffect.vitesse:
                    {
                        ChangeSpeed(skillTemplate.statEffectValue);
                    }
                    break;
                case Skill.SkillTemplate.StatEffect.Attack:
                    {
                        UpgradeAttack(skillTemplate.statEffectValue,skillTemplate.AttackDelayReduction);
                    }
                    break;
                case Skill.SkillTemplate.StatEffect.Life:
                    {
                        UpgradeSurv(skillTemplate.statEffectValue);
                    }
                    break;
                default: break;
            }
            SkillPoint -= skillTemplate.skillCost;
            playerStat.SkillPoint=SkillPoint;
            _textPoint.text = SkillPoint>1?($"{SkillPoint} Points"):($"{SkillPoint} Point");
            // Debug.Log($"{EvolutionPoints},{levelSettings.levelRequirement[level]}");
            GameManager.Instance.SaveData();
            // ChangeSpeed(skillTemplate.statEffectValue);
        }

        /// <summary>
        /// sert a augmenter la vitesse du joueur 
        /// </summary>
        /// <param name="Value"></param>
        void ChangeSpeed(float Value)
        {
            _moveSpeed += Value;
            playerStat.MoveSpeed=_moveSpeed;
        }

        /// <summary>
        /// sert a augmenter lattque du joueur 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="reduction"></param>
        void UpgradeAttack(int value,float reduction)
        {
            DamageValue += value;
            playerStat.DelayAttack+=reduction;
            playerStat.DamageValue=DamageValue;
        }

        //sert a augementer la vie du joueur
        void UpgradeSurv(int value)
        {
            _maxLife += value;
            Life+=value;
            playerStat.MaxLife=_maxLife;
            playerStat.Life=Life;
        }

        /// <summary>
        /// sert a quand le joueur prend des degats
        /// </summary>
        /// <param name="value"></param>
        internal void TakeDamage(int value)
        {
            Life -= value;
            UpdateSlider();
            if (Life == 0)
                PlayerDeath().ConfigureAwait(false);
        }


        /// <summary>
        /// sert a actuliser les valeurs des sliders
        /// </summary>
        void UpdateSlider()
        {
            _sliderFaim.gameObject.SetActive(false);
            // _sliderFaim.value = Faim;
            // _sliderFaim.maxValue = MaxValeurFaim;
            _sliderVie.maxValue = _maxLife;
            _sliderVie.value = Life;
            // _sliderFaim.gameObject.SetActive(false);
        }

        /// <summary>
        /// sert a actuliser les valeurs des sliders
        /// </summary>
        void UpdateSliderXP()
        {
            // _sliderXP.value = EvolutionPoints;
            StartCoroutine(AnimateSlider());
            _sliderXP.maxValue = levelSettings.levelRequirement[level];
            IEnumerator AnimateSlider(){
                do
                {
                    ++_sliderXP.value;
                    yield return null;
                } while (_sliderXP.value<=EvolutionPoints);
            }

        }

        /// <summary>
        /// sert a donner de la vie au joueur 
        /// </summary>
        /// <param name="value"></param>
        void GiveLife(int value)
        {
            Life += value;
            UpdateSlider();
        }

        /// <summary>
        /// sert a quand le joueur meurt
        /// </summary>
        /// <returns></returns>
        async Task PlayerDeath()
        {
            await Task.Yield();
            Destroy(GreatestParent);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Intro");
        }

        /// <summary>
        /// sert a ajouter lobjet collecter pour la quete
        /// </summary>
        /// <param name="objCollected"></param>
        /// <param name="tag"></param>
        internal void QuestItem(GameObject objCollected,string tag)
        {
            if (!activeQuest.IsSkillQuest)
            {
                // Debug.LogFormat("target:{0} coll:{1}",activeQuest.objectToCollect.transform.root.name,nameObj);
                if (activeQuest.objectToCollect.CompareTag(tag))
                {
                    ++activeQuest.NumberCollected;
                }
            }

            // questActive.NumberCollected++;
        }

        /// <summary>
        /// sert a enlever la quete active
        /// </summary>
        /// <param name="quest"></param>
        internal void RemoveQuestActive(Quest.QuestTemplate quest)
        {
            questActive.Remove(quest);
            hasQuest = questHolder.QuestLeft();

            if (hasQuest)
            {
                questHolder.ReceiveNewQuest();
            }
            if (activeQuest.PortalQuest)
            {
                GameManager.Instance.SpawnPortal();
            }
        }

        /// <summary>
        /// sert determine si la quete est lier a lachat de la competence
        /// </summary>
        internal void SkillQuest()
        {
            if (activeQuest.IsSkillQuest)
            {
                activeQuest.NumberCollected++;
                // Debug.Log("yup");
            }
            else
            {
                // Debug.Log("nope");
            }
        }

        /// <summary>
        /// sert a mettre la quete active
        /// </summary>
        /// <param name="quest"></param>
        internal void SetActiveQuest(Quest.QuestTemplate quest)
        {
            activeQuest = quest;
        }



        // Start is called before the first frame update
        void Start()
        {
            if (_rb is null)
                _rb = GetComponent<Rigidbody>();
            _mainCamera = Camera.main;
            playerInput = GetComponent<PlayerInput>();
            Cursor.lockState = CursorLockMode.Confined;
            // LoopAction();

        }

        #region InputEvent
        /// <summary>
        /// sert a collecter la position de la souris
        /// </summary>
        /// <param name="ctx"></param>
        public void ReadMovePress(InputAction.CallbackContext ctx)
        {
            KeyPress = ctx.ReadValue<Vector2>() != Vector2.zero;
            moveInput = ctx.ReadValue<Vector2>();
        }
        /// <summary>
        /// sert a collecter la position de la souris
        /// </summary>
        /// <param name="ctx"></param>
        public void ReadMousePos(InputAction.CallbackContext ctx)
        {
            if (mousePress)
            {
                mousePos = ctx.ReadValue<Vector2>();
                mousePosWorld = _mainCamera.ScreenToWorldPoint(mousePos);
                moveInput = new(mousePosWorld[0], mousePosWorld[2]);
            }
            else
            {
                moveInput = Vector2.zero;
            }
        }
        /// <summary>
        /// sert a collecter le clique de la souris
        /// </summary>
        /// <param name="ctx"></param>
        public void ReadMousePress(InputAction.CallbackContext ctx)
        {
            mousePress = ctx.phase.IsInProgress();
        }
        #endregion



        // Update is called once per frame
        void Update()
        {
            if (mousePress&&!GameManager.Instance.Paused)
            {
                mousePos = Mouse.current.position.ReadValue();
                mousePosWorld = _mainCamera.ScreenToWorldPoint(mousePos);
                moveInput = new(mousePosWorld[0], mousePosWorld[2]);
                MoveCharacter();
                _rb.isKinematic=false;
            }
            else
            {
                mousePosWorld = Vector3.zero;
                mousePos = Vector2.zero;
                force = Vector3.zero;
                _rb.velocity=Vector3.zero;
                _rb.isKinematic=true;
            }
        }

        /// <summary>
        /// sert a bouger le perso dans la direction de la souris
        /// </summary>
        void MoveCharacter()
        {
            if (moveInput != Vector2.zero)
            {
                playerStat.hasMove=true;
                // Debug.Log("if");
                _speed = _moveSpeed;
                force = new((mousePosWorld[0] - transform.position[0]) * _speed, 0f, (mousePosWorld[2] - transform.position[2]) * _speed);
                //force = new Vector3(Mathf.Clamp(moveInput[0],-1f,1f), 0f, Mathf.Clamp(moveInput[1],-1f,1f))*_speed;
                _rb.AddForce(force, ForceMode.Force);
                RotateTowardTarget();
            }
        }

        /// <summary>
        /// sert a tourner en direction que pointe la souris
        /// </summary>
        void RotateTowardTarget()
        {
            Vector3 dir = new Vector3(mousePosWorld.x - transform.position.x, 0f, mousePosWorld.z - transform.position.z) * -1f;
            transform.forward = dir;
        }

        /// <summary>
        /// sert a jouer un flipbook montrant lanimation de la souris qui bouge
        /// </summary>
        public void ShowControl(){
            StartCoroutine(ShowingControl());
            IEnumerator ShowingControl(){
                mouseAnim.SetActive(true);
                StartCoroutine(mouseAnim.GetComponent<Mouse_anim>().Start());
                do
                {
                    // Debug.Log("move the fuck up");
                    yield return null;
                } while (moveInput==Vector2.zero);
                mouseAnim.SetActive(false);
                Destroy(mouseAnim);
            }
        }

        /// <summary>
        /// sert a jouer lanimation de manger du joueur 
        /// </summary>
        public void PlayAnimEat(){
            if(playerStat.DelayAttack==0)
                return;
            _anim.SetFloat(eatingSpeedParam,CalculateMultiplier(playerStat.DelayAttack));
            _anim.SetTrigger(_eatAnimID);
        }

        /// <summary>
        /// sert a caluler le multiplicateur de vitesse necessaire pour laniamtion de manger du joueur 
        /// </summary>
        /// <param name="targetTime"></param>
        /// <returns></returns>
        float CalculateMultiplier(float targetTime){
            //samplerate is the number of frame in my animation
            AnimationClip clip=UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<AnimationClip>(AddressablePath.PlayerEat).WaitForCompletion();
            float sampleRate=clip.frameRate;
            float lenght=clip.length;
            float totalFrame=sampleRate*targetTime;
            return sampleRate/totalFrame;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(new(transform.position, (transform.position + Vector3.forward) * 500));
        }
    }
}