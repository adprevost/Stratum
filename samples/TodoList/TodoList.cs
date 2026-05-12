// TodoList.cs
using System.Linq;
using Stratum.Controls;
using Stratum.Core;

public class TodoItem { public string Text = ""; public bool Done; }

public partial class TodoList
{
    private readonly System.Collections.Generic.List<TodoItem> _items = new();

    protected override void OnPageStart()
    {
        addBtn.Click   += AddItem;
        clearBtn.Click += ClearCompleted;
        Refresh();
    }

    private void AddItem()
    {
        if (string.IsNullOrWhiteSpace(inputBox.Text)) return;
        _items.Add(new TodoItem { Text = inputBox.Text.Trim() });
        inputBox.Text = "";
        Refresh();
    }

    private void ClearCompleted()
    {
        _items.RemoveAll(i => i.Done);
        Refresh();
    }

    private void Refresh()
    {
        listPanel.Children.Clear();
        for (int i = 0; i < _items.Count; i++)
        {
            var it = _items[i];
            var cb = new CheckBox(it.Done ? "✓ " + it.Text : it.Text, 0, i * 32, 600, 28)
            {
                Checked = it.Done,
                Color   = it.Done ? Theme.TextMuted : Theme.TextColor
            };
            var captured = it;
            cb.CheckedChanged += v => { captured.Done = v; Refresh(); };
            listPanel.Add(cb);
        }
        int remaining = _items.Count(i => !i.Done);
        statusLabel.Text = $"{remaining} of {_items.Count} remaining";
    }
}
