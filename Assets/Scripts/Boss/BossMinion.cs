using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Boss.Minion
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class BossMinion : MonoBehaviour
    {
        NavMeshAgent agent;
        int life=1;
        Transform player;
        ParticleSystem ps;
        Renderer rend;
        // Use this for initialization
        void Start()
        {
            player=FindObjectOfType<Player.Rework.Player>().transform;
            agent = GetComponent<NavMeshAgent>();
            rend = GetComponent<Renderer>();
            StartCoroutine(ChasePlayerToAttack());
        }

        IEnumerator ChasePlayerToAttack()
        {
            do
            {
                agent.SetDestination(player.position);
                yield return null;  
            } while (agent.remainingDistance > 1f);
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
            var particle=Instantiate(ps,transform);

            if (!particle.isPlaying)
            {
                particle.Play();
            }
            Material mat = rend.material;
            do
            {
                mat.color = new Color(mat.color.r,mat.color.g,mat.color.b,mat.color.a-.1f);
                yield return new WaitForSeconds(.2f);
            } while (mat.color.a>.1f);
            yield return null;
            Destroy(gameObject);
        }
    }
}