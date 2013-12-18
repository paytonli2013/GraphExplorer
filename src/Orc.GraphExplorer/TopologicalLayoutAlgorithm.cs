using GraphX.GraphSharp.Algorithms.Layout;
using GraphX.GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using Orc.GraphExplorer.Model;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Orc.GraphExplorer
{
    public class TopologicalLayoutAlgorithm<TVertex, TEdge, TGraph> : IExternalLayout<TVertex>
        where TVertex : class
        where TEdge : global::QuickGraph.IEdge<TVertex>
        where TGraph : global::QuickGraph.IVertexAndEdgeListGraph<TVertex, TEdge>
    {
        TGraph _graph;
        public TopologicalLayoutAlgorithm(TGraph graph)
        {
            _graph = graph;
        }

        public void Compute()
        {
            var eslaParameters = new EfficientSugiyamaLayoutParameters()
            {
                MinimizeEdgeLength = true,
                LayerDistance = 80
            };

            var esla = new EfficientSugiyamaLayoutAlgorithm<TVertex, TEdge, TGraph>(_graph, eslaParameters,null, VertexSizes);

            esla.Compute();

            vertexPositions = new Dictionary<TVertex, Point>();

            foreach (var item in esla.VertexPositions)
            {
                vertexPositions.Add(item.Key, new Point(item.Value.Y*1.5, item.Value.X));
            }
        }

        public bool NeedVertexSizes
        {
            get { return true; }
        }

        IDictionary<TVertex, Point> vertexPositions;
        public IDictionary<TVertex, Point> VertexPositions
        {
            get { return vertexPositions; }
        }

        IDictionary<TVertex, Size> vertexSizes;
        public IDictionary<TVertex, Size> VertexSizes
        {
            get
            {
                return vertexSizes;
            }
            set
            {
                vertexSizes = value;
            }
        }
    }
}
