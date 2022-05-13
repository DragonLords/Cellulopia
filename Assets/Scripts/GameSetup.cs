using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;

public class GameSetup 
{
    private bool playerMoved=false;
    public bool PlayerHasMoved{get;set;}

    public System.DateTime dateTime;
    public int PlayerLevel=0;
    public int PlayerSkillPoint=0;
    public int PlayerXP=0;
    public int NextLevelXP=100;
    public int PlayerDamageValue=1;
    public float PlayerDelayAttack=2f;
    public float PlayerMoveSpeed=5f;
    public float PlayerLife=100f;
    public float PlayerMaxLife=100f;
}
