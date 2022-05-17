using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoaderScene : MonoBehaviour
{
    public static LoaderScene Instance;
    string loadScreenScene;
    internal string sceneToLoad;
    public Queue<SceneInstance> oldScene=new();
    void Awake(){
        Instance=this;
        DontDestroyOnLoad(gameObject);
        loadScreenScene=AddressablePath.LoadingScene;
        SetSceneToLoad(AddressablePath.GameScene);
    }

    /// <summary>
    /// STEPS TO FOLLOW:
    /// load the loading scene and enqueue
    /// unload the old scene and unqueue
    /// load the new scene
    /// unload the loading scene and unqueue
    /// </summary>

    public void SetSceneToLoad(string sceneAdress){
        sceneToLoad=sceneAdress;
        StartCoroutine(LoadSceneLoading());
    }

    IEnumerator LoadSceneLoading(){
        AsyncOperationHandle<SceneInstance> handle=Addressables.LoadSceneAsync(loadScreenScene);
        // Debug.Log(handle.PercentComplete);
        do
        {
            yield return null;
            // Debug.Log(handle.PercentComplete);
        } while (!handle.IsDone);
        yield return handle;
        oldScene.Enqueue(handle.Result);
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        //add the progress of loading this scene through the progress bar
        AsyncOperationHandle<SceneInstance> handle=Addressables.LoadSceneAsync(sceneToLoad,LoadSceneMode.Additive);
        do
        {
            yield return null;
            // Debug.Log(handle.PercentComplete);
        } while (!handle.IsDone);
        yield return handle;
        oldScene.Enqueue(handle.Result);
        
        StartCoroutine(UnloadOldScene());
    }

    IEnumerator UnloadOldScene(){
        oldScene.Reverse();
        SceneInstance sceneToUnload=oldScene.Dequeue();
        AsyncOperationHandle<SceneInstance> handle=Addressables.UnloadSceneAsync(sceneToUnload);
        yield return handle;
    }
}
