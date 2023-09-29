namespace IfcConverter.Domain.Models.Vue.Services.Common
{
    /// <summary>
    /// Graphic element reading states.
    /// </summary>
    public enum ElementReadingStates
    {
        /// <summary>
        /// Reading data that's not mentioned to a graphic element.
        /// </summary>
        NOT_READING,
        /// <summary>
        /// Reading data that's mentioned to a graphic element.
        /// </summary>
        READING,
        /// <summary>
        /// Reading data that's mentioned to a graphic element as element's label properties.
        /// </summary>
        READING_PROPERTIES,
        /// <summary>
        /// Reading data that's mentioned to a graphic element as element's geometries.
        /// </summary>
        READING_GEOMETRY,
        /// <summary>
        /// A graphic element was created recently.
        /// </summary>
        CREATED,
        /// <summary>
        /// Reading of a graphic element was finished recently.
        /// </summary>
        COMPLETED
    }
}
