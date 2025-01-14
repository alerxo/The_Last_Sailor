using UnityEngine;
using UnityEngine.Assertions;

public class VideoQualityManager : MonoBehaviour
{
    public static VideoQualityManager Instance { get; private set; }

    public VideoQuality VideoQuality { get; private set; } = VideoQuality.High;

    private void Awake()
    {
        Assert.IsNull(Instance);
        Instance = this;

        if (PlayerPrefs.HasKey("VideoQuality"))
        {
            SetVideoQuality((VideoQuality)PlayerPrefs.GetInt("VideoQuality"));
        }
    }

    public void SetVideoQuality(VideoQuality _videoQuality)
    {
        VideoQuality = _videoQuality;

        switch (VideoQuality)
        {
            case VideoQuality.Low:
                SetLow();
                break;

            case VideoQuality.High:
                SetHigh();
                break;

        }

        PlayerPrefs.SetInt("VideoQuality", (int)VideoQuality);
    }

    private void SetLow()
    {
        QualitySettings.SetQualityLevel(2);
    }

    private void SetHigh()
    {
        QualitySettings.SetQualityLevel(0);
    }
}

public enum VideoQuality
{
    High,
    Low,
}
