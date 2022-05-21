using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSkillTree : MonoBehaviour
{
    public Button button;
    Image image;
    [SerializeField] GameObject skillTreeHolder;
    [SerializeField] Sprite[] spriteButtons;
    //On cache tout les elements necessaire dans le Awake
    private void Awake()
    {
        image = GetComponent<Image>();
        button=GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        if(skillTreeHolder is null)
            skillTreeHolder=FindObjectOfType<SkillTreeHandler>().gameObject;
    }

    /// <summary>
    /// Sert a linteractivite du bouton
    /// </summary>
    public void OnClick(){
        //on active ou desactive le gamobject qui continent le skll tree
        skillTreeHolder.SetActive(!skillTreeHolder.activeSelf);
        //si le skill tree est actif alors on met limage du X pour le fermer sinon on met limage pour louvrir
        image.sprite = spriteButtons[skillTreeHolder.activeSelf ? (int)SpriteButton.Close : (int)SpriteButton.Open];
        //on alterne la pause du jeu (si le menu est ouvert on met le jeu en pause sinon on enleve la pause)
        GameManager.Instance.PauseGame();
    }
    enum SpriteButton { Open,Close}
}
