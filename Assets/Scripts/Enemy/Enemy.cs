using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{

    public class Enemy : MonoBehaviour
    {
        public float foodToGive = 15f;
        public int xpToGive = 15;
        private int MaxLife = 10;
        private int _life = 1;
        public int Life { get => _life; set { _life = Mathf.Clamp(value, 0, MaxLife); } }
        Player.PlayerDanger player;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TakeDamage(int damageValue, Player.PlayerDanger player)
        {
            this.player = player;
            Life -= damageValue;
        }
    }

}