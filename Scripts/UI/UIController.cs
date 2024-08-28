using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public GameObject addressInput;
    public TMP_Text addressErrorText;

    public GameObject nameInput;
    public TMP_Text nameErrorText;

    const string ADDRESS_ERROR = "Address must be in format x.x.x.x:port!";
    const string NAME_ERROR = "Nickname must be nonempty!";

    void Start()
    {
        TryLoadPreviousData();
    }

    void TryLoadPreviousData()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            nameInput.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString("PlayerName");
        }
        if (PlayerPrefs.HasKey("ServerAddress"))
        {
            addressInput.GetComponent<TMP_InputField>().text = PlayerPrefs.GetString("ServerAddress");
        }
    }

    public void CheckAddressField()
    {
        addressErrorText.text = "";

        if (!IsAddressFieldCorrect())
        {
            addressErrorText.text = ADDRESS_ERROR;
        }
        else
        {
            PlayerPrefs.SetString("ServerAddress", addressInput.GetComponent<TMP_InputField>().text);
        }
    }

    bool IsAddressFieldCorrect()
    {
        try
        {
            string text = addressInput.GetComponent<TMP_InputField>().text;
            string[] addressAndPort = text.Split(':');
            if (addressAndPort.Length != 2)
            {
                return false;
            }

            string[] addressParts = addressAndPort[0].Split(".");
            if (addressParts.Length != 4)
            {
                return false;
            }

            foreach (string addressPart in addressParts)
            {
                int part = int.Parse(addressPart);
                if (part < 0 || part >= 256)
                {
                    return false;
                }
            }

            int portNumber = int.Parse(addressAndPort[1]);
            if (portNumber < 1024 || portNumber >= 65536)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
        return true;
    }

    public void CheckNameField()
    {
        nameErrorText.text = "";

        if (!IsNameFieldCorrect())
        {
            nameErrorText.text = NAME_ERROR;
        }
        else
        {
            PlayerPrefs.SetString("PlayerName", nameInput.GetComponent<TMP_InputField>().text);
        }
    }

    bool IsNameFieldCorrect()
    {
        string text = nameInput.GetComponent<TMP_InputField>().text;
        return text.Trim().Length > 0;
    }

    bool IsAllDataFieldsCorrect()
    {
        CheckAddressField();
        CheckNameField();
        return IsAddressFieldCorrect() && IsNameFieldCorrect();
    }

    public void TryProceedAsServer()
    {
        if (IsNameFieldCorrect())
        {
            PlayerPrefs.SetString("PlayerName", nameInput.GetComponent<TMP_InputField>().text);

            SceneManager.LoadScene("ServerGame");
        }
    }

    public void TryProceedAsClient()
    {
        if (IsAllDataFieldsCorrect())
        {
            PlayerPrefs.SetString("PlayerName", nameInput.GetComponent<TMP_InputField>().text);
            PlayerPrefs.SetString("ServerAddress", addressInput.GetComponent<TMP_InputField>().text);

            SceneManager.LoadScene("ClientGame");
        }
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }
}
