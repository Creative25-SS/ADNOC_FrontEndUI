using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using VInspector;
using TJ.Networking;

public class VoiceButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI References")]
    public Image buttonBackground;
    public CanvasGroup recordIcon;
    public CanvasGroup gearIcon;
    
    [Header("Colors")]
    public Color idleColor = Color.gray;
    public Color recordColor = new Color(0.8f, 0.2f, 0.2f);
    public Color activeRecordColor = new Color(1f, 0.3f, 0.3f);
    public Color thinkingColor = new Color(1f, 0.6f, 0.2f);
    
    [Header("Animation")]
    public float fadeDuration = 0.3f;
    
    [Header("References")]
    public WaveformController waveformController;
    public UDPServer udpServer;
    
    void Start()
    {
        recordIcon.alpha = 1;
        gearIcon.alpha = 0;
        
        UDPServer.OnDatagramReceived += HandleUDPMessage;
    }
    
    void OnDestroy()
    {
        UDPServer.OnDatagramReceived -= HandleUDPMessage;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        udpServer.SendMsg("START_RECORDING");
        SetActiveRecordingState();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        udpServer.SendMsg("STOP_RECORDING");
        SetThinkingState();
    }
    
    private void HandleUDPMessage(string senderIp, int senderPort, string message)
    {
        switch (message)
        {
            case "avatar_talking":
                SetRecordState();
                waveformController.StartTalking();
                break;
            case "avatar_idle":
                waveformController.StopTalking();
                break;
            case "processing_failed":
            case "error":
                SetRecordState();
                waveformController.StopTalking();
                break;
        }
    }
    
    [Button]
    public void SetRecordState()
    {
        buttonBackground.DOColor(recordColor, fadeDuration);
        recordIcon.DOFade(1f, fadeDuration);
        gearIcon.DOFade(0f, fadeDuration);
        buttonBackground.raycastTarget = true;
    }
    
    [Button]
    public void SetActiveRecordingState()
    {
        buttonBackground.DOColor(activeRecordColor, fadeDuration);
    }
    
    [Button]
    public void SetThinkingState()
    {
        buttonBackground.DOColor(thinkingColor, fadeDuration);
        recordIcon.DOFade(0f, fadeDuration);
        gearIcon.DOFade(1f, fadeDuration);
        buttonBackground.raycastTarget = false;
    }
    
    [Button]
    public void SetIdleState()
    {
        buttonBackground.DOColor(idleColor, fadeDuration);
        recordIcon.DOFade(0f, fadeDuration);
        gearIcon.DOFade(0f, fadeDuration);
        buttonBackground.raycastTarget = true;
    }
}