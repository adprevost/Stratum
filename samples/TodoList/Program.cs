using Stratum.Core;

TodoList app = new();
app.Run();

while (true) await Task.Delay(1000);
