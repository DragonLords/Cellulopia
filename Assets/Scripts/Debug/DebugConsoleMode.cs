using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using TMPro;

public class DebugConsoleMode : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TMP_InputField inputField;
    string debugFood = "spawn food";
    // Start is called before the first frame update
    void Start()
    {
        inputField.onEndEdit.AddListener(OnTextChange);
    }
    private void Update()
    {
        if(Keyboard.current.wKey.wasPressedThisFrame)
            gameObject.SetActive(!gameObject.activeSelf);
    }

    public void OnTextChange(string value){
        value=value.ToLower();
        if(value==debugFood){
            Addressables.InstantiateAsync("Food",FindObjectOfType<Player.Player>().transform.position,Quaternion.identity).WaitForCompletion();
        }
        Debug.Log(value);
    }
}
