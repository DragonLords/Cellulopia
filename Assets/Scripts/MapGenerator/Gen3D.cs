using UnityEngine;
#if UNITY_EDITOR    
using UnityEditor;
#endif

namespace Generator
{

    public class Gen3D : MonoBehaviour
    {
        public Vector2Int emptyTiles = new();
        public void Generate()
        {

        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(Gen3D))]
    class Gen3DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Gen3D gen3D = (Gen3D)target;
            if (GUILayout.Button("Generate"))
            {
                gen3D.Generate();
            }
            base.OnInspectorGUI();
        }
    }
#endif
}