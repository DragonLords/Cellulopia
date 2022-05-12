using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button continueButton;
    // Start is called before the first frame update
    void Start()
    {
        if(!File.Exists(Application.dataPath+"/Data/Save.json")){
            Destroy(continueButton.gameObject);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ContinueGame(){
        
    }

    
}
