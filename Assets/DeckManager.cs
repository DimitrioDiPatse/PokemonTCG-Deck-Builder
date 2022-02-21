using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using System.Linq;

public class DeckManager : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform availablesViewport;
    [SerializeField] Transform deckViewport;
    [SerializeField] Image availableCardPreview;
    [SerializeField] Image deckCardPreview;
    [SerializeField] Button toDeckButton;
    [SerializeField] Button toAvailablesButton;
    [SerializeField] Transform cardRestPosition;


    public enum SortType { Type, Hp, Rarity}
    [HideInInspector]
    public List<GameObject> availableCards = new List<GameObject>();
    public List<GameObject>[] decks = new List<GameObject>[3];

    public int activeDeck { get; set; }

    PokemonTcgApi api;

    CardInfo deckSelectedCard;
    CardInfo availablesSelectedCard;
    

    private void Start()
    {
        decks[0] = new List<GameObject>();
        decks[1] = new List<GameObject>();
        decks[2] = new List<GameObject>();

        api = GetComponent<PokemonTcgApi>();

        toAvailablesButton.interactable = false;
        toDeckButton.interactable = false;
        availableCardPreview.enabled = false;
        deckCardPreview.enabled = false;

        SetContrainerSizes();
    }

    /// <summary>
    /// Set the sizes of the containers cell size by calculating the screen size.
    /// Best use for diffenern sreen ratios.
    /// </summary>
    void SetContrainerSizes()
    {
        float width = deckViewport.GetComponent<RectTransform>().rect.width;
        float height = width * 1.39f;
        Vector2 newSize = new Vector2(width/6.1f, height/6.1f);

        var contD = deckViewport.GetComponent<GridLayoutGroup>();
        contD.cellSize = newSize;
        contD.spacing = new Vector2(width / 24, height / 23);

        var contA = availablesViewport.GetComponent<GridLayoutGroup>();
        contA.cellSize = newSize;
        contA.spacing = new Vector2(width / 24, height / 23);
    }

    /// <summary>
    /// Sorts the availables stack by the given SortType
    /// </summary>
    public void SortAvailablesCardsBy(int sortType)
    {
        SortType type = (SortType)sortType;
        switch (type)
        {
            case SortType.Type:
                availableCards = availableCards.OrderBy(x => x.GetComponent<CardInfo>().data.pokeType).ToList();
                break;
            case SortType.Hp:
                availableCards = availableCards.OrderBy(x => x.GetComponent<CardInfo>().data.hp).ToList();
                break;
            case SortType.Rarity:
                availableCards = availableCards.OrderBy(x => x.GetComponent<CardInfo>().data.rarity).ToList();
                break;
            default:
                break;
        }
        foreach (var card in availableCards)
        {
            card.transform.transform.SetParent(cardRestPosition);
            card.transform.localPosition = cardRestPosition.localPosition;
        }
        for (int i = availableCards.Count()-1; i >= 0; i--)
        {
            availableCards[i].transform.SetParent(availablesViewport.transform);
        }
    }
    /// <summary>
    /// Sorts the decks by the given SortType
    /// </summary>
    public void SortDeckCardsBy(int sortType)
    {
        SortType type = (SortType)sortType;
        switch (type)
        {
            case SortType.Type:
                decks[activeDeck] = decks[activeDeck].OrderBy(x => x.GetComponent<CardInfo>().data.pokeType).ToList();
                break;
            case SortType.Hp:
                decks[activeDeck] = decks[activeDeck].OrderBy(x => x.GetComponent<CardInfo>().data.hp).ToList();
                break;
            case SortType.Rarity:
                decks[activeDeck] = decks[activeDeck].OrderBy(x => x.GetComponent<CardInfo>().data.rarity).ToList();
                break;
            default:
                break;
        }

        foreach (var card in decks[activeDeck])
        {
            card.transform.transform.SetParent(cardRestPosition);
            card.transform.localPosition = cardRestPosition.localPosition;
        }

        for (int j = decks[activeDeck].Count() - 1; j >= 0; j--)
        {
            decks[activeDeck][j].transform.SetParent(deckViewport.transform);
        }

        GetComponent<UI_Manager>().PositionDeckForPreview(activeDeck);
    }

    /// <summary>
    /// Creates cards with proper card data for the availables stack.
    /// Sets the UiText of the card and the buttons listeners.
    /// Called by the API when the call for availables Set is complete.
    /// </summary>
    public void LoadAvailablesStack(JSONNode cardsData)
    {
        for (int i = 0; i < cardsData["data"].Count; i++)
        {
            GameObject card = (GameObject)Instantiate(cardPrefab, availablesViewport);
            CardInfo cardInfo = card.GetComponent<CardInfo>();

            CardData data = new CardData
            {
                id = cardsData["data"][i]["id"],
                pokeName = cardsData["data"][i]["name"],
                rarity = cardsData["data"][i]["rarity"],
                pokeType = cardsData["data"][i]["types"][0],
                hp = cardsData["data"][i]["hp"],
                imgUrl = cardsData["data"][i]["images"]["small"],
            };

            cardInfo.data = data;
            
            StartCoroutine(api.ImageRequest(card));

            AddCardToAvailables(card);
            card.GetComponent<Button>().onClick.AddListener(()=> LoadCardForPreview(card));
        }
        GetComponent<UI_Manager>().callSetsStarted++;
    }


    /// <summary>
    /// Creates cards with proper card data for the deck Sets.
    /// Sets the UiText of the card and the buttons listeners.
    /// Called by the API when the call for deck Sets is complete.
    /// </summary>
    public void LoadDeckCardInfo(List<string>[] nameList)
    {
        for (int i = 0; i < nameList.Length; i++)
        {
            if (nameList[i] != null)
            {
                foreach (var pokeId in nameList[i])
                {
                    GameObject card = Instantiate(cardPrefab, deckViewport) as GameObject;
                    CardInfo cardInfo = card.GetComponent<CardInfo>();
                    cardInfo.data.id = pokeId;
                    cardInfo.data.onDeck = true;
                    StartCoroutine(api.ProcessRequestById(cardInfo));
                    AddCardToDeck(card, i);
                    card.GetComponent<Button>().onClick.AddListener(() => LoadCardForPreview(card));
                }
            }
        }
        GetComponent<UI_Manager>().callSetsStarted++;
    }

    /// <summary>
    /// Loads card's image for preview and manages the button's listeners.
    /// Also activate the Add/Remove buttons
    /// </summary>
    public void LoadCardForPreview(GameObject card)
    {
        CardInfo cardInfo = card.GetComponent<CardInfo>();
        Button cardButton = card.GetComponent<Button>();

        if (!cardInfo.data.onDeck)
        {
            if (availablesSelectedCard)
            {
                Button but = availablesSelectedCard.GetComponent<Button>();
                but.onClick.RemoveAllListeners();
                but.onClick.AddListener(() => LoadCardForPreview(but.gameObject));
                availablesSelectedCard.GetComponent<Outline>().enabled = false;
            }
            
            availablesSelectedCard = cardInfo;

            availableCardPreview.sprite = cardInfo.image;
            availableCardPreview.enabled = true;
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => AddCardToDeck(card, activeDeck));
            
            
            toDeckButton.interactable = true;
            toDeckButton.onClick.RemoveAllListeners();
            toDeckButton.onClick.AddListener(() => AddCardToDeck(card, activeDeck));          
        }
        else
        {
            if (deckSelectedCard)
            {
                Button but = deckSelectedCard.GetComponent<Button>();
                but.onClick.RemoveAllListeners();
                but.onClick.AddListener(() => LoadCardForPreview(but.gameObject));
                deckSelectedCard.GetComponent<Outline>().enabled = false;
            }
            deckSelectedCard = cardInfo;

            deckCardPreview.sprite = cardInfo.image;
            deckCardPreview.enabled = true;
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => AddCardToAvailablesFromDeck(card));

            toAvailablesButton.interactable = true;
            toAvailablesButton.onClick.RemoveAllListeners();
            toAvailablesButton.onClick.AddListener(() => AddCardToAvailablesFromDeck(card));
        }
        card.GetComponent<Outline>().enabled = true;
    }

    /// <summary>
    /// Adds Cards to Availables and wieport. !!For start load only!!
    /// </summary>
    void AddCardToAvailables(GameObject card)
    {
        availableCards.Add(card);
        card.transform.SetParent(availablesViewport);
    }
    /// <summary>
    /// Takes a card from the availables stack and places it to deck
    /// </summary>
    public void AddCardToAvailablesFromDeck(GameObject card)
    {
        availableCards.Add(card);
        decks[activeDeck].Remove(card);

        deckCardPreview.enabled = false;
        card.transform.SetParent(availablesViewport);

        card.GetComponent<CardInfo>().data.onDeck = false;

        Button b = card.GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => LoadCardForPreview(card));

        card.GetComponent<Outline>().enabled = false;

        toAvailablesButton.interactable = false;
        toAvailablesButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Removes a card from the deck and places it to availables
    /// </summary>
    public void AddCardToDeck(GameObject card, int deckNumber)
    {
        decks[deckNumber].Add(card);
        availableCards.Remove(card);

        availableCardPreview.enabled = false;
        card.transform.SetParent(deckViewport);

        card.GetComponent<CardInfo>().data.onDeck = true;

        Button b = card.GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => LoadCardForPreview(card));
        
        card.GetComponent<Outline>().enabled = false;

        toDeckButton.interactable = false;
        toDeckButton.onClick.RemoveAllListeners();
    }
}