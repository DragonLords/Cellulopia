using UnityEngine;

/// <summary>
/// Classe qui sert a linteraction du portail
/// </summary>
public class Portal : MonoBehaviour
{
    public void TriggerBossFight()
    {
        GameManager.Instance.SpawnNewBoss();
        Destroy(gameObject);
    }
}
