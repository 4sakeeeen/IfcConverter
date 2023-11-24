using IfcConverter.Client.Services.Base;
using System;
using System.Threading.Tasks;

namespace IfcConverter.Client.Services
{
    public sealed class RelayCommandAsync : CommandAsync
    {
        private readonly Func<Task> _Callback;

        public RelayCommandAsync(Func<Task> callback, Action<Exception>? onException = null) : base(onException) => this._Callback = callback;

        protected override async Task ExecuteAsync(object? parameter) => await this._Callback.Invoke();
    }
}
