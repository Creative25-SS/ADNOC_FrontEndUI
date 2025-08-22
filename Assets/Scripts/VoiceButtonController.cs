using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using VInspector;
using TJ.Networking;

public class VoiceButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    // Event to notify other systems about loading state changes
    public static event System.Action<bool> OnLoadingStateChanged;
    [Header("UI References")]
    public Image buttonBackground;
    public GameObject inactiveMic;
    public GameObject activeMic;
    public CanvasGroup gearIcon;

    [Header("Animation")]
    public float fadeDuration = 0.3f;


    [Header("References")]
    public UDPServer udpServer;

    private Image buttonImage;
    private bool isUserPressed = false; // Track if user is currently pressing the button
    private bool isLoading = false; // Track if we're in loading/thinking state

    void Start()
    {
        buttonImage = buttonBackground;

        // Initialize mic states
        if (inactiveMic != null)
        {
            inactiveMic.GetComponent<CanvasGroup>().alpha = 1f;
        }

        if (activeMic != null)
        {
            activeMic.GetComponent<CanvasGroup>().alpha = 0f;
        }

        if (gearIcon != null)
            gearIcon.alpha = 0;

        UDPServer.OnDatagramReceived += HandleUDPMessage;
    }

    void OnDestroy()
    {
        UDPServer.OnDatagramReceived -= HandleUDPMessage;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Don't allow interaction during loading/thinking state
        if (isLoading) return;

        isUserPressed = true; // Mark that user is pressing

        // Animate to active mic
        AnimateToActiveMic();

        udpServer.SendMsg("START_RECORDING");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Don't allow interaction during loading/thinking state
        if (isLoading) return;

        isUserPressed = false; // Mark that user is no longer pressing

        udpServer.SendMsg("STOP_RECORDING");
        SetThinkingState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // If user drags off the button, treat it as release
        if (isUserPressed && !isLoading)
        {
            isUserPressed = false;
            udpServer.SendMsg("STOP_RECORDING");
            SetThinkingState();
        }
    }

    private void HandleUDPMessage(string senderIp, int senderPort, string message)
    {
        switch (message)
        {
            case "avatar_talking":
                SetRecordState();
                break;
            case "avatar_idle":
                // Avatar finished talking, return to record state
                SetRecordState();
                break;
            case "processing_failed":
            case "error":
                SetRecordState();
                break;
            case "recording_started":
                Debug.Log("Recording Started");
                break;
            case "processing_complete":
                Debug.Log("Processing Complete");
                break;
            case "processing_started":
                Debug.Log("Processing Started");
                break;
        }
    }

    [Button]
    public void SetRecordState()
    {
        isLoading = false; // Mark that we're no longer loading

        // Only animate to inactive mic if user is not currently pressing the button
        if (!isUserPressed)
        {
            AnimateToInactiveMic();
        }

        if (gearIcon != null)
            gearIcon.DOFade(0f, fadeDuration);
        buttonBackground.raycastTarget = true;

        // Notify that loading has finished - language button can be enabled
        OnLoadingStateChanged?.Invoke(false);
    }


    [Button]
    public void SetThinkingState()
    {
        isLoading = true; // Mark that we're now loading

        // Hide both mics and show gear
        if (inactiveMic != null)
        {
            inactiveMic.GetComponent<CanvasGroup>().DOFade(0f, fadeDuration);
        }
        if (activeMic != null)
        {
            activeMic.GetComponent<CanvasGroup>().DOFade(0f, fadeDuration);
        }
        if (gearIcon != null)
            gearIcon.DOFade(1f, fadeDuration);
        buttonBackground.raycastTarget = false;

        // Notify that loading has started - language button should be disabled
        OnLoadingStateChanged?.Invoke(true);
    }



    private void AnimateToActiveMic()
    {
        if (inactiveMic != null && activeMic != null)
        {
            // Fade out inactive mic
            inactiveMic.GetComponent<CanvasGroup>().DOFade(0f, fadeDuration);

            // Fade in active mic
            var activeCanvasGroup = activeMic.GetComponent<CanvasGroup>();
            activeCanvasGroup.DOFade(1f, fadeDuration);
        }
    }

    private void AnimateToInactiveMic()
    {
        if (inactiveMic != null && activeMic != null)
        {
            // Fade in inactive mic
            inactiveMic.GetComponent<CanvasGroup>().DOFade(1f, fadeDuration);

            // Fade out active mic
            var activeCanvasGroup = activeMic.GetComponent<CanvasGroup>();
            activeCanvasGroup.DOFade(0f, fadeDuration);
        }
    }
}