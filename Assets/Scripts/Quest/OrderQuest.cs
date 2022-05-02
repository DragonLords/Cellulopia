using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quest
{

    [CreateAssetMenu(fileName = "OrderQuest", menuName = "Cellulopia/OrderQuest", order = 0)]
    public class OrderQuest : ScriptableObject {
        public QuestTemplate[] orderQuest;    
    }
}