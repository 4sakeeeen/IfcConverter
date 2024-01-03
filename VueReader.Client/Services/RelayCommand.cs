using System;
using System.Windows.Input;

namespace IfcConverter.Client.Services
{
    internal sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _Execute;

        private readonly Predicate<T>? _CanExecute = null;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _CanExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _CanExecute == null || _CanExecute((T)parameter);

        public void Execute(object? parameter) => _Execute.Invoke((T)parameter);
    }

    internal sealed class RelayCommand : ICommand
    {
        private readonly Action _Execute;

        private readonly Predicate<object?>? _CanExecute = null;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action execute, Predicate<object?>? canExecute = null)
        {
            _Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _CanExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _CanExecute == null || _CanExecute(parameter);

        public void Execute(object? parameter) => _Execute.Invoke();
    }
}
