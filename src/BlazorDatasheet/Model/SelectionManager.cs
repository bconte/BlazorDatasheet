namespace BlazorDatasheet.Model;

public class SelectionManager
{
    private Sheet _sheet;
    public bool IsSelecting => ActiveSelection != null;

    /// <summary>
    /// The selection that is happening but has not been finalised, e.g
    /// intended to be when the user is dragging the mouse across the cells
    /// </summary>
    public Selection? ActiveSelection { get; private set; }

    private List<Selection> _selections;

    /// <summary>
    /// The list of current selections
    /// </summary>
    public IReadOnlyCollection<Selection> Selections => _selections;

    public SelectionManager(Sheet sheet)
    {
        _sheet = sheet;
        _selections = new List<Selection>();
    }

    /// <summary>
    /// Start selecting at a position (row, col). This selection is not finalised until EndSelecting() is called.
    /// </summary>
    /// <param name="row">The row where the selection should start</param>
    /// <param name="col">The col where the selection should start</param>
    public void BeginSelectingCell(int row, int col)
    {
        ActiveSelection = new Selection(new Range(row, col), _sheet, SelectionMode.Cell);
        emitSelectingChange();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="row"></param>
    public void BeginSelectingRow(int row)
    {
        var range = new Range(row, row, 0, _sheet.NumCols - 1);
        ActiveSelection = new Selection(range, _sheet, SelectionMode.Row);
        emitSelectingChange();
    }

    public void BeginSelectingCol(int col)
    {
        var range = new Range(0, _sheet.NumRows - 1, col, col);
        ActiveSelection = new Selection(range, _sheet, SelectionMode.Column);
        emitSelectingChange();
    }

    public void ClearSelections()
    {
        _selections.Clear();
        emitSelectionChange();
    }

    /// <summary>
    /// Updates the current selecting selection by extending it to row, col
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void UpdateSelectingEndPosition(int row, int col)
    {
        if (!IsSelecting)
            return;

        switch (ActiveSelection?.Mode)
        {
            case SelectionMode.Cell:
                ActiveSelection?.ExtendTo(row, col);
                break;
            case SelectionMode.Column:
                ActiveSelection?.ExtendTo(_sheet.NumRows, col);
                break;
            case SelectionMode.Row:
                ActiveSelection?.ExtendTo(row, _sheet.NumCols);
                break;
        }

        emitSelectingChange();
    }

    /// <summary>
    /// Ends the selecting process and adds the selection to the stack
    /// </summary>
    public void EndSelecting()
    {
        if (!IsSelecting)
            return;
        _selections.Add(ActiveSelection);
        ActiveSelection = null;
        emitSelectingChange();
    }

    /// <summary>
    /// Clears the selecting process and discards the active selecting object
    /// </summary>
    public void CancelSelecting()
    {
        ActiveSelection = null;
        emitSelectingChange();
    }

    /// <summary>
    /// Extends the most recently added selection to the row, col position and makes it the active selection
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void ExtendSelection(int row, int col)
    {
        if (!_selections.Any())
            return;

        ActiveSelection = _selections.Last();
        _selections.RemoveAt(_selections.Count - 1);

        ActiveSelection.ExtendTo(row, col);
        this.emitSelectionChange();
        this.emitSelectingChange();
    }

    /// <summary>
    /// Clears any selections or active selections and sets to the range specified
    /// </summary>
    /// <param name="range"></param>
    public void SetSelection(Range range)
    {
        this.CancelSelecting();
        var selectionRange = range.Copy();
        selectionRange.Constrain(_sheet.Range);
        this._selections.Clear();
        this._selections.Add(new Selection(range, _sheet, SelectionMode.Cell));
        emitSelectionChange();
    }

    /// <summary>
    /// Sets selection to a single cell and clears any current selections
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    public void SetSelection(int row, int col)
    {
        SetSelection(new Range(row, col));
    }

    /// <summary>
    /// Collapses the latest selection to a single cell and moves it by dRow and dCol
    /// </summary>
    /// <param name="dRow"></param>
    /// <param name="dCol"></param>
    public void MoveSelection(int dRow, int dCol)
    {
        if(IsSelecting)
            return;

        var recentSel = Selections.LastOrDefault();
        if (recentSel == null)
            return;
        
        SetSelection(recentSel.Range.RowStart+dRow, recentSel.Range.ColStart + dCol);
    }

    /// <summary>
    /// Returns all selections
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Selection> GetSelections()
    {
        return Selections;
    }

    /// <summary>
    /// Returns true if the position is inside any of the active selections
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public bool IsSelected(int row, int col)
    {
        if (IsSelecting && ActiveSelection.Range.Contains(row, col))
            return true;
        if (!Selections.Any())
            return false;
        return Selections
            .Any(x => x.Range.Contains(row, col));
    }

    /// <summary>
    /// Determines whether a column contains any cells that are selected or being selected
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    public bool IsColumnActive(int col)
    {
        if (IsSelecting && ActiveSelection.Range.ContainsCol(col))
            return true;
        return Selections.Any(x => x.Range.ContainsCol(col));
    }

    /// <summary>
    /// Determines whether a row contains any cells that are selected or being selected
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public bool IsRowActive(int row)
    {
        if (IsSelecting && ActiveSelection.Range.ContainsRow(row))
            return true;
        return Selections.Any(x => x.Range.ContainsRow(row));
    }

    /// <summary>
    /// Returns the position of the first cell that was selected in list of selections
    /// </summary>
    /// <returns></returns>
    public CellPosition? GetPositionOfFirstCell()
    {
        if (!_selections.Any())
            return null;

        var selection = _selections.First();
        return new CellPosition(selection.Range.RowStart, selection.Range.ColStart);
    }

    /// <summary>
    /// Fired when the active selection (currently being selected) changes
    /// </summary>
    public event Action<Selection?> OnSelectingChange;

    private void emitSelectingChange()
    {
        OnSelectingChange?.Invoke(this.ActiveSelection);
    }

    public void SetSheet(Sheet sheet)
    {
        _sheet = sheet;
    }

    /// <summary>
    /// Fired when the current selection changes
    /// </summary>
    public event Action<IEnumerable<Selection>> OnSelectionChange;

    private void emitSelectionChange()
    {
        OnSelectionChange?.Invoke(Selections);
    }
}