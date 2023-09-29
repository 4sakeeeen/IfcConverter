namespace IfcConverter.Domain.Models.Vue.Services.Common
{
    /// <summary>
    /// Geometry group reading states.
    /// </summary>
    public enum GeometryReadingStates
    {
        /// <summary>
        /// Reading data that's not mentioned to a graphic element's geometry.
        /// </summary>
        NOT_READING,
        /// <summary>
        /// Reading data that's mentioned to a graphic element's geometry.
        /// </summary>
        READING,
        READING_STARTED,
        READING_ENDED
    }
}
