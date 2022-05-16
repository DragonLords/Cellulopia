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
    [SerializeField] Player.Rework.Player player;
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
            float newStat = 0;
            var r = Regex.Match(strings[i], @"\d+").Value;
            if (float.TryParse(r, out newStat))
            {
                Debug.Log(newStat);
                if(skill.statEffect==Player.Skill.SkillTemplate.StatEffect.Attack&&i==1){
                    newStat=player.playerStat.DelayAttack+skill.AttackDelayReduction;
                    //on clamp le nombre pourne pas afficher un nombre en negatif et la raison du 10 est toute simple assez elever pour ne pas empecher de bloquer le nombre max sans pour autant etre abusurdement elever
                    newStat=Mathf.Clamp(newStat,0,10f);
                }else{
                    newStat = newStat + skill.statEffectValue;
                }
                string newStatStringify;
                bool isInt=newStat==(int)newStat;
                //verifie sil y a des decimal
                // if(!(newStat%1==0)){
                if(!(isInt)){
                    //sil y en a pas alors on larrondie en int (evite les nombre a virgule .00f)
                    newStatStringify=newStat.ToString("n2");
                    textNewStats[i].text = $"{newStatStringify}";
                }else{
                    //sinon on garde son nombre a virgule avec 2 decimal (nb.01f)

                    textNewStats[i].text = $"{newStat}";
                }
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
