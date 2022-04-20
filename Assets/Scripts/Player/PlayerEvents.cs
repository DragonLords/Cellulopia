using UnityEngine;
using UnityEngine.Events;

namespace Player.Events{
    // [System.Serializable]
    public class PlayerUpgradeSkill : UnityEngine.Events.UnityEvent<Skill.SkillTemplate>{}

    // [System.Serializable]
    public class PlayerUpgradeStats : UnityEngine.Events.UnityEvent<Skill.SkillTemplate>{}
    public class PlayerGiveFood : UnityEngine.Events.UnityEvent<float>{}
    public class PlayerTakeDamage: UnityEngine.Events.UnityEvent<int>{}
    public class PlayerGiveEXP:UnityEngine.Events.UnityEvent<int>{}
    public class PlayerRemoveQuest:UnityEngine.Events.UnityEvent<Quest.QuestTemplate>{}
}