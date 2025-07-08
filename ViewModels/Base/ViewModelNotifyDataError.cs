using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NanoTools2.ViewModels.Base
{
    class ViewModelNotifyDataError : BindableBase, INotifyDataErrorInfo
    {

        protected void FloatValidateProperty(string propertyName, string value)
        {
            float dummy;
            if (string.IsNullOrWhiteSpace(value))
                AddError(propertyName, "no input");
            else if (!float.TryParse(value, out dummy))
                AddError(propertyName, "not numeric");
            else
                RemoveError(propertyName);
        }

        protected void NumericValidateProperty(string propertyName, string value)
        {
            int dummy;
            if (string.IsNullOrWhiteSpace(value))
                AddError(propertyName, "no input");
            else if (!int.TryParse(value, out dummy))
                AddError(propertyName, "not numeric");
            else
                RemoveError(propertyName);
        }

        protected void StringValidateProperty(string propertyName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                AddError(propertyName, "no input");
            else
                RemoveError(propertyName);
        }



        #region 発生中のエラーを保持する処理を実装
        readonly Dictionary<string, List<string>> _currentErrors = new Dictionary<string, List<string>>();

        protected void AddError(string propertyName, string error)
        {
            if (!_currentErrors.ContainsKey(propertyName))
                _currentErrors[propertyName] = new List<string>();

            if (!_currentErrors[propertyName].Contains(error))
            {
                _currentErrors[propertyName].Add(error);
                OnErrorsChanged(propertyName);
            }
        }

        protected void RemoveError(string propertyName)
        {
            if (_currentErrors.ContainsKey(propertyName))
                _currentErrors.Remove(propertyName);

            OnErrorsChanged(propertyName);
        }
        #endregion

        private void OnErrorsChanged(string propertyName)
        {
            var h = this.ErrorsChanged;
            if (h != null)
            {
                h(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        #region INotifyDataErrorInfoの実装
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) ||
                !_currentErrors.ContainsKey(propertyName))
                return null;

            return _currentErrors[propertyName];
        }

        public bool HasErrors
        {
            get { return _currentErrors.Count > 0; }
        }
        #endregion
    }
}
