using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] int foodSaturation=15;
    public int FoodSaturation=>foodSaturation;
    public int XpGiven=5;
    internal string tagParent;
    // Start is called before the first frame update
    void Awake()
    {
        tagParent=transform.root.tag;
        Debug.Log(tagParent);
    }

}
