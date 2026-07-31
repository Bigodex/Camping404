using UnityEngine;

public class WorldPromptFollower : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform worldTarget;
    [SerializeField] private Camera targetCamera;

    [Header("Posição na tela")]
    [SerializeField] private Vector2 screenOffset = new(0f, 20f);

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (worldTarget == null || targetCamera == null)
            return;

        Vector3 screenPosition =
            targetCamera.WorldToScreenPoint(worldTarget.position);

        if (screenPosition.z <= 0f)
            return;

        rectTransform.position =
            (Vector2)screenPosition + screenOffset;
    }

    public void DefinirAlvo(Transform novoAlvo)
    {
        worldTarget = novoAlvo;
    }

    public void LimparAlvo()
    {
        worldTarget = null;
    }
}