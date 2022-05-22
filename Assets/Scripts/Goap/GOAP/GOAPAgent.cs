using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using System.IO;
using System.Linq;

public class GOAPAgent : GOAPManager
{
    public GameObject goapActionHolder;
    public int damage = 1;
    public GameObject GreatestParent;
    private int _life = 2;
    public int maxLife = 2;
    public int Life { get => _life; set { _life = Mathf.Clamp(value, 0, maxLife); } }
    [TagSelector, SerializeField] internal string foodTag;
    [TagSelector, SerializeField] internal string enemyTag;
    [TagSelector, SerializeField] internal string playerTag;
    [SerializeField] internal LayerMask dangerLayer;
    [SerializeField] internal float radiusDanger = 10f;
    List<SubGoal> subs = new();
    public Vector3 offset = new(1, 1, 1);
    WaitForSeconds wsSocial = new(10);
    public ActionEnum actionEnum;
    public bool ohFuck = false;
    internal int increaseDanger = 2;
    public int foodSaturation = 30;
    public List<GameObject> groupMembers = new();
    public GoapContainer container;
    public int IncreaseOfAggresivityInDanger=2;
    public void BornFromDuplication(GOAPAgent daddy)
    {
        //well just clean the list before assigningit so that way if people of a group died well forget about them
        daddy.groupMembers.RemoveAll(item => item == null);
        groupMembers = new(daddy.groupMembers);
        container.groupType = daddy.container.groupType;
        GOAPGroupManager.AddToSpecificGroup(container.groupType, container);
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        if (container == null)
            container = GetComponentInParent<GoapContainer>();
        //if its the first one of the group then add itself to the group
        if (groupMembers.Count == 0)
            groupMembers.Add(transform.root.gameObject);

        StartCoroutine(StartDoingAction());
        StartCoroutine(DetectDangerClose());
    }

    IEnumerator DetectDangerClose()
    {
        do
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius: radiusDanger, dangerLayer);
            ArrayList alreadyVerified = new();
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.CompareTag(playerTag)&&!alreadyVerified.Contains(collider.transform.root))
                {
                    AggressivityLevel+=IncreaseOfAggresivityInDanger;
                    alreadyVerified.Add(collider.transform.root);
                }
                else if (collider.gameObject.CompareTag(enemyTag))
                {
                    var cont=collider.gameObject.GetComponentInParent<GoapContainer>();
                    if(!alreadyVerified.Contains(cont)){
                        AggressivityLevel+=IncreaseOfAggresivityInDanger;
                        alreadyVerified.Add(cont);
                    }
                }
            }
            yield return new WaitForSeconds(.5f);
        } while (alive);
    }


    internal IEnumerator StartDoingAction()
    {
        yield return StartCoroutine(NewSelectionAction());
        acts = goapActionHolder.GetComponentsInChildren<Action>();
        foreach (var item in acts)
        {
            actions.Add(item);
        }
        base.OnStart();
        for (int i = 0; i < actions.Count; i++)
        {
            SubGoal sg = new($"obj{i}", Mathf.RoundToInt(actions[i].ActionCost), true);
            goals.Add(sg, Mathf.RoundToInt(actions[i].ActionCost));
        }
        Debug.LogFormat("sg:{0}", goals.Count);
        yield return StartCoroutine(InitQueue());
        yield return StartCoroutine(StartAction());
    }

    private IEnumerator TestAction()
    {
        actionEnum = ActionEnum.Hungry;
        int selected = (int)actionEnum;
        foreach (var item in objectives)
        {
            item.SetActive(false);
        }
        yield return new WaitForSeconds(1.7f);
        objectives[selected].SetActive(true);
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


    IEnumerator NewSelectionAction()
    {
        bool couldAttack = Random.Range(0, 101) < AggressivityLevel;
        int selected = 0;
        if (isHungry)
        {
            if (couldAttack)
            {
                actionEnum = ActionEnum.Attack;
                selected = (int)actionEnum;
            }
            else
            {
                actionEnum = ActionEnum.Hungry;
                selected = (int)actionEnum;
            }
        }


        foreach (var item in objectives)
        {
            item.SetActive(false);
        }
        yield return new WaitForSeconds(1.7f);
        objectives[selected].SetActive(true);
    }

    bool canAttack = false;
    bool FinalValidationAction()
    {
        bool couldAttack = Random.Range(0, 101) < AggressivityLevel;
        bool canReprod = Hunger > HungerCostDuplication;
        canAttack = enemiesClose.Length > 0;

        if (actionEnum == ActionEnum.Attack && !canAttack)
            return false;
        else if (actionEnum == ActionEnum.Attack && canAttack)
            return true;
        if (canReprod & actionEnum == ActionEnum.Reprod)
        {
            Hunger -= HungerCostDuplication;
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
        // Debug.Log("selct");
        // foreach (var objective in objectives)
        //     objective.SetActive(false);
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
    void GetActions()
    {
        acts = goapActionHolder.GetComponentsInChildren<Action>();
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
        acts = goapActionHolder.GetComponentsInChildren<Action>();
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
        // Debug.LogFormat("{0},{1}", actions.Count, goals.Count);
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

    public void Duplicate()
    {
        Vector3 pos = new(Random.Range(transform.position.x - 5f, transform.position.x + 6f), transform.position.y, Random.Range(transform.position.z - 5f, transform.position.z + 6f));
        var born = Addressables.InstantiateAsync(AddressablePath.enemy, pos, Quaternion.identity).WaitForCompletion();
        groupMembers.Add(born.gameObject);

        born.GetComponentInChildren<GOAPAgent>().BornFromDuplication(this);
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
                    Gizmos.color = Color.white;
                }
                break;
        }
        Gizmos.DrawWireSphere(transform.position, 3);

        Gizmos.color = Color.green;
        // Gizmos.DrawRay(new(transform.position, transform.forward * 500));
        Gizmos.DrawWireSphere(transform.position, radiusFoodDetection);

        Gizmos.color = new(255, 0, 155);
        Gizmos.DrawWireSphere(transform.position, radiusDanger);
    }
    #endregion
}
public enum GroupType
{
    A,
    B,
    C
}

public enum ActionEnum { Hungry, Attack, Reprod, Idling }