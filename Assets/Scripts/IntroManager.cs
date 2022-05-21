using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    WaitForSeconds delayChar = new(.1f);
    WaitForSeconds delayLine = new(1f);
    [SerializeField] TMPro.TextMeshProUGUI textMeshProUGUI;
    [SerializeField] GameObject SkipButton;
    public static IntroManager Instance { get; private set; }   
    readonly string[] quotes = { "La vie est emplie de myst�re.", "Mais le plus grand de tous est celui de sa cr�ation." };
    readonly List<string> lines = new() {
        "Lors de sa cr�ation nombre de myst�re se sont introduit.","Tous perdu dans sa complexit�.","Suivre son p�riple en est compliqu�.","Chaque sp�cimen va devoir tracer sa propre destin�.","Lequel va survivre.","Lequel va s��teindre.","Un autre myst�re sans r�ponse.","Survire et �volu� est la seule m�thode pour dominer son environnement.","Et ainsi,en sortir victorieux et persist�."};

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShowQuotes());
    }

    IEnumerator ShowQuotes()
    {
        SkipButton.SetActive(false);
        string show = "";
        foreach (var quote in quotes)
        {
            char[] allText=quote.ToCharArray();
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
