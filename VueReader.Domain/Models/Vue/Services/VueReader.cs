using IfcConverter.Domain.Models.Vue.Services.Common;

namespace IfcConverter.Domain.Models.Vue.Services
{
    public class VueReader
    {
        private readonly string _FilePath;

        private string? _CurrentLine = null;


        private LineType _LineType = LineType.UNDEFINDED;

        public LineType LineType
        {
            get { return _LineType; }
            set
            {
                if (value != LineType.DATA && GeometryReadingState != GeometryReadingStates.NOT_READING && _LineType == value) { throw new Exception($"Try to set line definition state to same value '{value}'"); }
                _LineType = value;
            }
        }


        private ElementReadingStates _ElementReadingState = ElementReadingStates.NOT_READING;

        public ElementReadingStates ElementReadingState
        {
            get { return _ElementReadingState; }
            set
            {
                if (_ElementReadingState == value) { throw new Exception($"Try to set element reading status to same value '{value}'"); }
                _ElementReadingState = value;
            }
        }


        private GeometryReadingStates _GeometryReadingState = GeometryReadingStates.NOT_READING;

        public GeometryReadingStates GeometryReadingState
        {
            get { return _GeometryReadingState; }
            set
            {
                if (_GeometryReadingState == value) { throw new Exception($"Try to set geometry reading status to same value '{value}'"); }
                _GeometryReadingState = value;
            }
        }


        public VueReader(string filePath) { _FilePath = filePath; }

        public VueModel GetModel()
        {
            VueModel model = new();
            using StreamReader reader = new(_FilePath);
            string? line = null;
            GraphicElement? currentGraphicElement = null;
            GeometryGroup? currentGeometryGroup = null;

            while ((line = reader.ReadLine()) != null)
            {
                _CurrentLine = line;
                try
                {
                    ActualizeStatuses();
                }
                catch (Exception e)
                {
                    throw new Exception("", e);
                }

                if (LineType == LineType.MARKER)
                {
                    switch (ElementReadingState)
                    {
                        case ElementReadingStates.CREATED:
                            currentGraphicElement = new GraphicElement();
                            break;

                        case ElementReadingStates.COMPLETED:
                            if (currentGraphicElement == null) { throw new Exception("Graphic element ending excepted but instance not created"); }
                            model.GraphicElements.Add(currentGraphicElement);
                            currentGraphicElement = null;
                            break;
                    }

                    switch (GeometryReadingState)
                    {
                        case GeometryReadingStates.READING_STARTED:
                            currentGeometryGroup = new GeometryGroup();
                            break;

                        case GeometryReadingStates.READING_ENDED:
                            if (currentGraphicElement == null) { throw new Exception("Graphic element's geometry ending excepted but instance not created"); }
                            currentGraphicElement.Geometry = currentGeometryGroup;
                            currentGeometryGroup = null;
                            break;
                    }
                }

                if (LineType == LineType.DATA && ElementReadingState == ElementReadingStates.READING_PROPERTIES)
                {
                    if (currentGraphicElement == null) { throw new Exception("Property excepted but instance not created"); }
                    string[] propValues = line.Split(" : ");
                    currentGraphicElement.LabelProperties.Add(propValues[0], propValues[1]);
                }
            }

            return model;
        }
    
        private void ActualizeStatuses()
        {
            if (_CurrentLine == null) { throw new Exception("Actualizing reader states was failed. Reader's current line is null."); }

            if (_CurrentLine.StartsWith(new string('*', 39) + " GRAPHIC : "))
            {
                LineType = LineType.MARKER;
                ElementReadingState = ElementReadingStates.CREATED;
                return;
            }

            if (_CurrentLine == new string('*', 39) + " COMPLETED " + new string('*', 39))
            {
                LineType = LineType.MARKER;
                ElementReadingState = ElementReadingStates.COMPLETED;
                return;
            }

            if (_CurrentLine == "Label properties { " && ElementReadingState == ElementReadingStates.READING)
            {
                LineType = LineType.MARKER;
                ElementReadingState = ElementReadingStates.READING_PROPERTIES;
                return;
            }

            if (_CurrentLine == "}" && ElementReadingState == ElementReadingStates.READING_PROPERTIES)
            {
                LineType = LineType.MARKER;
                ElementReadingState = ElementReadingStates.READING;
                return;
            }

            if (_CurrentLine == "GROUP_TYPE" && ElementReadingState == ElementReadingStates.READING)
            {
                LineType = LineType.MARKER;
                ElementReadingState = ElementReadingStates.READING_GEOMETRY;
                GeometryReadingState = GeometryReadingStates.READING_STARTED;
                return;
            }

            if (_CurrentLine == "GROUP_END" && ElementReadingState == ElementReadingStates.READING_GEOMETRY)
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
        }
    }
}
