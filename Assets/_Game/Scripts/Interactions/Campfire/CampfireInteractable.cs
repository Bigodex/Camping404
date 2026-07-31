using System.Collections;
using UnityEngine;

public class CampfireInteractable : MonoBehaviour
{
    [Header("Estado inicial")]
    [SerializeField] private bool iniciarAcesa = true;

    [Header("Componentes da fogueira")]
    [SerializeField] private ParticleSystem fogoNucleo;
    [SerializeField] private ParticleSystem fogoExterno;
    [SerializeField] private Light luzFogueira;

    [Header("Prompt de ação")]
    [SerializeField] private InteractionPromptUI interactionPrompt;
    [SerializeField] private Transform promptAnchor;

    [SerializeField] private string promptAcender =
        "[F] ACENDER";

    [SerializeField] private string promptApagar =
        "[F] APAGAR";

    [Header("Transição")]
    [SerializeField] private float duracaoFadeLuz = 0.3f;

    private bool fogueiraAcesa;
    private bool playerDentroDaArea;
    private bool executandoTransicao;
    private bool promptDaFogueiraVisivel;

    private float intensidadeOriginalDaLuz;
    private Coroutine transicaoAtual;

    private void Awake()
    {
        if (luzFogueira != null)
        {
            intensidadeOriginalDaLuz =
                luzFogueira.intensity;
        }

        fogueiraAcesa = iniciarAcesa;

        AplicarEstadoInicial();
    }

    private void Update()
    {
        if (!playerDentroDaArea ||
            executandoTransicao)
        {
            return;
        }

        if (!InteractionInputGate.TryConsumeF())
            return;

        transicaoAtual = StartCoroutine(
            AlternarFogueira()
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerDentroDaArea = true;

        MostrarPrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerDentroDaArea = false;

        EsconderPromptDaFogueira();
    }

    private IEnumerator AlternarFogueira()
    {
        executandoTransicao = true;
        EsconderPromptDaFogueira();

        if (fogueiraAcesa)
        {
            yield return ApagarFogueira();
        }
        else
        {
            yield return AcenderFogueira();
        }

        executandoTransicao = false;
        transicaoAtual = null;

        if (playerDentroDaArea)
        {
            MostrarPrompt();
        }
    }

    private IEnumerator AcenderFogueira()
    {
        fogueiraAcesa = true;

        AcenderParticulas(fogoNucleo);
        AcenderParticulas(fogoExterno);

        if (luzFogueira != null)
        {
            luzFogueira.enabled = true;

            yield return FazerFadeDaLuz(
                luzFogueira.intensity,
                intensidadeOriginalDaLuz
            );
        }
    }

    private IEnumerator ApagarFogueira()
    {
        fogueiraAcesa = false;

        // StopEmitting permite que as partículas atuais
        // terminem naturalmente em vez de sumirem de uma vez.
        ApagarParticulas(fogoNucleo);
        ApagarParticulas(fogoExterno);

        if (luzFogueira != null)
        {
            yield return FazerFadeDaLuz(
                luzFogueira.intensity,
                0f
            );

            luzFogueira.enabled = false;
        }
    }

    private void AcenderParticulas(
        ParticleSystem particleSystem
    )
    {
        if (particleSystem == null)
            return;

        particleSystem.gameObject.SetActive(true);
        particleSystem.Play(true);
    }

    private void ApagarParticulas(
        ParticleSystem particleSystem
    )
    {
        if (particleSystem == null)
            return;

        particleSystem.Stop(
            true,
            ParticleSystemStopBehavior.StopEmitting
        );
    }

    private IEnumerator FazerFadeDaLuz(
        float intensidadeInicial,
        float intensidadeFinal
    )
    {
        if (luzFogueira == null)
            yield break;

        if (duracaoFadeLuz <= 0f)
        {
            luzFogueira.intensity = intensidadeFinal;
            yield break;
        }

        float tempo = 0f;

        while (tempo < duracaoFadeLuz)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(
                tempo / duracaoFadeLuz
            );

            luzFogueira.intensity = Mathf.Lerp(
                intensidadeInicial,
                intensidadeFinal,
                progresso
            );

            yield return null;
        }

        luzFogueira.intensity = intensidadeFinal;
    }

    private void MostrarPrompt()
    {
        if (!playerDentroDaArea ||
            interactionPrompt == null ||
            promptAnchor == null)
        {
            return;
        }

        string texto =
            fogueiraAcesa
                ? promptApagar
                : promptAcender;

        interactionPrompt.Mostrar(
            promptAnchor,
            texto
        );

        promptDaFogueiraVisivel = true;
    }

    private void EsconderPromptDaFogueira()
    {
        if (!promptDaFogueiraVisivel)
            return;

        if (interactionPrompt != null)
        {
            interactionPrompt.Esconder();
        }

        promptDaFogueiraVisivel = false;
    }

    private void AplicarEstadoInicial()
    {
        if (fogueiraAcesa)
        {
            AcenderParticulas(fogoNucleo);
            AcenderParticulas(fogoExterno);

            if (luzFogueira != null)
            {
                luzFogueira.enabled = true;
                luzFogueira.intensity =
                    intensidadeOriginalDaLuz;
            }
        }
        else
        {
            PararImediatamente(fogoNucleo);
            PararImediatamente(fogoExterno);

            if (luzFogueira != null)
            {
                luzFogueira.intensity = 0f;
                luzFogueira.enabled = false;
            }
        }
    }

    private void PararImediatamente(
        ParticleSystem particleSystem
    )
    {
        if (particleSystem == null)
            return;

        particleSystem.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }

    private void OnDisable()
    {
        playerDentroDaArea = false;

        if (transicaoAtual != null)
        {
            StopCoroutine(transicaoAtual);
            transicaoAtual = null;
        }

        executandoTransicao = false;
        EsconderPromptDaFogueira();
    }
}