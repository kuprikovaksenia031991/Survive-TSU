using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryPanel;
    public Transform slotsContainer;
    public GameObject slotPrefab;

    [Header("Note Panel (для чтения записок)")]
    public GameObject notePanel;
    public TextMeshProUGUI noteTextDisplay;

    private bool isOpen = false;
    private Inventory inventory;
    private Item currentNote;

    void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        if (notePanel != null)
            notePanel.SetActive(false);

        inventory = Inventory.Instance;
        if (inventory != null)
            inventory.OnInventoryChanged += RefreshUI;

        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && currentNote == null)
        {
            ToggleInventory();
        }

        if (currentNote != null && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)))
        {
            CloseNote();
        }
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            RefreshUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    void RefreshUI()
    {
        if (slotsContainer == null || slotPrefab == null) return;

        foreach (Transform child in slotsContainer)
            Destroy(child.gameObject);

        foreach (Item item in inventory.GetItems())
        {
            GameObject slot = Instantiate(slotPrefab, slotsContainer);

            // Иконка
            Transform iconTransform = slot.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image icon = iconTransform.GetComponent<Image>();
                if (icon != null && item.icon != null)
                    icon.sprite = item.icon;
            }

            // Название
            Transform nameTransform = slot.transform.Find("NameText");
            if (nameTransform != null)
            {
                TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = item.itemName;
            }

            // Количество
            Transform amountTransform = slot.transform.Find("AmountText");
            if (amountTransform != null)
            {
                TextMeshProUGUI amountText = amountTransform.GetComponent<TextMeshProUGUI>();
                if (amountText != null && item.quantity > 1)
                    amountText.text = item.quantity.ToString();
                else if (amountText != null)
                    amountText.text = "";
            }

            // Кнопка
            Button button = slot.GetComponent<Button>();
            if (button == null)
                button = slot.AddComponent<Button>();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => UseItem(item));
        }
    }

    void UseItem(Item item)
    {
        // Записка
        if (item.isNote)
        {
            OpenNote(item);
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        // Аптечка
        if (item.itemName == "Аптечка" || item.itemName == "Medkit")
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                float newHealth = Mathf.Min(stats.health + 30, 100);
                stats.health = newHealth;
                if (stats.healthSlider != null)
                    stats.healthSlider.value = newHealth;

                HintManager.Instance?.ShowHint($"Вылечено! Здоровье: {newHealth}/100", 2f);
                inventory.RemoveItem(item, 1);
                RefreshUI();

                if (inventory.GetItems().Count == 0)
                    ToggleInventory();
            }
        }
        // Нож
        else if (item.itemName == "Нож" || item.itemName == "Knife")
        {
            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            if (attack != null)
            {
                attack.SetWeaponType(1);
                HintManager.Instance?.ShowHint("Нож экипирован! Урон повышен", 2f);
                ToggleInventory();
            }
        }
        // Огнетушитель
        else if (item.itemName == "Огнетушитель" || item.itemName == "FireExtinguisher")
        {
            PlayerAttack attack = player.GetComponent<PlayerAttack>();
            if (attack != null)
            {
                attack.SetWeaponType(2);
                HintManager.Instance?.ShowHint("Огнетушитель экипирован!", 2f);
                ToggleInventory();
            }
        }
    }

    void OpenNote(Item note)
    {
        if (notePanel == null)
        {
            Debug.LogError("notePanel не назначен в InventoryUI! Создайте NotePanel и назначьте его.");
            Debug.Log($"Текст записки: {note.noteText}");
            return;
        }

        if (noteTextDisplay == null)
        {
            Debug.LogError("noteTextDisplay не назначен в InventoryUI!");
            Debug.Log($"Текст записки: {note.noteText}");
            return;
        }

        currentNote = note;
        inventoryPanel.SetActive(false);

        notePanel.SetActive(true);
        noteTextDisplay.text = note.noteText;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    void CloseNote()
    {
        currentNote = null;
        notePanel.SetActive(false);
        inventoryPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshUI;
    }
}