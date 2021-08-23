using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class anagraphyScript : MonoBehaviour
{

    //public stuff
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public GameObject[] OtherDisplays;
    public TextMesh[] Text;
    public List<MeshRenderer> ButtonMesh;
    public KMBombModule Module;

    //functionality
    private bool solved = false;
    private List<int> input = new List<int> { };
    private int[] inbutton = { 2, 1 };
    private List<List<int>> answers;

    //logging
    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    private KMSelectable.OnInteractHandler Press(int pos)
    {
        return delegate
        {
            if (!solved)
            {
                Audio.PlaySoundAtTransform("Beep", Buttons[pos].transform);
                Buttons[pos].AddInteractionPunch(1f);
                switch (pos)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        inbutton[0] = (inbutton[0] + (pos <= 1 ? 4 : 1)) % 5;
                        inbutton[1] = (inbutton[1] + (pos % 2 == 1 ? 1 : 2)) % 3;
                        Text[2].text = "ABCDEFGHIJKLMNO"[inbutton[1] + inbutton[0] * 3].ToString();
                        break;
                    case 4:
                        if (input.Count() != 7)
                            input.Add(inbutton[1] + inbutton[0] * 3);
                        Text[1].text = input.Select(x => "ABCDEFGHIJKLMNO"[x]).Join("");
                        break;
                    case 5:
                        input = new List<int> { };
                        Text[1].text = "";
                        break;
                    case 6:
                        if (answers.Select(x => x.Select(y => "ABCDEFGHIJKLMNO"[y]).Join("")).Contains(input.Select(x => "ABCDEFGHIJKLMNO"[x]).Join("")))
                        {
                            Module.HandlePass();
                            solved = true;
                            foreach (var text in Text)
                                text.text = "";
                            StartCoroutine(Shutdown());
                        }
                        else
                        {
                            Debug.LogFormat("[Anagraphy #{0}] {1} is not a valid anagraph; Strike!", _moduleID, input.Select(x => "ksomxaftuhnipre"[x]).Join(""));
                            Module.HandleStrike();
                            StartCoroutine(Generate());
                        }
                        break;
                }
            }
            return false;
        };
    }

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].OnInteract += Press(i);
            int x = i;
            Buttons[i].OnHighlight += delegate { if (!solved) { ButtonMesh[x].material.color = new Color(.625f, .875f, 1); } };
            Buttons[i].OnHighlightEnded += delegate { if (!solved) { ButtonMesh[x].material.color = new Color(.25f, .75f, 1); } };
            ButtonMesh.Add(Buttons[i].GetComponent<MeshRenderer>());
        }
        foreach (var item in OtherDisplays)
            ButtonMesh.Add(item.GetComponent<MeshRenderer>());
        Text[2].text = "H";
    }

    void Start()
    {
        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        string letters = "ABCDEFGHIJKLMNO";
        List<bool> isVowel = new List<bool> { false, false, true, false, false, true, false, false, true, false, false, true, false, false, true };
        List<List<int>> structure = new List<List<int>> { new List<int> { 1, 1, 2, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 0, 0, 1, 0, 0 }, new List<int> { 1, 1, 0, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 1, 1, 0, 0, 0, 0 }, new List<int> { 1, 0, 1, 0, 0, 0, 0, 0 }, new List<int> { 1, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 1, 0, 0, 0, 0 }, new List<int> { 1, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 0, 0, 0, 1, 0 }, new List<int> { 1, 0, 1, 0, 0, 0, 0, 0 }, new List<int> { 1, 0, 2, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 0, 1, 0, 0, 0 }, new List<int> { 0, 1, 1, 1, 0, 0, 0, 0 }, new List<int> { 0, 1, 0, 0, 1, 0, 0, 0 }, new List<int> { 0, 0, 0, 0, 0, 0, 0, 1 } };

        notgood:
        List<string> syllables = new List<string> { };
        List<int> wordthing = new List<int> { };
        while (syllables.Select(x => x.Length).Sum() <= 5)
        {
            string syllable = "";
            wordthing.Add(Enumerable.Range(0, 15).Where(x => isVowel[x]).PickRandom());
            syllable += letters[wordthing.Last()];
            if (Rnd.Range(0, 7) != 0)
            {
                wordthing.Add(Enumerable.Range(0, 15).Where(x => !isVowel[x]).PickRandom());
                syllable += letters[wordthing.Last()];
            }
            syllables.Add(syllable);
        }
        syllables.Shuffle();
        string romanized = "ksomxaftuhnipre";
        string word = syllables.Join("");
        for (int i = 0; i < 15; i++)
            word = word.Replace(letters[i], romanized[i]);
        Debug.Log(word);

        Text[0].text = wordthing.Select(x => letters[x]).ToList().Join("");
        Text[1].text = "";

        List<int> contents = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int i = 0; i < syllables.Join("").Length; i++)
            for (int j = 0; j < 15; j++)
                if (syllables.Join("")[i] == letters[j])
                    for (int k = 0; k < 8; k++)
                        contents[k] += structure[j][k];

        List<List<int>> wordparts = new List<List<int>> { new List<int> { } };
        List<List<int>> segmentsremain = new List<List<int>> { contents };
        for (int i = 0; i < wordparts.Count(); i++)
            for (int j = 0; j < 15; j++)
            {
                List<int> temp = segmentsremain[i].ToList();
                for (int k = 0; k < 8; k++)
                    temp[k] -= structure[j][k];
                if (temp.All(x => x >= 0))
                {
                    wordparts.Add(wordparts[i].Concat(new int[] { j }).ToList());
                    segmentsremain.Add(temp);
                }

                //preventing lagspikes
                if (wordparts.Count() % 2500 == 0)
                    yield return null;
            }
        List<List<int>> wordpicks = Enumerable.Range(0, wordparts.Count()).Where(x => segmentsremain[x].Sum() == 0).Select(x => wordparts[x]).ToList();
        List<List<int>> wordpicks2 = new List<List<int>> { };
        foreach (List<int> item in wordpicks)
        {
            bool unused = true;
            for (int i = 0; i < wordpicks2.Count() + 1 && unused; i++)
            {
                bool maybeno = true;
                for (int j = 0; j < item.Count() && j < wordpicks2.Concat(new List<List<int>> { wordthing }).ToList()[i].Count(); j++)
                    maybeno &= (item.OrderBy(x => x).ToList()[j] == wordpicks2.Concat(new List<List<int>> { wordthing }).ToList()[i].OrderBy(x => x).ToList()[j]);
                unused &= !maybeno;
            }
            if (unused && item.Count(x => isVowel[x]) >= item.Count(x => !isVowel[x] && item.Count() <= 7))
                wordpicks2.Add(item);
        }
        if (wordpicks2.Count() == 0)
            goto notgood;

        List<List<int>> wordpicks3 = Enumerable.Repeat(new List<int> { }, wordpicks2.Count()).ToList();
        List<List<int>> stocked = wordpicks2.ToList();
        for (int i = 0; i < wordpicks3.Count(); i++)
            for (int j = 0; j < stocked[i].Distinct().Count(); j++)
                if (isVowel[stocked[i].Distinct().ToList()[j]] || (wordpicks3[i].Count() > 0 && isVowel[wordpicks3[i].Last()]))
                {
                    wordpicks3.Add(wordpicks3[i].Concat(new List<int> { stocked[i].Distinct().ToList()[j] }).ToList());
                    List<int> temp = stocked[i].ToList();
                    temp.RemoveAt(temp.IndexOf(stocked[i].Distinct().ToList()[j]));
                    stocked.Add(temp);

                    //preventing lagspikes
                    if (wordpicks3.Count() % 2500 == 0)
                        yield return null;
                }
        answers = Enumerable.Range(0, wordpicks3.Count()).Where(x => stocked[x].Count() == 0).Select(x => wordpicks3[x]).ToList(); 
        Debug.LogFormat("[Anagraphy #{0}] Displayed word is '{1}'. Valid anagraphs are any of the following: {2}.", _moduleID, wordthing.Select(x => romanized[x]).ToList().Join(""), answers.Select(x => x.Select(y => romanized[y]).ToList().Join("")).Join(", "));
    }

    private IEnumerator Shutdown()
    {
        for (int i = 0; i < 20; i++)
        {
            float x = Rnd.Range(0f, 1f);
            for (int j = 0; j < ButtonMesh.Count(); j++)
                ButtonMesh[j].material.color = Color.Lerp(new Color(.25f, .75f, 1), new Color(.0625f, .0625f, .0625f), x);
            yield return new WaitForSeconds(0.05f);
        }
        for (int i = 0; i < ButtonMesh.Count(); i++)
            ButtonMesh[i].material.color = new Color(.0625f, .0625f, .0625f);
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "'!{0} bdpqacs' to press 'top left', 'top right', 'bottom left', 'bottom right' (the tail points toward the corner to press), 'append', 'clear' and 'submit' buttons respectively.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        command = command.ToLowerInvariant();
        for (int i = 0; i < command.Length; i++)
            if (!"bdpqacs".Contains(command[i]))
            {
                yield return "sendtochaterror Invalid command.";
                yield break;
            }
        for (int i = 0; i < command.Length; i++)
            for (int j = 0; j < 7; j++)
                if ("bdpqacs"[j] == command[i])
                {
                    Buttons[j].OnInteract();
                    yield return null;
                }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return true;
        List<int> ans = answers.PickRandom();
        if (!solved)
        {
            Buttons[5].OnInteract();
            yield return true;
            while (input.Count() < ans.Count())
            {
                while (ans[input.Count()] != inbutton[1] + inbutton[0] * 3)
                {
                    Buttons[0].OnInteract();
                    yield return true;
                }
                Buttons[4].OnInteract();
                yield return true;
            }
            Buttons[6].OnInteract();
            yield return true;
        }
    }
}
