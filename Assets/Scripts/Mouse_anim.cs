using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mouse_anim : MonoBehaviour
{
    [SerializeField] Image image;
    // [SerializeField] GameObject targteGo;
    // Material target;
    [SerializeField] Sprite[] _sprites;
    [SerializeField] Texture2D[] _textures;
    [SerializeField] bool playing=true;
    [SerializeField] float WillingDuration;
    [SerializeField] float fps;
    [SerializeField] float Delay;
    WaitForSeconds ws; 
    Coroutine routine;
    [SerializeField] int i=0;
    // Start is called before the first frame update
    public IEnumerator Start()
    {
        // target=targteGo.GetComponent<Renderer>().material;
        Delay=WillingDuration/fps;
        ws=new(Delay);
        i=0;
        do
        {
            image.sprite=_sprites[i];
            // target.mainTexture=_textures[i];
            ++i;
            if(i>=_sprites.Length)
                i=0;
            yield return ws;
        } while (playing);
        routine=null;
    }

    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    void OnValidate()
    {
        if(playing&&Application.isPlaying){
            if(routine is null){
                routine=StartCoroutine(Start());
            }
        }     
    }
}
