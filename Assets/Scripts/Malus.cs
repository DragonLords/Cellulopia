using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ParticleSystemJobs;

public class Malus : MonoBehaviour
{
    [SerializeField] int malusFood=-15;
    [SerializeField] int lifeSub=-5;

    public int MalusFood { get => malusFood; set => malusFood = value; }
    public int LifeSub { get => lifeSub; set => lifeSub = value; }
    [SerializeField] ParticleSystem ps;
    string _keyParticleSystem;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async Task OnEated(){

        Instantiate(ps, transform.position,Quaternion.identity);
        var partGO=Addressables.InstantiateAsync(_keyParticleSystem,transform.position,Quaternion.identity).WaitForCompletion();
        ps=partGO.GetComponent<ParticleSystem>();
        await Task.Yield();
    }
}
