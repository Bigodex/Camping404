using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TentInteractionZone : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private InteractionPromptUI interactionPrompt;
    [SerializeField] private Transform promptAnchor;
    [SerializeField] private string promptTexto = "[F] ENTRAR";
    [SerializeField] private CanvasGroup fadePanel;

    [Header("Realidades")]
    [SerializeField] private GameObject presente;
    [SerializeField] private GameObject passado;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform tentExitPoint;

    [Header("Transição")]
    [SerializeField] private float duracaoFade = 0.5f;

    private CharacterController characterController;

    private bool playerDentroDaArea;
    private bool emTransicao;

    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.EsconderImediatamente();

        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }

        if (player != null)
            characterController = player.GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!playerDentroDaArea || emTransicao)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            StartCoroutine(TrocarRealidadeComFade());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerDentroDaArea = true;

        if (!emTransicao &&
            interactionPrompt != null &&
            promptAnchor != null)
        {
            interactionPrompt.Mostrar(
                promptAnchor,
                promptTexto
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerDentroDaArea = false;

        if (interactionPrompt != null)
            interactionPrompt.Esconder();
    }

    private IEnumerator TrocarRealidadeComFade()
    {
        if (presente == null ||
            passado == null ||
            fadePanel == null ||
            player == null ||
            tentExitPoint == null)
        {
            Debug.LogError(
                "Alguma referência do TentInteractionZone não foi conectada."
            );

            yield break;
        }

        emTransicao = true;
        playerDentroDaArea = false;

        if (interactionPrompt != null)
            interactionPrompt.Esconder();

        fadePanel.blocksRaycasts = true;

        yield return FazerFade(0f, 1f);

        TrocarRealidade();
        MoverPlayerParaSaida();

        yield return FazerFade(1f, 0f);

        fadePanel.blocksRaycasts = false;
        emTransicao = false;
    }

    private void MoverPlayerParaSaida()
    {
        if (characterController != null)
            characterController.enabled = false;

        player.SetPositionAndRotation(
            tentExitPoint.position,
            tentExitPoint.rotation
        );

        if (characterController != null)
            characterController.enabled = true;
    }

    private IEnumerator FazerFade(
        float alphaInicial,
        float alphaFinal
    )
    {
        float tempo = 0f;
        fadePanel.alpha = alphaInicial;

        while (tempo < duracaoFade)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoFade
            );

            fadePanel.alpha = Mathf.Lerp(
                alphaInicial,
                alphaFinal,
                progresso
            );

            yield return null;
        }

        fadePanel.alpha = alphaFinal;
    }

    private void TrocarRealidade()
    {
        bool estaNoPresente = presente.activeSelf;

        presente.SetActive(!estaNoPresente);
        passado.SetActive(estaNoPresente);

        Debug.Log(
            estaNoPresente
                ? "Player viajou para o Passado."
                : "Player voltou para o Presente."
        );
    }
}