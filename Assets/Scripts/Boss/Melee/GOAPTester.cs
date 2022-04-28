using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Linq;

public class GOAPTester : BossMelee
{
    [TagSelector, SerializeField] string foodTag;
    [TagSelector,SerializeField] string enemyTag;
    int hunger;
    List<SubGoal> subs=new();
    // Start is called before the first frame update
    void Start()
    {
        // var newObj=objectives[Random.Range(0,objectives.Length)];
        var newObj=objectives[1];
        newObj.SetActive(true);
        Debug.Log(newObj.name);
        #region test
        acts=GetComponentsInChildren<Action>();
        foreach(var a in acts){
            actions.Add(a);
        }
        // acts = GetComponentsInChildren<Action>();
        // foreach (var a in acts)
        // {
        //     actions.Add(a);
        // }
        // for (int i = 0; i < acts.Length; i++)
        // {
        //     SubGoal s1=new("isWaiting",1,true);
        //     goals.Add(s1,3);
        // }
        // // Redo();
        hunger = base.Hunger;
        tester=this;
        base.OnStart();
        
        // // SetGoal();
        // routineLoopAction=StartCoroutine(FinalDetection());
        #endregion
    }

    void Init(){
        if(routineLoopAction is not null)
            StopCoroutine(routineLoopAction);
        actions.Clear();
        goals.Clear();

        var childs=GetComponentsInChildren<Action>(true);
        foreach (var item in childs)
        {
            item.gameObject.SetActive(false);
        }
        var newObj=objectives[1];
        // var newObj=objectives[Random.Range(0,objectives.Length)];
        newObj.SetActive(true);
        Debug.Log(newObj.name);
        acts=GetComponentsInChildren<Action>();
        foreach(var a in acts){
            actions.Add(a);
        }
        for (int i = 0; i < actions.Count; i++)
        {
            SubGoal sg=new($"obj{i}",Mathf.RoundToInt(actions[i].ActionCost),true);
            goals.Add(sg,Mathf.RoundToInt(actions[i].ActionCost));
        }
        StartCoroutine(InitQueue());
        // routineLoopAction = StartCoroutine(FinalDetection());
    }

    private void Update()
    {
        if(Keyboard.current.pKey.wasPressedThisFrame)
            Init();
    }

    internal void OnSetGoal()
    {
        actions.Clear();
        goals.Clear();
        Debug.Log(goals.Count);
        acts = GetComponentsInChildren<Action>();
        foreach (var a in acts)
        {
            actions.Add(a);
        }
        for (int i = 0; i < actions.Count; i++)
        {
            SubGoal s1 = new($"bob{i}", 1, true);
            goals.Add(s1, 5);
            subs.Add(s1);
        }
    }


    IEnumerator DetectGoalEmpty()
    {

        yield return new WaitWhile(() => goals.Count > 0);
        // Redo();
        Debug.Log("Redo");
    }

    internal IEnumerator SelectNewObjective(){
        foreach(var item in objectives){
            item.SetActive(false);
        }
        var newObj=objectives[Random.Range(0,objectives.Length)];
        newObj.SetActive(true);
        Debug.LogFormat("new action is:{0}",newObj.name);
        yield return null;
    }

    internal IEnumerator Redo()
    {
        if(routineLoopAction is not null)
            StopCoroutine(routineLoopAction);
        OnSetGoal();
        HardReset();
        yield return StartCoroutine(InitQueue());
        if(actionQueue is null){
            do
            {
                Debug.Log("retry");
                yield return StartCoroutine(InitQueue());
            } while (actionQueue is null);
        }
        routineLoopAction=StartCoroutine(FinalDetection());
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(foodTag))
        {
            base.GiveFood(other.gameObject.GetComponent<Food>().FoodSaturation);
            Destroy(other.gameObject);
        }
        if(other.gameObject.CompareTag(enemyTag)){
            Destroy(other.gameObject);
            Debug.Log("EXPLOSIONS!?!");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color=Color.cyan;
        Gizmos.DrawWireSphere(transform.position,radiusFoodDetection);
    }
}
