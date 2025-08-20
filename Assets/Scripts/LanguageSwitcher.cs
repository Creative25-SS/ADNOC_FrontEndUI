using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TJ.Networking;

public class LanguageSwitcher : MonoBehaviour, IPointerClickHandler
{
    public Image englishImage;
    public Image arabicImage;
    public UDPServer udpServer;
    public float fadeDuration = 0.3f;
    
    private bool isEnglish = true;
    
    void Start()
    {
        englishImage.color = new Color(1, 1, 1, 1);
        arabicImage.color = new Color(1, 1, 1, 0);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        isEnglish = !isEnglish;
        
        if (isEnglish)
        {
            englishImage.DOFade(1f, fadeDuration);
            arabicImage.DOFade(0f, fadeDuration);
            udpServer.SendMsg("SWITCH_LANGUAGE_EN");
        }
        else
        {
            englishImage.DOFade(0f, fadeDuration);
            arabicImage.DOFade(1f, fadeDuration);
            udpServer.SendMsg("SWITCH_LANGUAGE_AR");
        }
    }
}