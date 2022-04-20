using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR    
using UnityEditor;
#endif

namespace Quest

{
    public class TestQuestUI : MonoBehaviour
    {
        [SerializeField] internal GameObject button;
        [SerializeField] internal Transform holderGrid;
        // Start is called before the first frame update
        void Start()
        {

        }
    }


    #region editor
#if UNITY_EDITOR
    [CustomEditor(typeof(TestQuestUI))]
    public class TestQuestUIEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TestQuestUI questUI = (TestQuestUI)target;
            if (GUILayout.Button("gen"))
            {
                Instantiate(questUI.button, questUI.holderGrid);
            }
            base.OnInspectorGUI();
        }
    }
#endif
#endregion
}