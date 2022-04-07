using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Threading.Tasks;

namespace Player
{
    public class Player : MonoBehaviour
    {

        Vector3 mousePos = Vector2.one;
        Vector3 worldPos = Vector3.one;
        Vector2 _directionDeplacement = Vector2.zero;
        CharacterController controller;
        Rigidbody2D _rb;
        float _vitesse = 5f;
        const float _maxVitesse = 50f;
        [property: SerializeField] public float Vitesse { get => _vitesse; set => _vitesse = Mathf.Clamp(value, .5f, _maxVitesse); }
        float _delayTickFaim=5;
        public float DelayTickFaim { get => _delayTickFaim=_delayTickFaim*1000; set => _delayTickFaim = value; }
        float _faim = 0;
        float MaxValeurFaim = 50;
        public float Faim { get => _faim; set => _faim = Mathf.Clamp(value, 0, MaxValeurFaim); }

        #region UI
        [SerializeField] GameObject _uiSkill;
        [SerializeField] Slider _sliderFaim;
        [SerializeField] Slider _sliderVie;
        #endregion

        #region Tags
        [TagSelector,SerializeField] String _tagNourriture;
        [TagSelector,SerializeField] String _tagEnnemy;
        #endregion

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            _rb = GetComponent<Rigidbody2D>();
            Faim = 10f;
            _sliderFaim.interactable = false;
            UpdateSlider();
        }

        // Start is called before the first frame update
        void Start()
        {
            LoopAction();
            Debug.Log("start ended");
        }

        // Update is called once per frame
        void Update()
        {
            mousePos = Mouse.current.position.ReadValue();
            worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            // worldPos.z = 0;

            if (Mouse.current.rightButton.wasPressedThisFrame)
                DetectClick();
            else if (Mouse.current.leftButton.isPressed)
                MoveToDirection();
            // transform.position = worldPos;

            // Debug.Log(transform.forward);
            //Vector3 diff = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;
            //float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(0, 0, rotZ);
            // Debug.LogFormat("X:{0}, Y:{1}",pos.x,pos.y);
            //transform.rotation=Quaternion.LookRotation(Vector3.forward,mousePos-transform.position);
            var k = Keyboard.current;
            if (k.wKey.isPressed)
            {
                _directionDeplacement.y = 1;
            }
            else if (k.sKey.isPressed)
            {
                _directionDeplacement.y = -1;
            }
            else
            {
                _directionDeplacement.y = 0;
            }
            if (k.aKey.isPressed)
            {
                _directionDeplacement.x = -1;
            }
            else if (k.dKey.isPressed)
            {
                _directionDeplacement.x = 1;
            }
            else
            {
                _directionDeplacement.x = 0;
            }

            if (k.pKey.wasPressedThisFrame)
                ChangeStateUI(_uiSkill);

            if (k.lKey.wasPressedThisFrame)
                ChangeSpeed(1);
            else if (k.kKey.wasPressedThisFrame)
                ChangeSpeed(-1);
        }

        private void DetectClick()
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider is not null)
            {
                Debug.Log(hit.collider.gameObject.name);
                //do stuff of collecting here
                if (hit.collider.gameObject.TryGetComponent<Nourriture>(out Nourriture n))
                {
                    Debug.Log("here");
                    GiveFood(n.GetFood());
                }
            }
            //if(Physics2D.Raycast())
        }

        void MoveToDirection()
        {
            //move player to target or direction
            Debug.Log(worldPos);
            // _rb.MovePosition(new(worldPos.x,worldPos.y));
            // _rb.velocity=new(worldPos.x*Vitesse,worldPos.y*Vitesse);
            _rb.AddForce(new((worldPos.x - transform.position.x) * Vitesse, (worldPos.y - transform.position.y) * Vitesse));
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

        void LoopAction(){
            LoopHunger();
        }

        internal async Task LoopHunger(){
            DelayTickFaim=1000;
            Debug.Log(DelayTickFaim);
            DelayTickFaim=2;
            Debug.Log(DelayTickFaim);
            await Task.Delay(1000);
            Debug.Log("aaa");
            await Task.Delay(5000);
            Debug.Log("aaa 5");
            await Task.Yield();
        }

        internal void DonnerCompetence(Skill.SkillTemplate skillTemplate)
        {

        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            if(other.gameObject.CompareTag(_tagNourriture)){
                Debug.Log("Nourriture");
                GiveFood(other.gameObject.GetComponent<Nourriture>().GetFood());
                Destroy(other.gameObject);
            }
        }
    }
}