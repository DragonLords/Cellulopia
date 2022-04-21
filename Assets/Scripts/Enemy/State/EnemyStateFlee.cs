using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Enemy.State{

    public class EnemyStateFlee : EnemyStateBase
    {
        Enemy enemy;
        public override void InitState(Enemy enemy)
        {
            // Debug.Log("Need to disapear");
            this.enemy=enemy;
            Runaway().ConfigureAwait(false).GetAwaiter();
        }

        async Task Runaway(){
            do
            {
                enemy.isInDanger=Physics.CheckSphere(enemy.transform.position,enemy.radiusDetectionDanger,enemy.dangerLayers);
                var colliders=Physics.OverlapSphere(enemy.transform.position,enemy.radiusDetectionDanger,enemy.dangerLayers);
                var a=colliders.ToList();
                a.Remove(enemy._collider);
                colliders=a.ToArray();
                await Task.Yield();
                if(colliders.Length==0){
                    // EndState(enemy);
                    // Debug.Log("im safe now");
                    await Task.Yield();
                }
                //get the closest one
                float dst=float.MaxValue;
                int selected=0;
                await Task.Yield();
                for(int i=0;i<colliders.Length;++i){
                    float distance=Vector3.Distance(enemy.transform.position,colliders[i].transform.position);
                    if(distance<dst){
                        selected=i;
                        dst=distance;
                        await Task.Yield();
                    }
                    await Task.Yield();
                }
                //go in opposite direction
                Vector3 targetDirection=enemy.transform.position+((enemy.transform.position-colliders[0].transform.position)*enemy.multiplierFlee);
                if(dst<enemy.rangeFlee)
                    enemy.agent.SetDestination(targetDirection);
                enemy.targetFlee=targetDirection;
                await Task.Yield();
                // Debug.Log("flee");
                
                await Task.Yield();
                //check if still in danger

            } while (enemy.alive&&enemy.isInDanger);
            EndState(enemy);
        }

        public override void UpdateState(Enemy enemy)
        {
            throw new System.NotImplementedException();
        }
        
        public override void EndState(Enemy enemy)
        {
            enemy.RequestNewState();
        }
    }

}