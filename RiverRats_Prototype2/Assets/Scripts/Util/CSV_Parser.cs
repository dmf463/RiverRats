using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSV_Parser : MonoBehaviour
{

    public TextAsset rulesCSV; //reference of CSV file

    private char fieldSeperator = ',';
    private char lineSeperator = '\n';
    public string[] rawRules;
    
    // Start is called before the first frame update
    void Start()
    {
        ReadData();
        int randomNum = Random.Range(0, rawRules.Length);
        char[] text = rawRules[randomNum].ToCharArray();
        string rule = rawRules[randomNum].Remove(rawRules[randomNum].IndexOf('('));
        List<int> ruleTargets = new List<int>();
        for(int i = 0; i < text.Length; i++)
        {
            if (text[i] == '0'|| text[i] == '1' || text[i] == '2' || text[i] == '3' || text[i] == '4')
            {
                ruleTargets.Add(GetSeatPos(text[i]));
            }
        }
        if(ruleTargets.Count == 0)
        {
            Debug.Log("rule = " + rule);
        }
        else if(ruleTargets.Count == 1)
        {
            Debug.Log("Player " + ruleTargets[0] + " : " + rule);
        }
        else if(ruleTargets.Count == 2)
        {
            Debug.Log("Player " + ruleTargets[0] + " " + rule + " player " + ruleTargets[1]);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ReadData()
    {
        rawRules = rulesCSV.text.Split(lineSeperator, fieldSeperator);
        for (int i = 0; i < rawRules.Length; i++)
        {
            Debug.Log(rawRules[i]);
        }
    }

    private int GetSeatPos(char c)
    {
        int pos;

        if (c == '0') pos = 0;
        else if (c == '1') pos = 1;
        else if (c == '2') pos = 2;
        else if (c == '3') pos = 3;
        else pos = 4;

        return pos;
    }
}
