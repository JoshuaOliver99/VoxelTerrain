using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SunnyValleyStudio;
public class KeeperController : MonoBehaviour
{
    [Header("Referneces")]
    [SerializeField, Tooltip("")]
    private World keeperWorld;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Transform MainCamera = Camera.main.transform;

            if (Physics.Raycast(MainCamera.position, MainCamera.forward, out RaycastHit hit))
            {
                Debug.Log(hit.point);

                //keeperWorld.SetVoxel(hit, VoxelType.Grass_Stone);


                keeperWorld.CreateNewChunk(hit, VoxelType.Grass_Stone);
            }
        }
    }
}
