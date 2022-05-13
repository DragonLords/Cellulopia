using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] int foodSaturation=15;
    public int FoodSaturation=>foodSaturation;
    public int XpGiven=5;
    // Start is called before the first frame update
    void Start()
    {
        
    }

}
