// See https://aka.ms/new-console-template for more information
using IfcConverter.Domain.Models.Vue.Common;
using IfcConverter.Domain.Models.Vue.Geometries;
using System.Text;
using System.Text.Json;

Console.WriteLine("Hello, World!");

string jsonData = string.Empty;
bool isRunning = true;

while (isRunning)
{
    Console.WriteLine("Select parse options: 1 - Dictionary, 2 - Lines, 3 - Boundary Lines >>");
    int option = int.Parse(Console.ReadLine());

    Console.WriteLine("Insert data >>");
    string inputData = Console.ReadLine();

    if (inputData == null) { break; }

    switch (option)
    {
        case 1:
            jsonData = JsonSerializer.Serialize(ParseDictionary(inputData), new JsonSerializerOptions { WriteIndented = true });
            break;

        case 2:
            jsonData = JsonSerializer.Serialize(ParseLines(inputData), new JsonSerializerOptions { WriteIndented = true });
            break;

        case 3:
            jsonData = JsonSerializer.Serialize(ParseBoundaryLines(inputData), new JsonSerializerOptions { WriteIndented = true });
            break;

        case 0:
            isRunning = false;
            break;
    }

    if (isRunning)
        File.WriteAllText("C:\\Users\\Windows 11\\source\\repos\\IfcConverter\\VueReader.Client\\ElProperties.txt", jsonData, Encoding.UTF8);
}


List<Line> ParseLines(string data)
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


List<ComponentLine> ParseBoundaryLines(string data)
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


Dictionary<string, string?> ParseDictionary(string data)
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