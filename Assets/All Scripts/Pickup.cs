using UnityEngine;
using TMPro;

public class Pickup : MonoBehaviour
{
    [SerializeField] private Item item;
    [SerializeField] private int amount = 1;

    [Header("UI Подсказка")]
    [SerializeField] private GameObject hintPanel;

    private bool isPlayerNear = false;

    private void Start()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (hintPanel != null)
            {
                TextMeshProUGUI text = hintPanel.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null && item != null)
                {
                    if (item.isNote)
                        text.text = $"Нажмите [E], чтобы взять записку";
                    else
                        text.text = $"Нажмите [E], чтобы взять {item.itemName}";
                }
                hintPanel.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }
    }

    public void TryPickup()
    {
        Debug.Log($"🔥 Pickup.TryPickup вызван на {gameObject.name}!");

        if (!isPlayerNear) return;

        if (item == null)
        {
            Debug.LogError($"На {gameObject.name} не назначен предмет!");
            return;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogError("Inventory.Instance не найден!");
            return;
        }

        if (Inventory.Instance.AddItem(item, amount))
        {
            if (item.isNote)
                HintManager.Instance?.ShowHint($"Вы взяли записку! Нажмите [I] для инвентаря, чтобы прочитать", 3f);
            else
                HintManager.Instance?.ShowHint($"Вы взяли {item.itemName}! Нажмите [I] для инвентаря", 2.5f);

            Destroy(gameObject);
            if (hintPanel != null)
                hintPanel.SetActive(false);
        }
        else
        {
            HintManager.Instance?.ShowHint("Инвентарь полон!", 1.5f);
        }
    }
}