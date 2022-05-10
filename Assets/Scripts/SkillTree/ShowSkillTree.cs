using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSkillTree : MonoBehaviour
{
    public Button button;
    [SerializeField] GameObject skillTreeHolder;
    private void Awake()
    {
        button=GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        if(skillTreeHolder is null)
            skillTreeHolder=FindObjectOfType<SkillTreeHandler>().gameObject;
    }

    public void OnClick(){
        skillTreeHolder.SetActive(!skillTreeHolder.activeSelf);
        // Debug.Log("click");
        GameManager.Instance.PauseGame();
    }
}
