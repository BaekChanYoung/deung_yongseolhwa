using System.Collections;
using TMPro;
using UnityEngine;

public class TypingEffect : MonoBehaviour
{
    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.05f;

    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;
    private string currentText = "";
    private bool isTyping = false;

    public bool IsTyping => isTyping;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    public void StartTyping(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        currentText = text;
        typingCoroutine = StartCoroutine(TypeText());
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            textComponent.text = currentText;
            isTyping = false;
        }
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char letter in currentText)
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}