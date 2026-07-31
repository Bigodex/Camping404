using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupInteractable : MonoBehaviour
{
    [Header("Dados do item")]
    [SerializeField] private string itemId = "apple";
    [SerializeField] private string itemNome = "Maçã";
    [SerializeField] private int quantidade = 1;

    [Header("Linha do tempo")]
    [SerializeField] private AppleTimelineManager timelineManager;

    [SerializeField]
    private AppleTimelineManager.Era era =
        AppleTimelineManager.Era.Presente;

    [Header("Prompt de ação")]
    [SerializeField] private InteractionPromptUI interactionPrompt;
    [SerializeField] private Transform promptAnchor;

    [SerializeField]
    private string promptPegar =
        "[F] PEGAR";

    [SerializeField]
    private string promptInteragir =
        "[Z] INTERAGIR";

    [Header("Pensamento do player")]
    [SerializeField] private InteractionPromptUI thoughtPrompt;
    [SerializeField] private Transform playerThoughtAnchor;

    [TextArea(2, 4)]
    [SerializeField]
    private string comentarioColeta =
        "Uma maçã... ainda parece fresca.";

    [TextArea(2, 4)]
    [SerializeField]
    private string comentarioTemporal =
        "Outra maçã... pensei ter pego ela ontem...";

    [TextArea(2, 4)]
    [SerializeField]
    private string comentarioInventarioCheio =
        "Não tenho espaço para carregar isso.";

    [SerializeField] private float duracaoComentario = 2.5f;

    [Header("Objeto coletável")]
    [SerializeField] private GameObject itemVisual;
    [SerializeField] private Collider interactionCollider;

    private PlayerInventory playerInventory;

    private bool playerDentroDaArea;
    private bool coletado;
    private bool executandoAcao;

    private Coroutine acaoAtual;

    private void Update()
    {
        if (!playerDentroDaArea ||
            coletado ||
            executandoAcao ||
            Keyboard.current == null)
        {
            return;
        }

        if (DeveInspecionarNoPassado())
        {
            if (InteractionInputGate.TryConsumeZ())
            {
                acaoAtual = StartCoroutine(
                    InspecionarMacaNoPassado()
                );
            }

            return;
        }

        if (PodeColetarNestaEra() &&
            InteractionInputGate.TryConsumeF())
        {
            acaoAtual = StartCoroutine(ColetarItem());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (coletado || !other.CompareTag("Player"))
            return;

        playerDentroDaArea = true;

        playerInventory =
            other.GetComponentInParent<PlayerInventory>();

        AtualizarPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerDentroDaArea = false;

        CancelarAcaoAtual();

        if (interactionPrompt != null)
            interactionPrompt.Esconder();

        if (thoughtPrompt != null)
            thoughtPrompt.Esconder();
    }

    private void AtualizarPrompt()
    {
        if (!playerDentroDaArea ||
            interactionPrompt == null ||
            promptAnchor == null)
        {
            return;
        }

        if (DeveInspecionarNoPassado())
        {
            interactionPrompt.Mostrar(
                promptAnchor,
                promptInteragir
            );

            return;
        }

        if (PodeColetarNestaEra())
        {
            interactionPrompt.Mostrar(
                promptAnchor,
                promptPegar
            );

            return;
        }

        interactionPrompt.Esconder();
    }

    private bool DeveInspecionarNoPassado()
    {
        return timelineManager != null &&
               era == AppleTimelineManager.Era.Passado &&
               timelineManager.DeveInspecionarMacaNoPassado;
    }

    private bool PodeColetarNestaEra()
    {
        if (timelineManager == null)
            return true;

        if (era == AppleTimelineManager.Era.Presente)
            return timelineManager.PodeColetarNoPresente();

        return timelineManager.PodeColetarNoPassado();
    }

    private IEnumerator InspecionarMacaNoPassado()
    {
        executandoAcao = true;

        if (interactionPrompt != null)
            interactionPrompt.Esconder();

        yield return MostrarPensamento(
            comentarioTemporal
        );

        if (timelineManager != null)
        {
            timelineManager.RegistrarInspecaoNoPassado();
        }

        executandoAcao = false;
        acaoAtual = null;

        // Depois da reflexão, troca para [F] PEGAR.
        if (playerDentroDaArea)
            AtualizarPrompt();
    }

    private IEnumerator ColetarItem()
    {
        executandoAcao = true;

        if (playerInventory == null)
        {
            Debug.LogError(
                "PlayerInventory não foi encontrado no Player."
            );

            FinalizarAcao();
            yield break;
        }

        bool itemAdicionado = playerInventory.AdicionarItem(
            itemId,
            itemNome,
            quantidade
        );

        if (!itemAdicionado)
        {
            if (interactionPrompt != null)
                interactionPrompt.Esconder();

            yield return MostrarPensamento(
                comentarioInventarioCheio
            );

            FinalizarAcao();

            if (playerDentroDaArea)
                AtualizarPrompt();

            yield break;
        }

        coletado = true;
        playerDentroDaArea = false;

        if (interactionPrompt != null)
            interactionPrompt.Esconder();

        if (interactionCollider != null)
            interactionCollider.enabled = false;

        if (itemVisual != null)
            itemVisual.SetActive(false);

        // No presente mantém o comentário normal já configurado.
        if (era == AppleTimelineManager.Era.Presente)
        {
            yield return MostrarPensamento(
                comentarioColeta
            );
        }

        RegistrarColetaTemporal();

        executandoAcao = false;
        acaoAtual = null;
    }

    private void RegistrarColetaTemporal()
    {
        if (timelineManager == null)
            return;

        if (era == AppleTimelineManager.Era.Presente)
        {
            timelineManager.RegistrarColetaNoPresente();
        }
        else
        {
            timelineManager.RegistrarColetaNoPassado();
        }
    }

    private IEnumerator MostrarPensamento(string texto)
    {
        if (thoughtPrompt == null ||
            playerThoughtAnchor == null)
        {
            yield break;
        }

        thoughtPrompt.Mostrar(
            playerThoughtAnchor,
            texto
        );

        yield return new WaitForSecondsRealtime(
            duracaoComentario
        );

        thoughtPrompt.Esconder();
    }

    private void FinalizarAcao()
    {
        executandoAcao = false;
        acaoAtual = null;
    }

    private void CancelarAcaoAtual()
    {
        if (acaoAtual != null)
        {
            StopCoroutine(acaoAtual);
            acaoAtual = null;
        }

        executandoAcao = false;
    }

    private void OnDisable()
    {
        playerDentroDaArea = false;

        CancelarAcaoAtual();

        if (interactionPrompt != null)
            interactionPrompt.Esconder();

        if (thoughtPrompt != null)
            thoughtPrompt.Esconder();
    }
}