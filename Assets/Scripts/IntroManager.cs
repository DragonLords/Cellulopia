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
    readonly string[] quotes = { "La vie est emplie de mystère.", "Mais le plus grand de tous est celui de sa création." };
    readonly List<string> lines = new() {
        "Lors de sa création nombre de mystère se sont introduit.","Tous perdu dans sa complexité.","Suivre son périple en est compliqué.","Chaque spécimen va devoir tracer sa propre destiné.","Lequel va survivre.","Lequel va s’éteindre.","Un autre mystère sans réponse.","Survire et évolué est la seule méthode pour dominer son environnement.","Et ainsi,en sortir victorieux et persisté."};

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
