using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Enemy.State
{

    public class EnemyStateHungry : EnemyStateBase
    {
        internal bool foodFound = false;
        internal Transform target;
        public override void InitState(Enemy enemy)
        {
            FindFood(enemy).ConfigureAwait(false).GetAwaiter();
        }

        async Task FindFood(Enemy enemy)
        {
            enemy.agent.isStopped=false;
            // Debug.Log("im hungry");
            do
            {
                // Debug.Log("food searching");
                var colls = Physics.OverlapSphere(enemy.transform.position, enemy.radius, enemy.foodLayer);
                if (colls.Length != 0)
                {

                    float dst = float.MaxValue;
                    int selected = 0;
                    for (int i = 0; i < colls.Length; i++)
                    {
                        float distance = Vector3.Distance(enemy.transform.position, colls[i].transform.position);
                        if (distance < dst)
                        {
                            dst = distance;
                            selected = i;
                        }
                    }
                    target = colls[selected].transform;
                    foodFound = true;
                    await Task.Yield();
                }
                await Task.Yield();
            } while (!foodFound);
            enemy.target = target.position;
            GetFood(enemy).ConfigureAwait(false).GetAwaiter();
        }

        async Task GetFood(Enemy enemy)
        {
            // Debug.Log("gotta get food");
            do
            {
                enemy.agent.SetDestination(target.position);
                await Task.Yield();
            } while (enemy.agent.remainingDistance > 1f || !enemy.agent.pathPending);
            // Debug.Log("food got");
            EndState(enemy);
        }

        public override void UpdateState(Enemy enemy)
        {
            throw new System.NotImplementedException();
        }

        public override void EndState(Enemy enemy)
        {
            // Debug.Log("State ended");
            enemy.RequestNewState();
        }
    }

}