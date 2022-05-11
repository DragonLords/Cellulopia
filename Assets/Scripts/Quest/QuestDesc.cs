using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestDesc : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void ShowDesc(string value){
        if(txt==null)
            txt=GetComponentInChildren<TextMeshProUGUI>();
        txt.text=value;
    }
}
