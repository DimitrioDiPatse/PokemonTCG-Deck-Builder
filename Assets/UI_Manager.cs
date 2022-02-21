using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Threading.Tasks;

public class UI_Manager : MonoBehaviour
{
    [Header("Home Screen")]
    [SerializeField] RectTransform blackout;
    [SerializeField] Button deckButton;
    [SerializeField] Button aboutButton;
    [SerializeField] Button quitButton;
    [Header("Deck Builder Screen")]
    [SerializeField] Button returnHomeButton;
    [Header("About Screen")]
    [SerializeField] Button returnHomeButtonn;
    [Header("General Settings")]
    [SerializeField] Ease easeType = Ease.InOutElastic;
    [SerializeField] float panelTweenTime = 1f;
    [SerializeField] RectTransform sortPanel;

    DeckManager deckManager;

    ScreenId activePanel;
    RectTransform loadingScreen, homeScreen, builderScreen, aboutScreen;

    Vector3 centerPos = new Vector3(Screen.width / 2, Screen.height / 2);
    Vector3 loadPos, builderPos, aboutPos;
    bool loaded;
    GameObject[] sortButtons;

    public int callsActive { get; set; }
    public int callSetsStarted { get; set; }

    void Start()
    {
        deckManager = GetComponent<DeckManager>();

        SetScreenAndPositions();
        SetupButtons();
        SetScreen(ScreenId.Loading);
    }

    /// <summary>
    /// Setup the start position of every scene image
    /// </summary>
    void SetScreenAndPositions()
    {
        var screens = GameObject.FindGameObjectsWithTag("SceneScreen");
        foreach (var scr in screens)
        {
            if (scr.name == "_LoadingScreen")
            {
                loadingScreen = scr.GetComponent<RectTransform>();
                loadingScreen.localPosition = Vector3.left * Screen.width;
                loadPos = loadingScreen.localPosition + centerPos;
            }
            else if (scr.name == "_HomeScreen")
            {
                homeScreen = scr.GetComponent<RectTransform>();
            }
            else if (scr.name == "_BuilderScreen")
            {
                builderScreen = scr.GetComponent<RectTransform>();
                builderScreen.localPosition = Vector3.up * Screen.height;
                builderPos = builderScreen.localPosition + centerPos;
            }
            else if (scr.name == "_AboutScreen")
            {
                aboutScreen = scr.GetComponent<RectTransform>();
                aboutScreen.localPosition = Vector3.right * Screen.width;
                aboutPos = aboutScreen.localPosition + centerPos;
            }
            else
            {
                Debug.LogError("Wrong naming in Screen Panels");
            }
        }
        

    }

    /// <summary>
    /// Async method that holds the LoadingScreen in view until all image
    /// calls og the API are completed.
    /// </summary>
    async void LoadingScreenWait()
    {
        //Ensures that the first image call is done
        while (callSetsStarted == 0) { await Task.Delay(300); }
        
        //Ensures that both the deck and availables started calling for images
        while (callSetsStarted < 2) { await Task.Delay(300); }     
       
        //Ensures that images are downloaded
        while (callsActive > 0) { await Task.Delay(300); }

        SetScreen(ScreenId.Home);
    }

    /// <summary>
    /// Async text animation for the LoadingScreen text
    /// </summary>
    async void LoadingTextAnimation()
    {
        TextMeshProUGUI text = loadingScreen.gameObject.GetComponentInChildren<TextMeshProUGUI>();
        int delay = 700;
        while (activePanel == ScreenId.Loading)
        {
            text.text = "Loading      ";
            await Task.Delay(delay);
            text.text = "Loading .    ";
            await Task.Delay(delay);
            text.text = "Loading . .  ";
            await Task.Delay(delay);
            text.text = "Loading . . .";
            await Task.Delay(delay);
        }
    }

    /// <summary>
    /// Setups the listeners of the buttons
    /// </summary>
    void SetupButtons()
    {
        sortButtons = GameObject.FindGameObjectsWithTag("SortButtons");
        sortPanel.gameObject.SetActive(false);
        deckButton.onClick.AddListener(() => SetScreen(ScreenId.DeckBuilder));
        deckButton.onClick.AddListener(() => PositionDeckForPreview(0));
        aboutButton.onClick.AddListener(() => SetScreen(ScreenId.About));
        quitButton.onClick.AddListener(() => Application.Quit());
        returnHomeButton.onClick.AddListener(() => SetScreen(ScreenId.Home));
        returnHomeButtonn.onClick.AddListener(() => SetScreen(ScreenId.Home));
    }

    /// <summary>
    /// Manages screen position change for changing scenes
    /// </summary>
    public void SetScreen(ScreenId screenId)
    {
        switch (screenId)
        {
            case ScreenId.Loading:
                loadingScreen.DOMove(centerPos, 0);
                activePanel = ScreenId.Loading;
                LoadingTextAnimation();
                LoadingScreenWait();
                blackout.gameObject.SetActive(false);
                break;
            case ScreenId.Home:
                homeScreen.DOMove(centerPos, 0);
                switch (activePanel)
                { 
                    case ScreenId.Loading:
                        loadingScreen.DOMove(loadPos, panelTweenTime).SetEase(easeType);
                        break;
                    case ScreenId.DeckBuilder:
                        builderScreen.DOMove(builderPos, panelTweenTime).SetEase(easeType);
                        break;
                    case ScreenId.About:
                        aboutScreen.DOMove(aboutPos, panelTweenTime).SetEase(easeType);
                        break;
                    default:
                        break;
                }  
                activePanel = ScreenId.Home;
                break;
            case ScreenId.DeckBuilder:
                builderScreen.DOMove(centerPos, panelTweenTime).SetEase(easeType);
                activePanel = ScreenId.DeckBuilder;
                break;
            case ScreenId.About:
                aboutScreen.DOMove(centerPos, panelTweenTime).SetEase(easeType);
                activePanel = ScreenId.About;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Deactivates the cards of the NON activeDeck so to show 
    /// only the activeDeck's card in the container.
    /// </summary>
    public void PositionDeckForPreview(int deckNumber)
    {
        //Is Called on Start so to have only the 1st deckSet visible
        if (!loaded)
        {
            foreach (var card in deckManager.decks[1]){ card.SetActive(false); }
            foreach (var card in deckManager.decks[2]) { card.SetActive(false); }
            loaded = true;
        }

        //Called by DeckSet buttons
        if (deckManager.activeDeck != deckNumber)
        {
            foreach (var card in deckManager.decks[deckManager.activeDeck])
            {
                card.SetActive(false);
            }
            foreach (var card in deckManager.decks[deckNumber])
            {
                card.SetActive(true);
            }
        }
        deckManager.activeDeck = deckNumber;
    }

    /// <summary>
    /// Activates the SortPanel and set the proper listeners to the sortType buttons
    /// depending the stack that is called for.
    /// </summary>
    public void ShowSortTypes(int forWho)
    {
        sortPanel.gameObject.SetActive(true);

        // Sort Availables
        if (forWho == 0)
        {
            foreach (var but in sortButtons)
            {
                var b = but.GetComponent<Button>();
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => sortPanel.gameObject.SetActive(false));

                if (but.name == "Srt_Type") 
                    b.onClick.AddListener(() => deckManager.SortAvailablesCardsBy(0));  
                else if (but.name == "Srt_Hp")
                    b.onClick.AddListener(() => deckManager.SortAvailablesCardsBy(1));
                else if (but.name == "Srt_Rarity")
                    b.onClick.AddListener(() => deckManager.SortAvailablesCardsBy(2));
                else
                    Debug.LogError("Sort Buttons not properly named");
            }
        }
        //Sort Deck
        else if (forWho == 1) 
        {
            foreach (var but in sortButtons)
            {
                var b = but.GetComponent<Button>();
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => sortPanel.gameObject.SetActive(false));

                if (but.name == "Srt_Type")
                    b.onClick.AddListener(() => deckManager.SortDeckCardsBy(0));
                else if (but.name == "Srt_Hp")
                    b.onClick.AddListener(() => deckManager.SortDeckCardsBy(1));
                else if (but.name == "Srt_Rarity")
                    b.onClick.AddListener(() => deckManager.SortDeckCardsBy(2));
                else
                    Debug.LogError("Sort Buttons not properly named");
            }
        }
        
        else
        {
            Debug.LogError("ShowSortPanel buttons have wrong int id. Check in editor!");
        }
    }
}
public enum ScreenId
{
    Loading, Home, DeckBuilder, About
}

