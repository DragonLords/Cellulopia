using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPHolder : MonoBehaviour
{
    public GameObject holder;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(5f);
        holder.SetActive(true);
    }
}
