using Unity.VisualScripting;
using UnityEngine;

public class RemoveMeshRenderer : MonoBehaviour
{
    private MeshRenderer[] mr;
    private bool toggled = false;

    void Start()
    {
        mr = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer m in mr)
        {
            m.enabled = false;
        }
    }

#if UNITY_EDITOR
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (toggled)
            {
                toggled = false;
            }
            else
            {
                toggled = true;
            }

        }

        if (toggled)
        {
            foreach (MeshRenderer m in mr)
            {
                m.enabled = true;

            }

        }

        else
        {
            foreach (MeshRenderer m in mr)
            {
                m.enabled = false;

            }
        }
    }
#endif
}
