using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Image slotBackground;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;

    [Header("Cores")]
    [SerializeField]
    private Color emptyColor =
        new Color(0.15f, 0.17f, 0.24f, 1f);

    [SerializeField]
    private Color filledColor =
        new Color(0.22f, 0.25f, 0.34f, 1f);

    private void Awake()
    {
        ConfigurarIcone();
        ClearSlot();
    }

    private void ConfigurarIcone()
    {
        if (itemIcon == null)
            return;

        itemIcon.type = Image.Type.Simple;
        itemIcon.preserveAspect = true;
        itemIcon.raycastTarget = false;
    }

    public void SetSlot(Sprite icon, int quantity)
    {
        if (slotBackground != null)
        {
            slotBackground.color = filledColor;
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.type = Image.Type.Simple;
            itemIcon.preserveAspect = true;
            itemIcon.enabled = icon != null;
            itemIcon.gameObject.SetActive(icon != null);
        }

        if (quantityText != null)
        {
            bool mostrarQuantidade = quantity > 1;

            quantityText.text = quantity.ToString();
            quantityText.gameObject.SetActive(
                mostrarQuantidade
            );
        }
    }

    public void ClearSlot()
    {
        if (slotBackground != null)
        {
            slotBackground.color = emptyColor;
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemIcon.gameObject.SetActive(false);
        }

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }
    }
}