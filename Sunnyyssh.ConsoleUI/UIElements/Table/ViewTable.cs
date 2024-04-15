using System.Collections.Immutable;
using Sunnyyssh.ConsoleUI.Binding;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Sunnyyssh.ConsoleUI;

public sealed class ViewTable : UIElement, IFocusable
{
     private static readonly VerticalAligning CellTextVerticalAligning = VerticalAligning.Top;

     private static readonly HorizontalAligning CellTextHorizontalAligning = HorizontalAligning.Left;

     private static readonly VerticalAligning HeaderTextVerticalAligning = VerticalAligning.Top;

     private static readonly HorizontalAligning HeaderTextHorizontalAligning = HorizontalAligning.Left;
     
     private readonly ImmutableList<ImmutableList<string?>> _initData;

     private readonly string?[][] _dataRows;

     // ReSharper disable once NotAccessedField.Local
     private ForceTakeFocusHandler? _forceTakeFocusHandler;

     // ReSharper disable once NotAccessedField.Local
     private ForceLoseFocusHandler? _forceLoseFocusHandler;

     private IObservable<IDataTable<string?>, DataTableUpdateArgs<string?>>? _observing;
     
     private IBindable<IDataTable<string?>, DataTableUpdateArgs<string?>>? _bound;

     public int CurrentTopAreaRow { get; private set; } = 0;

     public int CurrentViewRow { get; private set; } = 0;

     public int CurrentColumn { get; private set; } = 0;

     public IReadOnlyList<string> Headers { get; }
     
     public LineCharSet BorderCharSet { get; }

     public AbsoluteGridDefinition AbsoluteGridDefinition { get; }

     public int ViewRowsCount { get; }

     public bool Scrollable { get; }

     public int HeadersCount => Headers.Count;

     public int DataRowsCount => _dataRows.Length;

     #region init properties.
     
     public bool EnterOnCellChange { get; init; }
     
     public Color CellNotFocusedBackground { get; init; } = Color.Default;

     public Color HeaderBackground { get; init; } = Color.Default;

     public Color CellNotFocusedForeground { get; init; } = Color.Default;

     public Color HeaderForeground { get; init; } = Color.Default;

     public Color CellFocusedBackground { get; init; } = Color.Default;

     public Color CellFocusedForeground { get; init; } = Color.Default;

     public Color BorderColorNotFocused { get; init; } = Color.Default;

     public Color BorderColorFocused { get; init; } = Color.Default;
     
     public ImmutableList<ConsoleKey>? MoveRightKeys { get; init; }

     public ImmutableList<ConsoleKey>? MoveLeftKeys { get; init; }

     public ImmutableList<ConsoleKey>? MoveUpKeys { get; init; }

     public ImmutableList<ConsoleKey>? MoveDownKeys { get; init; }

     public bool CellsWordWrap { get; init; } = true;

     public bool UserEditable { get; init; } = true;
     
     public bool LoseFocusOnEnter { get; init; } = false; 
     
     #endregion
     
     public bool IsFocused { get; private set; }

     public bool IsWaitingFocus { get; set; } = true;

     bool IFocusable.IsWaitingFocus => IsWaitingFocus && (UserEditable || Scrollable);

     public string? this[int row, int column]
     {
          get => _dataRows[row][column];
          set => SetText(row, column, value);
     }

     private void SetText(int row, int column, string? value)
     {
          value = CharHelper.RemoveSpecialChars(value, false);
          
          _dataRows[row][column] = value;

          int possibleViewRow = row - CurrentTopAreaRow;
          if (IsStateInitialized && possibleViewRow >= 0 && possibleViewRow < ViewRowsCount)
          {
               RedrawCell(possibleViewRow, column);
          }
     }

     #region Binding.

     public void Observe(IObservable<IDataTable<string?>, DataTableUpdateArgs<string?>> tableObservable)
     {
          ArgumentNullException.ThrowIfNull(tableObservable, nameof(tableObservable));
        
          if (UserEditable)
          {
               throw new InvalidOperationException($"Can't observe {nameof(tableObservable)} when {nameof(UserEditable)}=true." +
                                                   $"Try {nameof(Bind)} or set {nameof(UserEditable)}=false.");
          }
        
          if (_observing is not null)
          {
               _observing.Updated -= HandleObservableUpdate;
          }

          _observing = tableObservable;
          _observing.Updated += HandleObservableUpdate;
          
          _observing.Value
               .ToRowArray()
               .CopyTo(_dataRows, 0);
          
          if (IsStateInitialized)
          {
               Redraw(CreateDrawState());
          }
     }

     public void Unobserve()
     {
          if (_observing is null)
          {
               throw new InvalidOperationException("Nothing was observed.");
          }

          _observing.Updated -= HandleObservableUpdate;
          _observing = null;

          _initData
               .Select(Enumerable.ToArray)
               .ToArray()
               .CopyTo(_dataRows, 0);

          if (IsStateInitialized)
          {
               Redraw(CreateDrawState());
          }
     }

     public void Bind(IBindable<IDataTable<string?>, DataTableUpdateArgs<string?>> tableBindable)
     {
          ArgumentNullException.ThrowIfNull(tableBindable, nameof(tableBindable));
        
          if (_observing is not null)
          {
               _observing.Updated -= HandleObservableUpdate;
          }

          _observing = tableBindable;
          _observing.Updated += HandleObservableUpdate;
          
          _observing.Value
               .ToRowArray()
               .CopyTo(_dataRows, 0);
          
          if (IsStateInitialized)
          {
               Redraw(CreateDrawState());
          }

          _bound = tableBindable;
     }

     public void Unbind()
     {
          if (_observing is null)
          {
               throw new InvalidOperationException("Nothing was observed.");
          }

          _observing.Updated -= HandleObservableUpdate;
          _observing = null;

          _initData
               .Select(Enumerable.ToArray)
               .ToArray()
               .CopyTo(_dataRows, 0);

          if (IsStateInitialized)
          {
               Redraw(CreateDrawState());
          }
        
          _bound = null;
     }

     private void HandleObservableUpdate(IObservable<IDataTable<string?>, DataTableUpdateArgs<string?>> sender, DataTableUpdateArgs<string?> args)
     {
          SetText(args.Row, args.Column, args.NewValue);
     }

     private void OnTextEntered(int row, int column, string? text)
     {
          if (_bound is null)
               return;

          if (_bound.Value[row, column] == text)
               return;
          
          var args = new DataTableUpdateArgs<string?>(row, column, text);
          _bound.HandleUpdate(args);
     }

     #endregion

     #region Drawing stuff.

     private void RedrawCell(int viewRow, int column)
     {
          int cellLeft = 1 + AbsoluteGridDefinition.Columns.Take(column).Sum(gridColumn => gridColumn.Width + 1);
          int cellTop = 1 + AbsoluteGridDefinition.Rows.Take(viewRow + 1).Sum(gridRow => gridRow.Height + 1);
          int cellWidth = AbsoluteGridDefinition.Columns[column].Width;
          int cellHeight = AbsoluteGridDefinition.Rows[viewRow + 1].Height;

          var stateBuilder = new DrawStateBuilder(cellWidth, cellHeight);

          bool isCellFocused = IsFocused && viewRow == CurrentViewRow && column == CurrentColumn;
          
          var cellBackground = isCellFocused ? CellFocusedBackground : CellNotFocusedBackground;
          var cellForeground = isCellFocused ? CellFocusedForeground : CellNotFocusedForeground;

          stateBuilder.Fill(cellBackground);

          var text = this[CurrentTopAreaRow + viewRow, column];
          if (text is not null)
          {
               TextHelper.PlaceText(0, 0, cellWidth, cellHeight,
                    CellsWordWrap, text, cellBackground, cellForeground,
                    CellTextVerticalAligning, CellTextHorizontalAligning,
                    stateBuilder);
          }

          var builtState = stateBuilder.ToDrawState().Shift(cellLeft, cellTop);
          
          Redraw(builtState);
     }
     
     protected override DrawState CreateDrawState()
     {
          var stateBuilder = new DrawStateBuilder(Width, Height);

          int headersAreaHeight = AbsoluteGridDefinition.Rows[0].Height + 2; // + 2 for borders.

          // Filling headers background.
          stateBuilder.Fill(0, 0, Width, headersAreaHeight, new PixelInfo(HeaderBackground));
          
          // Filling cells background.
          stateBuilder.Fill(0, headersAreaHeight, Width, Height - headersAreaHeight,
               new PixelInfo(CellNotFocusedBackground));

          var borderColor = IsFocused ? BorderColorFocused : BorderColorNotFocused;
          var border = GridBuilder.CreateBorder(Width, Height, AbsoluteGridDefinition, BorderCharSet, borderColor);

          // Creating border.
          var borderState = border.RequestDrawState(new DrawOptions());
          stateBuilder.OverlapWith(borderState);

          PlaceHeaders(stateBuilder);
          PlaceCells(stateBuilder);

          return stateBuilder.ToDrawState();
     }

     private void PlaceHeaders(DrawStateBuilder stateBuilder)
     {
          int top = 1; // init 1 for border.
          int accumulatedLeft = 1; // init 1 for border.
          int rowHeight = AbsoluteGridDefinition.Rows[0].Height;

          for (int column = 0; column < HeadersCount; column++)
          {
               int columnWidth = AbsoluteGridDefinition.Columns[column].Width;

               TextHelper.PlaceText(accumulatedLeft, top, Width, rowHeight, 
                    // No need in word-wrapping headers.
                    false, Headers[column], HeaderBackground, HeaderForeground,
                    HeaderTextVerticalAligning, HeaderTextHorizontalAligning, stateBuilder);

               accumulatedLeft += columnWidth + 1;
          }
     }

     private void PlaceCells(DrawStateBuilder stateBuilder)
     {
          int accumulatedTop = 2 + AbsoluteGridDefinition.Rows[0].Height; // init 2 because of 2 borders and headers row.
          
          for (int viewRow = 0; viewRow < ViewRowsCount; viewRow++)
          {
               int accumulatedLeft = 1; // init 1 because of border.
               int rowHeight = AbsoluteGridDefinition.Rows[viewRow + 1].Height;
               var thisRow = _dataRows[CurrentTopAreaRow + viewRow];
               
               for (int column = 0; column < HeadersCount; column++)
               {
                    int columnWidth = AbsoluteGridDefinition.Columns[column].Width;

                    var cellBackground = CellNotFocusedBackground;
                    var cellForeground = CellNotFocusedForeground;
                    
                    if (viewRow == CurrentViewRow && column == CurrentColumn && IsFocused)
                    {
                         cellBackground = CellFocusedBackground;
                         cellForeground = CellFocusedForeground;
                         // Filling focused cell.
                         stateBuilder.Fill(accumulatedLeft, accumulatedTop, columnWidth, rowHeight,
                              new PixelInfo(cellBackground));
                    }

                    var text = thisRow[column];
                    if (text is not null)
                    {
                         TextHelper.PlaceText(accumulatedLeft, accumulatedTop, columnWidth, rowHeight,
                              CellsWordWrap, text, cellBackground, cellForeground,
                              CellTextVerticalAligning, CellTextHorizontalAligning,
                              stateBuilder);
                    }

                    accumulatedLeft += columnWidth + 1; // + 1 for border.
               }

               accumulatedTop += rowHeight + 1; // + 1 for border.
          }
     }

     #endregion

     #region Handling keys

     bool IFocusable.HandlePressedKey(ConsoleKeyInfo keyInfo)
     {
          var key = keyInfo.Key;

          if (MoveUpKeys?.Contains(key) ?? false)
          {
               MoveUp();
               return true;
          }

          if (MoveDownKeys?.Contains(key) ?? false)
          {
               MoveDown();
               return true;
          }

          if (MoveLeftKeys?.Contains(key) ?? false)
          {
               MoveLeft();
               return true;
          }

          if (MoveRightKeys?.Contains(key) ?? false)
          {
               MoveRight();
               return true;
          }

          if (UserEditable)
          {
               return HandleCellEdit(keyInfo);
          }

          return true;
     }

     private bool HandleCellEdit(ConsoleKeyInfo keyInfo)
     {
          if (IsSpecialKey(keyInfo.Key))
          {
               HandleSpecialKey(keyInfo.Key, out bool loseFocus);
               return !loseFocus;
          }
        
          char newChar = keyInfo.KeyChar;
        
          if (newChar == '\0')
               return true;
        
          this[CurrentTopAreaRow + CurrentViewRow, CurrentColumn] += newChar;
        
          return true;
     }

     private static readonly ConsoleKey[] SpecialKeys = new[] { ConsoleKey.Backspace, ConsoleKey.Enter };
    
     private bool IsSpecialKey(ConsoleKey key)
     {
          return SpecialKeys.Contains(key);
     }

     private void HandleSpecialKey(ConsoleKey key, out bool loseFocus)
     {
          int currentDataRow = CurrentTopAreaRow + CurrentViewRow;
          
          if (key == ConsoleKey.Enter)
          {
               OnTextEntered(currentDataRow, CurrentColumn, this[currentDataRow, CurrentColumn]);
                    
               loseFocus = LoseFocusOnEnter;
               return;
          }
          
          if (key == ConsoleKey.Backspace)
          {
               if (this[currentDataRow, CurrentColumn]?.Length > 0)
               {   
                    this[currentDataRow, CurrentColumn] = this[currentDataRow, CurrentColumn]![..^1];
               }
                
               loseFocus = false;
               return;
          }

          throw new ArgumentException("Key wasn't special.", nameof(key));
     }

     private void MoveUp()
     {
          if (CurrentViewRow == 0)
          {
               if (CurrentTopAreaRow == 0)
                    return;

               int prevViewRow = CurrentTopAreaRow--;
               string? enteredText = this[prevViewRow + CurrentViewRow, CurrentColumn];
               OnTextEntered(prevViewRow + CurrentViewRow, CurrentColumn, enteredText);
               
               if (IsStateInitialized)
               {
                    Redraw(CreateDrawState());
               }
               return;
          }

          int prevRow = CurrentViewRow;
          CurrentViewRow--;
          
          RedrawCell(prevRow, CurrentColumn);
          
          RedrawCell(CurrentViewRow, CurrentColumn);

          string? text = this[prevRow + CurrentTopAreaRow, CurrentColumn];
          OnTextEntered(prevRow + CurrentTopAreaRow, CurrentColumn, text);
     }

     private void MoveDown()
     {
          if (CurrentViewRow == ViewRowsCount - 1)
          {
               if (CurrentTopAreaRow == _dataRows.Length - ViewRowsCount)
                    return;

               int prevViewRow = CurrentTopAreaRow++;
               string? enteredText = this[prevViewRow + CurrentViewRow, CurrentColumn];
               OnTextEntered(prevViewRow + CurrentViewRow, CurrentColumn, enteredText);
               
               if (IsStateInitialized)
               {
                    Redraw(CreateDrawState());
               }
               return;
          }

          int prevRow = CurrentViewRow;
          CurrentViewRow++;
          
          RedrawCell(prevRow, CurrentColumn);
          
          RedrawCell(CurrentViewRow, CurrentColumn);

          string? text = this[prevRow + CurrentTopAreaRow, CurrentColumn];
          OnTextEntered(prevRow + CurrentTopAreaRow, CurrentColumn, text);
     }

     private void MoveRight()
     {
          if (CurrentColumn == HeadersCount - 1)
               return;
          
          int prevColumn = CurrentColumn;
          CurrentColumn++;
          
          RedrawCell(CurrentViewRow, prevColumn);
          
          RedrawCell(CurrentViewRow, CurrentColumn);

          string? text = this[CurrentTopAreaRow + CurrentViewRow, prevColumn];
          OnTextEntered(CurrentTopAreaRow + CurrentViewRow, prevColumn, text);
     }

     private void MoveLeft()
     {
          if (CurrentColumn == 0)
               return;
          
          int prevColumn = CurrentColumn;
          CurrentColumn--;
          
          RedrawCell(CurrentViewRow, prevColumn);
          
          RedrawCell(CurrentViewRow, CurrentColumn);

          string? text = this[CurrentTopAreaRow + CurrentViewRow, prevColumn];
          OnTextEntered(CurrentTopAreaRow + CurrentViewRow, prevColumn, text);
     }
     
     #endregion

     #region IFocusable stuff.
     
     void IFocusable.TakeFocus()
     {
          IsFocused = true;
          
          if (IsStateInitialized)
          {
               if (BorderColorFocused != BorderColorNotFocused)
               {
                    Redraw(CreateDrawState());
                    return;
               }
               
               RedrawCell(CurrentViewRow, CurrentColumn);
          }
     }

     void IFocusable.LoseFocus()
     {
          IsFocused = false;
          
          if (IsStateInitialized)
          {
               if (BorderColorFocused != BorderColorNotFocused)
               {
                    Redraw(CreateDrawState());
                    return;
               }

               RedrawCell(CurrentViewRow, CurrentColumn);
          }
     }

     event ForceTakeFocusHandler IFocusable.ForceTakeFocus
     {
          add => _forceTakeFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
          remove => _forceTakeFocusHandler -= value ?? throw new ArgumentNullException(nameof(value));
     }

     event ForceLoseFocusHandler IFocusable.ForceLoseFocus
     {
          add => _forceLoseFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
          remove => _forceLoseFocusHandler -= value ?? throw new ArgumentNullException(nameof(value));
     }
     
     #endregion
     
     internal ViewTable(int width, int height, ImmutableList<string> headers, LineCharSet borderCharSet,
          ImmutableList<ImmutableList<string?>> initData, AbsoluteGridDefinition absoluteGridDefinition,
          OverlappingPriority overlappingPriority)
          : base(width, height, overlappingPriority)
     {
          ArgumentNullException.ThrowIfNull(headers, nameof(headers));
          ArgumentNullException.ThrowIfNull(borderCharSet, nameof(borderCharSet));
          ArgumentNullException.ThrowIfNull(absoluteGridDefinition, nameof(absoluteGridDefinition));
          ArgumentNullException.ThrowIfNull(initData, nameof(initData));

          Headers = headers;
          BorderCharSet = borderCharSet;
          AbsoluteGridDefinition = absoluteGridDefinition;
          if (Headers.Count != absoluteGridDefinition.ColumnCount)
               throw new ArgumentException("Headers count doesn't match grid rows count.", nameof(absoluteGridDefinition));

          ViewRowsCount = absoluteGridDefinition.RowCount - 1;
          if (ViewRowsCount < 1)
               throw new ArgumentException("It must have at least two rows.", nameof(absoluteGridDefinition));
          if (initData.Count < ViewRowsCount)
               throw new ArgumentException("Data rows count must be not less than view rows count.", nameof(initData));

          if (initData.Any(row => row.Count != Headers.Count))
               throw new ArgumentException("Data size doesn't match headers count.", nameof(initData));
          _initData = initData;
          _dataRows = initData.Select(Enumerable.ToArray).ToArray();

          Scrollable = _dataRows.Length > ViewRowsCount;
     }
}