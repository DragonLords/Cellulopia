using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

namespace Player
{
    public class Player : MonoBehaviour
    {
        public TextMeshProUGUI texte;
        InputAction _movePress;
        public TextMeshProUGUI debugText;
        [SerializeField] PlayerInput playerInput;
        public TextMeshProUGUI textMeshProUGUI;
        Vector3 mousePos = Vector2.one;
        Vector3 worldPos = Vector3.one;
        Vector2 _directionDeplacement = Vector2.zero;
        CharacterController controller;
        Rigidbody2D _rb;
        float _vitesse = 5f;
        const float _maxVitesse = 50f;
        [property: SerializeField] public float Vitesse { get => _vitesse; set => _vitesse = Mathf.Clamp(value, .5f, _maxVitesse); }
        float _delayTickFaim = 5;
        public float DelayTickFaim { get => _delayTickFaim = _delayTickFaim * 1000; set => _delayTickFaim = value; }
        float _faim = 0;
        float MaxValeurFaim = 50;
        public float Faim { get => _faim; set => _faim = Mathf.Clamp(value, 0, MaxValeurFaim); }

        #region UI
        [SerializeField] GameObject _uiSkill;
        [SerializeField] Slider _sliderFaim;
        [SerializeField] Slider _sliderVie;
        #endregion

        #region Tags
        [TagSelector, SerializeField] String _tagNourriture;
        [TagSelector, SerializeField] String _tagEnnemy;
        #endregion

        void Awake()
        {
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
            try
            {
                playerInput = GetComponent<PlayerInput>();
            }
            catch
            {
                playerInput = gameObject.AddComponent<PlayerInput>();

            }
            Binding();
        }

        void Binding()
        {
            _movePress = playerInput.actions["MovePress"];
            bool perfo = false;
            //_movePress.performed += ctx => MovePerfomed(ctx, perfo).ConfigureAwait(false);
            //_movePress.performed += _movePress_performed;
        }

        private void _movePress_performed()
        {
            Debug.Log("<color=red>sgdiasiudguy</color>");
            MoveToDirection();

        }

        async Task MovePerfomed(CallbackContext ctx,bool perf)
        {
            while (perf)
            {
                Debug.Log("<color=yello>adigsdfiuefuebfiufg</color>");
                await Task.Yield();
            }
        }

        // Start is called before the first frame update
        void Start()
        {

            LoopAction();
            //Debug.Log("start ended");
        }

        // Update is called once per frame
        void Update()
        {
            if (_movePress.inProgress)
            {
                _movePress_performed();
            }

            //if (playerInput.actions["MovePress"].IsPressed())
            //{
            //    MoveToDirection();
            //}
            

//#if UNITY_IOS
            //if (Touchscreen.current.primaryTouch.isInProgress)
            //{
            //    debugText.text = Touchscreen.current.press.IsPressed().ToString() + '\n' + Touchscreen.current.position.IsPressed().ToString();
            //}
//#endif


            //playerInput.actions["MovePress"].performed += Player_performed1;
            //mousePos = Mouse.current.position.ReadValue();
            //worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            // worldPos.z = 0;
//#if desktop
            //if (Mouse.current.rightButton.wasPressedThisFrame)
                //DetectClick();

            //if (Mouse.current.leftButton.isPressed)
            //    MoveToDirection();

            //textMeshProUGUI.text=Mouse.current.position.ReadValue().ToString(); 
            //Touchscreen.current.position.ReadValue();
            // transform.position = worldPos;

            // Debug.Log(transform.forward);
            //Vector3 diff = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;
            //float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(0, 0, rotZ);
            // Debug.LogFormat("X:{0}, Y:{1}",pos.x,pos.y);
            //transform.rotation=Quaternion.LookRotation(Vector3.forward,mousePos-transform.position);
            //var k = Keyboard.current;
            //if (k.wKey.isPressed)
            //{
            //    _directionDeplacement.y = 1;
            //}
            //else if (k.sKey.isPressed)
            //{
            //    _directionDeplacement.y = -1;
            //}
            //else
            //{
            //    _directionDeplacement.y = 0;
            //}
            //if (k.aKey.isPressed)
            //{
            //    _directionDeplacement.x = -1;
            //}
            //else if (k.dKey.isPressed)
            //{
            //    _directionDeplacement.x = 1;
            //}
            //else
            //{
            //    _directionDeplacement.x = 0;
            //}

            //if (k.pKey.wasPressedThisFrame)
            //    ChangeStateUI(_uiSkill);

            //if (k.lKey.wasPressedThisFrame)
            //    ChangeSpeed(1);
            //else if (k.kKey.wasPressedThisFrame)
            //    ChangeSpeed(-1);
        }

        private void Player_performed1(CallbackContext ctx)
        {
            //debugText.text = ctx.ToString();
        }

        private void Player_performed(CallbackContext obj)
        {
            Debug.Log("here");
        }

        #region Event Input
        public void ReadPositionInput(CallbackContext ctx)
        {
            if (ctx.performed)
            {
                mousePos = ctx.ReadValue<Vector2>();
                worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                //MoveToDirection();
                textMeshProUGUI.text = mousePos.ToString();
                Move();
                Rot();
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
            //float hold=ctx.ReadValue<float>();
            //Debug.Log(hold);
            //Debug.Log(ctx.time);

            debugText.text=ctx.ToString();

            

            //debugText.text = ctx.performed.ToString()+"\n hello";
            if (ctx.performed)
            {
                //Debug.Log(true);

            }
            //if (ctx.performed)
            //    Debug.Log("aaa");
            //MoveToDirection();
        }
        #endregion

        private void DetectClick()
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider is not null)
            {
                //Debug.Log(hit.collider.gameObject.name);
                //do stuff of collecting here
                if (hit.collider.gameObject.TryGetComponent(out Nourriture n))
                {
                    //Debug.Log("here");
                    GiveFood(n.GetFood());
                }
            }
            //if(Physics2D.Raycast())
        }

        void FaceCam()
        {
            //fix this
            //Vector3 mousePos = Mouse.current.position.ReadValue();
            //mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 dir = new(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
            transform.up = dir;
        }
        void MoveToDirection()
        {
            //move player to target or direction
            //Debug.Log(worldPos);
            // _rb.MovePosition(new(worldPos.x,worldPos.y));
            // _rb.velocity=new(worldPos.x*Vitesse,worldPos.y*Vitesse);

            

            _rb.AddForce(new((worldPos.x - transform.position.x) * Vitesse, (worldPos.y - transform.position.y) * Vitesse));
            FaceCam();
        }

        void ChangeStateUI(GameObject target)
        {
            target.SetActive(!target.activeSelf);
        }

        void FixedUpdate()
        {
            // if (_directionDeplacement != Vector2.zero)
            // {
            //     // Vector2 newPos=new(transform.position.x+_directionDeplacement.x,transform.position.y+_directionDeplacement.y);
            //     // _rb.MovePosition(newPos);
            //     // _rb.AddForce(_directionDeplacement,ForceMode2D.Force);
            //     _rb.velocity = _directionDeplacement * Vitesse;
            //     //_rb.MovePosition(new(transform.position.x+_directionDeplacement.x,transform.position.y+_directionDeplacement.y));
            // }
            // else{
            //     _rb.velocity=Vector2.zero;
            // }

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            // Vector3 direction=transform.TransformDirection(Vector3.forward)*500;
            Gizmos.DrawRay(transform.position, worldPos);
        }

        void ChangeSpeed(float Value)
        {
            Vitesse += Value;
            Debug.Log(Vitesse);
        }

        void GiveFood(float value)
        {
            Faim += value;
            UpdateSlider();
        }

        internal void UpgradeStats(Skill.SkillTemplate skillTemplate)
        {
            ChangeSpeed(skillTemplate.statEffectValue);
        }

        void UpdateSlider()
        {
            _sliderFaim.value = Faim;
            _sliderFaim.maxValue = MaxValeurFaim;
            _sliderFaim.gameObject.SetActive(false);
        }

        //Detect if attack
        //detect if hungry
        //detect if eating
        //detect if evolving
        //detect if mutatting

        void LoopAction()
        {
            LoopHunger().ConfigureAwait(false);
        }

        internal async Task LoopHunger()
        {
            DelayTickFaim = 1000;
            //Debug.Log(DelayTickFaim);
            DelayTickFaim = 2;
            //Debug.Log(DelayTickFaim);
            await Task.Delay(1000);
            //Debug.Log("aaa");
            await Task.Delay(5000);
            //Debug.Log("aaa 5");
            await Task.Yield();
        }

        internal void DonnerCompetence(Skill.SkillTemplate skillTemplate)
        {

        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(_tagNourriture))
            {
                Debug.Log("Nourriture");
                GiveFood(other.gameObject.GetComponent<Nourriture>().GetFood());
                Destroy(other.gameObject);
            }
        }
    }
}