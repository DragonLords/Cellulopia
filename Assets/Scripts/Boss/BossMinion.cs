using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace Boss.Minion
{
    public class BossMinion : MonoBehaviour
    {
        [SerializeField] float DelayAlive=5f;
        WaitForSeconds wsDelayAlive;
        [SerializeField] NavMeshAgent agent;
        int life=1;
        Transform player;
        internal int damage=1;
        ParticleSystem ps;
        Renderer[] rends;
        bool alive=true;
        [SerializeField,TagSelector] internal string playerTag;


        // Use this for initialization
        void Start()
        {
            wsDelayAlive=new(DelayAlive);
            player=FindObjectOfType<Player.Rework.Player>().transform;
            rends = GetComponentsInChildren<Renderer>();
            StartCoroutine(ChasePlayerToAttack());
            // StartCoroutine(DieDelay());
        }

        IEnumerator DieDelay(){
            yield return wsDelayAlive;
            TakeDamage(life);
        }

        IEnumerator ChasePlayerToAttack()
        {
            do
            {
                agent.SetDestination(player.position);
                yield return null;  
            } while (agent.remainingDistance > 1f&&alive);
            yield return null; 
        }

        public void TakeDamage(int value)
        {
            life-=value;
            if (life < 1)
            {
                StartCoroutine(Death());
            }
        }

        IEnumerator Death()
        {
            alive=false;
            agent.isStopped=true;
            agent.destination=transform.position;
            if(ps is not null){
                var particle=Instantiate(ps,transform);
                if (!particle.isPlaying)
                {
                    particle.Play();
                }
            }
            Material mat = rends.First().material;
            mat.SetFloat("_Transparency",1f);
            float newVal=1f;
            do
            {
                foreach(var r in rends)
                {
                    r.material.SetFloat("_Transparency",newVal);
                    newVal-=.1f;
                }
                yield return new WaitForSeconds(.2f);
            } while (newVal>0f);
            yield return null;
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color=Color.magenta;
            Gizmos.DrawRay(new(transform.position,transform.position+transform.forward*500f));
        }
    }
}