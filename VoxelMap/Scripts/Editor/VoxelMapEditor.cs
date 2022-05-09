using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(VoxelMap))]
//[CanEditMultipleObjects]
public class VoxelMapEditor : Editor
{
    Texture2D SolidColorTex(Color c)
    {
        Texture2D t = new Texture2D(16, 16, TextureFormat.RGBA32, false, true);
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                t.SetPixel(x,y,c);
            }
        }
        t.Apply();
        return t;
    }
    Texture2D ResizeTex(Texture2D tex, Vector2 scale)
    {
        if (tex == null)
        {
            Debug.LogError("Texture Cannot Be null");
            return Texture2D.whiteTexture;
        }
        Texture2D o = new Texture2D(Mathf.RoundToInt(tex.width * scale.x), Mathf.RoundToInt(tex.height * scale.y));
        for (int x = 0; x < o.width; x++)
        {
            for (int y = 0; y < o.height; y++)
            {
                o.SetPixel(x,y,tex.GetPixel(Mathf.RoundToInt(x / scale.x), Mathf.RoundToInt( y / scale.y)));
            }
        }
        o.Apply();
        return o;
    }

    GUIStyle selectedStyle;
    Texture2D bgTex;
    static int saves = 0;
    readonly string savePath = "Assets/VoxelMap/Maps";

    List<Texture2D> paletteIcons = new List<Texture2D>();

    bool palettefoldout = false;
    bool foldout = false;

    void OnEnable()
    {
        VoxelMap t = (VoxelMap)target;
        bgTex = SolidColorTex(new Color(0.1f, 0.1f, 0.1f));
        selectedStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState() { background = bgTex },
        };
        SetPaletteIcons(t);

        //I tried to turn off the box collier gizmo, but then collisions didnt work.
        //Debug.Log("Enable");
        //SceneView.lastActiveSceneView.drawGizmos = false;
    }
    private void OnDisable()
    {
        //Debug.Log("Disable");
        //SceneView.lastActiveSceneView.drawGizmos = true;
    }

    public void SetPaletteIcons(VoxelMap t)
    {
        paletteIcons.Clear();
        for (int i = 0; i < t.palette.Count; i++)
        {
            Texture2D tex = AssetPreview.GetAssetPreview(t.palette[i]);
            tex = ResizeTex(tex, Vector2.one * 0.5f);
            paletteIcons.Add(tex);
        }

    }

    public override void OnInspectorGUI()
    {
        VoxelMap t = (VoxelMap)target;
        serializedObject.Update();
        //EditorGUILayout.LabelField("")
        EditorGUILayout.LabelField("Recreate or Delete all voxels (doesn't delete any data)");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Repaint"))//, GUILayout.MinWidth(320), GUILayout.Width(320)
        {
            t.RepaintVoxels();
        }
        if (GUILayout.Button("Destroy"))
        {
            t.DestroyAllChildren(t.transform);
        }
        EditorGUILayout.EndHorizontal();


        //Save
        EditorGUILayout.LabelField("Save/Load the current map to an asset");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("save"), true);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save to Asset"))
        {
            if (!AssetDatabase.IsValidFolder(savePath))
                AssetDatabase.CreateFolder("Assets", "VoxelMap");
            if (!AssetDatabase.IsValidFolder(savePath))
                AssetDatabase.CreateFolder("VoxelMap", "Maps");

            VoxelSO so = ScriptableObject.CreateInstance<VoxelSO>();
            so.voxels = t.voxels;
            saves++;
            string path = savePath + "/" + "VoxelMap" + saves + ".asset";
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = so;
            t.save = so;
        }
        if (GUILayout.Button("Load from Asset"))
        {
            if(t.save != null)
            {
                t.voxels = t.save.voxels;
            }
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.LabelField("");
        palettefoldout = EditorGUILayout.Foldout(palettefoldout, "Palette");
        if (palettefoldout)
        {
            EditorGUILayout.LabelField("Left Click to Remove, Shift-Left Click to Add");
            for (int i = 0; i < t.palette.Count; i++)
            {
                Texture2D tex = paletteIcons[i];
                if (i == t.brush)
                {
                    if (GUILayout.Button(tex, selectedStyle)) { t.brush = i; }
                }
                else
                {
                    if (GUILayout.Button(tex)) { t.brush = i; }
                }
            }
        }


        foldout = EditorGUILayout.Foldout(foldout, "Settings");
        if (foldout)
        {
            EditorGUILayout.LabelField("Palette");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("palette"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("brush"), true);

            EditorGUILayout.LabelField(" ");
            EditorGUILayout.LabelField("Data");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("voxels"), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("created"), true);
        }
        serializedObject.ApplyModifiedProperties();

        //DrawDefaultInspector();
    }


    public void OnSceneGUI()
    {
        VoxelMap t = (VoxelMap)target;

        Tools.current = Tool.View;
        EditorGUI.BeginChangeCheck();
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            //https://answers.unity.com/questions/1260602/how-to-get-mouse-click-world-position-in-the-scene.html
            Vector3 screenPosition = Event.current.mousePosition;
            screenPosition.y = Camera.current.pixelHeight - screenPosition.y;
            Ray ray = Camera.current.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // do stuff here using hit.point
                if (e.shift)
                {
                    t.PaintVoxel(RoundInt(hit.point + hit.normal * 0.5f));
                    Undo.RecordObject(t, "Add Voxel");
                }
                else
                {
                    t.RemoveVoxel(RoundInt(hit.point - hit.normal * 0.5f));
                    Undo.RecordObject(t, "Remove Voxel");
                }

                // tell the event system you consumed the click
                Event.current.Use();
                Selection.activeGameObject = t.gameObject;
            }
        }
        
        Selection.activeGameObject = t.gameObject;
    }

    public int CheckCubes(Ray ray, List<Vector3Int> exist)
    {
        int o = -1;
        //create your own raycast so we dont use layers. check to see for the first voxel area we would reach.
        List<int> indecies = IdentityList(exist.Count);
        List<int> sortedIndecies = indecies.OrderBy(i => Vector3.Distance(exist[i], ray.origin)).ToList();
        for (int i = 0; i < sortedIndecies.Count; i++)
        {
            if(Mathf.Sin(Mathf.Acos(Vector3.Dot(ray.direction, exist[sortedIndecies[i]] - ray.origin))) * Vector3.Distance(ray.origin, exist[sortedIndecies[i]]) < 0.5f)
            {
                return o;
            }
        }
        return -1;
        /*
         * acos(vector3.dot(dir, pos - pos)) < 0.5f
        */
    }
    public List<int> IdentityList(int len)
    {
        List<int> o = new List<int>();
        for (int i = 0; i < len; i++)
        {
            o.Add(i);
        }
        return o;
    }
    public Vector3Int RoundInt(Vector3 v)
    {
        return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }
}

/// <summary>
/// A struct for holding a position and direction in Vector3int s
/// Pretty much the same as a Ray
/// </summary>
[SerializeField]
public struct PosDirInt
{
    public Vector3Int position;
    public Vector3Int direction;

    public PosDirInt(Vector3Int pos, Vector3Int dir)
    {
        position = pos;
        direction = dir;
    }
    public PosDirInt[] AnyDir()
    {
        Vector3Int[] d = { Vector3Int.right, Vector3Int.up, Vector3Int.forward, Vector3Int.left, Vector3Int.down, Vector3Int.back };
        PosDirInt[] o = new PosDirInt[6];
        for (int i = 0; i < o.Length; i++)
            o[i] = new PosDirInt(position, d[i]);
        return o;
    }
    public static PosDirInt[] AnyDir(Vector3Int pos)
    {
        return new PosDirInt(pos, Vector3Int.zero).AnyDir();
    }
}