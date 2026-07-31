using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Serialization;

public class SeatInteractable : MonoBehaviour
{
    [System.Serializable]
    private class SeatData
    {
        /*
         * O campo originalmente se chamava sitPoint.
         * Isso preserva as referências antigas da cena.
         */
        [FormerlySerializedAs("sitPoint")]
        [SerializeField] private Transform seatPoint;

        [SerializeField] private Transform standPoint;

        public Transform SeatPoint => seatPoint;
        public Transform StandPoint => standPoint;

        public bool EstaConfigurado =>
            seatPoint != null &&
            standPoint != null;

        public void Configurar(
            Transform novoSeatPoint,
            Transform novoStandPoint
        )
        {
            seatPoint = novoSeatPoint;
            standPoint = novoStandPoint;
        }
    }

    [Header("Assentos disponíveis")]
    [SerializeField] private List<SeatData> seats = new();

    [Header("Prompt de ação")]
    [SerializeField] private InteractionPromptUI interactionPrompt;
    [SerializeField] private Transform promptAnchor;
    [SerializeField] private string promptSentar = "[F] SENTAR";
    [SerializeField] private string promptLevantar = "[F] LEVANTAR";

    [Header("Posicionamento")]
    [SerializeField] private float duracaoAlinhamento = 0.15f;
    [SerializeField] private float duracaoSaida = 0.2f;

    [Header("Animação")]
    [Tooltip("Tempo para executar a animação de sentar.")]
    [SerializeField] private float duracaoSentar = 1.7f;

    [Tooltip("Tempo para executar a animação de levantar.")]
    [SerializeField] private float duracaoLevantar = 1.5f;

    [Tooltip(
        "Ponto do clipe utilizado como pose sentada final. " +
        "Valores menores deixam o personagem mais ereto."
    )]
    [Range(0.6f, 1f)]
    [SerializeField] private float poseSentadaProgress = 0.86f;

    [SerializeField] private float limiteEsperaAnimator = 5f;

    private static readonly int IsSittingHash =
        Animator.StringToHash("IsSitting");

    private static readonly int SeatProgressHash =
        Animator.StringToHash("SeatProgress");

    private static readonly int SeatSequenceHash =
        Animator.StringToHash(
            "Base Layer.SeatSequence"
        );

    private static readonly int LocomotionHash =
        Animator.StringToHash(
            "Base Layer.Idle Walk Run Blend"
        );

    private ThirdPersonController playerController;
    private CharacterController characterController;
    private Animator playerAnimator;
    private Transform playerTransform;

    private SeatData assentoAtual;
    private Coroutine transicaoAtual;

    private bool playerDentroDaArea;
    private bool playerSentado;
    private bool emTransicao;
    private bool promptDoTroncoVisivel;

    private void Awake()
    {
        TentarRecuperarReferenciasDosAssentos();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        TentarRecuperarReferenciasDosAssentos();
    }
#endif

    private void Update()
    {
        if (emTransicao)
            return;

        if ((!playerDentroDaArea && !playerSentado) ||
            playerTransform == null)
        {
            return;
        }

        if (!InteractionInputGate.TryConsumeF())
            return;

        transicaoAtual = playerSentado
            ? StartCoroutine(Levantar())
            : StartCoroutine(Sentar());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        ThirdPersonController controllerEncontrado =
            other.GetComponentInParent<ThirdPersonController>();

        if (controllerEncontrado == null)
        {
            Debug.LogWarning(
                "ThirdPersonController não encontrado no Player."
            );

            return;
        }

        playerController = controllerEncontrado;
        playerTransform = playerController.transform;

        characterController =
            playerTransform.GetComponent<CharacterController>();

        playerAnimator =
            playerTransform.GetComponent<Animator>();

        if (playerAnimator == null)
        {
            Debug.LogWarning(
                "Animator não encontrado no Player."
            );
        }

        playerDentroDaArea = true;

        if (!playerSentado && !emTransicao)
        {
            MostrarPrompt(promptSentar);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        /*
         * Durante a animação, o Player pode sair
         * fisicamente da área do trigger.
         */
        if (playerSentado || emTransicao)
            return;

        playerDentroDaArea = false;
        EsconderPromptDoTronco();
    }

    private IEnumerator Sentar()
    {
        /*
         * Faz uma nova tentativa caso alguma referência
         * tenha sido perdida após recompilar o script.
         */
        TentarRecuperarReferenciasDosAssentos();

        SeatData assentoMaisProximo =
            EncontrarAssentoMaisProximo();

        if (assentoMaisProximo == null)
        {
            Debug.LogWarning(
                "Nenhum par SeatPoint/StandPoint válido " +
                "foi encontrado em Seats."
            );

            yield break;
        }

        emTransicao = true;
        assentoAtual = assentoMaisProximo;

        EsconderPromptDoTronco();
        BloquearMovimento();

        /*
         * SeatPoint controla a posição usada
         * durante a animação e enquanto o personagem
         * permanece sentado.
         */
        yield return MoverSuavementeAte(
            assentoAtual.SeatPoint,
            duracaoAlinhamento
        );

        FixarNoPonto(
            assentoAtual.SeatPoint
        );

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(
                SeatProgressHash,
                0f
            );

            playerAnimator.SetBool(
                IsSittingHash,
                true
            );

            yield return AguardarEstadoEstavel(
                SeatSequenceHash
            );

            yield return AnimarSeatProgress(
                0f,
                poseSentadaProgress,
                duracaoSentar
            );

            playerAnimator.SetFloat(
                SeatProgressHash,
                poseSentadaProgress
            );
        }

        FixarNoPonto(
            assentoAtual.SeatPoint
        );

        playerSentado = true;
        playerDentroDaArea = true;
        emTransicao = false;
        transicaoAtual = null;

        MostrarPrompt(promptLevantar);
    }

    private IEnumerator Levantar()
    {
        if (assentoAtual == null ||
            !assentoAtual.EstaConfigurado)
        {
            Debug.LogWarning(
                "O assento atual perdeu a referência " +
                "do SeatPoint ou StandPoint."
            );

            emTransicao = false;
            transicaoAtual = null;

            yield break;
        }

        emTransicao = true;
        EsconderPromptDoTronco();

        /*
         * O movimento de levantar começa na mesma
         * posição usada enquanto estava sentado.
         */
        FixarNoPonto(
            assentoAtual.SeatPoint
        );

        if (playerAnimator != null)
        {
            yield return AnimarSeatProgress(
                poseSentadaProgress,
                0f,
                duracaoLevantar
            );

            playerAnimator.SetFloat(
                SeatProgressHash,
                0f
            );

            playerAnimator.SetBool(
                IsSittingHash,
                false
            );

            yield return AguardarEstadoEstavel(
                LocomotionHash
            );
        }

        /*
         * Depois de terminar a animação,
         * leva o Player para o ponto de saída.
         */
        yield return MoverSuavementeAte(
            assentoAtual.StandPoint,
            duracaoSaida
        );

        FixarNoPonto(
            assentoAtual.StandPoint
        );

        LiberarMovimento();

        playerSentado = false;
        playerDentroDaArea = true;
        emTransicao = false;
        assentoAtual = null;
        transicaoAtual = null;

        MostrarPrompt(promptSentar);
    }

    private IEnumerator AnimarSeatProgress(
        float valorInicial,
        float valorFinal,
        float duracao
    )
    {
        if (playerAnimator == null)
            yield break;

        if (duracao <= 0f)
        {
            playerAnimator.SetFloat(
                SeatProgressHash,
                valorFinal
            );

            yield break;
        }

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracao
            );

            progresso = Mathf.SmoothStep(
                0f,
                1f,
                progresso
            );

            float valorAtual = Mathf.Lerp(
                valorInicial,
                valorFinal,
                progresso
            );

            playerAnimator.SetFloat(
                SeatProgressHash,
                valorAtual
            );

            /*
             * Durante a sequência, o root permanece
             * fixado no SeatPoint.
             */
            if (assentoAtual != null &&
                assentoAtual.SeatPoint != null)
            {
                FixarNoPonto(
                    assentoAtual.SeatPoint
                );
            }

            yield return null;
        }

        playerAnimator.SetFloat(
            SeatProgressHash,
            valorFinal
        );
    }

    private IEnumerator AguardarEstadoEstavel(
        int estadoHash
    )
    {
        if (playerAnimator == null)
            yield break;

        float tempo = 0f;

        while (tempo < limiteEsperaAnimator)
        {
            AnimatorStateInfo estadoAtual =
                playerAnimator.GetCurrentAnimatorStateInfo(0);

            bool estadoCorreto =
                estadoAtual.fullPathHash == estadoHash;

            bool estaEmTransicao =
                playerAnimator.IsInTransition(0);

            if (estadoCorreto && !estaEmTransicao)
            {
                yield break;
            }

            if (assentoAtual != null &&
                assentoAtual.SeatPoint != null)
            {
                FixarNoPonto(
                    assentoAtual.SeatPoint
                );
            }

            tempo += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning(
            "O Animator não entrou no estado esperado. " +
            "Confira SeatSequence, Idle Walk Run Blend " +
            "e as condições das transições."
        );
    }

    private IEnumerator MoverSuavementeAte(
        Transform destino,
        float duracao
    )
    {
        if (playerTransform == null ||
            destino == null)
        {
            yield break;
        }

        Vector3 posicaoInicial =
            playerTransform.position;

        Quaternion rotacaoInicial =
            playerTransform.rotation;

        if (duracao <= 0f)
        {
            FixarNoPonto(destino);
            yield break;
        }

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracao
            );

            progresso = Mathf.SmoothStep(
                0f,
                1f,
                progresso
            );

            Vector3 posicao =
                Vector3.Lerp(
                    posicaoInicial,
                    destino.position,
                    progresso
                );

            Quaternion rotacao =
                Quaternion.Slerp(
                    rotacaoInicial,
                    destino.rotation,
                    progresso
                );

            playerTransform.SetPositionAndRotation(
                posicao,
                rotacao
            );

            yield return null;
        }

        FixarNoPonto(destino);
    }

    private SeatData EncontrarAssentoMaisProximo()
    {
        if (seats == null ||
            seats.Count == 0 ||
            playerTransform == null)
        {
            return null;
        }

        SeatData assentoMaisProximo = null;
        float menorDistancia = float.MaxValue;

        foreach (SeatData seat in seats)
        {
            if (seat == null ||
                !seat.EstaConfigurado)
            {
                continue;
            }

            float distancia = Vector3.SqrMagnitude(
                playerTransform.position -
                seat.SeatPoint.position
            );

            if (distancia >= menorDistancia)
                continue;

            menorDistancia = distancia;
            assentoMaisProximo = seat;
        }

        return assentoMaisProximo;
    }

    private void TentarRecuperarReferenciasDosAssentos()
    {
        /*
         * Se a lista já estiver completamente configurada,
         * não altera nada.
         */
        if (TodosOsAssentosEstaoConfigurados())
            return;

        Transform raizDoTronco =
            transform.parent;

        if (raizDoTronco == null)
            return;

        Transform pastaSeats =
            raizDoTronco.Find("Seats");

        if (pastaSeats == null)
            return;

        List<SeatData> assentosEncontrados = new();

        foreach (Transform objetoSeat in pastaSeats)
        {
            Transform seatPointEncontrado =
                objetoSeat.Find("SeatPoint");

            Transform standPointEncontrado =
                objetoSeat.Find("StandPoint");

            if (seatPointEncontrado == null ||
                standPointEncontrado == null)
            {
                continue;
            }

            SeatData novoAssento =
                new SeatData();

            novoAssento.Configurar(
                seatPointEncontrado,
                standPointEncontrado
            );

            assentosEncontrados.Add(
                novoAssento
            );
        }

        if (assentosEncontrados.Count > 0)
        {
            seats = assentosEncontrados;
        }
    }

    private bool TodosOsAssentosEstaoConfigurados()
    {
        if (seats == null ||
            seats.Count == 0)
        {
            return false;
        }

        foreach (SeatData seat in seats)
        {
            if (seat == null ||
                !seat.EstaConfigurado)
            {
                return false;
            }
        }

        return true;
    }

    private void FixarNoPonto(
        Transform ponto
    )
    {
        if (playerTransform == null ||
            ponto == null)
        {
            return;
        }

        playerTransform.SetPositionAndRotation(
            ponto.position,
            ponto.rotation
        );
    }

    private void BloquearMovimento()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }

    private void LiberarMovimento()
    {
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    private void MostrarPrompt(
        string texto
    )
    {
        if (interactionPrompt == null ||
            promptAnchor == null)
        {
            return;
        }

        interactionPrompt.Mostrar(
            promptAnchor,
            texto
        );

        promptDoTroncoVisivel = true;
    }

    private void EsconderPromptDoTronco()
    {
        if (!promptDoTroncoVisivel)
            return;

        if (interactionPrompt != null)
        {
            interactionPrompt.Esconder();
        }

        promptDoTroncoVisivel = false;
    }

    private void OnDisable()
    {
        EsconderPromptDoTronco();

        if (transicaoAtual != null)
        {
            StopCoroutine(transicaoAtual);
            transicaoAtual = null;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat(
                SeatProgressHash,
                0f
            );

            playerAnimator.SetBool(
                IsSittingHash,
                false
            );
        }

        if (assentoAtual != null &&
            assentoAtual.StandPoint != null)
        {
            FixarNoPonto(
                assentoAtual.StandPoint
            );
        }

        LiberarMovimento();

        playerDentroDaArea = false;
        playerSentado = false;
        emTransicao = false;
        assentoAtual = null;
    }
}