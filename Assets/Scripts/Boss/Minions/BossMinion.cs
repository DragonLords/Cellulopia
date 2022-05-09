using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace Boss.Minion
{
    public class BossMinion : MonoBehaviour
    {
        [SerializeField] MinionSetting setting;
        public bool needToProtect = false;
        [SerializeField] float maxOffsetRadius = 5f;
        public Vector3 offset;
        public Vector3 offsetClamped;
        public Vector3 dir;
        public Vector3 final;
        public Vector3 finalClamped;
        float radiusTurn = 0f;
        [SerializeField] bool AddRandomnessOrbit = true;
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

        IEnumerator ChasePlayerToAttack()
        {
            do
            {
                agent.SetDestination(player.position);
                yield return null;
            } while (agent.remainingDistance > 1f && alive);
            yield return null;
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

        IEnumerator RotateAroundBoss()
        {
            bool positive = Random.Range(0, 2) == 1;
            if (!positive)
                _orbitSpeed *= -1f;
            do
            {
                selfContainer.RotateAround(boss.transform.position, Vector3.up, _orbitSpeed * Time.deltaTime);
                yield return null;
            } while (alive);
        }

        IEnumerator RotateAroundNav()
        {
            bool positive = Random.Range(0, 2) == 1;
            if (!positive)
                _orbitSpeed *= -1f;

            // if(AddRandomnessOrbit){
            //     radiusTurn=Random.Range(0,maxOffsetRadius+1);
            // }
            float angularSpd = agent.angularSpeed;
            // agent.speed=_orbitSpeed;
            do
            {
                float distance = Vector3.Distance(selfContainer.position, boss.BossCenter.position);
                if (distance > boss._maxDistanceMinions)
                {

                }
                offset = boss.BossCenter.transform.position - selfContainer.transform.position;
                offsetClamped = new(Mathf.Clamp(offset.x, -boss._maxDistanceMinions, boss._maxDistanceMinions), offset.y, Mathf.Clamp(offset.z, -boss._maxDistanceMinions, boss._maxDistanceMinions));
                //FIXME: when the boss move the minions doesnt follow
                //FIXED: It works now the clamp finished by working
                //Hourray!!!
                dir = Vector3.Cross(offset, Vector3.up * _orbitSpeed);
                final = selfContainer.position + dir;
                finalClamped = new(Mathf.Clamp(final.x, boss.BossCenter.position.x - boss._maxDistanceMinions, boss.BossCenter.position.x + boss._maxDistanceMinions), final.y, Mathf.Clamp(final.z, boss.BossCenter.position.z - boss._maxDistanceMinions, boss.BossCenter.position.z + boss._maxDistanceMinions));
                agent.SetDestination(finalClamped);
                yield return null;
            } while (alive && !tooFar && !needToProtect);
            agent.angularSpeed = angularSpd;
        }
        bool tooFar = false;
        Coroutine routineClose = null;
        IEnumerator CheckIfTooFarBoss()
        {

            float distance = Vector3.Distance(selfContainer.position, boss.BossCenter.position);
            if (distance > boss._maxDistanceMinions)
            {
                tooFar = true;
                Debug.LogFormat("I am too far of {0} unit", distance);
            }
            do
            {
                float dst = Vector3.Distance(selfContainer.position, boss.BossCenter.position);
                if (dst > boss._maxDistanceMinions)
                {
                    tooFar = true;
                    Debug.LogFormat("I am too far of {0} unit", dst);
                }
                else
                {
                    tooFar = false;
                }
                yield return null;
                // if(routineClose == null)
                //     routineClose=StartCoroutine(GoBackInRadiusBoss());
            } while (alive);
        }

        IEnumerator GoBackInRadiusBoss()
        {
            do
            {
                CheckIfTooFarBoss();
                // Vector3 point=new(selfContainer.position-(boss.BossCenter.position+boss._maxDistanceMinions));
                // agent.SetDestination();
                yield return null;
            } while (alive && Vector3.Distance(selfContainer.position, boss.BossCenter.position) > boss._maxDistanceMinions);
            tooFar = false;
            routineClose = null;
            StartCoroutine(RotateAroundNav());
        }

        //FIXME: will have to stop the chase for this to take place
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
            Debug.Log("protecting is my duty");
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
                    if (distance > boss._maxDistanceProtection)
                    {
                        Debug.Log("too far");
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(new(transform.position, transform.position + transform.forward * 500f));
        }
    }
}