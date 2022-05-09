using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Voxels", menuName = "Voxel Save")]

/// <summary>
/// This holds Data so that it can be transfered from projects. It is created with the save button on the Voxelmap script
/// </summary>
public class VoxelSO : ScriptableObject
{
    public List<Voxel> voxels = new List<Voxel>();
}
