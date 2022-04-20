using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Player
{

    public class PlayerDanger : MonoBehaviour
    {
        [SerializeField] Player playerRef;
        [TagSelector] public string EnemyTag;
        [TagSelector] public string FoodTag;
        [TagSelector] public string MalusTag;
        public LayerMask enemyLayer;

        void awake(){

        }

        // Start is called before the first frame update
        void Start()
        {
            // StartCoroutine(GetPosEn());
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            Debug.Log(playerRef.hasQuest);
            if(other.gameObject.CompareTag(EnemyTag)){
                var en=other.gameObject.GetComponent<Enemy.Enemy>();
                // playerRef.playerGiveFood.Invoke(food);
                playerRef.GiveFood(en.foodToGive);
                playerRef.GetEvolutionGrade(en.xpToGive);
                if(playerRef.hasQuest){
                    playerRef.QuestItem(other.gameObject);
                }
                // Destroy(other.gameObject);
                en.TakeDamage(1,this);
            }
            else if(other.gameObject.CompareTag(FoodTag)){
                var food=other.gameObject.GetComponent<Nourriture>();
                playerRef.GiveFood(food.GetFood());
                playerRef.GetEvolutionGrade(food.EvolutionPointToGive);
                // Debug.Log(playerRef.hasQuest);
                if(playerRef.hasQuest){
                    playerRef.QuestItem(other.gameObject);
                }
                Destroy(other.gameObject);
            }else if(other.gameObject.CompareTag(MalusTag)){
                var malus=other.gameObject.GetComponent<Malus>();
                playerRef.GiveFood(malus.MalusFood);
                playerRef.TakeDamage(malus.LifeSub);
                if(playerRef.hasQuest){
                    playerRef.QuestItem(other.gameObject);
                }
                Destroy(other.gameObject);
            }
        }

        internal void GetReward(int xp,float food){
            playerRef.GiveFood(food);
            playerRef.TakeDamage(xp);
        }
        
        IEnumerator GetPosEn(){
            Collider2D collider=Physics2D.OverlapCircle(transform.position,float.MaxValue,enemyLayer);
            var go=Addressables.InstantiateAsync("FoodPath").WaitForCompletion();
            LineRenderer line=go.GetComponent<LineRenderer>();
            Vector3[] pos={transform.position,collider.gameObject.transform.position};
            line.SetPositions(pos);
            yield return null;
        }
    }

}