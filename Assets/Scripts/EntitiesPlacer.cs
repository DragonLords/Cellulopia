using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitiesPlacer : MonoBehaviour
{
    GameManager manager;
    [SerializeField] Transform player;
    [SerializeField] GameObject boss;
    /// <summary>
    /// Jai besoin de offset la pos du boss un peu vers la droite puisquil est tres large
    /// </summary>
    float offsetBoss=3f;
    // Start is called before the first frame update
    public void OnStart()
    {
        manager=GameManager.Instance;
        List<Vector2Int> empty=new(GameManager.Instance.emptyTiles);
        
        
        // foreach(var place in empty){
        //     if(manager.ValidatePos(place.x,place.y)){
        //         player.position=new(place[0],player.position.y,place[1]);
        //     }
        // }

        // empty.Reverse();
        // foreach(var place in empty){
        //     if(manager.ValidatePos(place[0],place[1])){
        //         boss.position=new(place[0]+offsetBoss,boss.position.y,place[1]);
        //     }
        // }  
        var b=Instantiate(boss);
        b.transform.position=new(empty[empty.Count/2].x,b.transform.position.y,empty[empty.Count/2].y);

        Destroy(gameObject);  
    }
}
