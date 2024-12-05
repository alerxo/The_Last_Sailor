using Unity.VisualScripting;
using UnityEngine;

public class PostCombatSoundFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip defeatClip;
    [SerializeField] private AudioClip inconclusiveClip;
    private void Awake()
    {
        CombatManager.OnBattleConcluded += CombatManager_Results;
    }

    private void OnDestroy()
    {
        CombatManager.OnBattleConcluded -= CombatManager_Results;
    }
    private void CombatManager_Results(BattleResult result)
    {
        if (result == BattleResult.Victory)
        {
            audioSource.PlayOneShot(winClip);
        }
        if (result == BattleResult.Defeat)
        {
            audioSource.PlayOneShot(defeatClip);
        }
        if (result == BattleResult.Inconclusive)
        {
            audioSource.PlayOneShot(inconclusiveClip);
        }
    }
}
