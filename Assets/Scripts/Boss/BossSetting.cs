using UnityEngine;


[CreateAssetMenu(fileName = "BossSetting", menuName = "Cellulopia/BossSetting", order = 0)]
public class BossSetting : ScriptableObject {
    public int Life=50;
    public int MaxLife;
    public float speed=3.5f;
    public int MinionAtStart=5;
    public int MaxMinionCount=500;
    public float delaySpawnMinion=3f;
    public WaitForSeconds DelaySpawnMinion=new(3f);
    public float rangeMinion=7.5f;
    public float rangeDetectionPlayer=13f;
    public float rangeMinionAttack=20f;
}
