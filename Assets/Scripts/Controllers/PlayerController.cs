using Signals;
using UnityEngine;
using Zenject;

namespace Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private Camera _mainCamera;

        [Inject]
        private SignalBus _signalBus;

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleCardClick();
            }
        }

        private void HandleCardClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                CardComponent card = hit.collider.GetComponent<CardComponent>();
                
                if (card != null)
                {
                    _signalBus.Fire(new CardSelectedSignal { CardComponent = card });
                }
            }
        }
    }
} 