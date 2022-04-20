using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSkillTree : MonoBehaviour
{
    Button button;
    [SerializeField] GameObject skillTreeHolder;
    private void Awake()
    {
        button=GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        if(skillTreeHolder is null)
            skillTreeHolder=FindObjectOfType<SkillTreeHandler>().gameObject;
    }

    void OnClick(){
        skillTreeHolder.SetActive(!skillTreeHolder.activeSelf);
    }
}
