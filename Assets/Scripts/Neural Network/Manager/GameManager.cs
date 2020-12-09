using System;
using System.Collections.Generic;
using System.Linq;
using Neural_Network.NeuralNetwork;
using Neural_Network.UI;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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

        private List<Tuple<Brain, float>> _neuralNetworks;
        private float[] _lifetimes;

        private int _completedEncounters;
        private int _generationCount;
        private int _cameraIndex;

        private void createFightingGame()
        {
            var newGame = Instantiate(_gamePrefab, new Vector3(_activeGames.Count * 30, 0, 0), Quaternion.identity, transform);
            newGame.Initialize(CompleteEncounter);
            _activeGames.Add(newGame);
        }

        private void Start()
        {
            _generationCount = 0;
            _completedEncounters = 0;

            _activeGames = new List<FightingGame>();
            _neuralNetworks = new List<Tuple<Brain, float>>();

            for (var brainIndex = 0; brainIndex < _brainsCount * 2; brainIndex += 2)
            {
                var brain1 = new Brain(_inputLayerNodes, _outputLayerNodes, _hiddenLayerNodes);
                
                _neuralNetworks.Add(new Tuple<Brain, float>(brain1, 0.0f));
                
                var brain2 = new Brain(_inputLayerNodes, _outputLayerNodes, _hiddenLayerNodes);
                _neuralNetworks.Add(new Tuple<Brain, float>(brain2, 0.0f));

                createFightingGame();
            }

            StartGames();
        }

        private static void startGame(FightingGame game, Tuple<Brain, float> brain1, Tuple<Brain, float> brain2)
        {
            if (Random.Range(0.0f, 1.0f) > 0.5) game.StartGame(brain1, brain2);
            else game.StartGame(brain2, brain1);
        }

        private void StartGames()
        {
            while (_activeGames.Count < (_neuralNetworks.Count / 2))
            {
                createFightingGame();
            }

            while (_activeGames.Count > (_neuralNetworks.Count / 2))
            {
                var lFightingGame = _activeGames[_activeGames.Count - 1];
                _activeGames.RemoveAt(_activeGames.Count - 1);
                Object.Destroy(lFightingGame);
            }
            
            for (var gameIndex = 0; gameIndex < _activeGames.Count; gameIndex++)
            {
                startGame(_activeGames[gameIndex], _neuralNetworks[gameIndex * 2 + 1], _neuralNetworks[gameIndex * 2]);
            }
            
            _cameraIndex = 0;
            _mainCamera.transform.position = _activeGames[0].CameraPlace.position;
            
            _generationCount++;
            Debug.Log(_generationCount);
            UIManager.Instance.SetGeneration(_generationCount);
            
            _neuralNetworks.Clear();
        }

        private void CompleteEncounter(FightingGame game)
        {
            var gameID = game.ID;
            _neuralNetworks.Add(game.playedBrain1);
            _neuralNetworks.Add(game.playedBrain2);
            
            _completedEncounters++;

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

            while (_neuralNetworks.Count > _brainsCount * 2)
            {
                _neuralNetworks.RemoveAt(_neuralNetworks.Count - 1);
            }
 
            var newGeneration = new List<Brain>();
            var roulette = new List<Tuple<Brain, float>>();

            var averageF = 0.0f;

            for (var i = 0; i < _neuralNetworks.Count; i++)
            {
                averageF += _neuralNetworks[i].Item2;
            }

            averageF /= _neuralNetworks.Count;
        
            var average_dispersion = 0.0f;
            for (var i = 0; i < _neuralNetworks.Count; i++)
            {
                average_dispersion += Mathf.Pow(_neuralNetworks[i].Item2 - averageF, 2.0f);
            }

            average_dispersion /= _neuralNetworks.Count;

            var dispersion = Mathf.Sqrt(average_dispersion);

            var F = new float[_neuralNetworks.Count];
            var sumF = 0.0f;
            
            for (var i = 0; i < _neuralNetworks.Count; i++)
            {
                F[i] = _neuralNetworks[i].Item2 + (averageF - 2.0f * dispersion);
                sumF += F[i];
            }

            for (var i = 0; i < _neuralNetworks.Count; i++)
            {
                for (var j = 0; j < Mathf.RoundToInt((F[i] / sumF) * 1000.0f); j++)
                    roulette.Add(_neuralNetworks[i]);
            }
            for (var i = 0; i < _brainsCount / 2; i++)
            {
                var randIndexF = Random.Range(0, roulette.Count);
                var randIndexS = Random.Range(0, roulette.Count);
                var crossover = Brain.Crossover(roulette[randIndexF].Item1, roulette[randIndexS].Item1);
                newGeneration.Add(crossover.Item1);
                newGeneration.Add(crossover.Item2);
            }
            

            var defaultMutationCount = Mathf.RoundToInt(_brainsCount * 0.2f);
            for (int i = 0; i < defaultMutationCount; i++)
            {
                var randomIndex = Random.Range(0, newGeneration.Count);
                var mutatedBrain = newGeneration[randomIndex].DefaultMutate();
                
                _neuralNetworks.Add(new Tuple<Brain, float>(mutatedBrain, 0.0f) );
            }
            
            var shiftMutationCount = Mathf.RoundToInt(_brainsCount * 0.2f);
            for (int i = 0; i < shiftMutationCount; i++)
            {
                var randomIndex = Random.Range(0, newGeneration.Count);
                var mutatedBrain = newGeneration[randomIndex].ShiftMutate();
                
                _neuralNetworks.Add(new Tuple<Brain, float>(mutatedBrain, 0.0f) );
            }
            
            for (var i = 0; i < newGeneration.Count; i++)
            {
                _neuralNetworks.Add(new Tuple<Brain, float>(newGeneration[i], 0.0f) );
            }

            UIManager.Instance.SetLifeTime(_neuralNetworks[0].Item2);
        }


        private void SortBrainsAndLifetimes()
        {
            for (var i = 0; i < _neuralNetworks.Count; i++)
            {
                for (var j = i + 1; j < _neuralNetworks.Count; j++)
                {
                    if (_neuralNetworks[i].Item2 > _neuralNetworks[j].Item2) continue;

                    var tempBrain = _neuralNetworks[i];
                    _neuralNetworks[i] = _neuralNetworks[j];
                    _neuralNetworks[j] = tempBrain;
                }
            }
        }
    }
}
