using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;

public class GameSetup 
{
    private bool playerMoved=false;
    public bool PlayerHasMoved{get=>playerMoved;set{playerMoved=value;GameManager.Instance.SaveData();}}

    public System.DateTime dateTime;
}
