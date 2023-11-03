using Xbim.Common;

namespace IfcConverter.Client.Services.Model.Base
{
    public interface IIfcConvertable<T>
    {
        public T IfcConvert(IModel model);
    }
}
