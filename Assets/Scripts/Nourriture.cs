using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Nourriture : MonoBehaviour
{
    UnityEventQueueSystem queueSystem=new();
    UnityEvent donnerNourritureEvent=new();
    float _nombreNourritureDonner=5f;

    public void Init(){
        // _nombreNourritureDonner=Random.Range(1,float.MaxValue);
    }

    private void Awake()
    {
        Init();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetFood()=>_nombreNourritureDonner;
}