using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Referência")]
    [SerializeField] private Light luzDoSol;

    [Header("Tempo")]
    [Tooltip("Duração de um dia completo em minutos reais.")]
    [SerializeField] private float duracaoDoDiaEmMinutos = 5f;

    [Range(0f, 24f)]
    [SerializeField] private float horaInicial = 8f;

    [Header("Intensidade do sol")]
    [SerializeField] private float intensidadeDia = 1.2f;
    [SerializeField] private float intensidadeNoite = 0.02f;

    [Header("Iluminação ambiente")]
    [SerializeField] private float ambienteDia = 1f;
    [SerializeField] private float ambienteNoite = 0.15f;

    [Header("Cores")]
    [SerializeField] private Color corDia =
        new Color(1f, 0.95f, 0.85f);

    [SerializeField] private Color corHorizonte =
        new Color(1f, 0.45f, 0.2f);

    [SerializeField] private Color corNoite =
        new Color(0.15f, 0.2f, 0.45f);

    [Header("Direção")]
    [SerializeField] private float rotacaoHorizontalDoSol = 170f;

    private float horaAtual;

    private void Start()
    {
        horaAtual = horaInicial;

        AtualizarIluminacao();
    }

    private void Update()
    {
        PassarTempo();
        AtualizarIluminacao();
    }

    private void PassarTempo()
    {
        // Evita divisão por zero
        if (duracaoDoDiaEmMinutos <= 0f)
            return;

        float segundosPorDia = duracaoDoDiaEmMinutos * 60f;
        float horasPorSegundo = 24f / segundosPorDia;

        horaAtual += horasPorSegundo * Time.deltaTime;

        // Volta para meia-noite depois das 24 horas
        if (horaAtual >= 24f)
        {
            horaAtual -= 24f;
        }
    }

    private void AtualizarIluminacao()
    {
        if (luzDoSol == null)
            return;

        float tempoNormalizado = horaAtual / 24f;

        // 06:00 = nascer do sol
        // 12:00 = sol no alto
        // 18:00 = pôr do sol
        // 00:00 = noite
        float rotacaoVertical =
            tempoNormalizado * 360f - 90f;

        luzDoSol.transform.rotation = Quaternion.Euler(
            rotacaoVertical,
            rotacaoHorizontalDoSol,
            0f
        );

        // Vai de 0 à noite até 1 ao meio-dia
        float quantidadeDeLuz = Mathf.Clamp01(
            Mathf.Sin(
                (tempoNormalizado - 0.25f) *
                Mathf.PI * 2f
            )
        );

        luzDoSol.intensity = Mathf.Lerp(
            intensidadeNoite,
            intensidadeDia,
            quantidadeDeLuz
        );

        RenderSettings.ambientIntensity = Mathf.Lerp(
            ambienteNoite,
            ambienteDia,
            quantidadeDeLuz
        );

        AtualizarCorDoSol(
            tempoNormalizado,
            quantidadeDeLuz
        );
    }

    private void AtualizarCorDoSol(
        float tempoNormalizado,
        float quantidadeDeLuz
    )
    {
        // Força da coloração no nascer e no pôr do sol
        float nascerDoSol = CalcularProximidade(
            tempoNormalizado,
            0.25f,
            0.08f
        );

        float porDoSol = CalcularProximidade(
            tempoNormalizado,
            0.75f,
            0.08f
        );

        float corDoHorizonte =
            Mathf.Max(nascerDoSol, porDoSol);

        Color corAtual = Color.Lerp(
            corNoite,
            corDia,
            quantidadeDeLuz
        );

        corAtual = Color.Lerp(
            corAtual,
            corHorizonte,
            corDoHorizonte
        );

        luzDoSol.color = corAtual;
    }

    private float CalcularProximidade(
        float valor,
        float alvo,
        float distancia
    )
    {
        return 1f - Mathf.Clamp01(
            Mathf.Abs(valor - alvo) / distancia
        );
    }
}