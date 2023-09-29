using System;
using System.IO;
using IfcConverter.Domain.Models.Vue;
using IfcConverter.Domain.Models.Vue.Services;

namespace IfcConverter.Domain.Services
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


    public static class VueService
    {
        private static LineType _LineType = LineType.UNDEFINDED;

        public static LineType LineType
        {
            get { return _LineType; }
            set
            {
                if (value != LineType.DATA && GeometryReadingState != GeometryReadingStates.NOT_READING && _LineType == value) { throw new Exception($"Try to set line definition state to same value '{value}'"); }
                _LineType = value;
            }
        }


        private static ElementReadingStates _ElementReadingState = ElementReadingStates.NOT_READING;

        public static ElementReadingStates ElementReadingState
        {
            get { return _ElementReadingState; }
            set
            {
                if (_ElementReadingState == value) { throw new Exception($"Try to set element reading status to same value '{value}'"); }
                _ElementReadingState = value;
            }
        }


        private static GeometryReadingStates _GeometryReadingState = GeometryReadingStates.NOT_READING;

        public static GeometryReadingStates GeometryReadingState
        {
            get { return _GeometryReadingState; }
            set
            {
                if (_GeometryReadingState == value) { throw new Exception($"Try to set geometry reading status to same value '{value}'"); }
                _GeometryReadingState = value;
            }
        }


        public static VueModel ReadVueTextFile(string path)
        {
            VueReader vueReader = new();

            VueModel model = new();
            using StreamReader reader = new(path);
            string? line = null;
            GraphicElement? graphicElement = null;
            GeometryGroup? geometryGroup = null;


            Action<string> actualizeStatuses = (l) =>
            {
                if (l.StartsWith(new string('*', 39) + " GRAPHIC : "))
                {
                    LineType = LineType.MARKER;         
                    ElementReadingState = ElementReadingStates.CREATED;
                    return;
                }
                
                if (l == new string('*', 39) + " COMPLETED " + new string('*', 39))
                {
                    LineType = LineType.MARKER;
                    ElementReadingState = ElementReadingStates.COMPLETED;
                    return;
                }

                if (l == "Label properties { " && ElementReadingState == ElementReadingStates.READING)
                {
                    LineType = LineType.MARKER;
                    ElementReadingState = ElementReadingStates.READING_PROPERTIES;
                    return;
                }

                if (l == "}" && ElementReadingState == ElementReadingStates.READING_PROPERTIES)
                {
                    LineType = LineType.MARKER;
                    ElementReadingState = ElementReadingStates.READING;
                    return;
                }

                if (l == "GROUP_TYPE" && ElementReadingState == ElementReadingStates.READING)
                {
                    LineType = LineType.MARKER;
                    ElementReadingState = ElementReadingStates.READING_GEOMETRY;
                    GeometryReadingState = GeometryReadingStates.READING_STARTED;
                    return;
                }

                if (l == "GROUP_END" && ElementReadingState == ElementReadingStates.READING_GEOMETRY)
                {
                    LineType = LineType.MARKER;
                    ElementReadingState = ElementReadingStates.READING;
                    GeometryReadingState = GeometryReadingStates.READING_ENDED;
                    return;
                }

                // ТОЧНО НЕ МАРКЕР

                // переключаем, т.к.CREATED - всего одна строчка
                if (ElementReadingState == ElementReadingStates.CREATED) { ElementReadingState = ElementReadingStates.READING; }

                // переключаем, т.к. COMPLETED - всего одна строчка
                if (ElementReadingState == ElementReadingStates.COMPLETED) { ElementReadingState = ElementReadingStates.NOT_READING; }

                // переключаем, т.к. READING_STARTED - всего одна строчка
                if (GeometryReadingState == GeometryReadingStates.READING_STARTED) { GeometryReadingState = GeometryReadingStates.READING; }

                // переключаем, т.к. READING_ENDED - всего одна строчка
                if (GeometryReadingState == GeometryReadingStates.READING_ENDED) { GeometryReadingState = GeometryReadingStates.NOT_READING; }

                // Какие данные считаются нужными для парсинга
                if (ElementReadingState == ElementReadingStates.READING_PROPERTIES) { LineType = LineType.DATA; }

                // в течении всей фукнкции не переключился LineDefinition,
                // и Denifnition - точно не MARKER. По этому переключаем на неизвестный
                if (LineType == LineType.MARKER) { LineType = LineType.UNDEFINDED; }
            };

            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    actualizeStatuses(line);
                }
                catch(Exception e)
                {
                    throw new Exception("", e);
                }

                if (LineType == LineType.MARKER)
                {
                    switch (ElementReadingState)
                    {
                        case ElementReadingStates.CREATED:
                            graphicElement = new GraphicElement();
                            break;

                        case ElementReadingStates.COMPLETED:
                            if (graphicElement == null) { throw new Exception("Graphic element ending excepted but instance not created"); }
                            model.GraphicElements.Add(graphicElement);
                            graphicElement = null;
                            break;
                    }

                    switch (GeometryReadingState)
                    {
                        case GeometryReadingStates.READING_STARTED:
                            geometryGroup = new GeometryGroup();
                            break;

                        case GeometryReadingStates.READING_ENDED:
                            if (graphicElement == null) { throw new Exception("Graphic element's geometry ending excepted but instance not created"); }
                            graphicElement.Geometry = geometryGroup;
                            geometryGroup = null;
                            break;
                    }
                }

                if (LineType == LineType.DATA && ElementReadingState == ElementReadingStates.READING_PROPERTIES)
                {
                    if (graphicElement == null) { throw new Exception("Property excepted but instance not created"); }
                    string[] propValues = line.Split(" : ");
                    graphicElement.LabelProperties.Add(propValues[0], propValues[1]);
                }
            }

            return model;
        }
    }
}
