using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] textNewStats;
    [SerializeField] TextMeshProUGUI[] textOldStats;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TextMeshProUGUI textDesc;
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
        button = GetComponent<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        button.onClick.AddListener(OnClick);
        skill.button = button;
        if (skill.skillRequirement is not null)
            button.interactable = false;
        skillTreeHandler = GetComponentInParent<SkillTreeHandler>();
        text.text = skill.name;
        textDesc.text = skill.skillDescription;
        ShowStatSkill();
    }

    private void ShowStatSkill()
    {
        List<string> strings = new(GameManager.Instance.GetPlayerStat(skill.statEffect));
        for (int i = 0; i < strings.Count; i++)
        {
            textOldStats[i].text = strings[i];
            int newStat = 0;
            var r = Regex.Match(strings[i], @"\d+").Value;
            if (System.Int32.TryParse(r, out newStat))
            {
                newStat = newStat + skill.statEffectValue;
                textNewStats[i].text = $"{newStat}";
            }
        }
    }

    public void OnClick(){
        // Debug.LogFormat("Clicked on the button {0} with the text {1}",gameObject.name,text.text);
        // Debug.LogFormat("name: {0} \n desc: {1} \n bonus: {2} \n effect: {3} \n value:{4}",skill.skillName,skill.skillDescription,skill.bonusType,skill.statEffect,skill.statEffectValue);
        //attribute at the player the skill
        if (GameManager.Instance.CanBuySkill(skill)) { 
            GameManager.Instance.AddStats.Invoke(skill);
        }
        ShowStatSkill();
        //button.interactable=false;
        // if(skill.skillRequirement is not null)
        //skillTreeHandler.skillRequirementUnlock(skill);
    }
}
