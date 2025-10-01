using System.Collections;
using System.Collections.Generic;
using Configs;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CardComponent : MonoBehaviour
{
    [SerializeField]
    private DOTweenAnimation _rotateShowAnimation;

    [SerializeField]
    private DOTweenAnimation _scaleDownAnimation;

    [SerializeField]
    private Image _iconImage;

    [Inject]
    private SoundManager _soundManager;

    private CardConfig _assignedCardConfig;

    public CardConfig AssignedCardConfig => _assignedCardConfig;

    public void Initialize(CardConfig config, Vector2 layoutIndex)
    {
        _assignedCardConfig = config;
        _iconImage.sprite = _assignedCardConfig.CardIcon;
    }

    public void Show()
    {
        _soundManager.PlayCardFlip();
        _rotateShowAnimation.DOPlayForward();
    }

    public void Hide()
    {
        _rotateShowAnimation.DOPlayBackwards();
    }

    public void Complete()
    {
        _scaleDownAnimation.DOPlayForward();
    }
}
