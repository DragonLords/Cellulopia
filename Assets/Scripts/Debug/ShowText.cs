using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;
using System.IO;
using System.Text;

public class ShowText : MonoBehaviour, IPointerClickHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI txt;
    [SerializeField] Color _hoverColor;
    [SerializeField] Color _normalColor;
    [SerializeField] Color _clickedColor;
    [SerializeField] float speed;
    [SerializeField] float speedClick=3f;
    [SerializeField] bool needSave=false;
    [SerializeField] bool resetSave=false;
    [SerializeField] bool quit=false;
    [SerializeField] bool test=false;
    [SerializeField,Tooltip("Asset de la scene à charger")] AssetReference _sceneToLoad;
    Coroutine routineAnim=null;
    // Start is called before the first frame update
    void Start()
    {
        if(needSave&&!SaveManager.SaveExist())
            Destroy(gameObject);                
        if(txt is null)
            txt=GetComponent<TextMeshProUGUI>();
        txt.color=_normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // txt.color=_clickedColor;
        if(routineAnim!=null){
            StopCoroutine(routineAnim);
            routineAnim=null;
        }
        routineAnim=StartCoroutine(ChangeColor(txt.color,_clickedColor,true));
        if(quit){
            Application.Quit();
        }
        else if(test){
            LoaderScene.Instance.SetSceneToLoad(AddressablePath.testLoad);
        }
        else if(resetSave){
            SaveManager.ResetSave();
            LoaderScene.Instance.SetSceneToLoad(AddressablePath.GameScene);
        }else{
            LoaderScene.Instance.SetSceneToLoad(AddressablePath.GameScene);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(routineAnim!=null){
            StopCoroutine(routineAnim);
            routineAnim=null;
        }
        routineAnim=StartCoroutine(ChangeColor(txt.color,_hoverColor));
        // Debug.Log("enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(routineAnim!=null){
            StopCoroutine(routineAnim);
            routineAnim=null;
        }
        routineAnim=StartCoroutine(ChangeColor(txt.color,_normalColor));
        // Debug.Log("exit");
    }

    IEnumerator ChangeColor(Color start,Color target,bool cliked=false){
        float tick=0f;
        do
        {
            if(cliked)
                tick+=Time.deltaTime*speedClick;
            else
                tick+=Time.deltaTime*speed;
            Color finalColor=Color.Lerp(start,target,tick);
            txt.color=finalColor;
            yield return null;
        } while (txt.color!=target);
        routineAnim=null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }


    public static Dictionary<string,int> bob=new(){
        {"Bob",21783},{"shguidf",23487234},{"edioufgsdgu",234863245},{"dfjhdsf",382463246}
    };
    void Init(){
        string path=$"{Application.dataPath}/Data/Test.json";
        using(FileStream fs=new(path,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.ReadWrite)){
            Data data=new(bob:bob);
            string json=JsonConvert.SerializeObject(data,Formatting.Indented);
            Debug.Log(json);
            byte[] bytes=new UTF8Encoding(true).GetBytes(json);
            fs.Write(bytes);
            fs.Close();
        }
    }
}

internal static class Test{
    public static void Init(){
        
    }
}
public class Data{
    public Dictionary<string,int> bob;

    public Data(Dictionary<string,int> bob)
    {
        this.bob=bob;
    }
} 
