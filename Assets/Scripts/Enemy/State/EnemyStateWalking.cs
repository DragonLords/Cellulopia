using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Enemy.State{

    public class EnemyStateWalking : EnemyStateBase
    {
        Enemy enemy;
        public override void InitState(Enemy enemy)
        {
            this.enemy=enemy;
            enemy.walking=true;
            // KeepMoving(enemy).ConfigureAwait(false).GetAwaiter();
            Moves().ConfigureAwait(false).GetAwaiter();
        }

        async Task KeepMoving(Enemy enemy){
            do
            {
                // Debug.LogFormat("path:{0} dst:{1} dst:{2}",enemy.agent.pathPending,enemy.agent.remainingDistance,enemy.agent.destination);
                await Task.Yield();
                if(enemy.agent.remainingDistance<1f&&!enemy.isHungry){
                    enemy.agent.isStopped=true;
                    await Task.Delay(1500);
                    enemy.agent.isStopped=false;
                    Vector3 target=new(Random.Range(enemy.bounds[0].x,enemy.bounds[1].x),0,Random.Range(enemy.bounds[0].z,enemy.bounds[1].z));
                    MoveToPoint(enemy,target);
                }
                // await Task.Delay(1500);
                await Task.Yield();
            } while (enemy.alive&&enemy.walking&&enemy.agent.remainingDistance>1f&&!enemy.isHungry);
            // Debug.Log("break");
        }

        async Task Moves(){
            Vector3 target=enemy.RandomPosBounds();
            await Task.Yield();
            do
            {   
                if(enemy.isHungry){
                    await Task.Yield();
                    break;
                }
                enemy.agent.SetDestination(target);
                await Task.Yield();
            } while (enemy.alive&&enemy.walking&&enemy.agent.remainingDistance>1f&&!enemy.isHungry);
            await Task.Yield();
            EndState(enemy);
        }

        void MoveToPoint(Enemy enemy,Vector3 target){
            enemy.agent.SetDestination(target);
        }

        public override void UpdateState(Enemy enemy)
        {
            throw new System.NotImplementedException();
        }

        public override void EndState(Enemy enemy)
        {
            // Debug.Log("end walking");
            enemy.walking=false;
            enemy.agent.isStopped=true;
            enemy.RequestNewState();
        }
    }

}