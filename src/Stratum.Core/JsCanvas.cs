// Stratum.Core/JsCanvas.cs
using System.Runtime.InteropServices.JavaScript;

namespace Stratum.Core;

public static partial class JsCanvas
{
    [JSImport("clearRect",      "Stratum.js")] public static partial void   ClearRect(int x, int y, int w, int h);
    [JSImport("fillRect",       "Stratum.js")] public static partial void   FillRect(int x, int y, int w, int h);
    [JSImport("strokeRect",     "Stratum.js")] public static partial void   StrokeRect(int x, int y, int w, int h);
    [JSImport("fillText",       "Stratum.js")] public static partial void   FillText(string text, int x, int y);
    [JSImport("strokeText",     "Stratum.js")] public static partial void   StrokeText(string text, int x, int y);
    [JSImport("measureText",    "Stratum.js")] public static partial double  MeasureText(string text);
    [JSImport("beginPath",      "Stratum.js")] public static partial void   BeginPath();
    [JSImport("closePath",      "Stratum.js")] public static partial void   ClosePath();
    [JSImport("moveTo",         "Stratum.js")] public static partial void   MoveTo(int x, int y);
    [JSImport("lineTo",         "Stratum.js")] public static partial void   LineTo(int x, int y);
    [JSImport("arc",            "Stratum.js")] public static partial void   Arc(int x, int y, int r, double start, double end, bool ccw);
    [JSImport("roundRect",      "Stratum.js")] public static partial void   RoundRect(int x, int y, int w, int h, int r);
    [JSImport("fill",           "Stratum.js")] public static partial void   Fill();
    [JSImport("stroke",         "Stratum.js")] public static partial void   Stroke();
    [JSImport("save",           "Stratum.js")] public static partial void   Save();
    [JSImport("restore",        "Stratum.js")] public static partial void   Restore();
    [JSImport("setClip",        "Stratum.js")] public static partial void   SetClip(int x, int y, int w, int h);
    [JSImport("setFillStyle",   "Stratum.js")] public static partial void   SetFillStyle(string color);
    [JSImport("setStrokeStyle", "Stratum.js")] public static partial void   SetStrokeStyle(string color);
    [JSImport("setLineWidth",   "Stratum.js")] public static partial void   SetLineWidth(double w);
    [JSImport("setFont",        "Stratum.js")] public static partial void   SetFont(string font);
    [JSImport("setTextBaseline","Stratum.js")] public static partial void   SetTextBaseline(string b);
    [JSImport("setTextAlign",   "Stratum.js")] public static partial void   SetTextAlign(string a);
    [JSImport("setGlobalAlpha", "Stratum.js")] public static partial void   SetGlobalAlpha(double a);
    [JSImport("getCanvasWidth", "Stratum.js")] public static partial int    GetCanvasWidth();
    [JSImport("getCanvasHeight","Stratum.js")] public static partial int    GetCanvasHeight();
    [JSImport("requestFrame",   "Stratum.js")] public static partial void   RequestFrame([JSMarshalAs<JSType.Function>] Action callback);
}
