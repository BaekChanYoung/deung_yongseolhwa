using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStand : MonoBehaviour
{
    [Header("Character Settings")]
    [SerializeField] private Image characterImage;
    [SerializeField] private string characterName;

    [Header("Expressions")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite surprisedSprite;
    [SerializeField] private Sprite sadSprite;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float moveDistance = 50f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalPosition = rectTransform.anchoredPosition;
        Hide();
    }

    public void Show(string position)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn(position));
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void SetExpression(string expression)
    {
        Sprite targetSprite = expression.ToLower() switch
        {
            "normal" => normalSprite,
            "happy" => happySprite,
            "surprised" => surprisedSprite,
            "sad" => sadSprite,
            _ => normalSprite
        };

        if (targetSprite != null)
        {
            characterImage.sprite = targetSprite;
        }
    }

    private IEnumerator FadeIn(string position)
    {
        // 위치 설정
        Vector2 startPos = originalPosition;

        switch (position.ToLower())
        {
            case "left":
                startPos = new Vector2(originalPosition.x - moveDistance, originalPosition.y);
                break;
            case "right":
                startPos = new Vector2(originalPosition.x + moveDistance, originalPosition.y);
                break;
        }

        rectTransform.anchoredPosition = startPos;

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = originalPosition;
    }
}
