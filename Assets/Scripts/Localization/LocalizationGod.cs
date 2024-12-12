using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;


public static class LocalizationGod
{
    private class Table
    {
        public readonly string Name;
        
        public Dictionary<string, string> English, Spanish;


        public Table(string name)
        {
            Name = name;
        }
        public void LoadCSV(Action onCompleted = null, bool hasLinebreaks = false)
        {
            // Llamar a un MonoBehaviour para hacer la solicitud asíncrona
            var loaderGameObject = new GameObject("CSVLoader");
            var loaderComponent = loaderGameObject.AddComponent<CSVLoader>();
            loaderComponent.LoadCSVAsync(Name, () =>
            {
                English = loaderComponent.English;
                Spanish = loaderComponent.Spanish;
                onCompleted?.Invoke();
            }, hasLinebreaks);
        }
    }


    public static bool Spanish { get; private set; } = true;
    
    private static readonly Dictionary<string, Table> _tables = new();

    public static bool IsInitialized
    {
        get => _cardsLoaded && _tutorialLoaded && _feedbackLoaded && _radioInfoLoaded && _menuBooksLoaded && _letterLoaded;
    }
    
    private static bool _cardsLoaded = false;
    private static bool _tutorialLoaded = false;
    private static bool _feedbackLoaded = false;
    private static bool _radioInfoLoaded = false;
    private static bool _menuBooksLoaded = false;
    private static bool _letterLoaded = false;

    public static void Init()
    {
        if (IsInitialized) return;
        
        Debug.Log("inicializando localizacion...");
        LoadTable("Cards", () => _cardsLoaded = true);
        LoadTable("Tutorial", () => _tutorialLoaded = true);
        LoadTable("Feedback", () => _feedbackLoaded = true);
        LoadTable("RadioInfo", () => _radioInfoLoaded = true);
        LoadTable("MenuBooks", () => _menuBooksLoaded = true);
        LoadTable("Letter", () => _letterLoaded = true, hasLineBreaks: true);
    }

    public static string GetLocalized(string tableName, string tablekey)
    {
        if (!_tables.TryGetValue(tableName, out var table))
        {
            // table = LoadTable(tableName);
            throw new Exception($"tabla {tableName} no encontrada o no cargada");
        }
        return GetFromTable(table, tablekey);
    }

    public static void ToggleLanguage()
    {
        Spanish = !Spanish;
        
        if(Spanish) Debug.Log("Lenguaje español");
        else Debug.Log("English language");
    }


    private static string GetFromTable(Table table, string key)
    {
        var dict = Spanish ? table.Spanish : table.English;

        if (dict.TryGetValue(key, out var value)) return value;
        throw new Exception($"Key {key} no encontrada en la tabla {table.Name}:");
    }

    private static void LoadTable(string tableName, Action callback, bool hasLineBreaks = false)
    {
        var table = new Table(tableName);

        table.LoadCSV(() =>
        {
            _tables.Add(tableName, table);
            callback?.Invoke();
        }, hasLineBreaks);
    }
}