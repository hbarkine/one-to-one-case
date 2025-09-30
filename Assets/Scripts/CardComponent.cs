using System.Collections;
using System.Collections.Generic;
using Configs;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardComponent : MonoBehaviour
{
    [SerializeField]
    private DOTweenAnimation _rotateShowAnimation;

    [SerializeField]
    private DOTweenAnimation _scaleDownAnimation;

    [SerializeField]
    private Image _iconImage;

    private CardConfig _assignedCardConfig;

    public void Initialize(CardConfig config, Vector2 layoutIndex)
    {
        _assignedCardConfig = config;
        _iconImage.sprite = _assignedCardConfig.CardIcon;
    }

    public void Show()
    {
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
