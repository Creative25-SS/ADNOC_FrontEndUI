using UnityEngine;
using UTool.TabSystem;

[HasTabField]
public class TouchTimeoutManager : MonoBehaviour
{
    [TabField] public float timeoutDuration = 90f;
    public UDPCommandUtility udpCommandUtility;

    private float lastTouchTime;
    private bool hasTimedOut = false;
    private bool hasFirstTouchOccurred = false;

    void Start()
    {
        if (udpCommandUtility == null)
        {
            udpCommandUtility = FindObjectOfType<UDPCommandUtility>();
        }
    }

    void Update()
    {
        // Check for touch input
        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            if (!hasFirstTouchOccurred)
            {
                // First touch ever - start the timeout system
                hasFirstTouchOccurred = true;
                lastTouchTime = Time.time;
            }
            else if (hasTimedOut)
            {
                // Reset everything when touch happens after timeout
                lastTouchTime = Time.time;
                hasTimedOut = false;
            }
            else
            {
                // Update last touch time during normal interaction
                lastTouchTime = Time.time;
            }
        }

        // Check timeout - only after first touch has occurred and we haven't timed out yet
        if (hasFirstTouchOccurred && !hasTimedOut && Time.time - lastTouchTime >= timeoutDuration)
        {
            hasTimedOut = true;
            if (udpCommandUtility != null)
            {
                udpCommandUtility.TriggerResetSession();
            }
        }
    }
}
