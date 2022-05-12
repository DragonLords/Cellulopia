using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestDesc : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt;
    [SerializeField] Image _imgIcon;
    

    public void ShowDesc(string value,Sprite icon){
        if(txt==null)
            txt=GetComponentInChildren<TextMeshProUGUI>();
        txt.text=value;
        if(_imgIcon is null)
            _imgIcon=GetComponentInChildren<Image>();
        _imgIcon.sprite=icon;
        
    }
}
