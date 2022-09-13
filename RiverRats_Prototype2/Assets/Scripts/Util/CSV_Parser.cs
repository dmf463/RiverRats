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
}
