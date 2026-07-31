using System.Collections;
using UnityEngine;

public class AppleTableInteractable : MonoBehaviour
{
    private enum AcaoDaMesa
    {
        Nenhuma,
        Colocar,
        Pegar
    }

    [Header("Linha do tempo")]
    [SerializeField] private AppleTimelineManager timelineManager;

    [SerializeField]
    private AppleTimelineManager.Era era =
        AppleTimelineManager.Era.Presente;

    [Header("Item")]
    [SerializeField] private string itemId = "apple";
    [SerializeField] private string itemNome = "Maçã";
    [SerializeField] private int quantidade = 1;

    [Header("Prompt de ação")]
    [SerializeField] private InteractionPromptUI interactionPrompt;
    [SerializeField] private Transform promptAnchor;

    [SerializeField] private string promptColocar =
        "[F] COLOCAR";

    [SerializeField] private string promptPegar =
        "[F] PEGAR";

    [Header("Pensamento do player")]
    [SerializeField] private InteractionPromptUI thoughtPrompt;
    [SerializeField] private Transform playerThoughtAnchor;

    [TextArea(2, 4)]
    [SerializeField] private string comentarioColocarPresente =
        "Vou deixar a maçã aqui por enquanto...";

    [TextArea(2, 4)]
    [SerializeField] private string comentarioColocarPassado =
        "Vou deixar a maçã sobre a mesa...";

    [TextArea(2, 4)]
    [SerializeField] private string comentarioPegar =
        "Vou levar a maçã comigo...";

    [TextArea(2, 4)]
    [SerializeField] private string comentarioInventarioCheio =
        "Não tenho espaço para carregar isso.";

    [SerializeField] private float duracaoComentario = 2.5f;

    private PlayerInventory playerInventory;

    private bool playerDentroDaArea;
    private bool executandoAcao;
    private bool promptDaMesaVisivel;
    private bool pensamentoDaMesaVisivel;

    private AcaoDaMesa acaoExibida =
        AcaoDaMesa.Nenhuma;

    private Coroutine acaoAtual;

    private void Update()
    {
        if (!playerDentroDaArea || executandoAcao)
            return;

        // Enquanto qualquer pensamento estiver aparecendo,
        // a mesa não mostra botões de ação.
        if (thoughtPrompt != null &&
            thoughtPrompt.EstaVisivel)
        {
            EsconderPromptDaMesa();
            return;
        }

        AcaoDaMesa novaAcao =
            DeterminarAcaoDisponivel();

        AtualizarPrompt(novaAcao);

        if (novaAcao == AcaoDaMesa.Nenhuma)
            return;

        if (!InteractionInputGate.TryConsumeF())
            return;

        if (novaAcao == AcaoDaMesa.Pegar)
        {
            acaoAtual = StartCoroutine(
                PegarMacaDaMesa()
            );
        }
        else
        {
            acaoAtual = StartCoroutine(
                ColocarMacaNaMesa()
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerDentroDaArea = true;

        playerInventory =
            other.GetComponentInParent<PlayerInventory>();

        if (thoughtPrompt == null ||
            !thoughtPrompt.EstaVisivel)
        {
            AtualizarPrompt(
                DeterminarAcaoDisponivel()
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerDentroDaArea = false;

        CancelarAcaoAtual();
        EsconderPromptDaMesa();
        EsconderPensamentoDaMesa();
    }

    private AcaoDaMesa DeterminarAcaoDisponivel()
    {
        if (timelineManager == null ||
            playerInventory == null)
        {
            return AcaoDaMesa.Nenhuma;
        }

        // A maçã original tem prioridade sobre a mesa.
        // No passado, depois do [Z] INTERAGIR, isso permite
        // que apareça [F] PEGAR na maçã original, sem a mesa
        // substituir o prompt por [F] COLOCAR.
        if (timelineManager.MacaOriginalEstaDisponivel(era))
        {
            return AcaoDaMesa.Nenhuma;
        }

        // Existe uma maçã sobre a mesa:
        // a ação disponível é pegar.
        if (timelineManager.MacaEstaNaMesa(era))
        {
            return AcaoDaMesa.Pegar;
        }

        // Mesa vazia e player possui maçã:
        // a ação disponível é colocar.
        bool possuiMaca =
            playerInventory.PossuiItem(
                itemId,
                quantidade
            );

        if (possuiMaca)
        {
            return AcaoDaMesa.Colocar;
        }

        return AcaoDaMesa.Nenhuma;
    }

    private void AtualizarPrompt(
        AcaoDaMesa novaAcao
    )
    {
        if (!playerDentroDaArea ||
            interactionPrompt == null ||
            promptAnchor == null)
        {
            return;
        }

        if (novaAcao == AcaoDaMesa.Nenhuma)
        {
            EsconderPromptDaMesa();
            return;
        }

        // Não reinicia a animação de pop em todos os frames.
        if (promptDaMesaVisivel &&
            acaoExibida == novaAcao)
        {
            return;
        }

        string texto =
            novaAcao == AcaoDaMesa.Pegar
                ? promptPegar
                : promptColocar;

        interactionPrompt.Mostrar(
            promptAnchor,
            texto
        );

        acaoExibida = novaAcao;
        promptDaMesaVisivel = true;
    }

    private IEnumerator ColocarMacaNaMesa()
    {
        executandoAcao = true;
        EsconderPromptDaMesa();

        if (playerInventory == null ||
            timelineManager == null)
        {
            Debug.LogError(
                "A mesa não está completamente configurada."
            );

            FinalizarAcao();
            yield break;
        }

        bool removeuItem =
            playerInventory.RemoverItem(
                itemId,
                quantidade
            );

        if (!removeuItem)
        {
            FinalizarAcao();
            yield break;
        }

        timelineManager.ColocarNaMesa(era);

        string comentario =
            era == AppleTimelineManager.Era.Passado
                ? comentarioColocarPassado
                : comentarioColocarPresente;

        yield return MostrarPensamento(comentario);

        FinalizarAcao();
    }

    private IEnumerator PegarMacaDaMesa()
    {
        executandoAcao = true;
        EsconderPromptDaMesa();

        if (playerInventory == null ||
            timelineManager == null)
        {
            Debug.LogError(
                "A mesa não está completamente configurada."
            );

            FinalizarAcao();
            yield break;
        }

        bool adicionouItem =
            playerInventory.AdicionarItem(
                itemId,
                itemNome,
                quantidade
            );

        if (!adicionouItem)
        {
            yield return MostrarPensamento(
                comentarioInventarioCheio
            );

            FinalizarAcao();
            yield break;
        }

        timelineManager.RetirarDaMesa(era);

        yield return MostrarPensamento(
            comentarioPegar
        );

        FinalizarAcao();
    }

    private IEnumerator MostrarPensamento(
        string texto
    )
    {
        if (thoughtPrompt == null ||
            playerThoughtAnchor == null ||
            string.IsNullOrWhiteSpace(texto))
        {
            yield break;
        }

        pensamentoDaMesaVisivel = true;

        thoughtPrompt.Mostrar(
            playerThoughtAnchor,
            texto
        );

        yield return new WaitForSecondsRealtime(
            duracaoComentario
        );

        EsconderPensamentoDaMesa();
    }

    private void EsconderPensamentoDaMesa()
    {
        // Evita apagar um pensamento criado por outro objeto.
        if (!pensamentoDaMesaVisivel)
            return;

        if (thoughtPrompt != null)
        {
            thoughtPrompt.EsconderImediatamente();
        }

        pensamentoDaMesaVisivel = false;
    }

    private void EsconderPromptDaMesa()
    {
        if (!promptDaMesaVisivel)
            return;

        if (interactionPrompt != null)
        {
            interactionPrompt.Esconder();
        }

        promptDaMesaVisivel = false;
        acaoExibida = AcaoDaMesa.Nenhuma;
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
        EsconderPromptDaMesa();
        EsconderPensamentoDaMesa();
    }
}