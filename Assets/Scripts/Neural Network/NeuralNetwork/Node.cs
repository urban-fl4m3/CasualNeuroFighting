namespace Neural_Network.NeuralNetwork
{
    /// <summary> Representation of neuron. Contains position in brain (neural network) system and unique value. </summary>
    public class Node
    {
        public readonly NodeInfo Info;

        public void SetValue(float value) => Value = value;
        public float Value { get; private set; }

        public Node(int layerIndex, int nodeID) => Info = new NodeInfo(layerIndex, nodeID);
    }

    public readonly struct NodeInfo
    {
        public readonly int LayerIndex;
        public readonly int NodeIndex;

        public NodeInfo(int layer, int index)
        {
            LayerIndex = layer;
            NodeIndex = index;
        }
    }
}