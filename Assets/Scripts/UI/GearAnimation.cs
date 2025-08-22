using UnityEngine;
using DG.Tweening;

public class GearAnimation : MonoBehaviour
{
    [Header("Gear References")]
    public Transform gear1;
    public Transform gear2;
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 360f; // degrees per second
    
    void Start()
    {
        // Gear 1 - Clockwise
        gear1.DORotate(new Vector3(0, 0, -360), rotationSpeed / 360f, RotateMode.FastBeyond360)
              .SetLoops(-1, LoopType.Incremental)
              .SetEase(Ease.Linear);
        
        // Gear 2 - Counter-clockwise
        gear2.DORotate(new Vector3(0, 0, 360), rotationSpeed / 360f, RotateMode.FastBeyond360)
              .SetLoops(-1, LoopType.Incremental)
              .SetEase(Ease.Linear);
    }
}