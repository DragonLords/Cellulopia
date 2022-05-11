using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Rework.Events
{
    public class EventsPlayer
    {
        public class PlayerUpgradeSkill : UnityEngine.Events.UnityEvent<Skill.SkillTemplate> { }

        // [System.Serializable]
        public class PlayerUpgradeStats : UnityEngine.Events.UnityEvent<Skill.SkillTemplate> { }
        public class PlayerGiveFood : UnityEngine.Events.UnityEvent<float> { }
        public class PlayerTakeDamage : UnityEngine.Events.UnityEvent<int> { }
        public class PlayerGiveEXP : UnityEngine.Events.UnityEvent<int> { }
        public class PlayerRemoveQuest : UnityEngine.Events.UnityEvent<Quest.QuestTemplate> { }
        public class PlayerShowControl : UnityEngine.Events.UnityEvent { }
    }
}
