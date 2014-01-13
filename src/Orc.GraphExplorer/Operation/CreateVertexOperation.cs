using GraphX;
using Orc.GraphExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer
{
    public class CreateVertexOperation : VertexOperation
    {
        const string CreateVertex = "Create Vertex";
        public override string Sammary
        {
            get { return CreateVertex; }
        }

        public override void Do()
        {
            _graph.Graph.AddVertex(_vertex);
            _vCtrl= new VertexControl(_vertex);
            _graph.AddVertex(_vertex, _vCtrl);

            if (_callback != null)
            {
                _callback.Invoke(_vertex,_vCtrl);
            }
        }

        public override void UnDo()
        {
            _graph.Graph.RemoveVertex(_vertex);
            _graph.RemoveVertex(_vertex);

            if (_undoCallback != null)
            {
                _undoCallback.Invoke(_vertex);
            }
        }

        public CreateVertexOperation(GraphArea graph, DataVertex data = null, Action<DataVertex, VertexControl> callback = null, Action<DataVertex> undoCallback = null)
            : base(graph, data, callback, undoCallback)
        {
            _vCtrl = new VertexControl(_vertex);
        }
    }
}
