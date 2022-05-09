using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using System.Linq;

public class GOAPTester : BossMelee
{
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
    public int increaseDanger = 10;
    public int foodSaturation = 30;

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        RangeDetectionFoodDanger *= radiusFoodDetection;
        SelectAction();
        #region test
        acts = GetComponentsInChildren<Action>();
        foreach (var a in acts)
        {
            actions.Add(a);
        }
        hunger = base.Hunger;
        tester = this;
        base.OnStart();
        if (Move)
            Init();
        #endregion
        if (spawner is null)
            spawner = FindObjectOfType<GoapSpawner>();
        StartCoroutine(ValidateDanger());
        StartCoroutine(ValidateTarget());
    }

    private void FixedUpdate()
    {
        // base.enemiesClose=Physics.OverlapSphere(transform.position,radiusFoodDetection,enemyLayer);
        // print(enemiesClose.Length);
        base.GetEnemiesClose();
    }

    void SelectAction()
    {
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
        Destroy(gameObject);
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
    int youTriggerMeNow = 0;

    internal void CollsionFood(Collision other)
    {
        if (other.gameObject.CompareTag(foodTag))
        {
            base.GiveFood(other.gameObject.GetComponent<Food>().FoodSaturation);
            Destroy(other.gameObject);
        }
    }

    internal void CollsionEnemy(Collision other)
    {
        if (other != null)
        {

            if (other.gameObject.CompareTag(enemyTag))
            {
                if (other.gameObject.GetComponent<GOAPTester>().isSocializing && canSocialize || this.isSocializing && canSocialize)
                {
                    if (isSocializing && canSocialize)
                    {
                        //Debug.Log("ohhh");
                        if (CapperEntities.CanSpawn())
                        {
                            currentAction.Duplicate(this);
                        }
                        else
                        {
                            //Debug.Log("meh");
                            currentAction.Achieved = true;
                            youTriggerMeNow++;
                            if (youTriggerMeNow >= 10)
                            {
                                Destroy(gameObject);
                            }
                        }
                        StartCoroutine(CoolDownSocializing());
                    }
                }
                else if (this.isAttacking)
                {
                    base.GiveFood(other.gameObject.GetComponent<GOAPTester>().foodSaturation);
                    Destroy(other.gameObject);
                    //Debug.Log("EXPLOSIONS!?!");
                }
            }
        }
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
        }
        Gizmos.DrawWireSphere(transform.position, 3);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(new(transform.position, transform.forward * 500));
    }
}


public enum ActionEnum { Hungry, Attack, Reprod }