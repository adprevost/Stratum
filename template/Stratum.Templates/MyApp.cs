using Stratum.Core;
using Stratum.Controls;

public class MyApp : Application
{
    private Label _label = null!;
    private int _count = 0;

    protected override void OnStart()
    {
        _label = new Label("Hello from Stratum!", 40, 40, 400, 36) { FontSize = 22, Bold = true };
        var btn = new Button("Click me", 40, 100);
        btn.Click += () =>
        {
            _count++;
            _label.Text = $"Clicked {_count} time{(_count == 1 ? "" : "s")}!";
            RequestRedraw();
        };

        Add(_label);
        Add(btn);
    }
}
