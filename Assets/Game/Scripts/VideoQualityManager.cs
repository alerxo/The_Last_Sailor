using UnityEngine;
using UnityEngine.Assertions;

public class VideoQualityManager : MonoBehaviour
{
    public static VideoQualityManager Instance { get; private set; }

    public VideoQuality VideoQuality { get; private set; }

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;
    }

    public void SetVideoQuality(VideoQuality _videoQuality)
    {
        VideoQuality = _videoQuality;

        switch (VideoQuality)
        {
            case VideoQuality.Low:
                SetLow();
                break;

            case VideoQuality.Medium:
                SetMedium();
                break;

            case VideoQuality.High:
                SetHigh();
                break;

        }
    }

    private void SetLow()
    {
        
    }

    private void SetMedium()
    {
        
    }

    private void SetHigh()
    {
        
    }
}

public enum VideoQuality
{
    Low,
    Medium,
    High
}
