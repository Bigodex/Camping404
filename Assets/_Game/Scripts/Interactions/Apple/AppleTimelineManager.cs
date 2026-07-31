using UnityEngine;

public class AppleTimelineManager : MonoBehaviour
{
    public enum Era
    {
        Presente,
        Passado
    }

    [Header("Maçãs originais")]
    [SerializeField] private GameObject appleOriginalPresente;
    [SerializeField] private GameObject appleOriginalPassado;

    [Header("Maçãs colocadas na mesa")]
    [SerializeField] private GameObject appleMesaPresente;
    [SerializeField] private GameObject appleMesaPassado;

    [Header("Estado das maçãs")]
    [SerializeField] private bool presenteColetada;
    [SerializeField] private bool passadoColetada;

    [SerializeField] private bool colocadaNaMesaPresente;
    [SerializeField] private bool colocadaNaMesaPassado;

    [Header("Descoberta temporal")]
    [SerializeField] private bool presenteFoiColetadaPrimeiro;

    [SerializeField]
    private bool passadoFoiInspecionadoDepoisDoPresente;

    [Header("Alterações no presente")]
    [SerializeField]
    private bool macaDoPassadoRetiradaNoPresente;

    public bool PresenteColetada =>
        presenteColetada;

    public bool PassadoColetada =>
        passadoColetada;

    public bool ColocadaNaMesaPresente =>
        colocadaNaMesaPresente;

    public bool ColocadaNaMesaPassado =>
        colocadaNaMesaPassado;

    public bool DeveInspecionarMacaNoPassado =>
        presenteFoiColetadaPrimeiro &&
        !passadoFoiInspecionadoDepoisDoPresente &&
        !passadoColetada &&
        !colocadaNaMesaPassado;

    private void Start()
    {
        AtualizarVisuais();
    }

    public bool PodeColetarNoPresente()
    {
        return MacaOriginalEstaDisponivel(
            Era.Presente
        );
    }

    public bool PodeColetarNoPassado()
    {
        if (!MacaOriginalEstaDisponivel(
                Era.Passado
            ))
        {
            return false;
        }

        // Caso o jogador tenha coletado primeiro
        // no presente, precisa inspecionar a maçã
        // do passado antes de poder pegá-la.
        if (presenteFoiColetadaPrimeiro &&
            !passadoFoiInspecionadoDepoisDoPresente)
        {
            return false;
        }

        return true;
    }

    public bool MacaOriginalEstaDisponivel(
        Era era
    )
    {
        if (era == Era.Passado)
        {
            return !passadoColetada &&
                   !colocadaNaMesaPassado;
        }

        return !presenteColetada &&
               !passadoColetada &&
               !MacaEstaNaMesa(Era.Presente);
    }

    public bool MacaEstaNaMesa(Era era)
    {
        if (era == Era.Passado)
        {
            return colocadaNaMesaPassado;
        }

        bool veioDoPassado =
            colocadaNaMesaPassado &&
            !macaDoPassadoRetiradaNoPresente;

        return colocadaNaMesaPresente ||
               veioDoPassado;
    }

    public void RegistrarInspecaoNoPassado()
    {
        if (!DeveInspecionarMacaNoPassado)
            return;

        passadoFoiInspecionadoDepoisDoPresente =
            true;

        Debug.Log(
            "A maçã do passado foi inspecionada. " +
            "Agora ela pode ser coletada."
        );
    }

    public void RegistrarColetaNoPresente()
    {
        if (!PodeColetarNoPresente())
            return;

        presenteColetada = true;

        presenteFoiColetadaPrimeiro =
            !passadoColetada &&
            !colocadaNaMesaPassado;

        passadoFoiInspecionadoDepoisDoPresente =
            false;

        AtualizarVisuais();

        Debug.Log(
            "A maçã foi coletada primeiro no presente."
        );
    }

    public void RegistrarColetaNoPassado()
    {
        if (!PodeColetarNoPassado())
            return;

        passadoColetada = true;

        // Uma maçã retirada no passado deixa de
        // existir na versão original do presente.
        presenteColetada = true;

        presenteFoiColetadaPrimeiro = false;

        passadoFoiInspecionadoDepoisDoPresente =
            false;

        AtualizarVisuais();

        Debug.Log(
            "A maçã foi coletada no passado. " +
            "A versão original do presente desapareceu."
        );
    }

    public void ColocarNaMesa(Era era)
    {
        if (era == Era.Passado)
        {
            ColocarNaMesaNoPassado();
        }
        else
        {
            ColocarNaMesaNoPresente();
        }
    }

    private void ColocarNaMesaNoPassado()
    {
        passadoColetada = true;
        presenteColetada = true;

        colocadaNaMesaPassado = true;

        macaDoPassadoRetiradaNoPresente = false;

        presenteFoiColetadaPrimeiro = false;

        passadoFoiInspecionadoDepoisDoPresente =
            false;

        AtualizarVisuais();

        Debug.Log(
            "A maçã foi colocada na mesa no passado. " +
            "Ela também aparece na mesa no presente."
        );
    }

    private void ColocarNaMesaNoPresente()
    {
        presenteColetada = true;
        colocadaNaMesaPresente = true;

        AtualizarVisuais();

        Debug.Log(
            "A maçã foi colocada na mesa no presente."
        );
    }

    public void RetirarDaMesa(Era era)
    {
        if (era == Era.Passado)
        {
            colocadaNaMesaPassado = false;

            macaDoPassadoRetiradaNoPresente =
                false;

            Debug.Log(
                "A maçã foi retirada da mesa no passado. " +
                "Ela também desapareceu da mesa no presente."
            );
        }
        else
        {
            if (colocadaNaMesaPresente)
            {
                colocadaNaMesaPresente = false;
            }
            else if (colocadaNaMesaPassado)
            {
                // A maçã continua na mesa do passado,
                // mas foi retirada em um momento futuro.
                macaDoPassadoRetiradaNoPresente =
                    true;
            }

            Debug.Log(
                "A maçã foi retirada da mesa no presente."
            );
        }

        AtualizarVisuais();
    }

    private void AtualizarVisuais()
    {
        bool mostrarOriginalPassado =
            MacaOriginalEstaDisponivel(
                Era.Passado
            );

        bool mostrarMesaPassado =
            MacaEstaNaMesa(
                Era.Passado
            );

        bool mostrarOriginalPresente =
            MacaOriginalEstaDisponivel(
                Era.Presente
            );

        bool mostrarMesaPresente =
            MacaEstaNaMesa(
                Era.Presente
            );

        DefinirObjetoAtivo(
            appleOriginalPassado,
            mostrarOriginalPassado
        );

        DefinirObjetoAtivo(
            appleMesaPassado,
            mostrarMesaPassado
        );

        DefinirObjetoAtivo(
            appleOriginalPresente,
            mostrarOriginalPresente
        );

        DefinirObjetoAtivo(
            appleMesaPresente,
            mostrarMesaPresente
        );
    }

    private void DefinirObjetoAtivo(
        GameObject objeto,
        bool ativo
    )
    {
        if (objeto != null)
        {
            objeto.SetActive(ativo);
        }
    }
}