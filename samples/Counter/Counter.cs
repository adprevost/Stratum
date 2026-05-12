// Counter.cs
public partial class Counter
{
    private int _count = 0;

    protected override void OnPageStart()
    {
        incBtn.Click   += Increment;
        decBtn.Click   += Decrement;
        resetBtn.Click += Reset;
    }

    private void Increment() { _count++; countLabel.Text = $"Count: {_count}"; }
    private void Decrement() { _count--; countLabel.Text = $"Count: {_count}"; }
    private void Reset()     { _count = 0; countLabel.Text = "Count: 0"; }
}
