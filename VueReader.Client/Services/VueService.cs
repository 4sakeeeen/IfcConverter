using System;
using System.IO;
using VueReader.Domain.Models.Vue;

namespace VueReader.Domain.Services
{
    public enum ElementReadingStatus { NOT_READING, READING, READING_PROPERTIES, READING_GEOMETRY, COMPLETED }
    public enum LineDefinition { UNDEFINDED, DATA, MARKER }


    public static class VueService
    {
        private static ElementReadingStatus _ElementReadingStatus = ElementReadingStatus.NOT_READING;

        public static ElementReadingStatus ElementReadingStatus
        {
            get { return _ElementReadingStatus; }
            set
            {
                if (_ElementReadingStatus == value) { throw new Exception($"Try to set element reading status to same value '{value}'"); }
                _ElementReadingStatus = value;
            }
        }

        private static LineDefinition _LineDefinition = LineDefinition.UNDEFINDED;

        public static LineDefinition LineDefinition
        {
            get { return _LineDefinition; }
            set
            {
                if (_LineDefinition == value) { throw new Exception($"Try to set line definition state to same value '{value}'"); }
                _LineDefinition = value;
            }
        }


        public static VueModel ReadVueTextFile(string path)
        {
            VueModel model = new();
            using StreamReader reader = new(path);
            string? line = null;
            GraphicElement? graphicElement = null;


            Action<string> actualizeStatuses = (l) =>
            {
                if (l.StartsWith(new string('*', 39) + " GRAPHIC : "))
                {
                    LineDefinition = LineDefinition.MARKER;         
                    ElementReadingStatus = ElementReadingStatus.READING;
                    return;
                }
                
                if (l == new string('*', 39) + " COMPLETED " + new string('*', 39))
                {
                    LineDefinition = LineDefinition.MARKER;
                    ElementReadingStatus = ElementReadingStatus.COMPLETED;
                    return;
                }

                if (l == "Label properties { " && ElementReadingStatus == ElementReadingStatus.READING)
                {
                    LineDefinition = LineDefinition.MARKER;

                    ElementReadingStatus = ElementReadingStatus.READING_PROPERTIES;
                    return;
                }

                if (l == "}" && ElementReadingStatus == ElementReadingStatus.READING_PROPERTIES)
                {
                    LineDefinition = LineDefinition.MARKER;

                    ElementReadingStatus = ElementReadingStatus.READING;
                    return;
                }

                // тут НЕ маркеры
                if (ElementReadingStatus == ElementReadingStatus.COMPLETED)
                {
                    ElementReadingStatus = ElementReadingStatus.NOT_READING;
                    LineDefinition = LineDefinition.UNDEFINDED;
                }

                if (ElementReadingStatus == ElementReadingStatus.READING)
                {
                    LineDefinition = LineDefinition.DATA;
                }
            };

            while ((line = reader.ReadLine()) != null)
            {
                actualizeStatuses(line);

                if (lineDef == LineDefinition.MARKER)
                {
                    switch (ElementReadingStatus)
                    {
                        case ElementReadingStatus.READING:
                            graphicElement = new GraphicElement();
                            break;

                        case ElementReadingStatus.COMPLETED:
                            if (graphicElement == null) { throw new Exception("Graphic element ending excepted but instance not created"); }
                            model.GraphicElements.Add(graphicElement);
                            graphicElement = null;
                            break;
                    }
                }
            }

            return model;
        }
    }
}
