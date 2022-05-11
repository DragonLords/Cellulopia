using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerStat", menuName = "Player")]
public class PlayerStat : ScriptableObject
{
    public int DamageValue=1;
    [SerializeField] float delayReduction=2f;
    public float DelayAttack{get=>delayReduction;set{delayReduction=Mathf.Clamp(value,0,10f);}}
    public float MoveSpeed=1f;
    public int SkillPoint=0;
    public int Level=0;
    public float Life=100f;
    public float MaxLife=100f;

    public void Stack(GameSetup setup){
        Level=setup.PlayerLevel;
        SkillPoint=setup.PlayerSkillPoint;
        DamageValue=setup.PlayerDamageValue;
        DelayAttack=setup.PlayerDelayAttack;
        MoveSpeed=setup.PlayerMoveSpeed;
        Life=setup.PlayerLife;
        MaxLife=setup.PlayerMaxLife;
    }

    public void UnStack(GameSetup setup){
        setup.PlayerLevel=Level;
        setup.PlayerSkillPoint=SkillPoint;
        setup.PlayerDamageValue=DamageValue;
        setup.PlayerDelayAttack=DelayAttack;
        setup.PlayerMoveSpeed=MoveSpeed;
        setup.PlayerLife=Life;
        setup.PlayerMaxLife=MaxLife;
    }
}