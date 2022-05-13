using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;

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
    [SerializeField,Tooltip("Asset de la scene à charger")] AssetReference _sceneToLoad;
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
        StartCoroutine(ChangeColor(txt.color,_clickedColor,true));
        
        if(_sceneToLoad is not null){
            try{
                _sceneToLoad.LoadSceneAsync(LoadSceneMode.Single,true,100);
            }catch{
                Debug.LogError("L'asset doit être une Scene");
            // Debug.Log(_sceneToLoad.GetType());
            }
            
        }
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(ChangeColor(txt.color,_hoverColor));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(ChangeColor(txt.color,_normalColor));
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }
}