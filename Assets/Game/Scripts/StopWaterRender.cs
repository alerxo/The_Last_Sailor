using UnityEngine;

public class StopWaterRender : MonoBehaviour
{
    private Camera cam;
    private int defaultCullingMask;

    void Awake()
    {
        cam = Camera.main;
        defaultCullingMask = cam.cullingMask;
    }

    void OnTriggerEnter(Collider _collision)
    {
        if (_collision.CompareTag("Player"))
        {
            if (cam != null)
            {
                cam.cullingMask &= ~(1 << LayerMask.NameToLayer("Water"));
            }
        }
    }
    private void OnTriggerExit(Collider _collision)
    {
        if (_collision.CompareTag("Player"))
        {
            if (cam != null)
            {
                cam.cullingMask = defaultCullingMask;
            }
        }
    }
}
