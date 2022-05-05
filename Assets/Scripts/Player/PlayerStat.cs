using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerStat", menuName = "Player")]
public class PlayerStat : ScriptableObject
{
    public int DamageValue=1;
    public float MoveSpeed=1f;
    public int SkillPoint=0;
    public int Level=0;
    public float Life=100f;
    public float MaxLife=100f;
}