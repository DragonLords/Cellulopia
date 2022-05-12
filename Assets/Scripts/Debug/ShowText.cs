using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class ShowText : MonoBehaviour, IPointerClickHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI txt;
    [SerializeField] Color _hoverColor;
    [SerializeField] Color _normalColor;
    [SerializeField] Color _clickedColor;
    [SerializeField] float speed;
    [SerializeField] float speedClick=3f;
    [SerializeField] bool needSave=false;
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
        Debug.Log("Pointer click");
        // txt.color=_clickedColor;
        StartCoroutine(ChangeColor(txt.color,_clickedColor,true));
        SceneManager.LoadScene(3);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer enter");
        // txt.color=_hoverColor;
        StartCoroutine(ChangeColor(txt.color,_hoverColor));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exit");
        // txt.color=_normalColor;
        // txt.color=Color.Lerp(txt.color,_normalColor,Mathf.PingPong(Time.time,1f));
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