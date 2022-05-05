using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Quest
{

    public class QuestEvent : MonoBehaviour
    {
        public QuestHolder questHolder;
        public QuestTemplate quest;
        Button button;
        [SerializeField] internal Slider progress;

        public void Init(QuestTemplate quest,QuestHolder questHolder){
            this.quest=quest;
            this.questHolder=questHolder;
            GiveValue();
        }

        private void GiveValue()
        {
            // questHolder=GetComponentInParent<QuestHolder>();
            button=GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            var text=button.GetComponentInChildren<TextMeshProUGUI>();
            button.GetComponentInChildren<TextMeshProUGUI>().text=quest.QuestName;
            quest.questButton=gameObject;
            quest.buttonSc=this;
            quest.NumberCollected=0;
            progress.maxValue=quest.numberToCollect;
            progress.value=quest.NumberCollected;
            switch(quest.state){
                case QuestSate.Finish:{
                    button.GetComponent<Image>().color=Color.magenta;
                }break;
                case QuestSate.Progress:{
                    button.GetComponent<Image>().color=new(150,0,150);
                }break;
                case QuestSate.Incomplete:{
                    button.GetComponent<Image>().color=Color.cyan;
                }break;
            }
            quest.text=text;

        }

        public void OnClick(){
            
        }

        internal void OnCompletion()
        {
            GameManager.Instance.PlayerGiveEXP.Invoke(quest.xpReward);
            GameManager.Instance.PlayerRemoveQuest.Invoke(quest);
            questHolder.OnQuestEnded(quest);
            Destroy(gameObject);
        }
    }

}