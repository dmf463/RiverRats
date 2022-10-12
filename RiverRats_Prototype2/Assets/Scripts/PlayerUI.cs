using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Accord.Statistics.Distributions.Univariate;

public class PlayerUI : MonoBehaviour
{
    public int numCardsInDeck = 36;
    public int numSuccessesinDeck = 4;
    public int numOfDrawsToMake = 2;
    // Start is called before the first frame update
    void Start()
    {
        HypergeometricDistribution test = new HypergeometricDistribution(numCardsInDeck, numSuccessesinDeck, numOfDrawsToMake);
        Debug.Log("Avg = " + (int)(test.Mean * 100));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
