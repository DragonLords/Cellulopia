using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Quest
{
    [CreateAssetMenu(fileName = "QuestTemplate", menuName = "")]
    public class QuestTemplate : ScriptableObject
    {
        public TMPro.TextMeshProUGUI text;
        [SerializeField] internal QuestSate state = QuestSate.Incomplete;
        internal GameObject questButton;
        internal QuestEvent buttonSc;
        public string QuestName = "";
        public string nameRun="";
        public string QuestDescription = "";
        public GameObject objectToCollect;
        public int xpReward = 0;
        public int numberToCollect = 0;
        [SerializeField] internal int numberCollected = 0;
        [SerializeField] internal Sprite _imageObjectToCollect;
        [property: SerializeField]
        public int NumberCollected
        {
            get => numberCollected;
            set
            {
                if (value != numberCollected)
                {
                    // Debug.Log("a");
                    numberCollected = value;
                    ValidateNumber();
                    // Debug.LogFormat("number collected:{0} {1}",numberCollected,value);
                }
            }
        }

        int num=0;
        string q=string.Empty;
        public void Init(){
            var r=Regex.Match(nameRun,@"\d+").Value;
            num=System.Int32.Parse(r);
            num=numberToCollect-numberCollected;
            nameRun=$"";
        }

        public bool IsSkillQuest = false;
        public bool PortalQuest = false;


        void ValidateNumber()
        {
            if (IsSkillQuest)
            {
                questButton.GetComponent<QuestEvent>().OnCompletion();
            }
            else
            {
                // Debug.Log("validation");
                buttonSc.progress.value = numberCollected;
                nameRun=QuestName;
                var r=Regex.Match(nameRun,@"\d+").Value;
                int number=0;
                if(System.Int32.TryParse(r, out number)){
                    number=numberToCollect-numberCollected;
                    // Debug.LogFormat("{0}\n{1}",r,number);
                    r=number.ToString();
                    // Debug.LogFormat("{0}\n{1}",r,number);
                    string test=Regex.Replace(nameRun,@"\d",number.ToString());
                    // Debug.Log(test);
                    nameRun=test;
                    if(number>1){
                        nameRun=test.Remove(test.Length-1,1);
                    }
                    text.text=nameRun;
                }
                // Debug.LogFormat("dpiong great he {}",NumberCollected,numbert);
                if (numberToCollect <= numberCollected)
                {
                    // Debug.Log("quest ended");
                    questButton.GetComponent<QuestEvent>().OnCompletion();
                }
            }
        }

    }
}