using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class Ennemy : MonoBehaviour
{
    Tilemap map;
    Rigidbody2D rb;
    float speed;
    [SerializeField] NavMeshAgent agent;
    public Transform trans;
    public bool stop;

    [SerializeField] LayerMask _foodLayer;
    [TagSelectorAttribute] public string tagFood;
    public readonly float foodToGive=15f;
    public readonly int xpToGive=5;
    #region State
    
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // rb=GetComponent<Rigidbody2D>();
        // agent=GetComponent<NavMeshAgent>();
        Vector2 direction=new(15,5);
        // StartCoroutine(MoveToPoint(new(15,5)));
        rb=GetComponent<Rigidbody2D>();
        // rb.AddForce(Vector2.up*15);
        // PickRandomDirection();
        // StartCoroutine(DetectFood());
    }

    // Update is called once per frame
    void Update()
    {
        // trans.position=new(trans.position.x,trans.position.y,0);
        // trans.rotation=Quaternion.Euler(0,0,0);
    }

    IEnumerator MoveToPoint(Vector2 point){
        
        agent.SetDestination(point);
        do
        {
            agent.gameObject.transform.rotation=Quaternion.Euler(0,0,0);
            yield return null;
        } while (agent.remainingDistance<1.5f||agent.pathPending||stop);
        stop=!true;
    }
    Vector2 offset=new(15,15);
    public Vector2 forceFinal;
    void PickRandomDirection(){
        stop=!true;
        float rndAngle=Random.Range(0,361);
        float rndX=Random.Range(transform.position.x-offset.x,(transform.position.x+offset.x)+1);
        float rndY=Random.Range(transform.position.y-offset.y,(transform.position.y+offset.y)+1);
        forceFinal=new(rndX-transform.position.x,rndY-transform.position.y);
        rb.AddForce(forceFinal*15);
    }

    private void FixedUpdate()
    {
        if(stop){
            rb.velocity=Vector2.zero;
            PickRandomDirection();
        }
    }

    float radius=5f;
    IEnumerator DetectFood(){
        do
        {
            
        Collider2D[] colliders=Physics2D.OverlapCircleAll(transform.position,radius,_foodLayer);
        foreach(var collider in colliders){
            // RaycastHit2D hit=Physics2D.Raycast(transform.position,collider.transform.position,radius,_foodLayer);
            // Debug.Log(hit.point);
            // Debug.Log(collider.name);
        }
        if(colliders.Length>0)
            {
                target=colliders[0].transform.position;
                StartCoroutine(MoveToFood());
            }
        yield return null;
        } while (alive||target==Vector2.zero);
    }
    bool alive=true;

    Vector2 PickRandomDestination(){
        int rnd=Random.Range(0,GameManager.Instance.emptyTiles.Count);
        return GameManager.Instance.emptyTiles[rnd]; 
    }

    public Vector2 target;
    IEnumerator MoveToFood(){
        do
        {
            transform.position=Vector2.MoveTowards(transform.position,target,5f*Time.deltaTime);
            yield return null;
        } while (!stop);
    }
    
    private void OnCollisionEnter2D(Collision2D other) {
        stop=true;
        if(other.gameObject.CompareTag(tagFood)){
            Destroy(other.gameObject);
            target=Vector2.zero;
            StartCoroutine(DetectFood());
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color=Color.red;
    }
}
