using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class IsometricCamera : MonoBehaviour
{
    [Header("Referência")]
    [SerializeField] private Transform alvo;

    [Header("Posição")]
    [SerializeField] private Vector3 deslocamento =
        new Vector3(0f, 12f, -10f);

    [SerializeField] private float suavidade = 10f;

    [Header("Zoom")]
    [SerializeField] private float tamanhoInicial = 7f;
    [SerializeField] private float zoomMinimo = 4f;
    [SerializeField] private float zoomMaximo = 12f;
    [SerializeField] private float velocidadeZoom = 0.01f;

    [Header("Rotação")]
    [SerializeField] private float velocidadeRotacao = 180f;

    private Camera cameraComponente;
    private float anguloAtual = 45f;
    private float anguloDesejado = 45f;

    private void Awake()
    {
        cameraComponente = GetComponent<Camera>();

        // Configura câmera ortográfica
        cameraComponente.orthographic = true;
        cameraComponente.orthographicSize = tamanhoInicial;
    }

    private void Update()
    {
        LerRotacao();
        LerZoom();
    }

    private void LateUpdate()
    {
        SeguirAlvo();
    }

    private void LerRotacao()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
            anguloDesejado -= 90f;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            anguloDesejado += 90f;
    }

    private void LerZoom()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        cameraComponente.orthographicSize -=
            scroll * velocidadeZoom;

        cameraComponente.orthographicSize = Mathf.Clamp(
            cameraComponente.orthographicSize,
            zoomMinimo,
            zoomMaximo
        );
    }

    private void SeguirAlvo()
    {
        if (alvo == null)
            return;

        anguloAtual = Mathf.MoveTowardsAngle(
            anguloAtual,
            anguloDesejado,
            velocidadeRotacao * Time.deltaTime
        );

        Quaternion rotacao =
            Quaternion.Euler(0f, anguloAtual, 0f);

        Vector3 posicaoDesejada =
            alvo.position + rotacao * deslocamento;

        transform.position = Vector3.Lerp(
            transform.position,
            posicaoDesejada,
            suavidade * Time.deltaTime
        );

        transform.LookAt(alvo.position + Vector3.up);
    }
}