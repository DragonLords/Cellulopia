using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Enemy
{
    public class EnemyFOV : MonoBehaviour
    {
        public float viewRadius;
        [Range(0, 360)] public float viewAngle = 15f;
        public LayerMask targetLayer;
        public LayerMask obstacleLayer;
        public bool seeTarget = false;
        public List<Transform> visibleTargets = new();
        public Coroutine routineFindTarget;
        public bool inSearch = false;
        public Enemy enemy;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            enemy = GetComponent<Enemy>();
            viewRadius = enemy.radiusDetectionDanger;
            StartSearching();
        }

        public Vector3 DirFromAngle(float angleInDegree, bool angleIsGlobal)
        {
            if (!angleIsGlobal)
            {
                angleInDegree += transform.eulerAngles.y;
            }
            return new Vector3(Mathf.Sin(angleInDegree * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegree * Mathf.Deg2Rad));
        }

        public void StartSearching()
        {
            inSearch = true;
            routineFindTarget = StartCoroutine(FindTarget());
        }

        public void StopSearching()
        {
            inSearch = false;
            if (routineFindTarget != null)
                StopCoroutine(routineFindTarget);
        }

        IEnumerator FindTarget()
        {
            do
            {
                FindVisibleTargets();
                yield return new WaitForSeconds(.2f);
            } while (inSearch);
        }

        void FindVisibleTargets()
        {
            visibleTargets.Clear();
            Collider[] targetInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, enemy.dangerLayers);
            for (int i = 0; i < targetInViewRadius.Length; ++i)
            {
                Transform target = targetInViewRadius[i].transform;
                if (target.root != transform.root)
                {
                    Vector3 dirToTarget = (target.position - transform.position).normalized;
                    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                    {
                        visibleTargets.Add(target);
                        seeTarget = true;
                    }
                }
            }
        }
    }

    #region EditorFOV
#if UNITY_EDITOR
    [CustomEditor(typeof(EnemyFOV))]
    public class ReworkEnemyFOVEditor : Editor
    {
        private void OnSceneGUI()
        {
            EnemyFOV enemyFOV = (EnemyFOV)target;
            Handles.color = Color.white;
            Handles.DrawWireArc(enemyFOV.transform.position, Vector3.up, Vector3.forward, 360, enemyFOV.viewRadius);
            Vector3 viewAngleA = enemyFOV.DirFromAngle(-enemyFOV.viewAngle / 2, false);
            Vector3 viewAngleB = enemyFOV.DirFromAngle(enemyFOV.viewAngle / 2, false);
            Handles.DrawLine(enemyFOV.transform.position, enemyFOV.transform.position + viewAngleA * enemyFOV.viewRadius);
            Handles.DrawLine(enemyFOV.transform.position, enemyFOV.transform.position + viewAngleB * enemyFOV.viewRadius);

            Handles.color = Color.red;
            foreach (var item in enemyFOV.visibleTargets)
            {
                Handles.DrawLine(enemyFOV.transform.position, item.position);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
    #endregion
}