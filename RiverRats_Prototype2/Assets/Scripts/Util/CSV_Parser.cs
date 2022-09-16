using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSV_Parser : MonoBehaviour
{

    public TextAsset rulesCSV; //reference of CSV file

    private char fieldSeperator = ',';
    private char lineSeperator = '\n';
    public string[] rawRules;
    public List<Rule> OrganizedRules = new List<Rule>();
    
    // Start is called before the first frame update
    void Start()
    {
        ReadData();
        CreateRules();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CreateRules()
    {
        for (int rulesIndex = 0; rulesIndex < rawRules.Length; rulesIndex++)
        {
            char[] text = rawRules[rulesIndex].ToCharArray();
            string rule = rawRules[rulesIndex].Remove(rawRules[rulesIndex].IndexOf('('));
            List<int> ruleTargets = new List<int>();
            for (int charIndex = 0; charIndex < text.Length; charIndex++)
            {
                if (text[charIndex] == '0' || text[charIndex] == '1' || text[charIndex] == '2' || text[charIndex] == '3' || text[charIndex] == '4')
                {
                    ruleTargets.Add(GetSeatPos(text[charIndex]));
                }
            }
            if (ruleTargets.Count == 0)
            {
                OrganizedRules.Add(new Rule(GetRuleName(rule)));
            }
            else if (ruleTargets.Count == 1)
            {
                OrganizedRules.Add(new Rule(GetRuleName(rule), ruleTargets[0]));
            }
            else if (ruleTargets.Count == 2)
            {
                OrganizedRules.Add(new Rule(GetRuleName(rule), ruleTargets[0], ruleTargets[1]));
            }
        }

        //for (int i = 0; i < OrganizedRules.Count; i++)
        //{
        //    Debug.Log("rule = " + OrganizedRules[i].RuleName + " and target players are " + OrganizedRules[i].TargetPlayer0 + " and " + OrganizedRules[i].TargetPlayer1);
        //}
    }

    private void ReadData()
    {
        rawRules = rulesCSV.text.Split(lineSeperator, fieldSeperator);
        //for (int i = 0; i < rawRules.Length; i++)
        //{
        //    Debug.Log(rawRules[i]);
        //}
    }

    private RuleNames GetRuleName(string text)
    {
        RuleNames rule = RuleNames.NULL;

        switch (text)
        {
            case "HATE":
                rule = RuleNames.Hate;
                break;
            case "LIKE":
                rule = RuleNames.Like;
                break;
            default:
                break;
        }

        return rule;
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
