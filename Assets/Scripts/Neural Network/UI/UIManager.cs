using TMPro;
using UnityEngine;

namespace Neural_Network.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI _lifeTimeText;
        private float _maxLifeTime;

        [SerializeField] private TextMeshProUGUI _generationText;

        private void Awake()
        {
            Instance = this;
            SetLifeTime(0);
        }

        public void SetLifeTime(float value)
        {
            if (value < _maxLifeTime) return;
            _maxLifeTime = value;
            _lifeTimeText.text = _maxLifeTime.ToString();
        }

        public void SetGeneration(int generation) => _generationText.text = generation.ToString();
    }
}
