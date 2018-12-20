using TensorFlow;

public static class ImageUtil
{
    public static TFTensor ResizeTensor(TFTensor tensor, int size)
    {
        TFOutput input, output;

        using (var graph = ConstructGraphToNormalizeImage(out input, out output, size))
        {
            using (var session = new TFSession(graph))
            {
                var normalized = session.Run(new[] {input}, new[] {tensor}, new[] {output});

                return normalized[0];
            }
        }
    }

    private static TFGraph ConstructGraphToNormalizeImage(out TFOutput input, out TFOutput output, int size)
    {
        var graph = new TFGraph();
        input = graph.Placeholder(TFDataType.Float);

        output = graph.ResizeNearestNeighbor(input, graph.Const(new[] {size, size}, "size"));

        return graph;
    }
}