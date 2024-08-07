using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public Settings settings;
    [SerializeField] private Transform Hostages;
    private int hostageCount;
    [SerializeField] private Color onColor, offColor, onToggle, offToggle;

    #region Lights

    private Light2D globalLight;
    private Transform others;

    #endregion

    #region Sounds

    [Header("Audio Sources")] [SerializeField]
    private AudioSource sfx;

    [SerializeField] private AudioSource sfxEnemys;

    [Header("Sounds")] [SerializeField] private AudioClip[] soundsinGame;

    #endregion

    #region Cursor

    [Header("Cursor")] [SerializeField] private Texture2D[] cursorTexture;

    #endregion

    #region Canvas

    [Header("Canvas")] [SerializeField] private Toggle myToggle;
    [SerializeField] private Image handle;
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private GameObject howToPlay, levelselectionpanel;

    #endregion

    private void Awake()
    {
        if (Hostages != null)
            hostageCount = Hostages.childCount;

        globalLight = GameObject.FindWithTag("GlobalLight")?.GetComponent<Light2D>();
        others = GameObject.FindWithTag("Others")?.transform;
    }

    private void Start()
    {
        UpdateLight();
        if (others != null)
            ChangeCursor(0);

        if (myToggle != null)
            RememberJuice();
    }

    public bool SavedHostage()
    {
        hostageCount--;
        if (hostageCount == 0)
        {
            int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (activeSceneIndex != 5)
                settings.levels[activeSceneIndex - 1] = true;
            return true;
        }

        return false;
    }

    public void PlaySfx(int value)
    {
        if (settings.isJuice)
        {
            sfx.PlayOneShot(soundsinGame[value]);
        }
    }

    void UpdateLight()
    {
        if (globalLight != null)
        {
            if (settings.isJuice)
            {
                globalLight.intensity = 0.2f;
                if (others != null)
                {
                    for (int i = 0; i < others.childCount; i++)
                    {
                        if (others.GetChild(i).GetChild(0).GetComponent<Light2D>() != null)
                        {
                            Light2D light2D = others.GetChild(i).GetChild(0).GetComponent<Light2D>();
                            light2D.enabled = true;
                        }
                    }
                }
            }
            else
            {
                globalLight.intensity = 1f;
                if (others != null)
                {
                    for (int i = 0; i < others.childCount; i++)
                    {
                        if (others.GetChild(i).GetChild(0).GetComponent<Light2D>() != null)
                        {
                            Light2D light2D = others.GetChild(i).GetChild(0).GetComponent<Light2D>();
                            light2D.enabled = false;
                        }
                    }
                }
            }
        }
    }

    public void ChangeCursor(int value)
    {
        if (value == -1)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        Cursor.SetCursor(cursorTexture[value], Vector2.zero, CursorMode.Auto);
    }

    public void MyToggle()
    {
        settings.isJuice = myToggle.isOn;
        if (myToggle.isOn)
        {
            handle.transform.localPosition = new Vector3(-450f, 289f);
            handle.color = onColor;
            ColorBlock colors = myToggle.colors;
            colors.normalColor = onToggle;
            colors.selectedColor = onToggle;
            myToggle.colors = colors;
        }
        else
        {
            handle.transform.localPosition = new Vector3(-650f, 289f);
            handle.color = offColor;
            ColorBlock colors = myToggle.colors;
            colors.normalColor = offToggle;
            colors.selectedColor = offToggle;
            myToggle.colors = colors;
        }

        UpdateLight();
    }

    void RememberJuice()
    {
        myToggle.isOn = settings.isJuice;
        MyToggle();
        RememberLevels();
    }

    public void SelectLevel(int value)
    {
        SceneManager.LoadScene(value);
    }

    void RememberLevels()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = settings.levels[i];
        }
    }

    public void MainMenuButton(int value)
    {
        switch (value)
        {
            case 0:
                levelselectionpanel.SetActive(!levelselectionpanel.activeSelf);
                break;
            case 1:
                howToPlay.SetActive(!howToPlay.activeSelf);
                break;
            case 2:
                Application.Quit();
                break;
        }
    }
}