using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleInteractable : MonoBehaviour
{
    [Header("Prompt de ação")]
    [SerializeField] private InteractionPromptUI interactionPrompt;
    [SerializeField] private Transform promptAnchor;
    [SerializeField] private string promptTexto = "[Z] INTERAGIR";

    [Header("Pensamento do player")]
    [SerializeField] private InteractionPromptUI thoughtPrompt;
    [SerializeField] private Transform playerThoughtAnchor;

    [TextArea(2, 4)]
    [SerializeField] private string comentario =
        "Então aquele tronco ali veio dessa árvore...";

    [SerializeField] private float duracaoComentario = 3f;

    private bool playerDentroDaArea;
    private bool mostrandoComentario;
    private Coroutine comentarioAtual;

    private void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.EsconderImediatamente();

        if (thoughtPrompt != null)
            thoughtPrompt.EsconderImediatamente();
    }

    private void Update()
    {
        if (!playerDentroDaArea || mostrandoComentario)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.zKey.wasPressedThisFrame)
        {
            comentarioAtual = StartCoroutine(MostrarComentario());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerDentroDaArea = true;

        if (interactionPrompt != null && promptAnchor != null)
        {
            interactionPrompt.Mostrar(promptAnchor, promptTexto);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerDentroDaArea = false;
        mostrandoComentario = false;

        if (comentarioAtual != null)
        {
            StopCoroutine(comentarioAtual);
            comentarioAtual = null;
        }

        if (interactionPrompt != null)
            interactionPrompt.Esconder();

        if (thoughtPrompt != null)
            thoughtPrompt.Esconder();
    }

    private IEnumerator MostrarComentario()
    {
        mostrandoComentario = true;

        if (interactionPrompt != null)
            interactionPrompt.Esconder();

        if (thoughtPrompt != null && playerThoughtAnchor != null)
        {
            thoughtPrompt.Mostrar(playerThoughtAnchor, comentario);
        }

        yield return new WaitForSecondsRealtime(duracaoComentario);

        mostrandoComentario = false;
        comentarioAtual = null;

        if (thoughtPrompt != null)
            thoughtPrompt.Esconder();

        if (playerDentroDaArea &&
            interactionPrompt != null &&
            promptAnchor != null)
        {
            interactionPrompt.Mostrar(promptAnchor, promptTexto);
        }
    }
}