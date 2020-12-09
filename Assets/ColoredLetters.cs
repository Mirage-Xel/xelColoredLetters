using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;
using KModkit;

public class ColoredLetters : MonoBehaviour {
    public KMSelectable[] keys;
    public TextMesh[] texts;
    public Color[] colors;
    string[][] letterSequences = new string[4][];
    int[][] colorSequences = new int[4][];
    int[] scores = new int[4];
    string[] alphabet = new string[26]
  { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
      "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
    int[][] startingScores = new int[][]
    {
        new int[] {5,4,2,8},
        new int[] {3,2,9,6},
        new int[] {1,2,7,5},
        new int[] {3,6,5,1},
        new int[] {2,3,7,4},
        new int[] {1,4,6,9},
        new int[] {8,3,7,2},
        new int[] {4,1,3,4},
        new int[] {2,1,1,6},
        new int[] {8,4,9,5},
        new int[] {2,6,9,2},
        new int[] {5,4,4,3},
        new int[] {8,5,6,4},
        new int[] {3,2,8,2},
        new int[] {5,1,7,1},
        new int[] {1,6,1,3},
        new int[] {1,5,7,8},
        new int[] {4,8,3,6},
        new int[] {7,8,3,9},
        new int[] {3,6,6,7},
        new int[] {8,5,5,6},
        new int[] {3,6,1,6},
        new int[] {2,3,2,8},
        new int[] {4,9,2,1},
        new int[] {2,7,2,2},
        new int[] {8,9,3,7}
    };
    int[][] subsequentScores = new int[][]
    {
        new int[] {3,1,2,6},
        new int[] {7,2,2,8},
        new int[] {2,2,3,6},
        new int[] {5,9,8,3},
        new int[] {4,1,8,5},
        new int[] {4,2,6,1},
        new int[] {2,6,7,3},
        new int[] {3,4,8,1},
        new int[] {5,6,3,9},
        new int[] {1,7,8,5},
        new int[] {3,6,1,8},
        new int[] {9,4,8,8},
        new int[] {9,3,8,6},
        new int[] {2,4,1,3},
        new int[] {5,2,7,1},
        new int[] {4,1,4,2},
        new int[] {1,6,8,2},
        new int[] {3,5,9,2},
        new int[] {7,2,9,2},
        new int[] {1,2,6,4},
        new int[] {7,9,2,8},
        new int[] {3,6,1,4},
        new int[] {3,4,7,2},
        new int[] {8,9,4,7},
        new int[] {3,9,4,9},
        new int[] {3,2,8,4}
    };
    List<int> order;
    int stage;
    bool[] correct = new bool[4];
    string[] messagePool = new string[] { "GOOD", "WOW!", "NICE", "GG:)", "PRUZ", "POG!", "ABCD", "XEL.", "LETR", "THX!", "SLVD", "COOL", "YEAH", "UWIN"};
    string message;
    string[] colorNames = new string[] { "Red ", "Yellow ", "Blue ", "Green " };
    public KMBombModule module;
    public KMAudio sound;
    int moduleId;
    static int moduleIdCounter = 1;
    bool solved;

    void Awake () {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < 4; i++)
        {
            int j = i;
            keys[j].OnInteract += delegate { PressKey(j); return false; };
        }
        module.OnActivate += delegate { Activate(); };
	}
    void Activate()
    {
        stage = 0;
        message = messagePool[rnd.Range(0, messagePool.Length)];
        GenerateScores();
        order = scores.ToList();
        order.Sort();
        order.Reverse();
        for (int i = 0; i < 4; i++)
        {
            string loggingString = "";
            for (int j = 0; j < letterSequences[i].Length - 1; j++)
            {
                loggingString += colorNames[colorSequences[i][j]] + letterSequences[i][j];
                if (j != letterSequences[i].Length - 2) loggingString += ", ";
            }
            Debug.LogFormat("[Colored Letters #{0}] Button {1}'s character string is {2}.", moduleId, i + 1, loggingString);
        }
        Debug.LogFormat("[Colored Letters #{0}] The scores of the buttons in order are {1}, {2}, {3}, and {4}.", moduleId, scores[0], scores[1], scores[2], scores[3]);
    }
    // Update is called once per frame
    void GenerateScores() {
        for (int i = 0; i < 4; i++) letterSequences[i] = new string[rnd.Range(5, 8)];
        for (int i = 0; i < 4; i++) colorSequences[i] = new int[letterSequences[i].Length];
        foreach (int[] i in colorSequences)
        {
            for (int j = 0; j < i.Length; j++)
            {
                i[j] = rnd.Range(0, 4);
            }
        }
        foreach (string[] i in letterSequences)
        {
            for (int j = 0; j < i.Length - 1; j++)
            {
                i[j] = alphabet[rnd.Range(0, 26)];
            }
        }
        int[][] table;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < letterSequences[i].Length - 1; j++)
            {
                if (j == 0) table = startingScores; else table = subsequentScores;
                scores[i] += table[letterSequences[i][j][0] - 65][colorSequences[i][j]];
            }
        } 
        for (int i = 0; i < 4; i++) StartCoroutine(ShowSequence(i));
    }
    void PressKey(int index)
    {
         if (!solved)
         {
             keys[index].AddInteractionPunch();
             sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
             Debug.LogFormat("[Colored Letters #{0}] You pressed button {1}.", moduleId, index + 1);
             if (scores[index] == order[stage])
             {
                 Debug.LogFormat("[Colored Letters #{0}] That was correct.", moduleId);
                 correct[index] = true;
                 texts[index].text = message[index].ToString();
                 texts[index].color = colors[2];
                 stage++;
                 if (stage == 4)
                 {
                     Debug.LogFormat("[Colored Letters #{0}] Module solved.", moduleId);
                     sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                     module.HandlePass();
                     solved = true;
                 }
             }
             else
             {
                 Debug.LogFormat("[Colored Letters #{0}] That was incorrect. Strike!", moduleId);
                 module.HandleStrike();
                for (int i = 0; i < 4; i++) correct[i] = false;
                for (int i = 0; i < 4; i++) scores[i] = 0;
                StopAllCoroutines();
                 Activate();
             }
         }
    }
    IEnumerator ShowSequence(int posIndex)
    {
        int sequenceIndex = rnd.Range(0, letterSequences[posIndex].Length);
        while (!correct[posIndex])
        {
            texts[posIndex].text = letterSequences[posIndex][sequenceIndex];
            texts[posIndex].color = colors[colorSequences[posIndex][sequenceIndex]];
            sequenceIndex++;
            if (sequenceIndex == letterSequences[posIndex].Length) sequenceIndex = 0;
            yield return new WaitForSeconds(0.5f);
        }
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "use '!{0} 1234' to press the buttons in reading order.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string validcmds = "1234";
        if (command.Contains(' '))
        {
            yield return "sendtochaterror @{0}, invalid command.";
            yield break;
        }
        else
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (!validcmds.Contains(command[i]))
                {
                    yield return "sendtochaterror Invalid command.";
                    yield break;
                }
            }
            for (int i = 0; i < command.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (command[i] == validcmds[j])
                    {
                        yield return null;
                        yield return new WaitForSeconds(1f);
                        keys[j].OnInteract();
                    }
                }
            }
        }
    }
}
