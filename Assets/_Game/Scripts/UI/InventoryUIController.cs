using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    [Serializable]
    private class ItemIconData
    {
        public string itemId;
        public Sprite icon;
    }

    [Header("Interface")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private CanvasGroup worldPromptsGroup;

    [Header("Inventário do Player")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Ícones dos itens")]
    [SerializeField] private ItemIconData[] itemIcons;

    private InventorySlotUI[] slots;
    private bool inventarioAberto;

    public static bool EstaAberto { get; private set; }

    private void Awake()
    {
        if (slotsContainer != null)
        {
            slots = slotsContainer.GetComponentsInChildren<InventorySlotUI>(
                true
            );
        }
    }

    private void Start()
    {
        FecharInventario();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            AlternarInventario();
        }

        if (inventarioAberto &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            FecharInventario();
        }
    }

    private void AlternarInventario()
    {
        if (inventarioAberto)
            FecharInventario();
        else
            AbrirInventario();
    }

    private void AbrirInventario()
    {
        inventarioAberto = true;
        EstaAberto = true;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(true);

        DefinirVisibilidadeDosPrompts(false);
        AtualizarInventario();
    }

    private void FecharInventario()
    {
        inventarioAberto = false;
        EstaAberto = false;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        DefinirVisibilidadeDosPrompts(true);
    }

    private void DefinirVisibilidadeDosPrompts(bool visiveis)
    {
        if (worldPromptsGroup == null)
            return;

        worldPromptsGroup.alpha = visiveis ? 1f : 0f;
        worldPromptsGroup.interactable = visiveis;
        worldPromptsGroup.blocksRaycasts = visiveis;
    }

    private void AtualizarInventario()
    {
        if (slots == null ||
            slots.Length == 0 ||
            playerInventory == null)
        {
            Debug.LogWarning(
                "O InventoryUIController não está completamente configurado."
            );

            return;
        }

        foreach (InventorySlotUI slot in slots)
        {
            slot.ClearSlot();
        }

        int quantidadeDeItens = Mathf.Min(
            playerInventory.Itens.Count,
            slots.Length
        );

        for (int i = 0; i < quantidadeDeItens; i++)
        {
            PlayerInventory.InventorySlot item =
                playerInventory.Itens[i];

            Sprite icone = BuscarIcone(item.itemId);

            slots[i].SetSlot(
                icone,
                item.quantidade
            );
        }
    }

    private Sprite BuscarIcone(string itemId)
    {
        if (itemIcons == null)
            return null;

        foreach (ItemIconData itemIcon in itemIcons)
        {
            if (itemIcon.itemId == itemId)
                return itemIcon.icon;
        }

        Debug.LogWarning(
            $"Nenhum ícone foi configurado para o item: {itemId}"
        );

        return null;
    }
}