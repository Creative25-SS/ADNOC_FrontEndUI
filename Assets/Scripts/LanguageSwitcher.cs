using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TJ.Networking;

public class LanguageSwitcher : MonoBehaviour
{
    public Image englishImage;
    public Image arabicImage;
    public Image englishTitleImage;
    public Image arabicTitleImage;
    public UDPServer udpServer;
    public float fadeDuration = 0.3f;
    
    [Header("Button References")]
    public Button englishButton;
    public Button arabicButton;
    
    private bool isEnglish = true;
    private bool isInteractable = true;
    private bool isRecording = false;
    private bool isLoading = false;
    
    void Start()
    {
        englishImage.color = new Color(1, 1, 1, 1);
        arabicImage.color = new Color(1, 1, 1, 0);
        englishTitleImage.color = new Color(1, 1, 1, 1);
        arabicTitleImage.color = new Color(1, 1, 1, 0);
        
        // Auto-setup button events
        if (englishButton != null)
            englishButton.onClick.AddListener(SwitchToEnglish);
        if (arabicButton != null)
            arabicButton.onClick.AddListener(SwitchToArabic);
        
        // Subscribe to voice button state changes
        VoiceButtonController.OnLoadingStateChanged += SetLoadingState;
        VoiceButtonController.OnRecordingStateChanged += SetRecordingState;
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (englishButton != null)
            englishButton.onClick.RemoveListener(SwitchToEnglish);
        if (arabicButton != null)
            arabicButton.onClick.RemoveListener(SwitchToArabic);
            
        // Unsubscribe to prevent memory leaks
        VoiceButtonController.OnLoadingStateChanged -= SetLoadingState;
        VoiceButtonController.OnRecordingStateChanged -= SetRecordingState;
    }
    
    public void SwitchToEnglish()
    {
        // Guard against clicks when disabled
        if (!isInteractable) return;
        
        // Don't switch if already English
        if (isEnglish) return;
        
        isEnglish = true;
        englishImage.DOFade(1f, fadeDuration);
        arabicImage.DOFade(0f, fadeDuration);
        englishTitleImage.DOFade(1f, fadeDuration);
        arabicTitleImage.DOFade(0f, fadeDuration);
        udpServer.SendMsg("SWITCH_LANGUAGE_EN");
    }
    
    public void SwitchToArabic()
    {
        // Guard against clicks when disabled
        if (!isInteractable) return;
        
        // Don't switch if already Arabic
        if (!isEnglish) return;
        
        isEnglish = false;
        englishImage.DOFade(0f, fadeDuration);
        arabicImage.DOFade(1f, fadeDuration);
        englishTitleImage.DOFade(0f, fadeDuration);
        arabicTitleImage.DOFade(1f, fadeDuration);
        udpServer.SendMsg("SWITCH_LANGUAGE_AR");
    }
    
    private void SetLoadingState(bool loading)
    {
        isLoading = loading;
        UpdateInteractableState();
    }
    
    private void SetRecordingState(bool recording)
    {
        isRecording = recording;
        UpdateInteractableState();
    }
    
    private void UpdateInteractableState()
    {
        // Disable language switching during recording OR loading
        isInteractable = !isRecording && !isLoading;
        
        // Control English button - get CanvasGroup from button itself
        if (englishButton != null)
        {
            var englishCG = englishButton.GetComponent<CanvasGroup>();
            if (englishCG != null)
            {
                englishCG.interactable = isInteractable;
                englishCG.blocksRaycasts = isInteractable; // Block raycasts when disabled
            }
        }
        
        // Control Arabic button - get CanvasGroup from button itself
        if (arabicButton != null)
        {
            var arabicCG = arabicButton.GetComponent<CanvasGroup>();
            if (arabicCG != null)
            {
                arabicCG.interactable = isInteractable;
                arabicCG.blocksRaycasts = isInteractable; // Block raycasts when disabled
            }
        }
        
        // Visual feedback - fade the actual language images
        float targetAlpha = isInteractable ? (isEnglish ? 1f : 0f) : 0.5f;
        if (englishImage != null)
        {
            if (isInteractable)
            {
                // When enabled, restore proper state (visible if English, hidden if Arabic)
                englishImage.DOFade(isEnglish ? 1f : 0f, fadeDuration);
            }
            else
            {
                // When disabled, show dimmed version of current state
                float currentAlpha = englishImage.color.a;
                if (currentAlpha > 0) // Only fade if currently visible
                    englishImage.DOFade(0.5f, fadeDuration);
            }
        }
        
        if (arabicImage != null)
        {
            if (isInteractable)
            {
                // When enabled, restore proper state (visible if Arabic, hidden if English)
                arabicImage.DOFade(isEnglish ? 0f : 1f, fadeDuration);
            }
            else
            {
                // When disabled, show dimmed version of current state
                float currentAlpha = arabicImage.color.a;
                if (currentAlpha > 0) // Only fade if currently visible
                    arabicImage.DOFade(0.5f, fadeDuration);
            }
        }
    }
}