using IfcConverter.Domain.Models.Vue.Common;
using IfcConverter.Domain.Models.Vue.Geometries;

namespace IfcConverter.Domain.Models.Vue.Services
{
    public static class VueReader
    {
        public static List<Line> ParseLines(string data)
        {
            List<Line> lines = new();

            foreach (string txtRow in data.Split("\r\n"))
            {
                if (txtRow.StartsWith("Element No. :")) { lines.Add(new Line { ElementNo = int.Parse(txtRow.Split(" : ")[1]) }); }
                if (txtRow.StartsWith("Aspect No. :")) { lines.Last().AspectNo = int.Parse(txtRow.Split(" : ")[1]); };

                if (txtRow == "Start Point :")
                {
                    lines.Last().StartPoint = new() { X = int.MaxValue, Y = int.MaxValue, Z = int.MaxValue };
                }
                else if (lines.Last().StartPoint != null)
                {
                    Position3D? startPos = lines.Last().StartPoint;
                    if (startPos != null && startPos.Z == int.MaxValue)
                    {
                        if (startPos.X == int.MaxValue) { startPos.X = double.Parse(txtRow.Trim()); continue; };
                        if (startPos.Y == int.MaxValue) { startPos.Y = double.Parse(txtRow.Trim()); continue; };
                        if (startPos.Z == int.MaxValue) { startPos.Z = double.Parse(txtRow.Trim()); continue; };
                    }
                }

                if (txtRow == "End   Point :")
                {
                    lines.Last().EndPoint = new() { X = int.MaxValue, Y = int.MaxValue, Z = int.MaxValue };
                }
                else if (lines.Last().EndPoint != null)
                {
                    Position3D? endPoint = lines.Last().EndPoint;
                    if (endPoint != null && endPoint.Z == int.MaxValue)
                    {
                        if (endPoint.X == int.MaxValue) { endPoint.X = double.Parse(txtRow.Trim()); continue; };
                        if (endPoint.Y == int.MaxValue) { endPoint.Y = double.Parse(txtRow.Trim()); continue; };
                        if (endPoint.Z == int.MaxValue) { endPoint.Z = double.Parse(txtRow.Trim()); continue; };
                    }
                }
            }

            return lines;
        }


        public static List<ComponentLine> ParseBoundaryLines(string data)
        {
            List<ComponentLine> lines = new();

            foreach (string txtRow in data.Split("\r\n"))
            {
                if (txtRow.StartsWith("CURVE :")) { lines.Add(new ComponentLine { CurveNo = int.Parse(txtRow.Split(" : ")[1]) }); }

                if (txtRow == "Start Point :")
                {
                    lines.Last().StartPoint = new() { X = int.MaxValue, Y = int.MaxValue, Z = int.MaxValue };
                }
                else if (lines.Last().StartPoint != null)
                {
                    Position3D? startPos = lines.Last().StartPoint;
                    if (startPos != null && startPos.Z == int.MaxValue)
                    {
                        if (startPos.X == int.MaxValue) { startPos.X = double.Parse(txtRow.Trim()); continue; };
                        if (startPos.Y == int.MaxValue) { startPos.Y = double.Parse(txtRow.Trim()); continue; };
                        if (startPos.Z == int.MaxValue) { startPos.Z = double.Parse(txtRow.Trim()); continue; };
                    }
                }

                if (txtRow == "End   Point :")
                {
                    lines.Last().EndPoint = new() { X = int.MaxValue, Y = int.MaxValue, Z = int.MaxValue };
                }
                else if (lines.Last().EndPoint != null)
                {
                    Position3D? endPoint = lines.Last().EndPoint;
                    if (endPoint != null && endPoint.Z == int.MaxValue)
                    {
                        if (endPoint.X == int.MaxValue) { endPoint.X = double.Parse(txtRow.Trim()); continue; };
                        if (endPoint.Y == int.MaxValue) { endPoint.Y = double.Parse(txtRow.Trim()); continue; };
                        if (endPoint.Z == int.MaxValue) { endPoint.Z = double.Parse(txtRow.Trim()); continue; };
                    }
                }
            }

            return lines;
        }


        public static Dictionary<string, string?> ParseDictionary(string data)
        {
            Dictionary<string, string?> values = new();

            foreach (string key in data.Split("\r\n"))
            {
                string[] pair = key.Split(':');
                string v = pair[1].Trim();
                values.Add(pair[0].Trim(), !string.IsNullOrEmpty(v) ? v : null);
            }

            return values;
        }


        public static VueModel ParseModel(string filePath)
        {
            using StreamReader reader = new(filePath);
            VueModel model = new();
            string? line = null;
            string index = "0";

            bool isGraphicElementReading = false;
            List<string> graphicElementLines = new();

            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("*************************************** COMPLETED ***************************************")) { isGraphicElementReading = false; }
                if (isGraphicElementReading)
                { 
                    graphicElementLines.Add(line);
                }
                else if (graphicElementLines.Count > 1)
                {
                    if (model.GraphicElements.Count < 1500)
                    {
                        if (!model.GraphicElements.ContainsKey(index))
                        {
                            model.GraphicElements.Add(index, ParseGraphicElement(graphicElementLines));
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                if (line.StartsWith("*************************************** GRAPHIC : "))
                {
                    isGraphicElementReading = true;
                    index = line.Split("GRAPHIC : ")[1].Split(" INFO **")[0];
                }             
            }

            return model;
        }


        public static GraphicElement ParseGraphicElement(List<string> graphicElementLines)
        {
            GraphicElement graphicElement = new();

            bool isLabelProperties = false;
            List<string> labelPropertiesLines = new();

            bool isGroupTypeReading = false;
            List<string> groupTypeLines = new();

            graphicElementLines.ForEach(l =>
            {
                if (l == "}") { isLabelProperties = false; }
                if (isLabelProperties)
                {
                    labelPropertiesLines.Add(l);
                }
                else if (labelPropertiesLines.Count > 1)
                {
                    graphicElement.LabelProperties = ParseLabelProperties(labelPropertiesLines);
                    labelPropertiesLines = new();
                }
                if (l == "Label properties { ") { isLabelProperties = true; }

                if (l == "GROUP_END") { isGroupTypeReading = false; }
                if (isGroupTypeReading)
                {
                    groupTypeLines.Add(l);
                }
                else if (groupTypeLines.Count > 1)
                {
                    graphicElement.Geometry = ParseGroupType(groupTypeLines);
                    groupTypeLines = new();
                }
                if (l == "GROUP_TYPE") { isGroupTypeReading = true; }
            });

            return graphicElement;
        }


        public static Dictionary<string, string?> ParseLabelProperties(List<string> labelPropertiesLines)
        {
            Dictionary<string, string?> properties = new(labelPropertiesLines.Count);
            labelPropertiesLines.ForEach(labelPropertiesLine =>
            {
                string[] values = labelPropertiesLine.Split(" : ");
                properties.Add(values[0], values[1]);
            });
            return properties;
        }


        public static GeometryGroup ParseGroupType(List<string> groupTypeLines)
        {
            GeometryGroup geometry = new();
            List<string> geometryLines = new();

            for (int i = 0; i < groupTypeLines.Count; i++)
            {
                if (groupTypeLines[i].StartsWith("Element No. :"))
                {
                    object? element = ParseGeometryElement(geometryLines);
                    if (element is Line line) { geometry.Elements.Lines.Add(line); }
                    if (element is Projection projection) { geometry.Elements.Projections.Add(projection); }
                    if (element is Plane plane) { geometry.Elements.Planes.Add(plane); }
                    geometryLines = new();
                }

                geometryLines.Add(groupTypeLines[i]);
            }

            object? element2 = ParseGeometryElement(geometryLines);
            if (element2 is Line line2) { geometry.Elements.Lines.Add(line2); }
            if (element2 is Projection projection2) { geometry.Elements.Projections.Add(projection2); }
            if (element2 is Plane plane2) { geometry.Elements.Planes.Add(plane2); }

            return geometry;
        }

        
        public static object? ParseGeometryElement(List<string> geometryLines)
        {
            if (geometryLines.Count < 4) return null;
            return geometryLines[2] switch
            {
                "LINE_TYPE" => ParseLine(geometryLines),
                "PROJECTION_TYPE" => ParseProjection(geometryLines),
                "PLANE_TYPE" => ParsePlane(geometryLines),
                _ => null,
            };
        }


        public static Line ParseLine(List<string> geometryLines)
        {
            return new Line
            {
                ElementNo = int.Parse(geometryLines[0].Split(" : ")[1]),
                AspectNo = int.Parse(geometryLines[1].Split(" : ")[1]),
                StartPoint = new Position3D
                {
                    X = double.Parse(geometryLines[5].Trim()),
                    Y = double.Parse(geometryLines[6].Trim()),
                    Z = double.Parse(geometryLines[7].Trim())
                },
                EndPoint = new Position3D
                {
                    X = double.Parse(geometryLines[9].Trim()),
                    Y = double.Parse(geometryLines[10].Trim()),
                    Z = double.Parse(geometryLines[11].Trim())
                }
            };
        }


        public static Projection ParseProjection(List<string> geometryLines)
        {
            List<string> positions = geometryLines.GetRange(geometryLines.IndexOf("Positions") + 1, geometryLines.Count - geometryLines.IndexOf("Positions") - 4);
            return new Projection
            {
                //Vector = new Vector3D
                //{
                //    X = double.Parse(geometryLines[geometryLines.IndexOf("Projec Vector: ") + 1]),
                //    Y = double.Parse(geometryLines[geometryLines.IndexOf("Projec Vector: ") + 2]),
                //    Z = double.Parse(geometryLines[geometryLines.IndexOf("Projec Vector: ") + 3])
                //},
                //Positions = ParsePositions(positions)
            };
        }


        public static List<Position3D> ParsePositions(List<string> positionLines)
        {
            List<Position3D> positions = new();
            
            for (int i = 0; i < positionLines.Count; i += 4)
            {
                positions.Add(new Position3D
                {
                    X = double.Parse(positionLines[i]),
                    Y = double.Parse(positionLines[i + 1]),
                    Z = double.Parse(positionLines[i + 2])
                });
            }

            return positions;
        }


        public static Plane? ParsePlane(List<string> geometryLines)
        {
            if (geometryLines.Contains("ELLIPSE_TYPE")) { return null; }
            Plane plane = new()
            {
                Normal = new Vector3D
                {
                    X = double.Parse(geometryLines.ElementAt(5)),
                    Y = double.Parse(geometryLines.ElementAt(6)),
                    Z = double.Parse(geometryLines.ElementAt(7))
                },
                UDirection = new Vector3D
                {
                    X = double.Parse(geometryLines.ElementAt(9)),
                    Y = double.Parse(geometryLines.ElementAt(10)),
                    Z = double.Parse(geometryLines.ElementAt(11))
                },
                Boundaries = new()
                {
                    Lines = ParseComponentLines(geometryLines.GetRange(geometryLines.IndexOf("BOUNDARY_START : 1") + 2, geometryLines.IndexOf("BOUNDARY_END : 1") - geometryLines.IndexOf("BOUNDARY_START : 1") - 2))
                }
            };
            return plane;
        }


        public static List<ComponentLine> ParseComponentLines(List<string> geometryLines)
        {
            List<ComponentLine> lines = new();

            for (int i = 0; i < geometryLines.FindAll(x => x == "LINE_TYPE").Count; i++)
            {
                lines.Add(new ComponentLine
                {
                    StartPoint = new Position3D
                    {
                        X = double.Parse(geometryLines[i + 4 + lines.Count * 11]),
                        Y = double.Parse(geometryLines[i + 5 + lines.Count * 11]),
                        Z = double.Parse(geometryLines[i + 6 + lines.Count * 11])
                    },
                    EndPoint = new Position3D
                    {
                        X = double.Parse(geometryLines[i + 8 + lines.Count * 11]),
                        Y = double.Parse(geometryLines[i + 9 + lines.Count * 11]),
                        Z = double.Parse(geometryLines[i + 10 + lines.Count * 11])
                    }
                });
            }

            return lines;
        }
    }
}
