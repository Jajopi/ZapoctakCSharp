using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinningScreenUI : MonoBehaviour
{
    void Start()
    {
        float distance = PlayerPrefs.HasKey("Distance") ? PlayerPrefs.GetFloat("Distance") : 0;
        transform.Find("DistanceText").GetComponent<TMP_Text>().text = $"Distance reached: {distance}";

        float bestDistance = PlayerPrefs.HasKey("BestDistance") ? PlayerPrefs.GetFloat("BestDistance") : 0;
        transform.Find("BestDistanceText").GetComponent<TMP_Text>().text = $"Best distance: {bestDistance}";
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
