using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Quest
{
    public class QuestHolder : MonoBehaviour
    {
        Player.Rework.Player player;
        string templateQuestKey="QuestButton";
        [SerializeField] Transform holderQuest;
        [SerializeField] Quest.QuestTemplate[] quests;
        [SerializeField,Range(1,5)] int maxQuestActive=3;
        [SerializeField] List<Quest.QuestTemplate> questEnded=new();
        [SerializeField] List<QuestTemplate> questActive=new();
        // [SerializeField] List<TDictionnary> questStatus=new();
        Dictionary<QuestTemplate,QuestSate> QuestStatus=new();
        [SerializeField] List<QuestTemplate> questPending=new();
        public OrderQuest questOrder;
        public Queue<QuestTemplate> questsOrdered=new();
        public List<QuestTemplate> queueQuest=new();
        // Start is called before the first frame update
        void Start()
        {
            player=FindObjectOfType<Player.Rework.Player>();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged+=BackUpAllQuest;
            #endif

            foreach(var quest in questOrder.orderQuest){
                var q=ScriptableObject.CreateInstance<QuestTemplate>();
                q = quest;
                questsOrdered.Enqueue(q);
            }

            queueQuest = new(questsOrdered);
            foreach(var q in queueQuest)
            {
                q.numberCollected = 0;
            }

            // foreach (var quest in quests)
            // {
            //     // questStatus.Add(new(quest.state,quest));
            //     QuestStatus.Add(quest,quest.state);
            //     if(maxQuestActive>questActive.Count)
            //         OnQuestActive(quest);
            //     else 
            //         questPending.Add(quest);
            // }
            
            // StringBuilder sb=new();
            // foreach (var kvp in QuestStatus)
            // {
            //     sb.Append(kvp.Key);
            //     sb.Append(' ');
            //     sb.Append(kvp.Value);
            //     sb.AppendLine();
            // }
            // Debug.Log(sb.ToString());
            SpawnQuest();
        }

        void SpawnQuest(){
            var quest=NextQuest();
            questActive.Clear();
            questActive.Add(quest);
            // foreach(var quest in questActive){
                Addressables.InstantiateAsync(templateQuestKey,holderQuest).WaitForCompletion()
                .GetComponent<QuestEvent>().Init(quest,this);
            // }
            //give quest here
            // FindObjectOfType<Player.Rework.Player>().SetQuest(questActive);
            //FindObjectOfType<Player.Player>().SetQuest(questActive);
            player.SetActiveQuest(quest);
        }

        internal void ReceiveNewQuest(){
            SpawnQuest();
        }

        internal QuestTemplate NextQuest()=>questsOrdered.Dequeue();
        internal bool QuestLeft()=>questsOrdered.Count>0;

        internal void SpawnSelectedQuest(QuestTemplate quest){
            Addressables.InstantiateAsync(templateQuestKey,holderQuest).WaitForCompletion().GetComponent<QuestEvent>().Init(quest,this);
        }

        internal void OnQuestActive(QuestTemplate questActive){
            this.questActive.Add(questActive);
            QuestStatus[questActive]=QuestSate.Progress;
        }

        internal void ActivateNewQuest(){
            if(questPending.Count==0)
                return;
            OnQuestActive(questPending.First());
            SpawnSelectedQuest(questPending.First());
            questPending.Remove(questPending.First());
        }

        internal void OnQuestEnded(QuestTemplate questEnded)
        {
            QuestStatus[questEnded]=QuestSate.Finish;
            this.questActive.Remove(questEnded);
            this.questEnded.Add(questEnded);
            this.ActivateNewQuest();
        }

        #if UNITY_EDITOR
        internal void BackUpAllQuest(UnityEditor.PlayModeStateChange playMode){
            Debug.Log(playMode);
            foreach (var item in quests)
            {
                item.NumberCollected=0;
            }
        }
        #endif
    }

    [System.Serializable]
    internal struct TDictionnary
    {
        public QuestSate state;
        public QuestTemplate quest;
        public void TurnInQuest(QuestTemplate quest)
        {
            state=QuestSate.Finish;
        }

        public TDictionnary(QuestSate state, QuestTemplate quest)
        {
            this.state = state;
            this.quest = quest;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    } 

    internal enum QuestSate{Finish,Progress,Incomplete}
}