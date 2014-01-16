using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


namespace Orc.GraphExplorer.Model
{
    public class PropertyViewmodel : NotificationObject
    {
        #region Properties

        int _index;

        public int Index
        {
            get { return _index; }
        }

        bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        bool _isEditing;

        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                _isEditing = value;
                RaisePropertyChanged("IsEditing");
            }
        }

        bool _isDirty;

        public bool IsDirty
        {
            get { return OriginalValue != Value; }
        }

        string _originalKey;

        public string OriginalKey
        {
            get { return _originalKey; }
        }

        string _key;

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                RaisePropertyChanged("Key");
            }
        }

        string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
                RaisePropertyChanged("IsDirty");
            }
        }

        string _originalValue;

        public string OriginalValue
        {
            get { return _originalValue; }
            private set
            {
                _originalValue = value;
                RaisePropertyChanged("OriginalValue");
                RaisePropertyChanged("IsDirty");
            }
        }

        DataVertex _data;

        #endregion

        public PropertyViewmodel(int index,string key, string value, DataVertex data)
        {
            _index = index;
            _originalKey = _key = key;
            _value = _originalValue = value;
            _data = data;
        }

        public void Reset()
        {
            Value = OriginalValue;
        }

        public void Commit()
        {
            OriginalValue = Value;
        }
    }
}
