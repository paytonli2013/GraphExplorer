using GraphX;
using Orc.GraphExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer
{
    /// <summary>
    /// base class for encapsulating vertex management
    /// </summary>
    public abstract class VertexOperation : IOperation
    {
        public virtual string Sammary
        {
            get;
            protected set;
        }

        ///Summary
        ///    perform an operation
        public abstract void Do();

        ///Summary
        ///    undo an operation
        public abstract void UnDo();

        bool _isUnDoable = true;
        public virtual bool IsUnDoable
        {
            get
            {
                return _isUnDoable;
            }
            protected set
            {
                _isUnDoable = value;
            }
        }

        protected DataVertex _vertex;
        protected VertexControl _vCtrl;
        protected Action<DataVertex, VertexControl> _callback;
        protected Action<DataVertex> _undoCallback;
        protected GraphArea _graph;

        public VertexOperation(GraphArea graph, DataVertex data = null, Action<DataVertex, VertexControl> callback = null, Action<DataVertex> undoCallback = null)
        {
            _graph = graph;
            _callback = callback;
            _undoCallback = undoCallback;

            if (data != null)
                _vertex = data;
            else
                _vertex = DataVertex.Create();
        }

        //dispose operation, release reference
        public virtual void Dispose()
        {
            _vertex.Dispose();
            _vertex = null;
            _vCtrl = null;
            _callback = null;
            _undoCallback = null;
            _graph = null;
        }
    }
}
