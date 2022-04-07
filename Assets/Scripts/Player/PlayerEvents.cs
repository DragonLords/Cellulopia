using UnityEngine;
using UnityEngine.Events;

namespace Player.Events{
    // [System.Serializable]
    public class PlayerUpgradeSkill : UnityEngine.Events.UnityEvent<Skill.SkillTemplate>{}

    // [System.Serializable]
    public class PlayerUpgradeStats : UnityEngine.Events.UnityEvent<Skill.SkillTemplate>{}
}