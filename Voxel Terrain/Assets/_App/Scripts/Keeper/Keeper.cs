using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keeper : MonoBehaviour
{
    [Header("Keeper Data")]
    [Tooltip("")]
    private KeeperData playerGameData;


    void Start()
    {
        if (playerGameData == null)
            playerGameData = new KeeperData();
    }
}
