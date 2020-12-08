using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Neural_Network.NeuralNetwork
{
    /// <summary> Representation of neural network system. </summary>
    public class Brain
    {
        private Dictionary<string, float> _edges;
        private List<float> _edgeValues;

        public int this[int index] => _nodeCounts[index];
        private readonly int[] _nodeCounts;
        private List<Node>[] _layers;

        private readonly int _layersCount;

        public Brain(int inputCount, int outputCount, IReadOnlyList<int> hiddenCounts)
        {
            var _hiddenLayersCount = hiddenCounts.Count;
            _layersCount = _hiddenLayersCount + 2;

            _nodeCounts = new int[_layersCount];
            _nodeCounts[0] = inputCount;

            for (var i = 0; i < _hiddenLayersCount; i++)
            {
                _nodeCounts[i + 1] = hiddenCounts[i];
            }

            _nodeCounts[_layersCount - 1] = outputCount;

            _edgeValues = new List<float>();
            CreateLayersAndEdges();
        }

        /// <summary> Optimized method for layers/edge creating. </summary>
        private void CreateLayersAndEdges()
        {
            _layers = new List<Node>[_layersCount];
            _edges = new Dictionary<string, float>();

            for (var layerIndex = 0; layerIndex < _layersCount; layerIndex++)
            {
                _layers[layerIndex] = new List<Node>();

                var createEdges = layerIndex > 0;
                List<Node> lastLayer = null;
                if (createEdges) lastLayer = _layers[layerIndex - 1];

                for (var nodeIndex = 0; nodeIndex < _nodeCounts[layerIndex]; nodeIndex++)
                {
                    var layerNode = new Node(layerIndex, nodeIndex);
                    _layers[layerIndex].Add(layerNode);

                    if (!createEdges) continue;
                    foreach (var lastNode in lastLayer)
                    {
                        var edgeID = GenerateEdgeID(lastNode.Info, layerNode.Info);
                        _edges.Add(edgeID, 0.0f);
                    }
                }
            }
        }

        /// <summary> This method is creating layers (1 input, 1 output, 1 hidden) and fill them with new nodes.
        /// (LEGACY) </summary>
        // ReSharper disable once UnusedMember.Local
        private void CreateLayers()
        {
            _layers = new List<Node>[_layersCount];

            for (var layerIndex = 0; layerIndex < _layersCount; layerIndex++)
            {
                _layers[layerIndex] = new List<Node>();
                for (var nodeIndex = 0; nodeIndex < _nodeCounts[layerIndex]; nodeIndex++)
                {
                    var layerNode = new Node(layerIndex, nodeIndex);
                    _layers[layerIndex].Add(layerNode);
                }
            }
        }

        /// <summary> This method is creating edges for every node which doesn't belong to input layers. Basically it means that
        /// layer index starts with 1.
        /// (LEGACY) </summary>
        // ReSharper disable once UnusedMember.Local
        private void CreateEdges()
        {
            _edges = new Dictionary<string, float>();

            for (var layerIndex = 1; layerIndex < _layersCount; layerIndex++)
            {
                var currentLayer = _layers[layerIndex];
                var lastLayer = _layers[layerIndex - 1];

                foreach (var currentNode in currentLayer)
                {
                    foreach (var lastNode in lastLayer)
                    {
                        var edgeID = GenerateEdgeID(lastNode.Info, currentNode.Info);
                        _edges.Add(edgeID, Random.Range(-1.0f, 1.0f));
                    }
                }
            }
        }

        public float[] Process(float[] inputValues)
        {
            if (inputValues.Length != _layers[0].Count) throw new ArgumentException();
            var result = new float[_nodeCounts[_layersCount - 1]];

            for (var layerIndex = 0; layerIndex < _layersCount; layerIndex++)
            {
                var currentLayer = _layers[layerIndex];
                for (var curNodeIndex = 0; curNodeIndex < currentLayer.Count; curNodeIndex++)
                {
                    var currentNode = currentLayer[curNodeIndex];

                    if (layerIndex == 0)
                    {
                        currentNode.SetValue(inputValues[curNodeIndex]);
                    }
                    else
                    {
                        var lastLayer = _layers[layerIndex - 1];

                        float value = 0;
                        foreach (var lastNode in lastLayer)
                        {
                            var edgeName = GenerateEdgeID(lastNode.Info, currentNode.Info);
                            value += _edges[edgeName] * lastNode.Value;
                        }

                        value = ActivateValue(value);
                        currentNode.SetValue(value);

                        if (layerIndex == _layersCount - 1)
                        {
                            result[curNodeIndex] = value;
                        }
                    }
                }
            }
            return result;
        }

        private List<float> GetEdges()
        {
            _edgeValues = new List<float>();
            foreach (var edge in _edges)
            {
                _edgeValues.Add(edge.Value);
            }

            return _edgeValues;
        }

        private void SetEdges(IReadOnlyList<float> newValues)
        {
            var index = 0;
            foreach (var key in _edges.Keys.ToList())
            {
                _edges[key] = newValues[index];
                index++;
            }
        }
        
        private const float mutationChance = 0.1f;
        public void Mutate()
        {
            var edges = GetEdges();
            for (var i = 0; i < edges.Count; i++)
            {
                var cont = Random.Range(0.0f, 1.0f);
                if (cont > mutationChance) continue;
                
                var additionValue = Random.Range(-1.0f, 1.0f);
                edges[i] += additionValue;
                edges[i] = Mathf.Clamp(edges[i], -1, 1);
            }

            SetEdges(edges);
        }

        private static float ActivateValue(float value) => 2 / (1 + Mathf.Exp(-2 * value)) - 1;
        //private static float ActivateValue(float value) => value < 0 ? value * 0.01f : value;

        /// <summary> Serializing node infos to string format: x:y:z, where
        /// x - layer index of past node,
        /// y - node index in past layer,
        /// z - node index in current layer </summary>
        private static string GenerateEdgeID(NodeInfo from, NodeInfo to)
        {
            var result = $"{from.LayerIndex}:{from.NodeIndex}:{to.NodeIndex}";
            return result;
        }

        /// <summary> Cross to brains in another one with new edges (parts from parameter brains)
        /// Also parse list on node counts and set all to new brain system. </summary>
        public static Brain Crossover(Brain f, Brain s)
        {
            var firstEdges = f.GetEdges();
            var secondEdges = s.GetEdges();

            var edgesCount = firstEdges.Count;
            var separator = Random.Range(0, edgesCount);

            var newEdgeValues = new List<float>();
            for (var edgeIndex = 0; edgeIndex < edgesCount; edgeIndex++)
            {
                var newValue = edgeIndex > separator ? secondEdges[edgeIndex] : firstEdges[edgeIndex];
                newEdgeValues.Add(newValue);
            }

            var counts = f._nodeCounts;
            var inputCount = counts[0];
            var outputCount = counts[counts.Length - 1];
            var hiddenCounts = new List<int>();

            var hiddenLayerCounts = counts.Length - 2;

            for (var index = 0; index < hiddenLayerCounts; index++)
            {
                hiddenCounts.Add(counts[index + 1]);
            }

            var crossover = new Brain(inputCount, outputCount, hiddenCounts);
            crossover.SetEdges(newEdgeValues);
            return crossover;
        }
    }
}
