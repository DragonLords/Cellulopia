using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class flee : MonoBehaviour
{
    public Vector3[] target = new Vector3[2];
    public LayerMask layer;
    public int numberContact;
    public Transform player;
    public NavMeshAgent agent;
    public int multiplier=1;
    public float range=30f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        target = new Vector3[3];
        var coll = Physics.OverlapSphere(transform.position, 15f, layer);
        numberContact = coll.Length;
        if (coll.Length > 0)
        {
            target[0] = coll[0].transform.position;
            target[1] = new(coll[0].transform.position.x * -1, transform.position.y, coll[0].transform.position.z * -1);
            // float dst = float.MaxValue;
            // int selected = 0;
            // for (int i = 0; i < coll.Length; ++i)
            // {
            //     float distance = Vector3.Distance(transform.position, coll[i].transform.position);
            //     if (distance < dst)
            //     {
            //         selected = i;
            //         dst = distance;
            //     }
            // }
            // target[2]=new(coll[selected].transform.position.x-transform.position.x,transform.position.y,coll[selected].transform.position.z-transform.position.z);
            // target[2] = new((coll[selected].transform.position.x * -1)*10, transform.position.y, (coll[selected].transform.position.z * -1)10);
            // transform.position=target[2];
            target[2]=transform.position+((transform.position-player.position)*1);
            float distance=Vector3.Distance(transform.position,player.position);
            if(distance<range)  agent.SetDestination(target[2]);
        }
    }
}
