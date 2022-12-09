using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SunnyValleyStudio;
public class KeeperController : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("")]
    private KeeperWorld keeperWorld;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Transform MainCamera = Camera.main.transform;

            if (Physics.Raycast(MainCamera.position, MainCamera.forward, out RaycastHit hit))
            {
                Debug.Log(hit.point);

                //keeperWorld.SetVoxel(hit, VoxelType.TreeLeavesSolid);

                keeperWorld.TagVoxel(hit.point);
            }
        }
    }
}
