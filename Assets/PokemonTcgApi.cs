using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using SimpleJSON;
using UnityEngine.UI;

public class PokemonTcgApi : MonoBehaviour
{
    const string URL = "https://api.pokemontcg.io/v2/cards";
    const string URLbyId = "https://api.pokemontcg.io/v2/cards?q=id:";
    const string API_KEY = "1efa4b33-16fc-4e6e-8a01-023a4ed4a8e6";

    UI_Manager ui;

    private void Start()
    {
        ui = GetComponent<UI_Manager>();
        StartCoroutine(ProcessRequestForAvailables(URL));
        
    }

    /// <summary>
    /// API call for first set of 250 cards to use as availables
    /// </summary>
    private IEnumerator ProcessRequestForAvailables(string uri)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            request.SetRequestHeader("X-Api-Key", API_KEY);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error);
            }
            else
            {
                JSONNode CardsData = JSON.Parse(request.downloadHandler.text);
                print(CardsData.ToString());
                GetComponent<DeckManager>().LoadAvailablesStack(CardsData);
            }
        }
    }

    /// <summary>
    /// API call specified by card id
    /// </summary>
    public IEnumerator ProcessRequestById(CardInfo cardInfo)
    {
        print(URLbyId + cardInfo.data.id.ToString());
        using (UnityWebRequest request = UnityWebRequest.Get(URLbyId + cardInfo.data.id.ToString()))
        {
            request.SetRequestHeader("X-Api-Key", API_KEY);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error);
            }
            else
            {
                JSONNode cardData = JSON.Parse(request.downloadHandler.text);

                cardInfo.data.pokeName = cardData["data"][0]["name"];
                cardInfo.data.rarity = cardData["data"][0]["rarity"];
                cardInfo.data.pokeType = cardData["data"][0]["types"][0];
                cardInfo.data.hp = cardData["data"][0]["hp"];
                cardInfo.data.imgUrl = cardData["data"][0]["images"]["small"];
                
                StartCoroutine(ImageRequest(cardInfo.gameObject));
            }
        }
    }

    /// <summary>
    /// API call for card image texture
    /// </summary>
    public IEnumerator ImageRequest(GameObject cardButton)
    {
        ui.callsActive++;
        CardData data = cardButton.GetComponent<CardInfo>().data;
        string url = data.imgUrl;
        UnityWebRequest itemImageRequest = UnityWebRequestTexture.GetTexture(url);

        yield return itemImageRequest.SendWebRequest();

        if (itemImageRequest.result == UnityWebRequest.Result.ConnectionError 
            || itemImageRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(itemImageRequest.error);
        }
        else
        {
            Texture2D tex;
            tex = DownloadHandlerTexture.GetContent(itemImageRequest);
            tex.filterMode = FilterMode.Point;

            Sprite image = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.zero);

            cardButton.GetComponent<CardInfo>().image = image;
            cardButton.gameObject.GetComponent<Image>().sprite = image;
        }
        ui.callsActive--;
    }
}