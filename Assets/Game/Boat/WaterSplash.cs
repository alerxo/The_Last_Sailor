using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterSplash : MonoBehaviour
{
    [SerializeField] private Transform wave;
    [SerializeField] private float waveSpeed=3f;
    [SerializeField] private float waveFallOffSpeed=1f;
    private WaterDecal[] waterDecals;
    void Awake()
    {
        wave.localScale = new Vector3(0, 1,0);
        waterDecals = GetComponentsInChildren<WaterDecal>();
        // Destroy(gameObject,10); // jag kan behöva hjälp med hur man poolar objekt
    }


    void Update()
    {
        wave.localScale += new Vector3(waveSpeed*Time.deltaTime, math.clamp(-waveFallOffSpeed/10*Time.deltaTime,0f,1f),waveSpeed*Time.deltaTime);
        foreach(WaterDecal wd in waterDecals)
        {
            float ampl = wd.amplitude;
            ampl -= waveFallOffSpeed*Time.deltaTime;
            ampl = Mathf.Clamp(ampl, 0,100);
            wd.amplitude = ampl;
        }
    }
}
