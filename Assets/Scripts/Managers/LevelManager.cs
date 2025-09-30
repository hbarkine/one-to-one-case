using System.Collections.Generic;
using Configs;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField]
        private Transform _cardContainer;
        
        [SerializeField]
        private CardComponent _cardPrefab;
        
        [SerializeField]
        private float _cardSpacing = 0.1f;
        
        [SerializeField]
        private float _scaleMultiplier = 1f;

        [Inject]
        private GameConfig _gameConfig;

        private List<CardComponent> _activeCards = new List<CardComponent>();

        public List<CardComponent> ActiveCards => _activeCards;

        public void InitializeLevel(DifficultyConfig config)
        {
            Reset();
            
            int gridX = (int)config.LevelLayout.x;
            int gridY = (int)config.LevelLayout.y;
            int totalCards = gridX * gridY;
            int pairsNeeded = totalCards / 2;

            List<CardConfig> shuffledConfigs = new List<CardConfig>(_gameConfig.AllCardsConfigs);
            
            
            ShuffleList(shuffledConfigs);
            
            List<CardConfig> cardPairs = new List<CardConfig>();
            for (int i = 0; i < pairsNeeded; i++)
            {
                cardPairs.Add(shuffledConfigs[i]);
                cardPairs.Add(shuffledConfigs[i]);
            }
            
            ShuffleList(cardPairs);

            float totalWidth = gridX + (gridX - 1) * _cardSpacing;
            float totalHeight = gridY + (gridY - 1) * _cardSpacing;
            Vector3 gridOffset = new Vector3(-totalWidth / 2f + 0.5f, -totalHeight / 2f + 0.5f, 0f);
            
            int cardIndex = 0;
            for (int y = 0; y < gridY; y++)
            {
                for (int x = 0; x < gridX; x++)
                {
                    Vector3 position = gridOffset + new Vector3(
                        x * (1 + _cardSpacing),
                        y * (1 + _cardSpacing),
                        0f
                    );

                    CardComponent card = Instantiate(_cardPrefab, _cardContainer);
                    card.transform.localPosition = position;
                    card.Initialize(cardPairs[cardIndex], new Vector2(x, y));
                    
                    _activeCards.Add(card);
                    cardIndex++;
                }
            }
            
            float scaleFactor = _scaleMultiplier / Mathf.Max(gridX, gridY);
            _cardContainer.localScale = Vector3.one * scaleFactor;
        }

        public void Reset()
        {
            foreach (CardComponent card in _activeCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            
            _activeCards.Clear();
            
            _cardContainer.localScale = Vector3.one;
        }

        // TODO: Optional make this an extension to list.
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}