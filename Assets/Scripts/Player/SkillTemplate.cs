using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Player.Skill{

[CreateAssetMenu(fileName = "SkillTemplate", menuName = "Cellulopia/SkillTemplate", order = 0)]
public class SkillTemplate : ScriptableObject {
    public int grade=1;
    public SkillTemplate skillRequirement;
    internal UnityEngine.UI.Button button;
    [HideInInspector] public string skillName;
    [HideInInspector] public int skillOrder;
    [HideInInspector] public int skillCost;
    [HideInInspector] public string skillDescription;
    public BonusType bonusType;
    [HideInInspector] public StatEffect statEffect;
    [HideInInspector] public float statEffectValue;
    [HideInInspector] public SkillEffect skillEffect;

    public enum BonusType{stats,skill}
    public enum SkillEffect{grappling,bob,dummy,another}
    public enum StatEffect{vitesse,not,here,hide};
}

#region editor
#if UNITY_EDITOR
[CustomEditor(typeof(SkillTemplate))]
public class SkillTemplateEditor : Editor{
    SerializedProperty prop;
    private void OnEnable()
    {
        prop=serializedObject.FindProperty("skillCost");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SkillTemplate skillTemplate=(SkillTemplate)target;
        skillTemplate.skillName=EditorGUILayout.TextField(skillTemplate.skillName) as string;
        skillTemplate.skillCost=EditorGUILayout.IntField(nameof(skillTemplate.skillCost),skillTemplate.skillCost);
        skillTemplate.skillDescription=EditorGUILayout.TextArea(skillTemplate.skillDescription) as string;
        switch(skillTemplate.bonusType){
            case SkillTemplate.BonusType.stats:{
                skillTemplate.statEffect=(SkillTemplate.StatEffect)EditorGUILayout.EnumPopup("Stats effect",skillTemplate.statEffect);
                // EditorGUILayout.PropertyField(serializedObject.FindProperty("Stat Effect"));
                // EditorGUILayout.PropertyField(prop);
                skillTemplate.statEffectValue=EditorGUILayout.FloatField(nameof(skillTemplate.statEffectValue),skillTemplate.statEffectValue);
            } break;
            case SkillTemplate.BonusType.skill:{
                EditorGUILayout.EnumFlagsField(skillTemplate.skillEffect);
            } break;
        }
        base.OnInspectorGUI();
    }
}
#endif
#endregion
}