using GraphX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using YAXLib;
using Orc.GraphExplorer.Model;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;

namespace Orc.GraphExplorer
{
    public class DataVertex : VertexBase, INotifyPropertyChanged,IDisposable
    {
        bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public event EventHandler ChangedCommited;

        bool isInEditMode;

        public bool IsInEditMode
        {
            get
            {
                return isInEditMode;
            }
            set
            {
                isInEditMode = value;
                RaisePropertyChanged("IsInEditMode");
            }
        }

        bool _isEditing;

        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                _isEditing = value;
                UpdatePropertiesIsEditing(_isEditing);
                RaisePropertyChanged("IsEditing");
            }
        }

        private void UpdatePropertiesIsEditing(bool isEditing)
        {
            if (Properties == null)
                return;

            foreach (var vm in Properties)
            {
                vm.IsEditing = isEditing;
            }
        }

        private static int _totalCount = 0;

        public static int TotalCount
        {
            get { return DataVertex._totalCount; }
        }

        private static int _maxId;

        public string Title { get; set; }
        public int Id { get; set; }

        Dictionary<string, string> _properties;

        public ObservableCollection<PropertyViewmodel> Properties { get; set; }

        [YAXDontSerialize]
        public ImageSource Icon { get; set; }

        #region Calculated or static props
        [YAXDontSerialize]
        public DataVertex Self
        {
            get { return this; }
        }

        public override string ToString()
        {
            return Title;
        }

        #endregion

        /// <summary>
        /// Default constructor for this class
        /// (required for serialization).
        /// </summary>
        public DataVertex()
            : this(-1, "")
        {

        }

        private static readonly Random Rand = new Random();

        public DataVertex(int id, string title = "")
        {
            base.ID = id;
            Id = id;
            Title = (title == string.Empty) ? id.ToString() : title;

            _totalCount++;
            _maxId = id > _maxId ? id : _maxId;
        }

        public static DataVertex Create()
        {
            return new DataVertex(++_maxId);
        }

        public void SetProperties(Dictionary<string, string> dictionary)
        {
            _properties = dictionary;
            Properties = GenerateProperties(dictionary, this);
        }

        private static ObservableCollection<PropertyViewmodel> GenerateProperties(Dictionary<string, string> dictionary, DataVertex data)
        {
            var pvs = from pair in dictionary select new PropertyViewmodel(pair.Key, pair.Value, data);

            return new ObservableCollection<PropertyViewmodel>(pvs);
        }

        #region INotifyPropertyChanged
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>        

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Commands

        DelegateCommand _addCommand;

        public DelegateCommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                    _addCommand = new DelegateCommand(ExecAdd, CanExecAdd);
                return _addCommand;
            }
        }

        void ExecAdd()
        {
            if (Properties == null)
                Properties = new ObservableCollection<PropertyViewmodel>();

            Properties.Add(new PropertyViewmodel("", "", this) { IsEditing = true});
        }

        DelegateCommand _resetCommand;

        public DelegateCommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                    _resetCommand = new DelegateCommand(ExecReset, CanExecReset);
                return _resetCommand;
            }
        }

        void ExecReset()
        {
            if (Properties == null)
                return;

            foreach (var vm in Properties)
            {
                vm.Reset();
            }
        }

        bool CanExecReset()
        {
            return true;
        }

        private void Submit()
        {
            var handler = ChangedCommited;
            if (handler != null)
            {
                ChangedCommited.Invoke(this, new EventArgs());
            }
            //throw new NotImplementedException();
        }

        private void Update(PropertyViewmodel vm)
        {
            Update(vm.OriginalKey, vm.Key, vm.Value);
        }

        private void Update(string oriKey, string key, string value, Action<Result> onCompleted = null)
        {
            if (!_properties.ContainsKey(oriKey))
                return;

            bool isKeyChanged = oriKey == key;

            try
            {
                if (isKeyChanged)
                {
                    _properties.Remove(oriKey);
                    _properties.Add(key, value);
                }
                else
                {
                    _properties[oriKey] = value;
                }
            }
            catch (Exception ex)
            {
                if (onCompleted != null)
                {
                    onCompleted.Invoke(new Result(new Exception(string.Format("error occured during updating property [{0}]", oriKey), ex)));
                }
                else
                {
                    throw new Exception(string.Format("error occured during updating property [{0}]", oriKey), ex);
                }
            }

            if (onCompleted != null)
            {
                onCompleted.Invoke(new Result(null));
            }
        }

        bool CanExecAdd()
        {
            return true;
        }

        #endregion

        public void Commit()
        {
            if (Properties == null)
                return;

            foreach (var p in Properties)
            {
                p.Commit();
            }
        }

        public void Dispose()
        {
            _properties = null;
            _totalCount--;
            //if (_maxId <= this.Id+1)
            //    _maxId--;
        }
    }
}
