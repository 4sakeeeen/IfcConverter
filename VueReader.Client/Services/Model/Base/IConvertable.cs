using Xbim.Common;

namespace IfcConverter.Client.Services.Model.Base
{
    public interface IConvertable<T>
    {
        T Convert(IModel model);
    }
}
