using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.IO;

namespace IFConvertable.IFCore
{
    public class IfcConverter
    {
        private readonly IfcStore _Model;

        public IfcConverter()
        {
            _Model = IfcStore.Create(
                new XbimEditorCredentials
                {
                    ApplicationDevelopersName = "App Devs Name",
                    ApplicationFullName = "Full name of Application",
                    ApplicationIdentifier = "Application ID",
                    ApplicationVersion = "App Ver.1",
                    EditorsFamilyName = "Erokhov",
                    EditorsGivenName = "Aleksandr",
                    EditorsOrganisationName = "Default Company"
                },
                XbimSchemaVersion.Ifc4,
                XbimStoreType.EsentDatabase);
        }
    }
}