using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Player.Rework
{
    public class Player : MonoBehaviour
    {
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
        [SerializeField] float _moveSpeed = 1f;
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
        float _maxLife = 100f;
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

        private void InitEvent()
        {
            //FIXME: not found the event
            PlayerGiveFood.AddListener(GiveFood);
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


        internal void GiveFood(float value)
        {
            Faim += value;
            if (Faim == 0 && !hungry)
                LoopHungry().ConfigureAwait(false);
            else if (Faim > 1 && hungry)
                hungry = false;
            UpdateSlider();
        }

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
            _textPoint.text = $"{SkillPoint} Points";
            // EvolutionCheck();
            UpdateSliderXP();
        }

        internal void GiveSkillPoint(int value)
        {
            SkillPoint += value;
        }

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
                        UpgradeAttack(skillTemplate.statEffectValue);
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
            _textPoint.text = SkillPoint>1?($"{SkillPoint} Points"):($"{SkillPoint} Point");
            // ChangeSpeed(skillTemplate.statEffectValue);
        }

        void ChangeSpeed(float Value)
        {
            _moveSpeed += Value;
            playerStat.MoveSpeed=_moveSpeed;
        }

        void UpgradeAttack(int value)
        {
            DamageValue += value;
            playerStat.DamageValue=DamageValue;
        }

        void UpgradeSurv(int value)
        {
            _maxLife += value;
            Life+=value;
            playerStat.MaxLife=_maxLife;
            playerStat.Life=Life;
        }

        internal void TakeDamage(int value)
        {
            Life -= value;
            UpdateSlider();
            // if(Life==0)
            //     PlayerDeath().ConfigureAwait(false);
        }



        void UpdateSlider()
        {
            _sliderFaim.gameObject.SetActive(false);
            // _sliderFaim.value = Faim;
            // _sliderFaim.maxValue = MaxValeurFaim;
            _sliderVie.maxValue = _maxLife;
            _sliderVie.value = Life;
            // _sliderFaim.gameObject.SetActive(false);
        }

        void UpdateSliderXP()
        {
            _sliderXP.value = EvolutionPoints;
            _sliderXP.maxValue = levelSettings.levelRequirement[level];
        }

        void LoopAction()
        {
            LoopHunger().ConfigureAwait(false);
        }

        internal async Task LoopHunger()
        {
            await Task.Delay(DelayTickFaim);
            do
            {
                if(!GameManager.Instance.Paused)
                    GiveFood(-1);
                await Task.Delay(DelayTickFaim);
            } while (true);
        }

        internal async Task LoopHungry()
        {
            hungry = true;
            do
            {
                GiveLife(penalityHunger);
                await Task.Delay(TickHungry);
            } while (hungry);
        }

        void GiveLife(int value)
        {
            Life += value;
            UpdateSlider();
        }

        async Task PlayerDeath()
        {
            await Task.Yield();
            Destroy(GreatestParent);
        }

        public void SetQuest(List<Quest.QuestTemplate> quests)
        {
            questActive = quests;
            hasQuest = true;
        }

        internal void QuestItem(GameObject objCollected)
        {

            // foreach(var quest in questActive){
            //     Debug.LogFormat("tag target:{0} quest:{2} n:{1}",objCollected.tag,questActive.Count,quest.objectToCollect.tag);
            //     if(quest.objectToCollect.CompareTag(objCollected.tag)){
            //         quest.NumberCollected++;
            //     }
            //     else 
            //         continue;
            // }

            // for (int i = 0; i < questActive.Count; i++)
            // {
            //     if(questActive[i].objectToCollect.CompareTag(objCollected.tag)){
            //         questActive[i].NumberCollected++;
            //     }
            //     else 
            //         continue;
            // }
            if (!activeQuest.IsSkillQuest)
            {
                if (activeQuest.objectToCollect.CompareTag(objCollected.tag))
                {
                    ++activeQuest.NumberCollected;
                }
            }

            // questActive.NumberCollected++;
        }

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
            else
            {
                // Debug.Log("All quest finished");
                ///spawn portal to boss here
                //GameManager.Instance.SpawnPortal();
            }
            // hasQuest=questActive.Count>0;
            // Debug.LogFormat("{0} completed", quest.QuestName);
        }

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
        public void ReadMovePress(InputAction.CallbackContext ctx)
        {
            KeyPress = ctx.ReadValue<Vector2>() != Vector2.zero;
            moveInput = ctx.ReadValue<Vector2>();
        }
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

            // if (Keyboard.current.escapeKey.wasPressedThisFrame)
            // {
            //     GameManager.Instance.PauseGame();
            // }


            if (Keyboard.current.f1Key.wasPressedThisFrame)
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else 
                Application.Quit();
#endif
        }

        void MoveCharacter()
        {
            if (moveInput != Vector2.zero)
            {
                GameManager.Instance.gameSetup.PlayerHasMoved=true;
                // Debug.Log("if");
                _speed = _moveSpeed;
                force = new((mousePosWorld[0] - transform.position[0]) * _speed, 0f, (mousePosWorld[2] - transform.position[2]) * _speed);
                //force = new Vector3(Mathf.Clamp(moveInput[0],-1f,1f), 0f, Mathf.Clamp(moveInput[1],-1f,1f))*_speed;
                _rb.AddForce(force, ForceMode.Force);
                RotateTowardTarget();
            }
        }

        void RotateTowardTarget()
        {
            Vector3 dir = new Vector3(mousePosWorld.x - transform.position.x, 0f, mousePosWorld.z - transform.position.z) * -1f;
            transform.forward = dir;
        }

        IEnumerator ShowClosestTarget()
        {
            do
            {

                yield return null;
            } while (alive);
        }

        Transform GetClosestTarget()
        {
            int layerTarget = activeQuest.objectToCollect.layer;
            var colls = Physics.OverlapSphere(transform.position, 500f, layerTarget);
            if (colls.Length > 0)
            {
                float dst = float.MaxValue;
                int selected = 0;
                for (int i = 0; i < colls.Length; i++)
                {
                    float distance = Vector3.Distance(transform.position, colls[i].transform.position);
                    if (distance < dst)
                    {
                        selected = i;
                        dst = distance;
                    }
                }
            }
            return null;
        }

        public void ShowControl(){
            StartCoroutine(ShowingControl());
            IEnumerator ShowingControl(){
                mouseAnim.SetActive(true);
                do
                {
                    Debug.Log("move the fuck up");
                    yield return null;
                } while (moveInput==Vector2.zero);
                mouseAnim.SetActive(false);
                Destroy(mouseAnim);
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(new(transform.position, (transform.position + Vector3.forward) * 500));
        }
    }
}