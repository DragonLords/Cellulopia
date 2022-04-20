using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Enemy.State{

    public class EnemyStateSocialize : EnemyStateBase
    {
        Enemy enemy;
        public override void InitState(Enemy enemy)
        {
            this.enemy=enemy;
            Debug.Log("i wanna socialize with poeple... <color=yellow>I fEeL sO aLoNe!!!</color>");
        }

        void CheckForProx(){
            Collider[] colls=Physics.OverlapSphere(enemy.transform.position,enemy.radiusDetectionSocial,enemy.othersLayer);
            if(colls is not null){
                //so we detect someone houray
                var coll=colls.First();
                //is it an enemy?
                if(enemy.enemies.ContainsKey(coll.gameObject)){
                    //oh no thats a baddy
                    //it is really unsafe with it
                }else if(enemy.neutrals.ContainsKey(coll.gameObject)){
                    //hey its neutral and maybe a friend... or enemy
                    //it is mid-safe with it
                }else{
                    //so thats an ally and maybe more ^^
                    //it is safe with it
                }
            }
        }
        async Task<bool> CooldownSocial(){
            await Task.Delay(enemy.cooldownSocial);
            return enemy.canSocialize=true;
        }

        public override void UpdateState(Enemy enemy)
        {
            throw new System.NotImplementedException();
        }
        
        public override void EndState(Enemy enemy)
        {
            throw new System.NotImplementedException();
        }
    }

}