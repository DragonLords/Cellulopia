using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace Boss.Minion
{
    public class BossMinion : MonoBehaviour
    {
        internal WaitForSeconds DelayBetweenAttack=new(3f);
        [SerializeField] MinionSetting? setting;
        public bool needToProtect = false;
        public Vector3 offset;
        public Vector3 offsetClamped;
        public Vector3 dir;
        public Vector3 final;
        public Vector3 finalClamped;
        Boss boss;
        [SerializeField] float _orbitSpeed = 5f;
        [SerializeField] Transform selfContainer;
        #region Attack
        [Header("Attack")]
        internal bool canAttack=true;
        [SerializeField] internal float RadiusPlayer=3f; 
        #endregion
        [SerializeField] float DelayAlive = 5f;
        WaitForSeconds wsDelayAlive;
        [SerializeField] NavMeshAgent agent;
        int life = 1;
        public Transform player;
        internal int damage = 1;
        ParticleSystem ps;
        Renderer[] rends;
        bool alive = true;
        [SerializeField, TagSelector] internal string playerTag;

        public void SetBoss(Boss boss)
        {
            this.boss = boss;
            selfContainer = agent.transform;
            OnStart();
        }

        // Use this for initialization
        void OnStart()
        {
            life=setting.life;
            damage=setting.Damage;
            agent.speed=setting.speed;


            wsDelayAlive = new(DelayAlive);
            // player=FindObjectOfType<Player.Rework.Player>().transform;
            rends = GetComponentsInChildren<Renderer>();
            // StartCoroutine(ChasePlayerToAttack());
            // StartCoroutine(DieDelay());
            // StartCoroutine(RotateAroundBoss());
            StartCoroutine(RotateAroundNav());
            // StartCoroutine(CheckIfTooFarBoss());
        }

        public void TakeDamage(int value)
        {
            life -= value;
            if (life < 1)
            {
                StartCoroutine(Death());
            }
        }

        IEnumerator Death()
        {
            alive = false;
            agent.isStopped = true;
            agent.destination = transform.position;
            if (ps is not null)
            {
                var particle = Instantiate(ps, transform);
                if (!particle.isPlaying)
                {
                    particle.Play();
                }
            }
            Material mat = rends.First().material;
            mat.SetFloat("_Transparency", 1f);
            float newVal = 1f;
            do
            {
                foreach (var r in rends)
                {
                    r.material.SetFloat("_Transparency", newVal);
                    newVal -= .1f;
                }
                yield return new WaitForSeconds(.2f);
            } while (newVal > 0f);
            yield return null;
            boss.minions.Remove(selfContainer.gameObject);
            Destroy(selfContainer.gameObject);
        }

        IEnumerator RotateAroundNav()
        {
            bool positive = Random.Range(0, 2) == 1;
            if (!positive)
                _orbitSpeed *= -1f;
            float angularSpd = agent.angularSpeed;
            do
            {
                float distance = Vector3.Distance(selfContainer.position, boss.BossCenter.position);
                if (distance > boss.setting.rangeMinion)
                {

                }
                offset = boss.BossCenter.transform.position - selfContainer.transform.position;
                offsetClamped = new(Mathf.Clamp(offset.x, -boss.setting.rangeMinion, boss.setting.rangeMinion), offset.y, Mathf.Clamp(offset.z, -boss.setting.rangeMinion, boss.setting.rangeMinion));
                dir = Vector3.Cross(offset, Vector3.up * _orbitSpeed);
                final = selfContainer.position + dir;
                finalClamped = new(Mathf.Clamp(final.x, boss.BossCenter.position.x - boss.setting.rangeMinion, boss.BossCenter.position.x + boss.setting.rangeMinion), final.y, Mathf.Clamp(final.z, boss.BossCenter.position.z - boss.setting.rangeMinion, boss.BossCenter.position.z + boss.setting.rangeMinion));
                agent.SetDestination(finalClamped);
                yield return null;
            } while (alive && !needToProtect);
            agent.angularSpeed = angularSpd;
        }

        
        internal IEnumerator TurnAroundPlayer(){
            do
            {
                if(player!=null){
                    offset=player.position-selfContainer.position;
                    dir = Vector3.Cross(offset, Vector3.up * _orbitSpeed);
                    final = selfContainer.position + dir;
                    finalClamped = new(Mathf.Clamp(final.x, player.position.x - RadiusPlayer, player.position.x + RadiusPlayer), final.y, Mathf.Clamp(final.z, player.position.z - RadiusPlayer, player.position.z + RadiusPlayer));
                    agent.SetDestination(finalClamped);
                }else{
                    break;
                }
                yield return null;
            } while (!canAttack);
        }

        public void Attack(Transform player)
        {
            // Debug.Log("protecting is my duty");
            needToProtect = true;
            this.player = player;
            StartCoroutine(Pursue());
        }

        IEnumerator Pursue()
        {
            do
            {
                if (player != null)
                {

                    agent.SetDestination(player.position);
                    float distance = Vector3.Distance(player.position, boss.BossCenter.position);
                    if (distance > boss.setting.rangeMinionAttack)
                    {
                        // Debug.Log("too far");
                        needToProtect = false;
                    }
                }
                else
                {
                    break;
                }
                yield return null;
            } while (alive && needToProtect&&canAttack);
            boss.MinionReturn(selfContainer.gameObject);
            StartCoroutine(RotateAroundNav());
        }
    }
}