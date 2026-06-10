using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName = "Предмет";
    public string description = "Описание";
    public Sprite icon;
    public int quantity = 1;
    public bool isStackable = true;

    [Header("Для записок")]
    [TextArea(5, 15)]
    public string noteText = ""; // Текст записки
    public bool isNote = false;   // Это записка?
}