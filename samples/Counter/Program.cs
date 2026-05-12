using Stratum.Core;

Counter app = new();
app.Run();

while (true) await Task.Delay(1000);
