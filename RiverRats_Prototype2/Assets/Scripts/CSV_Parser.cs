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
            List<Player> ruleTargets = new List<Player>();
            for (int charIndex = 0; charIndex < text.Length; charIndex++)
            {
                if (text[charIndex] == '0' || text[charIndex] == '1' || text[charIndex] == '2' || text[charIndex] == '3' || text[charIndex] == '4')
                {
                    //ruleTargets.Add(GetSeatPos(text[charIndex]));
                    ruleTargets.Add(Services.TableManager.players[GetSeatPos(text[charIndex])]);
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
            Debug.Log("Organized Rule Count = " + OrganizedRules.Count);
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

    private RuleType GetRuleName(string text)
    {
        RuleType rule = RuleType.TargetPlayer;

        switch (text)
        {
            case "HATE":
                rule = RuleType.Hate;
                break;
            case "LIKE":
                rule = RuleType.Like;
                break;
            case "NO_POS":
                rule = RuleType.NoPositive;
                break;
            case "NO_NEG":
                rule = RuleType.NoNegative;
                break;
            case "OUT_NEG":
                rule = RuleType.OutNegative;
                break;
            case "OUT_POS":
                rule = RuleType.OutPositive;
                break;
            case "SHORT_BIG":
                rule = RuleType.ShortToBig;
                break;
            case "BIG_SHORT":
                rule = RuleType.BigToShort;
                break;
            case "CHIP_FIVE":
                rule = RuleType.RoundFiveChips;
                break;
            case "CHIP_TEN":
                rule = RuleType.RoundTenChips;
                break;
            case "FINAL_TWO":
                rule = RuleType.FinalTwo;
                break;
            case "FIRST_OUT":
                rule = RuleType.FirstOut;
                break;
            case "THREE_WIN":
                rule = RuleType.ThreeWayWin;
                break;
            case "THREE_LOSE":
                rule = RuleType.ThreeWayLose;
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
