using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PromptPopAnimation : MonoBehaviour
{
    [Header("Entrada")]
    [SerializeField] private float duracaoEntrada = 0.18f;
    [SerializeField] private float escalaInicial = 0.7f;
    [SerializeField] private float escalaOvershoot = 1.08f;

    [Header("Saída")]
    [SerializeField] private float duracaoSaida = 0.12f;
    [SerializeField] private float escalaFinal = 0.85f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 escalaBase;
    private Coroutine animacaoAtual;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        escalaBase = rectTransform.localScale;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        OcultarImediatamente();
    }

    public void Mostrar()
    {
        PararAnimacaoAtual();
        animacaoAtual = StartCoroutine(AnimarEntrada());
    }

    public void Esconder()
    {
        PararAnimacaoAtual();
        animacaoAtual = StartCoroutine(AnimarSaida());
    }

    public void OcultarImediatamente()
    {
        PararAnimacaoAtual();

        canvasGroup.alpha = 0f;
        rectTransform.localScale = escalaBase * escalaInicial;
    }

    private void PararAnimacaoAtual()
    {
        if (animacaoAtual == null)
            return;

        StopCoroutine(animacaoAtual);
        animacaoAtual = null;
    }

    private IEnumerator AnimarEntrada()
    {
        canvasGroup.alpha = 0f;
        rectTransform.localScale = escalaBase * escalaInicial;

        float tempo = 0f;

        while (tempo < duracaoEntrada)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoEntrada
            );

            float suavizado =
                1f - Mathf.Pow(1f - progresso, 3f);

            canvasGroup.alpha = progresso;

            float escala = Mathf.Lerp(
                escalaInicial,
                escalaOvershoot,
                suavizado
            );

            rectTransform.localScale = escalaBase * escala;

            yield return null;
        }

        float duracaoAjuste = duracaoEntrada * 0.45f;
        tempo = 0f;

        while (tempo < duracaoAjuste)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoAjuste
            );

            float escala = Mathf.Lerp(
                escalaOvershoot,
                1f,
                Mathf.SmoothStep(0f, 1f, progresso)
            );

            rectTransform.localScale = escalaBase * escala;

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.localScale = escalaBase;
        animacaoAtual = null;
    }

    private IEnumerator AnimarSaida()
    {
        float alphaInicial = canvasGroup.alpha;
        Vector3 escalaAtual = rectTransform.localScale;

        float tempo = 0f;

        while (tempo < duracaoSaida)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoSaida
            );

            canvasGroup.alpha = Mathf.Lerp(
                alphaInicial,
                0f,
                progresso
            );

            rectTransform.localScale = Vector3.Lerp(
                escalaAtual,
                escalaBase * escalaFinal,
                progresso
            );

            yield return null;
        }

        canvasGroup.alpha = 0f;
        rectTransform.localScale = escalaBase * escalaInicial;
        animacaoAtual = null;
    }
}