using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("VoxelMap Designer")]
public class VoxelMap : MonoBehaviour
{ 
    public List<Voxel> voxels = new List<Voxel>();

    public VoxelSO save;

    public List<GameObject> palette;
    public int brush = 0;

    public List<GameObject> created = new List<GameObject>();
    
    public void RemoveVoxel(Vector3Int pos)
    {
        int index = -1;
        for (int i = 0; i < voxels.Count; i++)
        {
            if(voxels[i].position == pos)
            {
                index = i;
            }
        }
        if(index > -1)
        {
            GameObject v = created[index];
            voxels.RemoveAt(index);
            created.RemoveAt(index);
            DestroyImmediate(v);
        }
    }
    public void PaintVoxel(Vector3Int pos)
    {
        voxels.Add(new Voxel(brush, pos));
        GameObject v = Instantiate(palette[brush], transform);
        v.transform.position = pos;
        created.Add(v);
    }
    public void RepaintVoxels()
    {
        created.Clear();
        DestroyAllChildren(transform);
        for (int i = 0; i < voxels.Count; i++)
        {
            Voxel vox = voxels[i];
            GameObject v = Instantiate(palette[vox.index], transform);
            v.transform.position = vox.position;
            created.Add(v);
        }
    }
    public void DestroyAllChildren(Transform t)
    {
        while(t.childCount > 0)
        {
            DestroyImmediate(t.GetChild(0).gameObject);
        }
    }
}

/// <summary>
/// A Voxel is 3d cube. The int index holds the index in the palette that it will instantiate.
/// </summary>
[SerializeField]
[System.Serializable]//use this to show custom Structs in the inspector
public struct Voxel
{
    public int index;
    public Vector3Int position;
    public Quaternion rotation;

    public Voxel(int i, Vector3Int pos)
    {
        index = i;
        position = pos;
        rotation = Quaternion.identity;
    }
    public Voxel(Vector3Int pos)
    {
        index = 0;
        position = pos;
        rotation = Quaternion.identity;
    }
    public Voxel(int i, Vector3Int pos, Quaternion rot)
    {
        index = i;
        position = pos;
        rotation = rot;
    }
}