using System.Collections.Generic;
using System.Linq;

namespace SufiChain.SufiPlatform.FileManager.Uploads;

/// <summary>
/// Upload rows addressed by a stable id (sufi-chain#479). The browser used to report progress and results by
/// the file's position in its own batch, while the dialog list also held earlier rows and dropped finished ones
/// after a delay. Results then landed on the wrong row: earlier rows stayed on «در حال پردازش…» and a rejected
/// zip's error showed under the previous file.
/// </summary>
public sealed class UploadRowTracker<TRow> where TRow : class
{
    private readonly List<KeyValuePair<int, TRow>> _rows = new();
    private int _nextId;

    public IReadOnlyList<TRow> Rows => _rows.Select(row => row.Value).ToList();

    public int Count => _rows.Count;

    /// <summary>Adds one browser batch and returns the id of its first row; the rest follow in order.</summary>
    public int AddBatch(IEnumerable<TRow> rows)
    {
        var firstId = _nextId;
        foreach (var row in rows)
        {
            _rows.Add(new KeyValuePair<int, TRow>(_nextId++, row));
        }

        return firstId;
    }

    public TRow? Find(int id)
    {
        foreach (var row in _rows)
        {
            if (row.Key == id)
            {
                return row.Value;
            }
        }

        return null;
    }

    public bool Remove(TRow row)
    {
        var index = _rows.FindIndex(item => ReferenceEquals(item.Value, row));
        if (index < 0)
        {
            return false;
        }

        _rows.RemoveAt(index);
        return true;
    }
}
