using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Boss.Minion
{
    /// <summary>
    /// classe qui sert a linteraction du sbire du boss
    /// </summary>
    public class BossMinion : MonoBehaviour
    {
        [SerializeField] int xpGiven = 50;
        internal WaitForSeconds DelayBetweenAttack = new(3f);
        [SerializeField] MinionSetting setting;
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
        internal bool canAttack = true;
        [SerializeField] internal float RadiusPlayer = 3f;
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

        /// <summary>
        /// fonction qui sert a garder les infos par rapport au boss
        /// </summary>
        /// <param name="boss"></param>
        public void SetBoss(Boss boss)
        {
            this.boss = boss;
            selfContainer = agent.transform;
            OnStart();
        }

        // Fonction qui sert a donner les valeur a la naissance du sbire
        void OnStart()
        {
            life = setting.life;
            damage = setting.Damage;
            agent.speed = setting.speed;
            wsDelayAlive = new(DelayAlive);
            rends = GetComponentsInChildren<Renderer>();
            StartCoroutine(RotateAroundNav());
        }

        /// <summary>
        /// Sert a prendre des degats
        /// </summary>
        /// <param name="value">le nombre de degat subi</param>
        public void TakeDamage(int value)
        {
            life -= value;
            if (life < 1)
            {
                StartCoroutine(Death());
            }
            else
            {
                GameManager.Instance.PlaySoundClip(GameManager.Instance.soundStock[SoundType.Hit]);
            }
        }

        /// <summary>
        /// sert a effectuer les actions de mort
        /// </summary>
        /// <returns></returns>
        IEnumerator Death()
        {
            alive = false;
            GameManager.Instance.PlaySoundClip(GameManager.Instance.soundStock[SoundType.Killed]);
            GameManager.Instance.PlayerGiveEXP.Invoke(xpGiven);
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

        /// <summary>
        /// sert a tourner autour du boss
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// sert a tourner autour du joueur 
        /// </summary>
        /// <returns></returns>
        internal IEnumerator TurnAroundPlayer()
        {
            do
            {
                if (player != null)
                {
                    offset = player.position - selfContainer.position;
                    dir = Vector3.Cross(offset, Vector3.up * _orbitSpeed);
                    final = selfContainer.position + dir;
                    finalClamped = new(Mathf.Clamp(final.x, player.position.x - RadiusPlayer, player.position.x + RadiusPlayer), final.y, Mathf.Clamp(final.z, player.position.z - RadiusPlayer, player.position.z + RadiusPlayer));
                    agent.SetDestination(finalClamped);
                }
                else
                {
                    break;
                }
                yield return null;
            } while (!canAttack);
        }

        /// <summary>
        /// sert a attaquer
        /// </summary>
        /// <param name="player">le transfrom du joueur a attaquer</param>
        public void Attack(Transform player)
        {
            // Debug.Log("protecting is my duty");
            needToProtect = true;
            this.player = player;
            StartCoroutine(Pursue());
        }

        /// <summary>
        /// sert a poursuivre le joueur 
        /// </summary>
        /// <returns></returns>
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
            } while (alive && needToProtect && canAttack);
            boss.MinionReturn(selfContainer.gameObject);
            StartCoroutine(RotateAroundNav());
        }
    }
}