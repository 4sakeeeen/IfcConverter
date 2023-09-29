namespace IfcConverter.Domain.Models.Vue.Services.Common
{
    /// <summary>
    /// Types of a line string of a vue text file.
    /// </summary>
    public enum LineType
    {
        /// <summary>
        /// Redudant data that's shall not be used.
        /// </summary>
        UNDEFINDED,
        /// <summary>
        ///  Represented as vue useful data.
        /// </summary>
        DATA,
        /// <summary>
        /// Represented as the marker that's mean internal data used to business logic.
        /// </summary>
        MARKER
    }
}
