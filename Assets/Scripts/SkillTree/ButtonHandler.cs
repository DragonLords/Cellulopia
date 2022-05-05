using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    public Button button;
    public Player.Skill.SkillTemplate skill;
    public SkillTreeHandler skillTreeHandler;

    public void Init(int value,Button button){
        text.text=skill.skillName;
        this.button=button;
        this.button.onClick.AddListener(OnClick);
    }

    private void Awake()
    {
        button=GetComponent<Button>();
        text=GetComponentInChildren<TextMeshProUGUI>();
        button.onClick.AddListener(OnClick);
        skill.button=button;
        if(skill.skillRequirement is not null)
            button.interactable=false;
        skillTreeHandler=GetComponentInParent<SkillTreeHandler>();
        text.text=skill.name;
    }

    public void OnClick(){
        Debug.LogFormat("Clicked on the button {0} with the text {1}",gameObject.name,text.text);
        Debug.LogFormat("name: {0} \n desc: {1} \n bonus: {2} \n effect: {3} \n value:{4}",skill.skillName,skill.skillDescription,skill.bonusType,skill.statEffect,skill.statEffectValue);
        //attribute at the player the skill
        if (GameManager.Instance.CanBuySkill(skill)) { 
            GameManager.Instance.AddStats.Invoke(skill);
        }
        //button.interactable=false;
        // if(skill.skillRequirement is not null)
        //skillTreeHandler.skillRequirementUnlock(skill);
    }
}
