using UnityEngine;

public class StopWaterRender : MonoBehaviour
{
    Camera camera;
    int defaultCullingMask;

    void Awake()
    {
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        defaultCullingMask = camera.cullingMask;
        
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Player")
        { 
            if(camera!=null)
            camera.cullingMask &= ~(1 << LayerMask.NameToLayer("Water"));
        }

    }
    private void OnTriggerExit(Collider col)
    {
        if(col.tag == "Player")
        {
            if (camera != null)
            camera.cullingMask = defaultCullingMask;
            
        }
    }    
}
