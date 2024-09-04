using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    Canvas permanentCanvas;
    Canvas menuCanvas;
    Canvas messageCanvas;

    void Awake()
    {
        permanentCanvas = transform.Find("PermanentCanvas").GetComponent<Canvas>();
        menuCanvas = transform.Find("MenuCanvas").GetComponent<Canvas>();
        messageCanvas = transform.Find("MessageCanvas").GetComponent<Canvas>();

        menuCanvas.enabled = false;
        UpdateCursorLock(!menuCanvas.enabled);

        messageCanvas.enabled = false;
    }

    void UpdateCursorLock(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Input.GetKey(KeyCode.LeftShift))
        {
            ExitGame();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            menuCanvas.enabled = !menuCanvas.enabled;
            UpdateCursorLock(!menuCanvas.enabled);

            if (GetPlayer() is not null)
            {
                GetPlayer().movementEnabled = !menuCanvas.enabled;
            }
        }
    }

    public void UpdateDistance(float distance)
    {
        permanentCanvas.transform.Find("DistanceText").GetComponent<TMP_Text>().text = $"Distance: {(int)Mathf.Floor(distance)} pc";
    }

    public void UpdateOxygen(float oxygen)
    {
        permanentCanvas.transform.Find("OxygenText").GetComponent<TMP_Text>().text = $"Oxygen level: {(int)Mathf.Floor(oxygen * 100)}%";
    }

    public void UpdateServerAddress(string address)
    {
        permanentCanvas.transform.Find("AddressText").GetComponent<TMP_Text>().text = "Server address: " + address;
    }

    public void ExitGame(bool gameEnded = false)
    {
        UpdateCursorLock(false);

        if (GetPlayer() is not null)
        {
            GetPlayer().Disconnect();
        }

        if (gameEnded)
        {
            SceneManager.LoadScene("WinningScreen");
        }
        else
        {
            SceneManager.LoadScene("StartMenu");
        }
    }

    void OnApplicationQuit()
    {
        ExitGame();
    }

    public Canvas GetMessageCanvas()
    {
        return messageCanvas;
    }

    PlayerMovement GetPlayer()
    {
        foreach (PlayerMovement player in GameObject.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            if (player.enabled)
            {
                return player;
            }
        }
        return null;
    }
}
