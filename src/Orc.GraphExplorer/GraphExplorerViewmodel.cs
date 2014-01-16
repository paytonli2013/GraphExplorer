using GraphX;
using Microsoft.Practices.Prism.Commands;
using Orc.GraphExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer
{
    public class GraphExplorerViewmodel : NotificationObject ,IObserver<IOperation>
    {
        #region Properties

        private List<int> _selectedVertices = new List<int>();

        bool _isInEditing;

        public bool IsInEditing
        {
            get { return _isInEditing; }
            set
            {
                if (_isInEditing != value)
                {
                    _isInEditing = value;
                    _selectedVertices.Clear();
                    UpdateIsInEditing(_isInEditing);
                    if (_isInEditing)
                        CanDrag = true;
                    RaisePropertyChanged("IsInEditing");
                }
            }
        }

        private void UpdateIsInEditing(bool value)
        {
            if (View == null)
                return;

            var area = View.Area;
            var highlightEnable = !value;
            var highlighted = false;
            foreach (var v in area.VertexList)
            {
                v.Key.IsEditing = value;
                //if (value)
                //{
                //    v.Key.ChangedCommited += ChangedCommited;
                //}
                //else
                //{
                //    v.Key.ChangedCommited -= ChangedCommited;
                //}

                HighlightBehaviour.SetIsHighlightEnabled(v.Value, highlightEnable);
                HighlightBehaviour.SetHighlighted(v.Value, highlighted);
            }

            foreach (var edge in area.EdgesList)
            {
                HighlightBehaviour.SetIsHighlightEnabled(edge.Value, highlightEnable);
                HighlightBehaviour.SetHighlighted(edge.Value, highlighted);
            }
        }

        bool _canDrag;

        public bool CanDrag
        {
            get { return _canDrag; }
            set
            {
                if (_canDrag != value)
                {
                    _canDrag = value;
                    UpdateCanDrag(value);
                    RaisePropertyChanged("CanDrag");
                }
            }
        }

        private void UpdateCanDrag(bool value)
        {
            if (View == null)
                return;

            var area = View.Area;
            foreach (var item in area.VertexList)
            {
                DragBehaviour.SetIsDragEnabled(item.Value, value);
            }
            //throw new NotImplementedException();
        }

        bool _hasUnCommitChange;

        public bool HasChange
        {
            get
            {
                return _hasUnCommitChange;
            }
            set
            {
                _hasUnCommitChange = value;
                RaisePropertyChanged("HasChange");
            }
        }

        public bool HasUndoable
        {
            get
            {
                return _operations.Any(o => o.IsUnDoable);
            }
        }

        public bool HasRedoable
        {
            get
            {
                return _operationsRedo.Any(o => o.IsUnDoable);
            }
        }

        public IOperation LastOperation
        {
            get
            {
                return _operations.FirstOrDefault();
            }
        }

        public GraphExplorer View { get; set; }

        IGraphDataService GraphDataService { get; set; }

        List<IOperation> _operations;

        public List<IOperation> Operations
        {
            get { return _operations; }
            set
            {
                _operations = value;
                RaisePropertyChanged("Operations");
            }
        }

        List<IOperation> _operationsRedo;

        public List<IOperation> OperationsRedo
        {
            get { return _operationsRedo; }
            set
            {
                _operationsRedo = value;
                RaisePropertyChanged("OperationsRedo");
            }
        }

        #endregion

        //Summary
        //    constructor of GraphExplorerViewmodel
        public GraphExplorerViewmodel()
        {
            _operationsRedo = new List<IOperation>();
            _operations = new List<IOperation>();
        }

        //Summary
        //    Execute new operaton and put the operation in to undoable list
        public void Do(IOperation operation)
        {
            operation.Do();
            HasChange = true;
            _operations.Insert(0, operation);

            foreach (var v in _operationsRedo)
            {
                v.Dispose();
            }
            _operationsRedo.Clear();

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        //Summary
        //    Commit changes to data source, after commit, clear undo/redo list
        public void Commit()
        {
            _selectedVertices.Clear();
            _operations.Clear();
            _operationsRedo.Clear();
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
            HasChange = false;

            IsInEditing = false;
        }

        #region Commands

        DelegateCommand _undoCommand;

        public DelegateCommand UndoCommand
        {
            get
            {
                if (_undoCommand == null)
                    _undoCommand = new DelegateCommand(ExecuteUndo, CanExecuteUndo);
                return _undoCommand;
            }
        }

        void ExecuteUndo()
        {
            var op = Operations.FirstOrDefault();

            if (op == null || !op.IsUnDoable)
                return;

            op.UnDo();

            Operations.Remove(op);
            OperationsRedo.Insert(0, op);

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        bool CanExecuteUndo()
        {
            return HasUndoable;
        }

        DelegateCommand _redoCommand;

        public DelegateCommand RedoCommand
        {
            get
            {
                if (_redoCommand == null)
                    _redoCommand = new DelegateCommand(ExecuteRedo, CanExecuteRedo);
                return _redoCommand;
            }
        }

        void ExecuteRedo()
        {
            var op = OperationsRedo.FirstOrDefault();

            if (op == null || !op.IsUnDoable)
                return;

            op.Do();

            OperationsRedo.Remove(op);
            Operations.Insert(0, op);

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        bool CanExecuteRedo()
        {
            return HasRedoable;
        }

        public void CreateEdge(int fromId, int toId)
        {
            if (View == null)
                return;

            var area = View.Area;
            var source = area.VertexList.Where(pair => pair.Key.Id == fromId).Select(pair => pair.Key).FirstOrDefault();
            var target = area.VertexList.Where(pair => pair.Key.Id == toId).Select(pair => pair.Key).FirstOrDefault();
            if (source == null || target == null)
                return;

            Do(new CreateEdgeOperation(area, source, target,
                (e) =>
                {
                    //on vertex created
                    //_selectedVertices.Add(v.Id);

                    HighlightBehaviour.SetIsHighlightEnabled(e, false);
                    HighlightBehaviour.SetHighlighted(e, false);

                    HighlightBehaviour.SetHighlighted(area.VertexList[source], false);
                    HighlightBehaviour.SetHighlighted(area.VertexList[target], false);

                    //UpdateIsInEditing(true);
                },
                (e) =>
                {
                    //_selectedVertices.Remove(v.Id);
                    //on vertex recreated
                }));
        }

        #endregion

        #region IObserver<IOperation>

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            //throw new NotImplementedException();
        }

        public void OnNext(IOperation value)
        {
            //throw new NotImplementedException();
            Do(value);
        }

        List<IDisposable> _observers = new List<IDisposable>();

        public void OnVertexLoaded(IEnumerable<DataVertex> vertexes,bool clearHistory = false)
        {
            if (clearHistory)
                _observers.Clear();

            foreach (var vertex in vertexes)
            {
                _observers.Add(vertex.Subscribe(this));
            }
        }

        #endregion
    }
}
