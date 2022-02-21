using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SavingSystem : MonoBehaviour
{
    const string save = "save";

    private void Start()
    {
        Load();
    }

    /// <summary>
    /// Wrapper for Save
    /// </summary>
    public void Save()
    {
        Dictionary<int, object> state = LoadFile(save);
        CaptureState(state);
        SaveFile(save, state);
    }
    /// <summary>
    /// Wrapper for Load
    /// </summary>
    public void Load()
    {
        RestoreState(LoadFile(save));
    }

    /// <summary>
    /// Wrapper for deleting Savefile
    /// </summary>
    public void DeleteAll()
    {
        File.Delete(GetPathFromSaveFile(save));
        print("Save file deleted");
    }

    /// <summary>
    /// A Dictionary that acts as the savefile format
    /// </summary>
    Dictionary<int, object> LoadFile(string saveFile)
    {
        string path = GetPathFromSaveFile(saveFile);
        if (!File.Exists(path))
        {
            return new Dictionary<int, object>();
        }
        using (FileStream stream = File.Open(path, FileMode.Open))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (Dictionary<int, object>)formatter.Deserialize(stream);
        }
    }

    /// <summary>
    /// Creates the savefile in binary
    /// </summary>
    void SaveFile(string saveFile, object state)
    {
        string path = GetPathFromSaveFile(saveFile);
        print("Saving to " + path);
        using (FileStream stream = File.Open(path, FileMode.Create))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, state);
        }
    }

    /// <summary>
    /// The savefile folder path
    /// </summary>
    string GetPathFromSaveFile(string saveFile)
    {
        return Path.Combine(Application.persistentDataPath, saveFile + ".sav");
    }

    /// <summary>
    /// Captures a state of the data that are to be saved
    /// </summary>
    public object CaptureState(Dictionary<int, object> state)
    {
        List<string>[] ids = new List<string>[3];
        ids[0] = new List<string>();
        ids[1] = new List<string>();
        ids[2] = new List<string>();

        DeckManager deck = GetComponent<DeckManager>();

        for (int i = 0; i < 3; i++)
        {
            foreach (var item in deck.decks[i])
            {
                ids[i].Add(item.GetComponent<CardInfo>().data.id);
            }
            state[i] = ids[i];
        }
        return state;
    }

    /// <summary>
    /// Restores the saved data to lists for use and calls 
    /// deck builder for card creation
    /// </summary>
    public void RestoreState(Dictionary<int, object> state)
    { 
        List<string>[] ids = new List<string>[3];
        if (state.Count > 1)
        {
            for (int i = 0; i < 3; i++)
            {
                ids[i] = (List<string>)state[i];
            }
        }
        
        GetComponent<DeckManager>().LoadDeckCardInfo(ids);
    }
}