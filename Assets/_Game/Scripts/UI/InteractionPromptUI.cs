using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private WorldPromptFollower promptFollower;
    [SerializeField] private PromptPopAnimation popAnimation;

    [Header("Digitação")]
    [SerializeField] private bool revelarTextoGradualmente;
    [SerializeField] private float caracteresPorSegundo = 32f;

    [Header("Tamanho automático")]
    [SerializeField] private bool ajustarTamanhoAoTexto;
    [SerializeField] private RectTransform promptRect;

    [SerializeField] private float larguraMinima = 170f;
    [SerializeField] private float larguraMaxima = 440f;
    [SerializeField] private float alturaMinima = 48f;

    [SerializeField] private float paddingHorizontal = 18f;
    [SerializeField] private float paddingVertical = 10f;

    private Coroutine digitacaoAtual;

    public bool EstaVisivel { get; private set; }

    private void Awake()
    {
        if (promptText == null)
            promptText = GetComponentInChildren<TMP_Text>();

        if (promptFollower == null)
            promptFollower = GetComponent<WorldPromptFollower>();

        if (popAnimation == null)
            popAnimation = GetComponent<PromptPopAnimation>();

        if (promptRect == null)
            promptRect = GetComponent<RectTransform>();

        if (promptText != null)
            promptText.maxVisibleCharacters = int.MaxValue;
    }

    public void Mostrar(Transform alvo, string texto)
    {
        if (alvo == null)
        {
            Debug.LogWarning(
                "O alvo do prompt não foi definido."
            );

            return;
        }

        PararDigitacao();

        if (promptText != null)
        {
            promptText.text = texto;
            promptText.maxVisibleCharacters = int.MaxValue;
        }

        if (ajustarTamanhoAoTexto)
            AjustarTamanho(texto);

        if (promptFollower != null)
            promptFollower.DefinirAlvo(alvo);

        if (popAnimation != null)
            popAnimation.Mostrar();

        EstaVisivel = true;

        if (revelarTextoGradualmente &&
            promptText != null)
        {
            digitacaoAtual = StartCoroutine(
                RevelarTexto()
            );
        }
    }

    public void Esconder()
    {
        PararDigitacao();

        if (promptText != null)
            promptText.maxVisibleCharacters = int.MaxValue;

        if (popAnimation != null)
            popAnimation.Esconder();

        EstaVisivel = false;
    }

    public void EsconderImediatamente()
    {
        PararDigitacao();

        if (promptText != null)
            promptText.maxVisibleCharacters = int.MaxValue;

        if (popAnimation != null)
            popAnimation.OcultarImediatamente();

        if (promptFollower != null)
            promptFollower.LimparAlvo();

        EstaVisivel = false;
    }

    private IEnumerator RevelarTexto()
    {
        promptText.ForceMeshUpdate();

        int quantidadeDeCaracteres =
            promptText.textInfo.characterCount;

        promptText.maxVisibleCharacters = 0;

        float caracteresVisiveis = 0f;

        while (promptText.maxVisibleCharacters <
               quantidadeDeCaracteres)
        {
            caracteresVisiveis +=
                caracteresPorSegundo *
                Time.unscaledDeltaTime;

            promptText.maxVisibleCharacters =
                Mathf.Min(
                    quantidadeDeCaracteres,
                    Mathf.FloorToInt(
                        caracteresVisiveis
                    )
                );

            yield return null;
        }

        promptText.maxVisibleCharacters =
            int.MaxValue;

        digitacaoAtual = null;
    }

    private void AjustarTamanho(string texto)
    {
        if (promptText == null ||
            promptRect == null ||
            string.IsNullOrWhiteSpace(texto))
        {
            return;
        }

        float larguraDisponivelParaTexto =
            Mathf.Max(
                1f,
                larguraMaxima -
                paddingHorizontal * 2f
            );

        Vector2 tamanhoPreferido =
            promptText.GetPreferredValues(
                texto,
                larguraDisponivelParaTexto,
                Mathf.Infinity
            );

        float larguraFinal = Mathf.Clamp(
            tamanhoPreferido.x +
            paddingHorizontal * 2f,
            larguraMinima,
            larguraMaxima
        );

        // Recalcula a altura usando a largura final,
        // permitindo quebra de linha.
        float larguraRealDoTexto =
            Mathf.Max(
                1f,
                larguraFinal -
                paddingHorizontal * 2f
            );

        tamanhoPreferido =
            promptText.GetPreferredValues(
                texto,
                larguraRealDoTexto,
                Mathf.Infinity
            );

        float alturaFinal = Mathf.Max(
            alturaMinima,
            tamanhoPreferido.y +
            paddingVertical * 2f
        );

        promptRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            larguraFinal
        );

        promptRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            alturaFinal
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            promptRect
        );
    }

    private void PararDigitacao()
    {
        if (digitacaoAtual == null)
            return;

        StopCoroutine(digitacaoAtual);
        digitacaoAtual = null;
    }
}