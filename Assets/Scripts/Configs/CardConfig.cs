using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "CardConfig", menuName = "MatchGame/Configs/CardConfig")]
    public class CardConfig : ScriptableObject
    {
        public int CardId;
        public Sprite CardIcon;
        public string CardName;
    }
}