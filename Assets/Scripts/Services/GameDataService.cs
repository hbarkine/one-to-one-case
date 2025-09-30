using System;
using Data;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace Services
{
    public class GameDataService : IInitializable, IDisposable
    {
        [Inject]
        private SignalBus _signalBus;

        private const string GameDataPrefsKey = "game-data;";

        public bool HasSavedGameData => PlayerPrefs.HasKey(GameDataPrefsKey);

        public GameData CurrentGameData { get; private set; }

        public void Initialize()
        {
            if (HasSavedGameData)
            {
                LoadGameData();
            }
            else
            {
                InitializeData();
            }
        }

        private void InitializeData()
        {
            CurrentGameData = new GameData();
            CurrentGameData.Init();
        }

        public void SaveData<TData>(string key, TData data)
        {
            string dataJson = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, dataJson);
        }

        public TData LoadData<TData>(string key)
        {
            string dataJson = PlayerPrefs.GetString(key, "");
            return JsonConvert.DeserializeObject<TData>(dataJson);
        }

        public void SaveGameData()
        {
            SaveData(GameDataPrefsKey, CurrentGameData);
        }

        public void LoadGameData()
        {
            CurrentGameData = LoadData<GameData>(GameDataPrefsKey);
        }

        public void Dispose()
        {
            SaveGameData();
        }
    }

}

