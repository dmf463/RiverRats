using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//basically a class to hold and reference any prefab super easily
//it's an easy to use pattern

[CreateAssetMenu(menuName = "Prefab DB")]
public class PrefabDB : ScriptableObject {

    [SerializeField]
    private GameObject cardDeck;
    public GameObject CardDeck { get { return cardDeck; } }

    [SerializeField]
    private GameObject card;
    public GameObject Card { get { return card; } }

    [SerializeField]
    private GameObject redChip;
    public GameObject RedChip { get { return redChip; } }

    [SerializeField]
    private GameObject blueChip;
    public GameObject BlueChip { get { return blueChip; } }

    [SerializeField]
    private GameObject whiteChip;
    public GameObject WhiteChip { get { return whiteChip; } }

    [SerializeField]
    private GameObject blackChip;
    public GameObject BlackChip { get { return blackChip; } }

    [SerializeField]
    private GameObject dealerButton;
    public GameObject DealerButton { get { return dealerButton; } }

    [SerializeField]
    private GameObject genericAudioSource;
    public GameObject GenericAudioSource { get { return genericAudioSource; } }

    [SerializeField]
    private GameObject puffOfSmoke;
    public GameObject PuffOfSmoke { get { return puffOfSmoke; } }


}
