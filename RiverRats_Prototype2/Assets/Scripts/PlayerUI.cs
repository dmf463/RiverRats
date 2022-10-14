using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Accord.Statistics.Distributions.Univariate;

public class PlayerUI : MonoBehaviour
{
    public int numCardsInDeck = 36;
    public int numSuccessesinDeck = 4;
    public int numOfDrawsToMake = 2;
    public int successesInSample = 1;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Avg = " + GetCardDrawProbability(numCardsInDeck, numSuccessesinDeck, numOfDrawsToMake, successesInSample));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Ben says: pass in k for each value of successes you care about and probably sum them, so like if you needed one club for a flush in 2 draws you could call it with k=1 and k=2 since two clubs also gives you a flush, and sum those, vs if you needed 2 clubs then you'd just want k=2 etc
    public double GetCardDrawProbability(int cardsInDeck, int successes, int drawsNeeded, int sampleSuccess)
    {
        double probability;

        HypergeometricDistribution dist = new HypergeometricDistribution(cardsInDeck, successes, drawsNeeded);
        probability = dist.ProbabilityMassFunction(sampleSuccess) * 100;

        return probability;
    }
}
