﻿using System;
using System.Windows.Input;

namespace BSTClient
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public DelegateCommand(Action<object> execute) : this(execute, null) { }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }


        public void ChangeCanExecute()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand<T> : DelegateCommand
    {
        public DelegateCommand(Action<T> execute)
            : base(obj => execute?.Invoke((T)obj))
        {
        }

        public DelegateCommand(Action<T> execute, Predicate<object> canExecute) :
            base(obj => execute?.Invoke((T)obj), canExecute)
        {
        }
    }
}