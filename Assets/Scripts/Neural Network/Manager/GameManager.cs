using System.Collections.Generic;
using System.Linq;
using Neural_Network.NeuralNetwork;
using Neural_Network.UI;
using UnityEngine;

namespace Neural_Network.Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;

        [Header("Random subjects spawn generator")]
        [SerializeField] private float _minimumDistance;
        [SerializeField] private Vector2 _spawnRangeX;
        [SerializeField] private Vector2 _spawnRangeZ;

        [Header("Neural Network Values")]
        [SerializeField] private int _brainsCount;
        [SerializeField] private int _bestCount;
        [SerializeField] private int _inputLayerNodes;
        [SerializeField] private int _outputLayerNodes;
        [SerializeField] private int[] _hiddenLayerNodes;

        private List<FightingGame> _activeGames;
        [SerializeField] private FightingGame _gamePrefab;

        private Brain[] _neuralNetworks;
        private float[] _lifetimes;

        private int _completedEncounters;
        private int _generationCount;
        private int _cameraIndex;

        private void Awake()
        {
            _generationCount = 0;
            _completedEncounters = 0;

            _activeGames = new List<FightingGame>();
            _neuralNetworks = new Brain[_brainsCount * 2];
            _lifetimes = new float[_brainsCount * 2];

            for (var brainIndex = 0; brainIndex < _brainsCount * 2; brainIndex += 2)
            {
                var brain1 = new Brain(_inputLayerNodes, _outputLayerNodes, _hiddenLayerNodes);
                _neuralNetworks[brainIndex] = brain1;
                
                var brain2 = new Brain(_inputLayerNodes, _outputLayerNodes, _hiddenLayerNodes);
                _neuralNetworks[brainIndex + 1] = brain2;

                var newGame = Instantiate(_gamePrefab, new Vector3((brainIndex / 2) * 30, 0, 0), Quaternion.identity, transform);
                newGame.Initialize(brainIndex / 2, CompleteEncounter);
                _activeGames.Add(newGame);
            }

            StartGames();
        }

        private void StartGames()
        {
            _cameraIndex = 0;
            _mainCamera.transform.position = _activeGames[0].CameraPlace.position;

            for (var gameIndex = 0; gameIndex < _activeGames.Count; gameIndex++)
            {
                _activeGames[gameIndex].StartGame(_neuralNetworks[gameIndex * 2],
                    _neuralNetworks[gameIndex * 2 + 1]);
            }

            _generationCount++;
            Debug.Log(_generationCount);
            UIManager.Instance.SetGeneration(_generationCount);
        }

        private void CompleteEncounter(FightingGame game)
        {
            var gameID = game.ID;
            _lifetimes[gameID * 2] = game.FitnessValue.x;
            _lifetimes[gameID * 2 + 1] = game.FitnessValue.y;
            _completedEncounters++;

            UIManager.Instance.SetLifeTime(game.GameTime);

            if (_cameraIndex == gameID)
            {
                foreach (var activeGame in _activeGames)
                {
                    if (!activeGame.IsPlaying) continue;
                    _mainCamera.transform.position = activeGame.CameraPlace.position;
                    _cameraIndex = activeGame.ID;
                    break;
                }
            }

            if (_completedEncounters < _activeGames.Count) return;

            _completedEncounters = 0;
            NewGenerations();
            StartGames();
        }

        private void NewGenerations()
        {
            SortBrainsAndLifetimes();

            var newGeneration = new List<Brain>();
            var roulette = new List<Brain>();
            
            Debug.Log(_lifetimes[0]);

            for (var i = 0; i < _neuralNetworks.Length; i++)
            {
                if (i < _bestCount) newGeneration.Add(_neuralNetworks[i]);
                else roulette.Add(_neuralNetworks[i]);
            }

            for (var i = 0; i < _brainsCount - _bestCount; i++)
            {
                var randIndexF = Random.Range(0, _bestCount);
                var randIndexS = Random.Range(0, roulette.Count);

                var crossover = Brain.Crossover(_neuralNetworks[randIndexF], roulette[randIndexS]);
                newGeneration.Add(crossover);
            }

            for (var i = _bestCount; i < newGeneration.Count; i++)
            {
                var gen = newGeneration[i];
                gen.Mutate();
                _neuralNetworks[i] = gen;
            }
        }

        private void SortBrainsAndLifetimes()
        {
            for (var i = 0; i < _lifetimes.Length; i++)
            {
                for (var j = i + 1; j < _lifetimes.Length; j++)
                {
                    if (_lifetimes[i] > _lifetimes[j]) continue;

                    var tempTime = _lifetimes[i];
                    _lifetimes[i] = _lifetimes[j];
                    _lifetimes[j] = tempTime;

                    var tempBrain = _neuralNetworks[i];
                    _neuralNetworks[i] = _neuralNetworks[j];
                    _neuralNetworks[j] = tempBrain;
                }
            }
        }
    }
}
