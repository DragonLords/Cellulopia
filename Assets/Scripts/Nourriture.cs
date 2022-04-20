using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Nourriture : MonoBehaviour
{
    float _nombreNourritureDonner=5f;
    public readonly int EvolutionPointToGive=1;
    public void Init(){
        // System.Random rnd=new(69);
        // rnd.Next()
        // _nombreNourritureDonner=Random.Range(1,float.MaxValue);
    }


    private void Awake()
    {
        // SpriteRenderer sr=GetComponent<SpriteRenderer>();
        // sr.color=IsItGood?Color.blue:Color.red;
        Init();
    }

    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }

    public float GetFood()=>_nombreNourritureDonner;
}