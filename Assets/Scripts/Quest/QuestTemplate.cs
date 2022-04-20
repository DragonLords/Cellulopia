using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Quest
{
    [CreateAssetMenu(fileName = "QuestTemplate", menuName = "")]
    public class QuestTemplate : ScriptableObject
    {
        [SerializeField] internal QuestSate state=QuestSate.Incomplete;
        internal GameObject questButton;
        internal QuestEvent buttonSc;
        public string QuestName = "";
        public string QuestDescription = "";
        public GameObject objectToCollect;
        public int xpReward = 0;
        public int numberToCollect = 0;
        [SerializeField] private int numberCollected = 0;
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
        

        

        void ValidateNumber()
        {
            // Debug.Log("validation");
            buttonSc.progress.value=numberCollected;
            // Debug.LogFormat("dpiong great he {}",NumberCollected,numbert);
            if (numberToCollect <= numberCollected)
            {
                Debug.Log("quest ended");
                questButton.GetComponent<QuestEvent>().OnCompletion();
            }
        }

    }
}