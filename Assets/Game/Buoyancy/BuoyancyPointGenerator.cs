#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BouyancyPointGenerator : MonoBehaviour
{
    [SerializeField] private Transform excludeY, excludeZ;
    [SerializeField] private GameObject source;
    [SerializeField] private BuoyancyPoints target;

    public void Generate()
    {
        List<Vector3> points = new();

        Vector3 start = Buoyancy.POINT_SCALE * -10;
        Vector3 end = Buoyancy.POINT_SCALE * 10;

        for (float x = start.x; x <= end.x; x += Buoyancy.POINT_SCALE.x)
        {
            for (float y = start.y; y <= end.y; y += Buoyancy.POINT_SCALE.y)
            {
                if(y > excludeY.transform.position.y)
                {
                    continue;
                }

                for (float z = start.z; z <= end.z; z += Buoyancy.POINT_SCALE.z)
                {
                    if (z > excludeZ.transform.position.z)
                    {
                        continue;
                    }

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