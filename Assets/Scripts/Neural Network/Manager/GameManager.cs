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
            _neuralNetworks = new Brain[_brainsCount];
            _lifetimes = new float[_brainsCount];

            for (var brainIndex = 0; brainIndex < _brainsCount * 2; brainIndex++)
            {
                var brain = new Brain(_inputLayerNodes, _outputLayerNodes, _hiddenLayerNodes);
                _neuralNetworks[brainIndex] = brain;

                var newGame = Instantiate(_gamePrefab, new Vector3(brainIndex * 100, 0, 0), Quaternion.identity, transform);
                newGame.Initialize(brainIndex, CompleteEncounter);
                _activeGames.Add(newGame);
            }

            StartGames();
        }

        private void StartGames()
        {
            _cameraIndex = 0;
            _mainCamera.transform.position = _activeGames[0].CameraPlace.position;
            var (playerPosition, devourerPosition) = GetRandomPositions();

            for (var gameIndex = 0; gameIndex < _activeGames.Count; gameIndex+=2)
            {
                _activeGames[gameIndex].StartGame(_neuralNetworks[gameIndex],
                    _neuralNetworks[gameIndex+1], playerPosition, devourerPosition);
            }

            _generationCount++;
            UIManager.Instance.SetGeneration(_generationCount);
        }

        private (Vector3, Vector3) GetRandomPositions()
        {
            while (true)
            {
                var playerX = Random.Range(_spawnRangeX.x, _spawnRangeX.y);
                var devourerX = Random.Range(_spawnRangeX.x, _spawnRangeX.y);

                var playerZ = Random.Range(_spawnRangeZ.x, _spawnRangeZ.y);
                var devourerZ = Random.Range(_spawnRangeZ.x, _spawnRangeZ.y);

                var firstPos = new Vector3(playerX, 0.5f, playerZ);
                var secondPos = new Vector3(devourerX, 0.5f, devourerZ);

                if (Vector3.Distance(firstPos, secondPos) > _minimumDistance)
                {
                    return (firstPos, secondPos);
                }
            }
        }

        private void CompleteEncounter(FightingGame game)
        {
            var gameID = game.ID;
            _lifetimes[gameID] = game.GameTime;
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

            var lifetimeSum = _lifetimes.Sum();

            var newGeneration = new List<Brain>();
            var roulette = new List<Brain>();

            for (var i = 0; i < _neuralNetworks.Length; i++)
            {
                if (i < _bestCount) newGeneration.Add(_neuralNetworks[i]);
                var coefficient = (int)(_lifetimes[i] / lifetimeSum * 100);

                for (int j = 0; j < coefficient; j++)
                {
                    roulette.Add(_neuralNetworks[i]);
                }
            }

            for (var i = 0; i < _brainsCount - _bestCount; i++)
            {
                var randIndexF = Random.Range(0, roulette.Count);
                var randIndexS = Random.Range(0, roulette.Count);

                var crossover = Brain.Crossover(roulette[randIndexF], roulette[randIndexS]);
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
