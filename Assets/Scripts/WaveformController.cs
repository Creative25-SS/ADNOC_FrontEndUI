using UnityEngine;
using VInspector;

public class WaveformController : MonoBehaviour
{
    [Header("Material Reference")]
    public Material waveformMaterial;
    
    [Header("Talk Animation")]
    public bool isTalking = false;
    
    [Header("Amplitude Control")]
    [Range(0.0f, 1.0f)]
    public float minAmplitude = 0.1f;
    
    [Range(0.0f, 1.0f)]
    public float maxAmplitude = 1.0f;
    
    [Range(0.1f, 5.0f)]
    public float simulationSpeed = 2.0f;
    
    [Range(0.1f, 5.0f)]
    public float responseSpeed = 3.0f;
    
    // Private variables
    private float currentAmplitude = 0.0f;
    private float targetAmplitude = 0.0f;
    private float noiseOffset;
    
    void Start()
    {
        if (waveformMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                waveformMaterial = renderer.material;
            }
        }
        
        noiseOffset = Random.Range(0f, 100f);
    }
    
    void Update()
    {
        if (waveformMaterial == null) return;
        
        if (isTalking)
        {
            // Generate pseudo-random audio levels
            float time = Time.time * simulationSpeed + noiseOffset;
            float audioLevel = Mathf.PerlinNoise(time, 0f);
            audioLevel += Mathf.PerlinNoise(time * 2f, 10f) * 0.5f;
            audioLevel += Mathf.PerlinNoise(time * 4f, 20f) * 0.25f;
            audioLevel = Mathf.Clamp01(audioLevel / 1.75f);
            
            // Map to amplitude range
            targetAmplitude = Mathf.Lerp(minAmplitude, maxAmplitude, audioLevel);
        }
        else
        {
            // Return to minimum when not talking
            targetAmplitude = minAmplitude;
        }
        
        // Smooth interpolation
        currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, Time.deltaTime * responseSpeed);
        
        // Update shader
        waveformMaterial.SetFloat("_CurrentAmplitude", currentAmplitude);
    }
    
    [Button]
    public void StartTalking()
    {
        isTalking = true;
    }
    
    [Button]
    public void StopTalking()
    {
        isTalking = false;
    }
}