using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Enemy.State
{

    public class EnemyStateAttack : EnemyStateBase
    {
        Enemy enemy;
        public override void InitState(Enemy enemy)
        {
            this.enemy = enemy;
            AttackNear().ConfigureAwait(false).GetAwaiter();
        }

        async Task AttackNear()
        {
            enemy.agent.isStopped=false;
            var colls = Physics.OverlapSphere(enemy.transform.position, enemy.radius, enemy.othersLayer);
            if (colls.Length== 0)
                EndState(enemy);
            else
            {
                enemy.isAttacking=true;
                do
                {
                    Vector3 target=colls[0].transform.position;
                    await Task.Yield();
                    enemy.agent.SetDestination(target);
                } while (enemy.alive&&enemy.isAttacking&&enemy.agent.remainingDistance>1);
            } 
            await Task.Yield();
            enemy.isAttacking=false;
            enemy.agent.isStopped=true;
        }

        public override void UpdateState(Enemy enemy)
        {
            throw new System.NotImplementedException();
        }

        public override void EndState(Enemy enemy)
        {
            // Debug.Log("attack state ended");
            enemy.isAttacking=false;
            enemy.readyToDefend=false;
            enemy.inRangeToAttack=false;
            enemy.RequestNewState();
        }
    }

}