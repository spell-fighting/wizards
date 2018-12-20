using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using TensorFlow;

public class Predictor : MonoBehaviour
{
    private TFBuffer _modelBuffer;

    public TextAsset Model;

    private void Start()
    {
        _modelBuffer = new TFBuffer(Model.bytes);
    }

    private static void PlotLineWidth(Texture2D tex, int x0, int y0, int x1, int y1, float wd)
    {
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;

        int err = dx - dy, e2, x2, y2; /* error value e_xy */

        float ed = dx + dy == 0 ? 1 : (float) Math.Sqrt((float) dx * dx + (float) dy * dy);


        for (wd = (wd + 1) / 2;;)
        {
            /* pixel loop */
            tex.SetPixel(x0, y0, Color.white);
            e2 = err;
            x2 = x0;
            if (2 * e2 >= -dx)
            {
                /* x step */
                for (e2 += dy, y2 = y0; e2 < ed * wd && (y1 != y2 || dx > dy); e2 += dx)
                    tex.SetPixel(x0, y2 += sy, Color.white);
                if (x0 == x1) break;
                e2 = err;
                err -= dy;
                x0 += sx;
            }

            if (2 * e2 <= dy)
            {
                /* y step */
                for (e2 = dx - e2; e2 < ed * wd && (x1 != x2 || dx < dy); e2 += dy)
                    tex.SetPixel(x2 += sx, y0, Color.white);
                if (y0 == y1) break;
                err += dx;
                y0 += sy;
            }
        }
    }

    public Prediction? Predict(IList<Vector2> positions, int imageSize)
    {
        var minX = positions[0].x;
        var maxX = positions[0].x;
        var minY = positions[0].y;
        var maxY = positions[0].y;

        foreach (var position in positions)
        {
            if (position.x < minX)
            {
                minX = position.x;
            }

            if (position.x > maxX)
            {
                maxX = position.x;
            }

            if (position.y < minY)
            {
                minY = position.y;
            }

            if (position.y > maxY)
            {
                maxY = position.y;
            }
        }

        const int padding = 10;

        var width = (int) Math.Floor(maxX - minX) + padding;
        var height = (int) Math.Floor(maxY - minY) + padding;

        var paddingX = (float) padding / 2;
        var paddingY = (float) padding / 2;

        var size = width;

        if (width > height)
        {
            size = width;
            paddingY += (float) (width - height) / 2;
        }
        else if (height > width)
        {
            size = height;
            paddingX += (float) (height - width) / 2;
        }

        var tex = new Texture2D(imageSize, imageSize);
        tex.Apply();

        for (var y = 0; y < tex.height; y++)
        {
            for (var x = 0; x < tex.width; x++)
            {
                tex.SetPixel(x, y, Color.black);
            }
        }

        tex.Apply();

        var previousPosition = new Vector3(-1, -1, -1);
        const int border = 1;

        var factor = (double) imageSize / size;

        foreach (var position in positions)
        {
            if (previousPosition != -Vector3.one)
            {
                PlotLineWidth(
                    tex,
                    (int) Math.Floor((previousPosition.x - minX + paddingX) * factor),
                    (int) Math.Floor((previousPosition.y - minY + paddingY) * factor),
                    (int) Math.Floor((position.x - minX + paddingX) * factor),
                    (int) Math.Floor((position.y - minY + paddingY) * factor),
                    border
                );
            }

            previousPosition = position;
        }

        tex.Apply();

        var matrix = new float[1, imageSize, imageSize, 1];
        for (var i = 0; i < imageSize; i++)
        {
            for (var j = 0; j < imageSize; j++)
            {
                matrix[0, i, j, 0] = tex.GetPixel(i, j).r + tex.GetPixel(i, j).g + tex.GetPixel(i, j).b > 0 ? 255 : 0;
            }
        }

        var tensor = new TFTensor(matrix);

        using (var graph = new TFGraph())
        {
            graph.Import(_modelBuffer);

            using (var session = new TFSession(graph))
            {
                var runner = session.GetRunner();

                runner.AddInput(graph["conv2d_1_input"][0], tensor);
                runner.Fetch(graph["output_node0"][0]);

                var rawPredictions = runner.Run()[0].GetValue() as float[,];

                var maxPrediction = -1;
                var maxIndexPrediction = -1;

                for (var i = 0; i < 5; i++)
                {
                    var p = (int) rawPredictions[0, i] * 100;

                    if (p > maxPrediction)
                    {
                        maxPrediction = p;
                        maxIndexPrediction = i;
                    }
                }

                Debug.Log($"Circle {(int) rawPredictions[0, 1] * 100}%, " +
                          $"Hourglass {(int) rawPredictions[0, 2] * 100}%, " +
                          $"Square {(int) rawPredictions[0, 0] * 100}%, " +
                          $"Star {(int) rawPredictions[0, 4] * 100}%, " +
                          $"Triangle {(int) rawPredictions[0, 3] * 100}% \n");

                Prediction? prediction = null;
                if (maxPrediction >= 75)
                {
                    switch (maxIndexPrediction)
                    {
                        case 1:
                            prediction = Prediction.Circle;
                            break;
                        case 2:
                            prediction = Prediction.Hourglass;
                            break;
                        case 0:
                            prediction = Prediction.Square;
                            break;
                        case 4:
                            prediction = Prediction.Star;
                            break;
                        case 3:
                            prediction = Prediction.Triangle;
                            break;
                    }
                }

                return prediction;
            }
        }
    }
}