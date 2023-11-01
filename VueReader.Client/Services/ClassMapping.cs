
namespace IfcConverter.Client.Services
{
    internal readonly struct ClassMapping
    {
        public int SmartClassID { get; }

        public string SmartClassName { get; }

        public string SmartClassUserName { get; }

        public System.Type IfcClass { get; }

        public ClassMapping(int smartClassID, string smartClassName, string smartClassUserName, System.Type ifcClass)
        {
            SmartClassID = smartClassID;
            SmartClassName = smartClassName;
            SmartClassUserName = smartClassUserName;
            IfcClass = ifcClass;
        }
    }
}
