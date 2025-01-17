using UnityEngine;
using UnityEngine.VFX;

public class VFXLoader : MonoBehaviour
{
    [SerializeField] private VisualEffectAsset[] effects;


    private void Start()
    {
        foreach (VisualEffectAsset effectAsset in effects)
        {
            VisualEffect effect = new GameObject(effectAsset.name).AddComponent<VisualEffect>();
            effect.transform.position = transform.position;
            effect.visualEffectAsset = effectAsset;
            effect.Reinit();
            Destroy(effect.gameObject);
        }
    }
}