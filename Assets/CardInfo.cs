using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Acts as a data storage for cards
/// </summary>
public class CardInfo : MonoBehaviour
{
    public CardData data;
    public Sprite image;
}

[System.Serializable]
public struct CardData
{
    public string id;
    public string pokeName;
    public string imgUrl;
    public string rarity;
    public string pokeType;
    public int hp;    
    public bool onDeck;
}