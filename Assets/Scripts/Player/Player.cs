using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

namespace Player
{
    public class Player : MonoBehaviour
    {
        internal bool hasQuest=false;
        public List<Quest.QuestTemplate> questActive=new();
        [SerializeField] Gradient colorLine;
        string pathLineRend = "FoodPath";
        InputAction _movePress;
        [SerializeField] PlayerInput playerInput;
        Vector3 mousePos = Vector2.one;
        Vector3 worldPos = Vector3.one;
        Vector2 _directionDeplacement = Vector2.zero;
        CharacterController controller;
        Rigidbody2D _rb;
        float _vitesse = 5f;
        const float _maxVitesse = 50f;
        [property: SerializeField] public float Vitesse { get => _vitesse; set => _vitesse = Mathf.Clamp(value, .5f, _maxVitesse); }
        float _life = 100f;
        float _maxLife = 100f;
        [property: SerializeField] public float Life { get => _life; set => _life = Mathf.Clamp(value, 0, _maxLife); }
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

        #region Evolution
        int _evolutionPoints=0;
        public int EvolutionPoints{get=>_evolutionPoints;set=>_evolutionPoints=Mathf.Clamp(value,0,int.MaxValue);}
        int _skillPoint=0;
        public int SkillPoint{get=>_skillPoint;set=>_skillPoint=Mathf.Clamp(value,0,99);}
        public int level=0;
        public int MaxLevel{get=>levelSettings.levelRequirement.Count-1;}
        public LevelSettings levelSettings;
        [SerializeField] TextMeshProUGUI _textPoint;
        /// <summary>
        /// Dictionnary for the table of level
        /// the key is the level 
        /// the value number of xp to have
        /// </summary>
        /// <returns></returns>
        Dictionary<int,int> tableEvolution=new(){
            {1,50},{2,100},{3,200}
        };
        #endregion


        #region UI
        [SerializeField] GameObject _uiSkill;
        [SerializeField] Slider _sliderFaim;
        [SerializeField] Slider _sliderVie;
        [SerializeField] Slider _sliderXP;
        #endregion

        #region Tags
        [TagSelector, SerializeField] String _tagNourriture;
        [TagSelector, SerializeField] String _tagEnnemy;
        #endregion
        #region Layers
        [SerializeField] LayerMask _foodLayer;
        #endregion
        bool alive = true;

        public Events.PlayerGiveFood playerGiveFood=new();

        void Awake()
        {
            Cursor.lockState=CursorLockMode.Confined;
            controller = GetComponent<CharacterController>();
            try
            {
                _rb = GetComponent<Rigidbody2D>();
            }
            catch
            {
                _rb = gameObject.AddComponent<Rigidbody2D>();
            }
            Faim = 10f;
            if (_sliderFaim is not null)
            {
                _sliderFaim.interactable = false;
                UpdateSlider();
            }
            if (_sliderVie is not null)
                _sliderVie.interactable = false;
            try
            {
                playerInput = GetComponent<PlayerInput>();
            }
            catch
            {
                playerInput = gameObject.AddComponent<PlayerInput>();
            }
            InitEvent();
            Binding();
            InitializeValue();
            attackZone.SetActive(true);
            attackZone.GetComponent<SpriteRenderer>().enabled=false;
            GetEvolutionGrade(0);
            // SetActiveQuest();
            // InitTableLevel().ConfigureAwait(true).GetAwaiter().GetResult();
        }

        public List<int> listLevel=new(){50};
        async Task InitTableLevel(){
            // for (int i = 0; i < 10; i++){
            //     tableEvolution.Add(i+1,100*2);
            // }

            // tableEvolution.Clear();
            // tableEvolution.Add(1,50);

            // for (int i = 1; i < 10; i++)
            // {
            //     tableEvolution.Add(i+1,tableEvolution[i]*2);
            // }

            await Task.Yield();
            listLevel.Add(50);
            for (int i = 0; i <50; i++)
            {
                listLevel.Add(listLevel[i]*2);
                // try
                // {
                //     last=test[i-1];
                //     Debug.LogFormat("i:{0} l:{1}",test[i],test[i-1]);
                //     act*=last;
                //     test.Add(act);
                // }
                // catch (System.Exception)
                // {
                //     test.Add(50);
                // }
                // await Task.Delay(1000*1);
            }

            // foreach(var kvp in tableEvolution)
            //     Debug.LogFormat("level:{0}  xp:{1}",kvp.Key,kvp.Value);
        }   

        private void InitEvent()
        {
            playerGiveFood.AddListener(GiveFood);
        }

        void InitializeValue()
        {
            Faim = 5;
            Vitesse = 15f;
            levelSettings=ScriptableObject.CreateInstance<LevelSettings>();
            levelSettings.InitTable().Wait();
            UpdateSlider();
            UpdateSliderXP();
        }

        void Binding()
        {
            _movePress = playerInput.actions["MovePress"];
        }

        private void _movePress_performed()
        {
            MoveToDirection();

        }

        async Task MovePerfomed(CallbackContext ctx, bool perf)
        {
            while (perf)
            {
                await Task.Yield();
            }
        }

        void Start()
        {
            LoopAction();
            StartCoroutine(DetectFood());
        }

        float timeSpeed = 1;
        // Update is called once per frame
        void Update()
        {
            if (_movePress.inProgress)
            {
                _movePress_performed();
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
                PlayerAttack().ConfigureAwait(false);

            if(Keyboard.current.pKey.wasPressedThisFrame)
                _uiSkill.SetActive(!_uiSkill.activeSelf);


            #region debugMode
            #if UNITY_EDITOR
            if(Keyboard.current.lKey.wasPressedThisFrame)
                GetEvolutionGrade(49);
            else if (Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                timeSpeed++;
                Time.timeScale = timeSpeed;
                Debug.Log(Time.timeScale);
            }
            else if (Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                --timeSpeed;
                Time.timeScale=timeSpeed;
                Debug.Log(Time.timeScale);
            }
            else if(Keyboard.current.numpad0Key.wasPressedThisFrame)
            {
                timeSpeed=1;
                Time.timeScale=timeSpeed;
                Debug.Log(Time.timeScale);
            }
            #endif
            #endregion
        }

        private void Player_performed1(CallbackContext ctx)
        {
            
        }

        private void Player_performed(CallbackContext obj)
        {
            
        }

        #region Event Input
        public void ReadPositionInput(CallbackContext ctx)
        {
            if (ctx.performed)
            {
                mousePos = ctx.ReadValue<Vector2>();
                worldPos = Camera.main.ScreenToWorldPoint(mousePos);
#if UNITY_IOS
                Move();
                Rot();
#endif
            }
        }

        private void Move()
        {
            _rb.AddForce(new((worldPos.x - transform.position.x) * Vitesse, (worldPos.y - transform.position.y) * Vitesse));
        }

        void Rot()
        {
            Vector2 dir = new(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
            transform.up = dir;
        }
        public void ReadMovePress(CallbackContext ctx)
        {

        }
        #endregion

        bool canAttack = true;
        [SerializeField] GameObject attackZone;
        public async Task PlayerAttack()
        {
            // if (!canAttack)
            //     return;
            // canAttack = false;
            // attackZone.SetActive(true);
            // await Task.Delay(1000);
            // attackZone.SetActive(false);
            // await Task.Delay(2000);
            // canAttack = !false;
            await Task.Yield();
        }

        private void DetectClick()
        {
            // RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            // if (hit.collider is not null)
            // {
            //     //do stuff of collecting here
            //     if (hit.collider.gameObject.TryGetComponent(out Nourriture n))
            //     {
            //         GiveFood(n.GetFood());
            //     }
            // }
        }

        void FaceCam()
        {
            //FIXME: this is not working correctly on mobile
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 dir = new(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
            transform.up = dir;
        }
        void MoveToDirection()
        {
            _rb.AddForce(new((worldPos.x - transform.position.x) * Vitesse, (worldPos.y - transform.position.y) * Vitesse));
            FaceCam();
        }

        void ChangeStateUI(GameObject target)
        {
            target.SetActive(!target.activeSelf);
        }

        // void FixedUpdate()
        // {

        // }


        void ChangeSpeed(float Value)
        {
            Vitesse += Value;
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

        internal void GetEvolutionGrade(int value){
            if(level==MaxLevel)
            {
                Debug.Log("max level reaced");
                return;
            }
            EvolutionPoints+=value;
            int[] xp=levelSettings.AddExperience(EvolutionPoints,level,SkillPoint).Result;
            EvolutionPoints=xp[0];
            level=xp[1];
            SkillPoint=xp[2];
            _textPoint.text=$"{SkillPoint} Points";
            // EvolutionCheck();
            UpdateSliderXP();
        }

        internal void GiveSkillPoint(int value){
            SkillPoint+=value;
        }

        internal void EvolutionCheck(){
            // if(EvolutionPoints>=listLevel[level]){
            //     //if we have enough point to level up then we do 
            //     ++level;
            //     EvolutionPoints-=listLevel[level];
            // }
            //else we do nothing
        }

        void SetTarget(){
            
        }

        internal void UpgradeStats(Skill.SkillTemplate skillTemplate)
        {
            switch(skillTemplate.statEffect){
                case Skill.SkillTemplate.StatEffect.vitesse:{
                    ChangeSpeed(skillTemplate.statEffectValue);
                }break;
                default:break;
            }
            // ChangeSpeed(skillTemplate.statEffectValue);
        }

        internal void TakeDamage(int value){
            Life-=value;
            UpdateSlider();
        }

        void UpdateSlider()
        {
            _sliderFaim.value = Faim;
            _sliderFaim.maxValue = MaxValeurFaim;
            _sliderVie.maxValue = _maxLife;
            _sliderVie.value = Life;
            // _sliderFaim.gameObject.SetActive(false);
        }

        void UpdateSliderXP(){
            _sliderXP.value=EvolutionPoints;
            _sliderXP.maxValue=levelSettings.levelRequirement[level];
        }

        void LoopAction()
        {
            LoopHunger().ConfigureAwait(false);
        }

        internal async Task LoopHunger()
        {
            await Task.Delay(DelayTickFaim);
            for (int i = 0; i < 50; i++)
            {
                GiveFood(-1);
                await Task.Delay(1000);
                continue;
            }
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
            Destroy(gameObject);
        }

        internal void DonnerCompetence(Skill.SkillTemplate skillTemplate)
        {

        }

        IEnumerator DetectFood()
        {
            List<GameObject> lines = new();
            do
            {
                foreach (var line in lines)
                {
                    Destroy(line);
                }
                lines.Clear();
                foodProx.Clear();
                Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, radiusDetectFood, _foodLayer);
                foreach (var col in cols)
                {
                    foodProx.Add(col.gameObject.transform.position);
                }
                // DrawLine(lines);
                yield return null;
            } while (alive);
        }

        void DrawLine(List<GameObject> lines)
        {
            //Draw line for food
            foreach (var food in foodProx)
            {
                var rend = Addressables.InstantiateAsync(pathLineRend).WaitForCompletion();
                LineRenderer lr = rend.GetComponent<LineRenderer>();
                Vector3[] pos = { transform.position, food };
                lr.SetPositions(pos);
                lr.colorGradient = colorLine;
                lines.Add(rend);
            }
        }

        public void SetActiveQuest(){
            var events=FindObjectsOfType<Quest.QuestEvent>();
            foreach(var e in events){
                if(e.quest.state==Quest.QuestSate.Progress)
                    questActive.Add(e.quest);
            }
            hasQuest=questActive.Count>0;
        }

        public void SetQuest(List<Quest.QuestTemplate> quests){
            questActive=quests;
            hasQuest=true;
        }

        internal void QuestItem(GameObject objCollected){

            // foreach(var quest in questActive){
            //     Debug.LogFormat("tag target:{0} quest:{2} n:{1}",objCollected.tag,questActive.Count,quest.objectToCollect.tag);
            //     if(quest.objectToCollect.CompareTag(objCollected.tag)){
            //         quest.NumberCollected++;
            //     }
            //     else 
            //         continue;
            // }

            for (int i = 0; i < questActive.Count; i++)
            {
                if(questActive[i].objectToCollect.CompareTag(objCollected.tag)){
                    questActive[i].NumberCollected++;
                }
                else 
                    continue;
            }

            // questActive.NumberCollected++;
        }

        internal void RemoveQuestActive(Quest.QuestTemplate quest){
            questActive.Remove(quest);
            hasQuest=questActive.Count>0;
            Debug.LogFormat("{0} completed",quest.QuestName);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            // if (other.gameObject.CompareTag(_tagNourriture))
            // {
            //     var food=other.gameObject.GetComponent<Nourriture>();
            //     Debug.Log("Nourriture");
            //     GiveFood(food.GetFood());
            //     GetEvolutionGrade(food.EvolutionPointToGive);
            //     QuestItem();
            //     Destroy(other.gameObject);
            // }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, worldPos);
            //Draw ray for each food in proximity
            // foreach(KeyValuePair<Vector3,float> food in foodProx){
            //     Gizmos.color=Color.yellow;
            //     Gizmos.DrawRay(transform.position,food.Key);
            // }
        }



    }
}