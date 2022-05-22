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

/// <summary>
/// Classe qui sert a donner linteraction hover et onclick sur du TextMeshPro
/// lorsque la classe herite de IPointerClickHandler, IPointerEnterHandler et IPointerExitHandler nous donne access a 3 fonction qui seront appeler lors de leur evenement declencher
/// </summary>
public class ShowText : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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
    [SerializeField] bool skipIntro = false;
    [SerializeField,Tooltip("Asset de la scene à charger")] AssetReference _sceneToLoad;
    /// <summary>
    /// Sert a garder une reference interne de la coroutine qui sert a creer une effet progressif de changement de couleur sur le texte
    /// </summary>
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

    /// <summary>
    /// Sert a detecter lorsquon clique sur lelement de UI
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // txt.color=_clickedColor;
        if(routineAnim!=null){
            StopCoroutine(routineAnim);
            routineAnim=null;
        }
        //on commence la corutine qui sert a changer la couleur du texte en lui donnant la couleur desirer
        routineAnim=StartCoroutine(ChangeColor(txt.color,_clickedColor,true));

        if(quit){
            Application.Quit();
        }
        else if (skipIntro)
        {
            LoaderScene.Instance.SetSceneToLoad(AddressablePath.GameScene);
        }
        else if(test){
            LoaderScene.Instance.SetSceneToLoad(AddressablePath.testLoad);
        }
        else if(resetSave){
            SaveManager.ResetSave();
            LoaderScene.Instance.SetSceneToLoad(AddressablePath.IntroTxtScene);
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

    /// <summary>
    /// coroutine qui sert a changer progressivement la couleur vers une autre
    /// </summary>
    /// <param name="start">la couleur actuellement active</param>
    /// <param name="target">la couleur desirer</param>
    /// <param name="cliked">sert a determiner si on clique sur lelement donc on change plus vite la couleur</param>
    /// <returns></returns>
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
}