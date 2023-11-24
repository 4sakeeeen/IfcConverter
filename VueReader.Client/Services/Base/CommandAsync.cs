using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IfcConverter.Client.Services.Base
{
    public abstract class CommandAsync : ICommand
    {
        private readonly Action<Exception>? _OnException = null;

        private bool _IsExecuting;
        public bool IsExecuting
        {
            get => _IsExecuting;
            set
            {
                this._IsExecuting = value;
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? CanExecuteChanged;

        public CommandAsync(Action<Exception>? onExecption = null) => this._OnException = onExecption;

        public bool CanExecute(object? parameter) => !this.IsExecuting;

        public async void Execute(object? parameter)
        {
            this.IsExecuting = true;

            try
            {
                await this.ExecuteAsync(parameter);
            }
            catch (Exception ex)
            {
                this._OnException?.Invoke(ex);
            }
            finally
            {
                this.IsExecuting = false;
            }
        }

        protected abstract Task ExecuteAsync(object? parameter);
    }
}
