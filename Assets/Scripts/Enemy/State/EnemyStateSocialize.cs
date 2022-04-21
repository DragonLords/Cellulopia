using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Enemy.State{

    public class EnemyStateSocialize : EnemyStateBase
    {
        Enemy enemy;
        float tresholdStop=1.5f;
        
        public override void InitState(Enemy enemy)
        {
            this.enemy=enemy;
            enemy.canSocialize=true;
            enemy.isSocalizing=false;
            // Debug.Log("i wanna socialize with poeple... <color=yellow>I fEeL sO aLoNe!!!</color>");
            // CheckForProx();
            CheckForSocial().ConfigureAwait(false);
        }

        async Task CheckForSocial(){
            do
            {
                if(enemy.canSocialize&&!enemy.isSocalizing){
                    await Task.Yield();
                    CheckForProx();
                }
                await Task.Yield();
            } while (enemy.alive);
        }

        void CheckForProx(){
            Collider[] colls=Physics.OverlapSphere(enemy.transform.position,enemy.radiusDetectionSocial,enemy.othersLayer);
            if(colls.Length>0){
                //so we detect someone houray
                var coll=colls.First();
                //is it an enemy?
                if(enemy.enemies.ContainsKey(coll.gameObject)){
                    // Debug.Log("oh no thats a baddy");
                    //it is really unsafe with it
                }else if(enemy.neutrals.ContainsKey(coll.gameObject)){
                //    Debug.Log("hey its neutral and maybe a friend or enemy");
                    //it is mid-safe with it
                }else{
                    // Debug.Log("so thats an ally");
                    //it is safe with it
                    GoSocialize(coll.gameObject);
                }
            }
        }

        void GoSocialize(GameObject who){
            enemy.isSocalizing=false;
            enemy.agent.isStopped=false;
            enemy.agent.SetDestination(who.transform.position);
            StopNear(who).ConfigureAwait(false).GetAwaiter();
        }

        async Task StopNear(GameObject who){
            do
            {
                enemy.agent.SetDestination(who.transform.position);
                await Task.Yield();
                // Debug.Log("movbingdsfhisd`f");
            } while (enemy.agent.remainingDistance>tresholdStop&&enemy.alive);
            enemy.agent.isStopped=true;
            await Task.Yield();
            CooldownSocial().ConfigureAwait(false).GetAwaiter();
        }

        async Task<bool> CooldownSocial(){
            enemy.isSocalizing=false;
            // Debug.Log("socialization is the key");
            await Task.Delay(enemy.cooldownSocial);
            return enemy.canSocialize=true;
        }

        public override void UpdateState(Enemy enemy)
        {
            throw new System.NotImplementedException();
        }
        
        public override void EndState(Enemy enemy)
        {
            enemy.ChangeState(actionState.Hungry);    
        }
    }

}