using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Rework
{
    public class PlayerCollision : MonoBehaviour
    {
        Player player;
        // Start is called before the first frame update
        void Start()
        {
            player = GetComponentInParent<Player>();
        }

        public void TakeDamage(int value)
        {
            player.TakeDamage(value);
        }
    }
}