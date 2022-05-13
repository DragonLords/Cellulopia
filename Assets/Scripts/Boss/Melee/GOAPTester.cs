using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using System.IO;
using System.Linq;

public class GOAPTester : GOAPManager
{
    public GameObject goapActionHolder;
    public int damage=1;
    public GameObject GreatestParent;
    private int _life = 2;
    public int maxLife = 2;
    public int Life { get => _life; set { _life = Mathf.Clamp(value, 0, maxLife); } }
    [SerializeField] bool Move = true;
    [TagSelector, SerializeField] internal string foodTag;
    [TagSelector, SerializeField] internal string enemyTag;
    public int hunger;
    List<SubGoal> subs = new();
    public Vector3 offset = new(1, 1, 1);
    WaitForSeconds wsSocial = new(10);
    public ActionEnum actionEnum;
    public bool ohFuck = false;
    public float RangeDetectionFoodDanger = 2;
    internal int increaseDanger = 2;
    public int foodSaturation = 30;
    internal string goapKey="Rework_EnemyHolder_3D";
    public List<GameObject> groupMembers=new();

    public void BornFromDuplication(GOAPTester daddy){
        //well just clean the list before assigningit so that way if people of a group died well forget about them
        daddy.groupMembers.RemoveAll(item=>item==null);
        groupMembers=new(daddy.groupMembers);
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        BornFromDuplication(this);
        //if its the first one of the group then add itself to the group
        if(groupMembers.Count==0)
            groupMembers.Add(transform.root.gameObject);
        RangeDetectionFoodDanger *= radiusFoodDetection;
        GetActions();
        // SelectAction();
        foreach(var act in acts)
            actions.Add(act);
        hunger=base.Hunger;
        tester=this;
        base.OnStart();
        // Init();
        objectives[(int)actionEnum].SetActive(true);
        NewSelectionAction();





        #region test
        // acts = GetComponentsInChildren<Action>(true);
        // foreach (var a in acts)
        // {
        //     actions.Add(a);
        // }
        // hunger = base.Hunger;
        // tester = this;
        // base.OnStart();
        // if (Move)
        //     Init();
        // if (spawner is null)
        //     spawner = FindObjectOfType<GoapSpawner>();
        // StartCoroutine(ValidateDanger());
        // StartCoroutine(ValidateTarget());
        #endregion
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    // void Update()
    // {
    //     if(Keyboard.current.f2Key.wasPressedThisFrame)
    //         NewSelectionAction();
    //     // if(Keyboard.current.anyKey.wasPressedThisFrame)
    //     // {
    //     //     actionEnum=ActionEnum.Reprod;
    //     //     currentAction=objectives[(int)actionEnum].GetComponent<Action>();
    //     //     Duplicate();
    //     // }
    // }

    private void FixedUpdate()
    {
        // base.enemiesClose=Physics.OverlapSphere(transform.position,radiusFoodDetection,enemyLayer);
        // print(enemiesClose.Length);
        base.GetEnemiesClose();
    }


    void NewSelectionAction(){
        foreach(var objective in objectives)
            objective.SetActive(false);
        int selected = Random.Range(0, objectives.Length);
        actionEnum=(ActionEnum)selected;
        //if we cant have an action then we do a recursive call to get one
        if(!FinalValidationAction())
            NewSelectionAction();
    }

    bool canAttack=false;
    bool FinalValidationAction(){
        bool couldAttack=Random.Range(0, 101) < AggressivityLevel;
        bool canReprod=Hunger>HungerCostDuplication;
        canAttack=enemiesClose.Length>0;

        if(actionEnum==ActionEnum.Attack&&!canAttack)
            return false;
        else if(actionEnum==ActionEnum.Attack&&canAttack)
            return true;        
        if(canReprod&actionEnum==ActionEnum.Reprod){
            Hunger-=HungerCostDuplication;
            Duplicate();
            return true;
        }


        return true;
    }
    #region stuff
    //TODO: this thing will need a full rework 
    //make the ultimate fallback is to be in idling
    void SelectAction()
    {
        foreach(var objective in objectives)
            objective.SetActive(false);
        bool couldAttack = Random.Range(0, 100) < AggressivityLevel;
        int selected = Random.Range(0, objectives.Length);
        ValidateAction(selected);
        if (isHungry)
        {
            if (couldAttack)
            {
                actionEnum = ActionEnum.Attack;
            }
            else
            {

                actionEnum = ActionEnum.Hungry;
            }
            ForceAction();
        }
        else
        {
            if (couldAttack)
            {
                actionEnum = ActionEnum.Attack;
                selected = (int)actionEnum;
            }
            if (!ValidateAction(selected))
            {
                SelectAction();
            }

            if (!ohFuck)
            {
                var newObj = objectives[selected];
                newObj.SetActive(true);
            }

        }
        ohFuck = false;
        if (selected == (int)ActionEnum.Attack)
        {
            AggressivityLevel += increaseDanger;
        }

        // actionEnum=ActionEnum.Attack;
    }
    void GetActions(){
        acts=goapActionHolder.GetComponentsInChildren<Action>(true);
        // Debug.Log(acts.Length);
    }

    void Init()
    {
        if (routineLoopAction is not null)
            StopCoroutine(routineLoopAction);
        actions.Clear();
        goals.Clear();

        SelectAction();

        // Debug.Log(newObj.name);
        acts = GetComponentsInChildren<Action>();
        foreach (var a in acts)
        {
            actions.Add(a);
        }
        for (int i = 0; i < actions.Count; i++)
        {
            SubGoal sg = new($"obj{i}", Mathf.RoundToInt(actions[i].ActionCost), true);
            goals.Add(sg, Mathf.RoundToInt(actions[i].ActionCost));
        }
        StartCoroutine(InitQueue());
        routineLoopAction = StartCoroutine(FinalDetection());
    }

    internal void TakeDamage(int value)
    {
        Life -= value;
        if (Life == 0)
        {
            StartCoroutine(Death());
        }
    }

    internal void OnSetGoal()
    {
        actions.Clear();
        goals.Clear();
        // Debug.Log(goals.Count);
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

    bool ValidateAction(int actionSelected)
    {
        actionEnum = (ActionEnum)actionSelected;
        if (actionEnum == ActionEnum.Reprod)
        {
            if (canSocialize)
            {
                return true;
            }
        }
        else if (actionEnum == ActionEnum.Hungry)
        {
            return true;
        }
        else if (actionEnum == ActionEnum.Attack)
        {
            return true;
        }
        return false;

    }

    void ForceAction()
    {
        var newObj = objectives[(int)actionEnum];
        newObj.SetActive(true);
        //Debug.LogFormat("<color=red>oh fuck oh shit im gonna fucking die!!!</color><color=olive>{0}</color>",actionEnum);
        ohFuck = true;
    }

    IEnumerator ValidateDanger()
    {
        do
        {
            if (isHungry)
                SelectAction();
            yield return null;
        } while (alive);
    }

    IEnumerator ValidateTarget()
    {
        do
        {
            if (currentAction is not null)
            {
                if (currentAction.target == null)
                {
                    currentAction.Achieved = true;
                    //Debug.Log("target is missing");
                }
            }
            yield return null;
        } while (alive);
    }

    internal IEnumerator Death()
    {
        base.alive = false;
        yield return null;
        Destroy(GreatestParent);
    }



    IEnumerator DetectGoalEmpty()
    {

        yield return new WaitWhile(() => goals.Count > 0);
        // Redo();
        // Debug.Log("Redo");
    }

    internal IEnumerator SelectNewObjective()
    {
        foreach (var item in objectives)
        {
            item.SetActive(false);
        }
        SelectAction();
        // Debug.LogFormat("new action is:{0}",newObj.name);
        yield return null;
    }

    internal IEnumerator Redo()
    {
        if (routineLoopAction is not null)
            StopCoroutine(routineLoopAction);
        OnSetGoal();
        HardReset();
        yield return StartCoroutine(InitQueue());
        if (actionQueue is null)
        {
            do
            {
                // Debug.Log("retry");
                yield return StartCoroutine(InitQueue());
            } while (actionQueue is null);
        }
        routineLoopAction = StartCoroutine(FinalDetection());
    }

    internal IEnumerator CoolDownSocializing()
    {
        canSocialize = false;
        isSocializing = false;
        yield return wsSocial;
        canSocialize = true;
    }

    internal void CollisionFood(Collision other)
    {
        if (other.gameObject.CompareTag(foodTag))
        {
            base.GiveFood(other.gameObject.GetComponent<Food>().FoodSaturation);
            Destroy(other.gameObject);
        }
    }

    public void Duplicate(){
        Vector3 pos=new(Random.Range(transform.position.x-5f,transform.position.x+6f),transform.position.y,Random.Range(transform.position.z-5f,transform.position.z+6f));
        var born=Addressables.InstantiateAsync(goapKey,pos,Quaternion.identity).WaitForCompletion();
        groupMembers.Add(born.transform.root.gameObject);
        born.GetComponentInChildren<GOAPTester>().BornFromDuplication(this);
    }

    private void OnDrawGizmos()
    {

        switch (actionEnum)
        {
            case ActionEnum.Hungry:
                {
                    Gizmos.color = Color.yellow;
                }
                break;
            case ActionEnum.Attack:
                {
                    Gizmos.color = Color.red;
                }
                break;
            case ActionEnum.Reprod:
                {
                    Gizmos.color = Color.cyan;
                }
                break;
            case ActionEnum.Idling:
                {
                    Gizmos.color=Color.white;
                }
                break;
        }
        Gizmos.DrawWireSphere(transform.position, 3);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(new(transform.position, transform.forward * 500));
    }
    #endregion
}


public enum ActionEnum { Hungry, Attack, Reprod,Idling }