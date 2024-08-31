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
            GameObject.FindFirstObjectByType<PlayerMovement>().enabled = !menuCanvas.enabled;
        }
    }

    public void UpdateScore(string score)
    {
        permanentCanvas.transform.Find("ScoreText").GetComponent<TMP_Text>().text = "Score: " + score;
    }

    public void UpdateServerAddress(string address)
    {
        permanentCanvas.transform.Find("AddressText").GetComponent<TMP_Text>().text = "Server address: " + address;
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
