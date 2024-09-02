using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    Canvas permanentCanvas;
    Canvas menuCanvas;

    void Awake()
    {
        permanentCanvas = transform.Find("PermanentCanvas").GetComponent<Canvas>();
        menuCanvas = transform.Find("MenuCanvas").GetComponent<Canvas>();

        menuCanvas.enabled = false;
        UpdateCursorLock(!menuCanvas.enabled);
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
            GameObject.FindFirstObjectByType<PlayerMovement>().enabled = !menuCanvas.enabled;
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
        GameObject.FindFirstObjectByType<PlayerMovement>().Disconnect();

        UpdateCursorLock(false);

        if (gameEnded)
        {
            SceneManager.LoadScene("WinningScreen");
        }
        else
        {
            SceneManager.LoadScene("StartMenu");
        }
    }
}
