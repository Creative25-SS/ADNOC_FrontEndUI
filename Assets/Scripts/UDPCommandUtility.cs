using UnityEngine;
using UnityEngine.UI;
using TJ.Networking;
using UTool.TabSystem;

[HasTabField]
public class UDPCommandUtility : MonoBehaviour
{
    [Header("Configuration")]
    public UDPServer udpServer;
    
    [Header("Trigger Options")]
    public Button button;
    
    [Header("Spam Protection")]
    public float commandCooldown = 1.5f; // Cooldown in seconds
    
    // Spam protection - track last execution times (null = never used)
    private float? lastInstantSpeechTime = null;
    private float? lastResetSessionTime = null;
    
    void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(TriggerInstantSpeech);
        }
    }
    
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(TriggerInstantSpeech);
        }
    }
    
    [TabButton]
    public void TriggerInstantSpeech()
    {
        // Check cooldown to prevent spam (skip check if never used before)
        if (lastInstantSpeechTime.HasValue && Time.time - lastInstantSpeechTime.Value < commandCooldown)
        {
            float remainingTime = commandCooldown - (Time.time - lastInstantSpeechTime.Value);
            Debug.Log($"Instant Speech on cooldown. Wait {remainingTime:F1}s");
            return;
        }
        
        lastInstantSpeechTime = Time.time;
        
        if (udpServer != null)
        {
            udpServer.SendMsg("INSTANT_SPEECH");
            Debug.Log("Sent INSTANT_SPEECH command");
        }
    }
    
    [TabButton]
    public void TriggerResetSession()
    {
        // Check cooldown to prevent spam (skip check if never used before)
        if (lastResetSessionTime.HasValue && Time.time - lastResetSessionTime.Value < commandCooldown)
        {
            float remainingTime = commandCooldown - (Time.time - lastResetSessionTime.Value);
            Debug.Log($"Reset Session on cooldown. Wait {remainingTime:F1}s");
            return;
        }
        
        lastResetSessionTime = Time.time;
        
        if (udpServer != null)
        {
            udpServer.SendMsg("RESET_SESSION");
            Debug.Log("Sent RESET_SESSION command");
        }
    }
}
