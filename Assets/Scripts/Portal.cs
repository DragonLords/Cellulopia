using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void TriggerBossFight(){
        SceneManager.LoadScene(2,LoadSceneMode.Single);
    }
}