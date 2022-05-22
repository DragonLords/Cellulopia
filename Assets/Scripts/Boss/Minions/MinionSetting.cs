using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Classe qui sert a definir les parametres des sbires du boss
/// </summary>
[CreateAssetMenu(fileName = "MinionSetting", menuName = "Cellulopia/MinionSetting", order = 0)]
public class MinionSetting : ScriptableObject {
    public int Damage=2;
    public int life=3;
    public float speed=8f;
}