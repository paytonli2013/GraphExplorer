using Microsoft.Practices.Prism.Commands;
using Orc.GraphExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.GraphExplorer
{
    public class GraphExplorerViewmodel : NotificationObject
    {

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
            _operations.Clear();
            _operationsRedo.Clear();
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
            HasChange = false;
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

            if (op == null||!op.IsUnDoable)
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

        #endregion
    }
}
