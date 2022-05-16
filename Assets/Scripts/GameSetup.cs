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
    public float PlayerMoveSpeed=8f;
    public float PlayerLife=100f;
    public float PlayerMaxLife=100f;
    public MapSize worldSize;
    public List<WorldData> worldData=new();
}

public class WorldData{
    public float x,y,z;
    public TileType tileType;
    public WorldData(float x,float y,float z,TileType tileType)
    {
        this.x=x;this.y=y;this.z=z;this.tileType=tileType;
    }
}

public enum TileType{Fill=1,Empty=0}
