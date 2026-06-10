using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public float playerHealth = 100f;
    public List<ItemData> items = new List<ItemData>();

    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public int quantity;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject pauseMenu;
    public KeyCode toggleKey = KeyCode.Escape;

    private bool isPaused = false;
    private string savePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        savePath = Application.persistentDataPath + "/savegame.dat";
        Debug.Log($"Путь сохранения: {savePath}");
    }

    void Start()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseMenu != null)
            pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void SaveGame()
    {
        Debug.Log("🔵 SaveGame ВЫЗВАН!");

        SaveData data = new SaveData();

        data.sceneName = SceneManager.GetActiveScene().name;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
            data.playerPosZ = player.transform.position.z;

            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
                data.playerHealth = stats.health;
        }

        if (Inventory.Instance != null)
        {
            foreach (Item item in Inventory.Instance.GetItems())
            {
                SaveData.ItemData itemData = new SaveData.ItemData
                {
                    itemName = item.itemName,
                    quantity = item.quantity
                };
                data.items.Add(itemData);
                Debug.Log($"Сохранён предмет: {item.itemName}");
            }
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(savePath);
        bf.Serialize(file, data);
        file.Close();

        Debug.Log("Игра сохранена!");
        if (HintManager.Instance != null)
            HintManager.Instance.ShowHint("Игра сохранена!", 2f);
    }

    public void AutoSave()
    {
        SaveData data = new SaveData();

        data.sceneName = SceneManager.GetActiveScene().name;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
            data.playerPosZ = player.transform.position.z;

            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
                data.playerHealth = stats.health;
        }

        if (Inventory.Instance != null)
        {
            foreach (Item item in Inventory.Instance.GetItems())
            {
                SaveData.ItemData itemData = new SaveData.ItemData
                {
                    itemName = item.itemName,
                    quantity = item.quantity
                };
                data.items.Add(itemData);
            }
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(savePath);
        bf.Serialize(file, data);
        file.Close();

        Debug.Log($"Автосохранение в сцене: {data.sceneName}");
    }

    public void LoadGame()
    {
        Debug.Log("🔵 LoadGame ВЫЗВАН!");

        if (File.Exists(savePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(savePath, FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();

            if (SceneManager.GetActiveScene().name != data.sceneName)
            {
                PlayerPrefs.SetInt("LoadAfterScene", 1);
                PlayerPrefs.Save();
                SceneManager.LoadScene(data.sceneName);
                return;
            }

            ApplySaveData(data);
        }
        else
        {
            Debug.Log("Сохранение не найдено!");
            if (HintManager.Instance != null)
                HintManager.Instance.ShowHint("Сохранение не найдено!", 2f);
        }

        ResumeGame();
    }

    private void ApplySaveData(SaveData data)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.health = data.playerHealth;
                if (stats.healthSlider != null)
                    stats.healthSlider.value = data.playerHealth;
            }
        }

        if (Inventory.Instance != null)
        {
            // Получаем текущий инвентарь
            var currentItems = Inventory.Instance.GetItems();

            // Для каждого сохранённого предмета
            foreach (var itemData in data.items)
            {
                bool found = false;

                // Проверяем, есть ли уже такой предмет в инвентаре
                foreach (var existingItem in currentItems)
                {
                    if (existingItem.itemName == itemData.itemName)
                    {
                        found = true;
                        break;
                    }
                }

                // Если нет — добавляем
                if (!found)
                {
                    Item item = ScriptableObject.CreateInstance<Item>();
                    item.itemName = itemData.itemName;
                    Inventory.Instance.AddItem(item, itemData.quantity);
                    Debug.Log($"Добавлен предмет из сохранения: {itemData.itemName}");
                }
                else
                {
                    Debug.Log($"Предмет {itemData.itemName} уже есть в инвентаре, пропускаем");
                }
            }
        }

        Debug.Log("Игра загружена!");
        if (HintManager.Instance != null)
            HintManager.Instance.ShowHint("Игра загружена!", 2f);
    }

    public void ExitGame()
    {
        Debug.Log("🔵 ExitGame ВЫЗВАН!");
        Debug.Log("Выход из игры...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ChangeScene(string sceneName)
    {
        AutoSave();
        SceneManager.LoadScene(sceneName);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Transform pauseMenuTransform = canvas.transform.Find("PauseMenu");
            if (pauseMenuTransform != null)
                pauseMenu = pauseMenuTransform.gameObject;
        }

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (PlayerPrefs.GetInt("LoadAfterScene", 0) == 1)
        {
            PlayerPrefs.SetInt("LoadAfterScene", 0);
            PlayerPrefs.Save();
            LoadGame();
        }
    }
}