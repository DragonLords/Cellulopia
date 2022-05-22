using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// classe qui sert a montrer la breve introduction au jeu 
/// </summary>
public class IntroManager : MonoBehaviour
{
    WaitForSeconds delayChar = new(.1f);
    WaitForSeconds delayLine = new(1f);
    [SerializeField] TMPro.TextMeshProUGUI textMeshProUGUI;
    [SerializeField] GameObject SkipButton;
    public static IntroManager Instance { get; private set; }   
    readonly string[] intros = { "La vie est emplie de myst�re.", "Mais le plus grand de tous est celui de sa cr�ation." };
    readonly List<string> lines = new() {
        "Lors de sa cr�ation nombre de myst�re se sont introduit.","Tous perdu dans sa complexit�.","Suivre son p�riple en est compliqu�.","Chaque sp�cimen va devoir tracer sa propre destin�.","Lequel va survivre.","Lequel va s��teindre.","Un autre myst�re sans r�ponse.","Survire et �volu� est la seule m�thode pour dominer son environnement.","Et ainsi,en sortir victorieux et persist�."};

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShowIntro());
    }

    /// <summary>
    /// montre les deux breve
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowIntro()
    {
        SkipButton.SetActive(false);
        string show = "";
        foreach (var intro in intros)
        {
            char[] allText=intro.ToCharArray();
            foreach(var c in allText)
            {
                show += c;
                textMeshProUGUI.text = show;
                yield return delayChar;
            }
            show += "\n\n";
        }
        yield return delayLine;
        yield return delayLine;
        SkipButton.SetActive(true);
        StartCoroutine(ShowAllTxt());
    }
     
    /// <summary>
    /// sert a montrer le texte apparaitre lettre par lettre progressivement
    /// </summary>
    /// <param name="text">le texte a afficher</param>
    /// <returns></returns>
    IEnumerator ShowText(string text)
    {
        char[] allText = text.ToCharArray();
        string show = "";
        foreach (var c in allText)
        {
            show += c;
            textMeshProUGUI.text = show;
            yield return delayChar;
        }
        yield return delayLine;
    }

    /// <summary>
    /// sert a passer en revue tout le texte a afficher
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowAllTxt()
    {
        foreach (string line in lines)
        {
            yield return StartCoroutine(ShowText(line));
        }
        yield return null;
        //start the actual game
        LoaderScene.Instance.SetSceneToLoad(AddressablePath.GameScene);
    }


}
