using Unity.VisualScripting;
using UnityEngine;

public class RemoveMeshRenderer : MonoBehaviour
{
    MeshRenderer[] mr;
    bool toggled = false;
    void Start()
    {
        mr = GetComponentsInChildren<MeshRenderer>();

        
        
    }
    void LateUpdate()
    {
        if(Input.GetKeyDown("p"))
        {
            if(toggled)
            {
                toggled = false;
            }
            else
            {
                toggled = true;
            }
            
        }
        if(toggled)
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

}
