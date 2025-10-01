using UnityEngine;

namespace Managers
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _audioSource;

        [Header("Sound Effects")]
        [SerializeField]
        private AudioClip _cardFlipSound;

        [SerializeField]
        private AudioClip _cardMatchSound;

        [SerializeField]
        private AudioClip _cardMismatchSound;

        [SerializeField]
        private AudioClip _gameOverSound;

        public void PlayCardFlip()
        {
            if (_cardFlipSound != null)
            {
                _audioSource.PlayOneShot(_cardFlipSound);
            }
        }

        public void PlayCardMatch()
        {
            if (_cardMatchSound != null)
            {
                _audioSource.PlayOneShot(_cardMatchSound);
            }
        }

        public void PlayCardMismatch()
        {
            if (_cardMismatchSound != null)
            {
                _audioSource.PlayOneShot(_cardMismatchSound);
            }
        }

        public void PlayGameOver()
        {
            if (_gameOverSound != null)
            {
                _audioSource.PlayOneShot(_gameOverSound);
            }
        }
    }
} 