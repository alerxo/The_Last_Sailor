#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BouyancyPointGenerator : MonoBehaviour
{
    [SerializeField] private Transform start, end;
    [SerializeField] private GameObject source;
    [SerializeField] private BuoyancyPoints target;

    public void Generate()
    {
        List<Vector3> points = new();

        for (float x = start.position.x; x <= end.position.x; x += Buoyancy.POINT_SCALE.x)
        {
            for (float y = start.position.y; y <= end.position.y; y += Buoyancy.POINT_SCALE.y)
            {
                for (float z = start.position.z; z <= end.position.z; z += Buoyancy.POINT_SCALE.z)
                {
                    Vector3 position = new(x, y, z);

                    if (Physics.CheckBox(position, Buoyancy.POINT_SCALE / 2, Quaternion.identity))
                    {
                        points.Add(position);

                        DebugUtil.DrawBox(position, Quaternion.identity, Buoyancy.POINT_SCALE * 0.99f, Color.green, 1);
                    }
                }
            }
        }

        target.Values = points.ToArray();
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }
}

#endif