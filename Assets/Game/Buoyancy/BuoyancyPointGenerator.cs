#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BouyancyPointGenerator : MonoBehaviour
{
    [SerializeField] private Transform start, end;
    [SerializeField] private GameObject target;

    public void Generate()
    {
        List<Vector3> points = new();

        for (float x = start.position.x; x <= end.position.x; x += Bouyancy.POINT_SIZE)
        {
            for (float y = start.position.y; y <= end.position.y; y += Bouyancy.POINT_SIZE)
            {
                for (float z = start.position.z; z <= end.position.z; z += Bouyancy.POINT_SIZE)
                {
                    Vector3 position = new(x, y, z);

                    if (Physics.CheckBox(position, Bouyancy.POINT_SCALE / 2, Quaternion.identity))
                    {
                        points.Add(position);

                        DebugUtil.DrawBox(position, Quaternion.identity, Bouyancy.POINT_SCALE * 0.99f, Color.green, 1);
                    }
                }
            }
        }

        BuoyancyPoints buoyancyPoints = ScriptableObject.CreateInstance<BuoyancyPoints>();
        buoyancyPoints.Values = points.ToArray();

        AssetDatabase.CreateAsset(buoyancyPoints, $"Assets/Game/Buoyancy/{$"BouyancyPoints_{target.name}"}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

#endif