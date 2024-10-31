using UnityEditor;
using UnityEngine;

public class BouyancyPointGenerator : MonoBehaviour
{
    [SerializeField] private Transform start, end;
    [SerializeField] private GameObject target;

    public void Generate()
    {
        GameObject parent = new($"BouyancyPoints_{target.name}");

        for (float x = start.position.x; x <= end.position.x; x += Bouyancy.POINT_SIZE)
        {
            for (float y = start.position.y; y <= end.position.y; y += Bouyancy.POINT_SIZE)
            {
                for (float z = start.position.z; z <= end.position.z; z += Bouyancy.POINT_SIZE)
                {
                    Vector3 position = new(x, y, z);

                    if (Physics.CheckBox(position, Bouyancy.POINT_SCALE / 2, Quaternion.identity))
                    {
                        GameObject point = new($"BouyancyPoint {position}");
                        point.transform.position = position;
                        point.transform.SetParent(parent.transform);

                        DebugUtil.DrawBox(position, Quaternion.identity, Bouyancy.POINT_SCALE * 0.99f, Color.green, 1);
                    }

                    else
                    {
                        DebugUtil.DrawBox(position, Quaternion.identity, Bouyancy.POINT_SCALE * 0.99f, Color.red, 1);
                    }
                }
            }
        }

        PrefabUtility.SaveAsPrefabAsset(parent, $"Assets/Game/Buoyancy/{parent.name}.prefab");
        DestroyImmediate(parent);
    }
}
