using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//basically a class to hold and reference any prefab super easily
//it's an easy to use pattern

[CreateAssetMenu(menuName = "Prefab DB")]
public class PrefabDB : ScriptableObject {

    [SerializeField]
    private GameObject successMark;
    public GameObject SuccessMark { get { return successMark; } }

    [SerializeField]
    private GameObject failMark;
    public GameObject FailMark { get { return failMark; } }


}
