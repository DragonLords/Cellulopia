using UnityEngine;

namespace Player.Rework.Danger
{
    public class PlayerDanger : MonoBehaviour
    {
        Player player;
        [SerializeField, TagSelector] string foodTag;
        [SerializeField, TagSelector] string enemyTag;
        [SerializeField] Vector3 detectionSize=new(6,1,2);
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            player=GetComponentInParent<Player>();
            GetComponent<BoxCollider>().size=detectionSize;
        }

        private void OnCollisionEnter(Collision other)
        {
            if(other.gameObject.CompareTag(foodTag)){
                //do srtuff about food here
                Debug.Log("got food");
                Destroy(other.gameObject);
            }else if(other.gameObject.CompareTag(enemyTag)){
                //do stuff about enemy here
                Debug.Log("Killed");
                Destroy(other.gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color=Color.magenta;
            Gizmos.DrawWireCube(transform.position,detectionSize);
        }
    }
}