using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Serializable]
    public class InventorySlot
    {
        public string itemId;
        public string itemNome;
        public int quantidade;

        public InventorySlot(
            string novoItemId,
            string novoItemNome,
            int novaQuantidade
        )
        {
            itemId = novoItemId;
            itemNome = novoItemNome;
            quantidade = novaQuantidade;
        }
    }

    [Header("Configuração")]
    [SerializeField] private int quantidadeMaximaDeSlots = 12;

    [Header("Itens armazenados")]
    [SerializeField] private List<InventorySlot> itens = new();

    public IReadOnlyList<InventorySlot> Itens => itens;

    public bool AdicionarItem(
        string itemId,
        string itemNome,
        int quantidade
    )
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            Debug.LogWarning("Tentativa de adicionar item sem ID.");
            return false;
        }

        if (quantidade <= 0)
        {
            Debug.LogWarning(
                "A quantidade precisa ser maior que zero."
            );

            return false;
        }

        InventorySlot slotExistente = itens.Find(
            slot => slot.itemId == itemId
        );

        if (slotExistente != null)
        {
            slotExistente.quantidade += quantidade;

            Debug.Log(
                $"{itemNome} adicionado. Total: " +
                $"{slotExistente.quantidade}"
            );

            return true;
        }

        if (itens.Count >= quantidadeMaximaDeSlots)
        {
            Debug.Log("Inventário cheio.");
            return false;
        }

        InventorySlot novoSlot = new(
            itemId,
            itemNome,
            quantidade
        );

        itens.Add(novoSlot);

        Debug.Log(
            $"{itemNome} entrou no inventário. " +
            $"Quantidade: {quantidade}"
        );

        return true;
    }

    public bool PossuiItem(
        string itemId,
        int quantidadeNecessaria = 1
    )
    {
        InventorySlot slot = itens.Find(
            item => item.itemId == itemId
        );

        return slot != null &&
               slot.quantidade >= quantidadeNecessaria;
    }

    public bool RemoverItem(
        string itemId,
        int quantidade
    )
    {
        if (quantidade <= 0)
            return false;

        InventorySlot slot = itens.Find(
            item => item.itemId == itemId
        );

        if (slot == null || slot.quantidade < quantidade)
            return false;

        slot.quantidade -= quantidade;

        if (slot.quantidade <= 0)
            itens.Remove(slot);

        return true;
    }
}