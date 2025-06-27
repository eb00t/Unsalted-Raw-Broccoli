using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ItemPickupHandler : MonoBehaviour
{
    public static ItemPickupHandler Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI text, betweenTxtObj;
    [SerializeField] private UpdateButton updateButton1, updateButton2;
    [SerializeField] private DataHolder dataHolder;
    
    private Image[] _images;
    private Sequence _flashSeq, _flashEndSeq;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one ItemPickupHandler script in the scene.");
        }

        Instance = this;
    }

    private void Start()
    {
        _images = rectTransform.GetComponentsInChildren<Image>();
    }

    public void TogglePrompt(string promptText, bool toggle, ControlsManager.ButtonType button, string betweenText, ControlsManager.ButtonType? button2, bool forceTween)
    {
        if (toggle)
        {
            if (rectTransform.anchoredPosition.y < 0 || forceTween) // animate
            {
                rectTransform.anchoredPosition = new Vector3(0, 100, 0);
                rectTransform.localScale = new Vector3(0, 1, 1);
                rectTransform.DOScale(new Vector3(1, 1, 1), .1f).SetUpdate(true);

                if (forceTween)
                {
                    _flashSeq?.Kill();
                    _flashEndSeq?.Kill();
                    
                    _flashSeq = DOTween.Sequence().SetUpdate(true);
                    
                    foreach (var img in _images)
                    {
                        if (img.CompareTag("Animate"))
                        {
                            _flashSeq.Join(img.DOColor(Color.red, 0.2f));
                        }
                    }
                    
                    _flashSeq.OnComplete(() =>
                    {
                        _flashEndSeq = DOTween.Sequence().SetUpdate(true);
                        
                        foreach (var img in _images)
                        {
                            if (img.CompareTag("Animate"))
                            {
                                _flashEndSeq.Join(img.DOColor(new Color(0, 0.6352941f, 1), 2f));
                            }
                        }
                    });
                }
            }
            
            text.text = promptText;
            updateButton1.button = button;

            if (button2.HasValue)
            {
                updateButton2.button = button2.Value;
                betweenTxtObj.text = betweenText;
                betweenTxtObj.gameObject.SetActive(true);
                updateButton2.image.gameObject.SetActive(true);
                updateButton2.text.gameObject.SetActive(true);
                updateButton2.enabled = true;
            }
            else
            {
                updateButton2.image.gameObject.SetActive(false);
                updateButton2.text.gameObject.SetActive(false);
                betweenTxtObj.gameObject.SetActive(false);
                updateButton2.enabled = false;
            }
        }
        else
        {
            if (rectTransform.anchoredPosition.y > 0 || forceTween) // animate
            {
                text.text = "";
                rectTransform.DOScale(new Vector3(0, 1, 1), .1f).SetUpdate(true).OnComplete(() =>
                {
                    rectTransform.anchoredPosition = new Vector3(0, -100, 0);
                });
            }
        }
    }
}
