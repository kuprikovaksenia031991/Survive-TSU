using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    [SerializeField] private List<Item> items = new List<Item>();
    [SerializeField] private int maxSlots = 20;

    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool AddItem(Item item, int amount = 1)
    {
        // Поиск существующего стакаемого предмета
        if (item.isStackable)
        {
            foreach (var existingItem in items)
            {
                if (existingItem == item)
                {
                    existingItem.quantity += amount;
                    OnInventoryChanged?.Invoke();
                    Debug.Log($"Добавлено {amount} {item.itemName}. Теперь в инвентаре: {existingItem.quantity}");
                    return true;
                }
            }
        }

        if (items.Count >= maxSlots)
        {
            Debug.Log("Инвентарь полон!");
            return false;
        }

        Item newItem = Instantiate(item);
        newItem.quantity = amount;
        items.Add(newItem);
        OnInventoryChanged?.Invoke();
        Debug.Log($"Предмет {item.itemName} добавлен в инвентарь");
        return true;
    }

    public List<Item> GetItems() => items;

    public bool HasItem(string itemName)
    {
        return items.Exists(i => i.itemName == itemName);
    }

    public void RemoveItem(Item item, int amount = 1)
    {
        for (int i = items.Count - 1; i >= 0 && amount > 0; i--)
        {
            if (items[i] == item)
            {
                int toRemove = Mathf.Min(amount, items[i].quantity);
                items[i].quantity -= toRemove;
                amount -= toRemove;

                if (items[i].quantity <= 0)
                    items.RemoveAt(i);
            }
        }
        OnInventoryChanged?.Invoke();
        Debug.Log($"Удалён предмет {item.itemName}");
    }
}