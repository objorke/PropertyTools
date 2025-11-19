// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataGrid.cs" company="PropertyTools">
//   Copyright (c) 2014 PropertyTools contributors
// </copyright>
// <summary>
//   Displays enumerable data in a customizable grid.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PropertyTools.Wpf
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Threading;

    using PropertyTools.DataAnnotations;

    /// <summary>
    /// Displays enumerable data in a customizable grid.
    /// </summary>
    [ContentProperty("ItemsSource")]
    [DefaultProperty("ItemsSource")]
    [TemplatePart(Name = PartGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PartSheetScrollViewer, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = PartSheetGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PartColumnScrollViewer, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = PartColumnGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PartRowScrollViewer, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = PartRowGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PartSelectionBackground, Type = typeof(Border))]
    [TemplatePart(Name = PartRowSelectionBackground, Type = typeof(Border))]
    [TemplatePart(Name = PartColumnSelectionBackground, Type = typeof(Border))]
    [TemplatePart(Name = PartCurrentBackground, Type = typeof(Border))]
    [TemplatePart(Name = PartSelection, Type = typeof(Border))]
    [TemplatePart(Name = PartAutoFillSelection, Type = typeof(Border))]
    [TemplatePart(Name = PartAutoFillBox, Type = typeof(Border))]
    [TemplatePart(Name = PartTopLeft, Type = typeof(Border))]
    public class DataGrid : Control, IWeakEventListener
    {
        /// <summary>
        /// Identifies the <see cref="CustomSort"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CustomSortProperty = DependencyProperty.Register(
            nameof(CustomSort),
            typeof(IComparer),
            typeof(DataGrid),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="CreateColumnHeader"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CreateColumnHeaderProperty = DependencyProperty.Register(
            nameof(CreateColumnHeader),
            typeof(Func<int, object>),
            typeof(DataGrid),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="CreateItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CreateItemProperty = DependencyProperty.Register(
            nameof(CreateItem),
            typeof(Func<object>),
            typeof(DataGrid),
            new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="AddItemHeader"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AddItemHeaderProperty = DependencyProperty.Register(
            nameof(AddItemHeader),
            typeof(string),
            typeof(DataGrid),
            new UIPropertyMetadata("+"));

        /// <summary>
        /// Identifies the <see cref="AlternatingRowsBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AlternatingRowsBackgroundProperty = DependencyProperty.Register(
                nameof(AlternatingRowsBackground),
                typeof(Brush),
                typeof(DataGrid),
                new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="AutoGenerateColumns"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoGenerateColumnsProperty = DependencyProperty.Register(
                nameof(AutoGenerateColumns),
                typeof(bool),
                typeof(DataGrid),
                new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="AutoInsert"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoInsertProperty = DependencyProperty.Register(
            nameof(AutoInsert),
            typeof(bool),
            typeof(DataGrid),
            new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="CanClear"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CanClearProperty = DependencyProperty.Register(
            nameof(CanClear),
            typeof(bool),
            typeof(DataGrid),
            new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="CanDelete"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CanDeleteProperty = DependencyProperty.Register(
            nameof(CanDelete),
            typeof(bool),
            typeof(DataGrid),
            new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="CanInsert"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CanInsertProperty = DependencyProperty.Register(
            nameof(CanInsert),
            typeof(bool),
            typeof(DataGrid),
            new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="CanResizeColumns"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CanResizeColumnsProperty = DependencyProperty.Register(
                nameof(CanResizeColumns),
                typeof(bool),
                typeof(DataGrid),
                new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="CanResizeRows"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CanResizeRowsProperty = DependencyProperty.Register(
                nameof(CanResizeRows),
                typeof(bool),
                typeof(DataGrid),
                new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="MultiChangeInChangedColumnOnly"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MultiChangeInChangedColumnOnlyProperty = DependencyProperty.Register(
                nameof(MultiChangeInChangedColumnOnly),
                typeof(bool),
                typeof(DataGrid),
                new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="ColumnHeaderHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderHeightProperty = DependencyProperty.Register(
                nameof(ColumnHeaderHeight),
                typeof(GridLength),
                typeof(DataGrid),
                new UIPropertyMetadata(new GridLength(20)));

        /// <summary>
        /// Identifies the <see cref="SheetContextMenu"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SheetContextMenuProperty = DependencyProperty.Register(
                nameof(SheetContextMenu),
                typeof(ContextMenu),
                typeof(DataGrid),
                new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ColumnsContextMenu"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnsContextMenuProperty = DependencyProperty.Register(
                nameof(ColumnsContextMenu),
                typeof(ContextMenu),
                typeof(DataGrid),
                new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ControlFactory"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ControlFactoryProperty = DependencyProperty.Register(
            nameof(ControlFactory),
            typeof(IDataGridControlFactory),
            typeof(DataGrid),
            new UIPropertyMetadata(new DataGridControlFactory()));

        /// <summary>
        /// Identifies the <see cref="CellDefinitionFactory"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CellDefinitionFactoryProperty = DependencyProperty.Register(
            nameof(CellDefinitionFactory),
            typeof(ICellDefinitionFactory),
            typeof(DataGrid),
            new UIPropertyMetadata(new CellDefinitionFactory()));

        /// <summary>
        /// Identifies the <see cref="CurrentCell"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentCellProperty = DependencyProperty.Register(
            nameof(CurrentCell),
            typeof(CellRef),
            typeof(DataGrid),
            new FrameworkPropertyMetadata(
                new CellRef(0, 0),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, e) => ((DataGrid)d).CurrentCellChanged(),
                CoerceCurrentCell));

        /// <summary>
        /// Identifies the <see cref="DefaultColumnWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultColumnWidthProperty = DependencyProperty.Register(
                nameof(DefaultColumnWidth),
                typeof(GridLength),
                typeof(DataGrid),
                new UIPropertyMetadata(new GridLength(1, GridUnitType.Star)));

        /// <summary>
        /// Identifies the <see cref="DefaultHorizontalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultHorizontalAlignmentProperty = DependencyProperty.Register(
                nameof(DefaultHorizontalAlignment),
                typeof(System.Windows.HorizontalAlignment),
                typeof(DataGrid),
                new UIPropertyMetadata(System.Windows.HorizontalAlignment.Center));

        /// <summary>
        /// Identifies the <see cref="DefaultRowHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultRowHeightProperty = DependencyProperty.Register(
                nameof(DefaultRowHeight),
                typeof(GridLength),
                typeof(DataGrid),
                new UIPropertyMetadata(new GridLength(20)));

        /// <summary>
        /// Identifies the <see cref="IsEasyInsertByKeyboardEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEasyInsertByKeyboardEnabledProperty = DependencyProperty.Register(
            nameof(IsEasyInsertByKeyboardEnabled),
            typeof(bool),
            typeof(DataGrid),
            new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsEasyInsertByMouseEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEasyInsertByMouseEnabledProperty = DependencyProperty.Register(
            nameof(IsEasyInsertByMouseEnabled),
            typeof(bool),
            typeof(DataGrid),
            new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="GridLineBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GridLineBrushProperty = DependencyProperty.Register(
            nameof(GridLineBrush),
            typeof(Brush),
            typeof(DataGrid),
            new UIPropertyMetadata(new SolidColorBrush(Color.FromRgb(218, 220, 221))));

        /// <summary>
        /// Identifies the <see cref="HeaderBorderBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderBorderBrushProperty = DependencyProperty.Register(
                nameof(HeaderBorderBrush),
                typeof(Brush),
                typeof(DataGrid),
                new UIPropertyMetadata(new SolidColorBrush(Color.FromRgb(177, 181, 186))));

        /// <summary>
        /// Identifies the <see cref="InputDirection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InputDirectionProperty = DependencyProperty.Register(
            nameof(InputDirection),
            typeof(InputDirection),
            typeof(DataGrid),
            new UIPropertyMetadata(InputDirection.Vertical));

        /// <summary>
        /// Identifies the <see cref="IsAutoFillEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsAutoFillEnabledProperty = DependencyProperty.Register(
            nameof(IsAutoFillEnabled),
            typeof(bool),
            typeof(DataGrid),
            new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsMoveAfterEnterEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsMoveAfterEnterEnabledProperty = DependencyProperty.Register(
            nameof(IsMoveAfterEnterEnabled),
            typeof(bool),
            typeof(DataGrid),
            new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="ItemHeaderPropertyPath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemHeaderPropertyPathProperty = DependencyProperty.Register(
                nameof(ItemHeaderPropertyPath),
                typeof(string),
                typeof(DataGrid),
                new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList),
            typeof(DataGrid),
            new UIPropertyMetadata(null, (d, e) => ((DataGrid)d).ItemsSourceChanged()));

        /// <summary>
        /// Identifies the <see cref="RowHeadersSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RowHeadersSourceProperty = DependencyProperty.Register(
            nameof(RowHeadersSource),
            typeof(IList),
            typeof(DataGrid),
            new UIPropertyMetadata(null, (d, e) => ((DataGrid)d).UpdateGridContent()));

        /// <summary>
        /// Identifies the <see cref="ColumnHeadersSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnHeadersSourceProperty = DependencyProperty.Register(
            nameof(ColumnHeadersSource),
            typeof(IList),
            typeof(DataGrid),
            new UIPropertyMetadata(null, (d, e) => ((DataGrid)d).UpdateGridContent()));

        /// <summary>
        /// Identifies the <see cref="RowHeadersFormatString"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RowHeadersFormatStringProperty = DependencyProperty.Register(
            nameof(RowHeadersFormatString),
            typeof(string),
            typeof(DataGrid),
            new UIPropertyMetadata(null, (d, e) => ((DataGrid)d).UpdateGridContent()));

        /// <summary>
        /// Identifies the <see cref="ColumnHeadersFormatString"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnHeadersFormatStringProperty = DependencyProperty.Register(
            nameof(ColumnHeadersFormatString),
            typeof(string),
            typeof(DataGrid),
            new UIPropertyMetadata(null, (d, e) => ((DataGrid)d).UpdateGridContent()));

        /// <summary>
        /// Identifies the <see cref="RowHeaderWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RowHeaderWidthProperty = DependencyProperty.Register(
            nameof(RowHeaderWidth),
            typeof(GridLength),
            typeof(DataGrid),
            new UIPropertyMetadata(new GridLength(40)));

        /// <summary>
        /// Identifies the <see cref="RowsContextMenu"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RowsContextMenuProperty = DependencyProperty.Register(
                nameof(RowsContextMenu),
                typeof(ContextMenu),
                typeof(DataGrid),
                new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SelectedItems"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IEnumerable),
            typeof(DataGrid),
            new UIPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SelectionCell"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionCellProperty = DependencyProperty.Register(
            nameof(SelectionCell),
            typeof(CellRef),
            typeof(DataGrid),
            new UIPropertyMetadata(new CellRef(0, 0), (d, e) => ((DataGrid)d).SelectionCellChanged(), CoerceSelectionCell));

        /// <summary>
        /// Identifies the <see cref="WrapItems"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapItemsProperty = DependencyProperty.Register(
            nameof(WrapItems),
            typeof(bool),
            typeof(DataGrid),
            new UIPropertyMetadata(false));

        /// <summary>
        /// The auto fill box.
        /// </summary>
        private const string PartAutoFillBox = "PART_AutoFillBox";

        /// <summary>
        /// The auto fill selection.
        /// </summary>
        private const string PartAutoFillSelection = "PART_AutoFillSelection";

        /// <summary>
        /// The column grid.
        /// </summary>
        private const string PartColumnGrid = "PART_ColumnGrid";

        /// <summary>
        /// The column scroll viewer.
        /// </summary>
        private const string PartColumnScrollViewer = "PART_ColumnScrollViewer";

        /// <summary>
        /// The column selection background.
        /// </summary>
        private const string PartColumnSelectionBackground = "PART_ColumnSelectionBackground";

        /// <summary>
        /// The current background.
        /// </summary>
        private const string PartCurrentBackground = "PART_CurrentBackground";

        /// <summary>
        /// The grid.
        /// </summary>
        private const string PartGrid = "PART_Grid";

        /// <summary>
        /// The row grid.
        /// </summary>
        private const string PartRowGrid = "PART_RowGrid";

        /// <summary>
        /// The row scroll viewer.
        /// </summary>
        private const string PartRowScrollViewer = "PART_RowScrollViewer";

        /// <summary>
        /// The row selection background.
        /// </summary>
        private const string PartRowSelectionBackground = "PART_RowSelectionBackground";

        /// <summary>
        /// The selection.
        /// </summary>
        private const string PartSelection = "PART_Selection";

        /// <summary>
        /// The selection background.
        /// </summary>
        private const string PartSelectionBackground = "PART_SelectionBackground";

        /// <summary>
        /// The sheet grid.
        /// </summary>
        private const string PartSheetGrid = "PART_SheetGrid";

        /// <summary>
        /// The sheet scroll viewer.
        /// </summary>
        private const string PartSheetScrollViewer = "PART_SheetScrollViewer";

        /// <summary>
        /// The top left cell.
        /// </summary>
        private const string PartTopLeft = "PART_TopLeft";

        /// <summary>
        /// The horizontal scroll bar visibility to height converter
        /// </summary>
        private static readonly VisibilityConverter HorizontalScrollBarVisibilityConverter = new VisibilityConverter { CollapsedValue = 0d, HiddenValue = 0d, VisibleValue = SystemParameters.HorizontalScrollBarHeight };

        /// <summary>
        /// The vertical scroll bar visibility to height converter
        /// </summary>
        private static readonly VisibilityConverter VerticalScrollBarVisibilityConverter = new VisibilityConverter { CollapsedValue = 0d, HiddenValue = 0d, VisibleValue = SystemParameters.VerticalScrollBarWidth };

        /// <summary>
        /// The cell map.
        /// </summary>
        private readonly Dictionary<int, FrameworkElement> cellMap = new Dictionary<int, FrameworkElement>();

        /// <summary>
        /// The column header map.
        /// </summary>
        private readonly Dictionary<int, FrameworkElement> columnHeaderMap = new Dictionary<int, FrameworkElement>();

        /// <summary>
        /// The row header map.
        /// </summary>
        private readonly Dictionary<int, FrameworkElement> rowHeaderMap = new Dictionary<int, FrameworkElement>();

        /// <summary>
        /// The sort descriptors
        /// </summary>
        private readonly SortDescriptionCollection sortDescriptions = new SortDescriptionCollection();

        /// <summary>
        /// The sort description markers
        /// </summary>
        private readonly List<FrameworkElement> sortDescriptionMarkers = new List<FrameworkElement>();

        /// <summary>
        /// The auto fill box.
        /// </summary>
        private Border autoFillBox;

        /// <summary>
        /// The auto fill cell.
        /// </summary>
        private CellRef autoFillCell;

        /// <summary>
        /// The auto fill selection.
        /// </summary>
        private Border autoFillSelection;

        /// <summary>
        /// The auto fill tool tip.
        /// </summary>
        private ToolTip autoFillToolTip;

        /// <summary>
        /// The auto filler.
        /// </summary>
        private AutoFiller autoFiller;

        /// <summary>
        /// The index in the sheet grid where new cells can be inserted.
        /// </summary>
        /// <remarks>The selection and auto fill controls should always be at the end of the sheetGrid children collection.</remarks>
        private int cellInsertionIndex;

        /// <summary>
        /// The column grid control.
        /// </summary>
        private Grid columnGrid;

        /// <summary>
        /// The row grid control.
        /// </summary>
        private Grid rowGrid;

        /// <summary>
        /// The column scroll view control.
        /// </summary>
        private ScrollViewer columnScrollViewer;

        /// <summary>
        /// The column selection background control.
        /// </summary>
        private Border columnSelectionBackground;

        /// <summary>
        /// The current background control.
        /// </summary>
        private Border currentBackground;

        /// <summary>
        /// The current editor.
        /// </summary>
        private FrameworkElement currentEditControl;

        /// <summary>
        /// The editing cells.
        /// </summary>
        private IList<CellRef> editingCells;

        /// <summary>
        /// The end pressed.
        /// </summary>
        private bool endPressed;

        /// <summary>
        /// The sheet grid control.
        /// </summary>
        private Grid sheetGrid;

        /// <summary>
        /// The row scroll viewer control.
        /// </summary>
        private ScrollViewer rowScrollViewer;

        /// <summary>
        /// The row selection background control.
        /// </summary>
        private Border rowSelectionBackground;

        /// <summary>
        /// The selection control.
        /// </summary>
        private Border selection;

        /// <summary>
        /// The selection background control.
        /// </summary>
        private Border selectionBackground;

        /// <summary>
        /// The sheet scroll viewer control.
        /// </summary>
        private ScrollViewer sheetScrollViewer;

        /// <summary>
        /// Reference to the collection that has subscribed to the INotifyCollectionChanged event.
        /// </summary>
        private INotifyCollectionChanged subscribedCollection;

        /// <summary>
        /// The top/left control.
        /// </summary>
        private Border topLeft;

        /// <summary>
        /// Flag used for collection changed notification suspension.
        /// </summary>
        private bool suspendCollectionChangedNotifications;

        /// <summary>
        /// The synchronized collection
        /// </summary>
        private IList synchronizedCollection;

        /// <summary>
        /// Initializes static members of the <see cref="DataGrid" /> class.
        /// </summary>
        static DataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DataGrid),
                new FrameworkPropertyMetadata(typeof(DataGrid)));
            IsEnabledProperty.OverrideMetadata(
                typeof(DataGrid),
                new FrameworkPropertyMetadata((s, e) => ((DataGrid)s).HandleIsEnabledChanged(e)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGrid" /> class.
        /// </summary>
        public DataGrid()
        {
            this.CommandBindings.Add(
                new CommandBinding(
                    DataGridCommands.InsertRows,
                    (s, e) => this.InsertRows(),
                    (s, e) => e.CanExecute = this.CanInsertRows));
            this.CommandBindings.Add(
                new CommandBinding(
                   DataGridCommands.DeleteRows,
                    (s, e) => this.DeleteRows(),
                    (s, e) => e.CanExecute = this.CanDeleteRows));
            this.CommandBindings.Add(
                new CommandBinding(
                   DataGridCommands.InsertColumns,
                    (s, e) => this.InsertColumns(),
                    (s, e) => e.CanExecute = this.CanInsertColumns));
            this.CommandBindings.Add(
                new CommandBinding(
                   DataGridCommands.DeleteColumns,
                    (s, e) => this.DeleteColumns(),
                    (s, e) => e.CanExecute = this.CanDeleteColumns));
            this.CommandBindings.Add(
                new CommandBinding(
                   DataGridCommands.SortAscending,
                    (s, e) => this.Sort(ListSortDirection.Ascending),
                    (s, e) => e.CanExecute = this.CanSort()));
            this.CommandBindings.Add(
                new CommandBinding(
                   DataGridCommands.SortDescending,
                    (s, e) => this.Sort(ListSortDirection.Descending),
                    (s, e) => e.CanExecute = this.CanSort()));
            this.CommandBindings.Add(
                new CommandBinding(
                   DataGridCommands.ClearSort,
                    (s, e) => this.ClearSort(),
                    (s, e) => e.CanExecute = this.CanSort()));
        }

        /// <summary>
        /// Gets or sets the header used for the add item row/column. Default is "*".
        /// </summary>
        /// <value>The add item header.</value>
        public string AddItemHeader
        {
            get => (string)this.GetValue(AddItemHeaderProperty);
            set => this.SetValue(AddItemHeaderProperty, value);
        }

        /// <summary>
        /// Gets or sets the alternating rows background brush.
        /// </summary>
        /// <value>The alternating rows background.</value>
        public Brush AlternatingRowsBackground
        {
            get => (Brush)this.GetValue(AlternatingRowsBackgroundProperty);
            set => this.SetValue(AlternatingRowsBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the auto fill cell.
        /// </summary>
        /// <value>The auto fill cell.</value>
        public CellRef AutoFillCell
        {
            get => this.autoFillCell;
            set
            {
                this.autoFillCell = (CellRef)CoerceSelectionCell(this, value);
                this.SelectedCellsChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to auto generate columns.
        /// </summary>
        public bool AutoGenerateColumns
        {
            get => (bool)this.GetValue(AutoGenerateColumnsProperty);
            set => this.SetValue(AutoGenerateColumnsProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow automatic insert mode.
        /// </summary>
        public bool AutoInsert
        {
            get => (bool)this.GetValue(AutoInsertProperty);
            set => this.SetValue(AutoInsertProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this grid can clear cells.
        /// </summary>
        /// <value><c>true</c> if this instance can clear; otherwise, <c>false</c> .</value>
        public bool CanClear
        {
            get => (bool)this.GetValue(CanClearProperty);
            set => this.SetValue(CanClearProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can delete.
        /// </summary>
        /// <value><c>true</c> if this instance can delete; otherwise, <c>false</c> .</value>
        public bool CanDelete
        {
            get => (bool)this.GetValue(CanDeleteProperty);
            set => this.SetValue(CanDeleteProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can insert.
        /// </summary>
        /// <value><c>true</c> if this instance can insert; otherwise, <c>false</c> .</value>
        public bool CanInsert
        {
            get => (bool)this.GetValue(CanInsertProperty);
            set => this.SetValue(CanInsertProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can resize columns.
        /// </summary>
        /// <value><c>true</c> if this instance can resize columns; otherwise, <c>false</c> .</value>
        public bool CanResizeColumns
        {
            get => (bool)this.GetValue(CanResizeColumnsProperty);
            set => this.SetValue(CanResizeColumnsProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can resize rows.
        /// </summary>
        /// <value><c>true</c> if this instance can resize rows; otherwise, <c>false</c> .</value>
        public bool CanResizeRows
        {
            get => (bool)this.GetValue(CanResizeRowsProperty);
            set => this.SetValue(CanResizeRowsProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only cells in the changed column should be changed when changing value for a selection.
        /// </summary>
        /// <value><c>true</c> if only cells in the current column should be changed; otherwise, <c>false</c> .</value>
        public bool MultiChangeInChangedColumnOnly
        {
            get => (bool)this.GetValue(MultiChangeInChangedColumnOnlyProperty);
            set => this.SetValue(MultiChangeInChangedColumnOnlyProperty, value);
        }

        /// <summary>
        /// Gets the column definitions.
        /// </summary>
        /// <value>The column definitions.</value>
        public Collection<PropertyDefinition> ColumnDefinitions => this.PropertyDefinitions;

        /// <summary>
        /// Gets or sets the height of the column headers.
        /// </summary>
        /// <value>The height of the column header.</value>
        public GridLength ColumnHeaderHeight
        {
            get => (GridLength)this.GetValue(ColumnHeaderHeightProperty);
            set => this.SetValue(ColumnHeaderHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the columns context menu.
        /// </summary>
        /// <value>The columns context menu.</value>
        public ContextMenu ColumnsContextMenu
        {
            get => (ContextMenu)this.GetValue(ColumnsContextMenuProperty);
            set => this.SetValue(ColumnsContextMenuProperty, value);
        }

        /// <summary>
        /// Gets or sets the columns context menu.
        /// </summary>
        /// <value>The columns context menu.</value>
        public ContextMenu SheetContextMenu
        {
            get => (ContextMenu)this.GetValue(SheetContextMenuProperty);
            set => this.SetValue(SheetContextMenuProperty, value);
        }

        /// <summary>
        /// Gets or sets the control factory.
        /// </summary>
        public IDataGridControlFactory ControlFactory
        {
            get => (IDataGridControlFactory)this.GetValue(ControlFactoryProperty);
            set => this.SetValue(ControlFactoryProperty, value);
        }

        /// <summary>
        /// Gets or sets the cell definition factory.
        /// </summary>
        public ICellDefinitionFactory CellDefinitionFactory
        {
            get => (ICellDefinitionFactory)this.GetValue(CellDefinitionFactoryProperty);
            set => this.SetValue(CellDefinitionFactoryProperty, value);
        }

        /// <summary>
        /// Gets or sets the create item function.
        /// </summary>
        /// <value>The create item.</value>
        public Func<object> CreateItem
        {
            get => (Func<object>)this.GetValue(CreateItemProperty);
            set => this.SetValue(CreateItemProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom sort comparer.
        /// </summary>
        /// <value>The custom sort comparer.</value>
        public IComparer CustomSort
        {
            get => (IComparer)this.GetValue(CustomSortProperty);
            set => this.SetValue(CustomSortProperty, value);
        }

        /// <summary>
        /// Gets or sets the create column header function.
        /// </summary>
        /// <value>The create column header.</value>
        public Func<int, object> CreateColumnHeader
        {
            get => (Func<int, object>)this.GetValue(CreateColumnHeaderProperty);
            set => this.SetValue(CreateColumnHeaderProperty, value);
        }

        /// <summary>
        /// Gets or sets the current cell.
        /// </summary>
        public CellRef CurrentCell
        {
            get => (CellRef)this.GetValue(CurrentCellProperty);
            set => this.SetValue(CurrentCellProperty, value);
        }

        /// <summary>
        /// Gets or sets the default column width.
        /// </summary>
        /// <value>The default width of the column.</value>
        public GridLength DefaultColumnWidth
        {
            get => (GridLength)this.GetValue(DefaultColumnWidthProperty);
            set => this.SetValue(DefaultColumnWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the default horizontal alignment.
        /// </summary>
        /// <value>The default horizontal alignment.</value>
        public System.Windows.HorizontalAlignment DefaultHorizontalAlignment
        {
            get => (System.Windows.HorizontalAlignment)this.GetValue(DefaultHorizontalAlignmentProperty);
            set => this.SetValue(DefaultHorizontalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the default height of the row.
        /// </summary>
        /// <value>The default height of the row.</value>
        public GridLength DefaultRowHeight
        {
            get => (GridLength)this.GetValue(DefaultRowHeightProperty);
            set => this.SetValue(DefaultRowHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether easy insert by keyboard (press enter/down/right to add new rows/columns) is enabled.
        /// </summary>
        /// <value><c>true</c> if easy insert is enabled; otherwise, <c>false</c>.</value>
        public bool IsEasyInsertByKeyboardEnabled
        {
            get => (bool)this.GetValue(IsEasyInsertByKeyboardEnabledProperty);
            set => this.SetValue(IsEasyInsertByKeyboardEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether easy insert by mouse (mouse down outside existing rows/columns) is enabled.
        /// </summary>
        /// <value><c>true</c> if easy insert is enabled; otherwise, <c>false</c>.</value>
        public bool IsEasyInsertByMouseEnabled
        {
            get => (bool)this.GetValue(IsEasyInsertByMouseEnabledProperty);
            set => this.SetValue(IsEasyInsertByMouseEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the grid line brush.
        /// </summary>
        /// <value>The grid line brush.</value>
        public Brush GridLineBrush
        {
            get => (Brush)this.GetValue(GridLineBrushProperty);
            set => this.SetValue(GridLineBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the header border brush.
        /// </summary>
        /// <value>The header border brush.</value>
        public Brush HeaderBorderBrush
        {
            get => (Brush)this.GetValue(HeaderBorderBrushProperty);
            set => this.SetValue(HeaderBorderBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the input direction (the moving direction of the current cell when Enter is pressed).
        /// </summary>
        /// <value>The input direction.</value>
        public InputDirection InputDirection
        {
            get => (InputDirection)this.GetValue(InputDirectionProperty);
            set => this.SetValue(InputDirectionProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the auto fill is enabled or not.
        /// </summary>
        /// <value>If auto fill is enabled, <c>true</c>; otherwise <c>false</c>.</value>
        public bool IsAutoFillEnabled
        {
            get => (bool)this.GetValue(IsAutoFillEnabledProperty);
            set => this.SetValue(IsAutoFillEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current cell will change after the user has pressed Enter.
        /// </summary>
        /// <value>If the feature is enabled, <c>true</c>; otherwise <c>false</c>.</value>
        public bool IsMoveAfterEnterEnabled
        {
            get => (bool)this.GetValue(IsMoveAfterEnterEnabledProperty);
            set => this.SetValue(IsMoveAfterEnterEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the binding path to the item headers (row or column headers, depending on the <see cref="ItemsInRows" /> property.
        /// </summary>
        public string ItemHeaderPropertyPath
        {
            get => (string)this.GetValue(ItemHeaderPropertyPathProperty);
            set => this.SetValue(ItemHeaderPropertyPathProperty, value);
        }

        /// <summary>
        /// Gets or sets the items source.
        /// </summary>
        /// <value>The items source.</value>
        public IList ItemsSource
        {
            get => (IList)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the row headers source.
        /// </summary>
        /// <value>The row headers source.</value>
        public IList RowHeadersSource
        {
            get => (IList)this.GetValue(RowHeadersSourceProperty);
            set => this.SetValue(RowHeadersSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the column headers source.
        /// </summary>
        /// <value>The column headers source.</value>
        public IList ColumnHeadersSource
        {
            get => (IList)this.GetValue(ColumnHeadersSourceProperty);
            set => this.SetValue(ColumnHeadersSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the row headers format string.
        /// </summary>
        /// <value>The row headers format string.</value>
        public string RowHeadersFormatString
        {
            get => (string)this.GetValue(RowHeadersFormatStringProperty);
            set => this.SetValue(RowHeadersFormatStringProperty, value);
        }

        /// <summary>
        /// Gets or sets the column headers format string.
        /// </summary>
        /// <value>The column headers format string.</value>
        public string ColumnHeadersFormatString
        {
            get => (string)this.GetValue(ColumnHeadersFormatStringProperty);
            set => this.SetValue(ColumnHeadersFormatStringProperty, value);
        }

        /// <summary>
        /// Gets the row definitions.
        /// </summary>
        /// <value>The row definitions.</value>
        public Collection<PropertyDefinition> RowDefinitions => this.PropertyDefinitions;

        /// <summary>
        /// Gets or sets the width of the row headers.
        /// </summary>
        /// <value>The width of the row header.</value>
        public GridLength RowHeaderWidth
        {
            get => (GridLength)this.GetValue(RowHeaderWidthProperty);
            set => this.SetValue(RowHeaderWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the rows context menu.
        /// </summary>
        /// <value>The rows context menu.</value>
        public ContextMenu RowsContextMenu
        {
            get => (ContextMenu)this.GetValue(RowsContextMenuProperty);
            set => this.SetValue(RowsContextMenuProperty, value);
        }

        /// <summary>
        /// Gets the selected cells.
        /// </summary>
        /// <value>The selected cells.</value>
        public IEnumerable<CellRef> SelectedCells
        {
            get
            {
                var rowMin = Math.Min(this.CurrentCell.Row, this.SelectionCell.Row);
                var columnMin = Math.Min(this.CurrentCell.Column, this.SelectionCell.Column);
                var rowMax = Math.Max(this.CurrentCell.Row, this.SelectionCell.Row);
                var columnMax = Math.Max(this.CurrentCell.Column, this.SelectionCell.Column);

                for (var i = rowMin; i <= rowMax; i++)
                {
                    for (var j = columnMin; j <= columnMax; j++)
                    {
                        yield return new CellRef(i, j);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected items.
        /// </summary>
        /// <value>The selected items.</value>
        public IEnumerable SelectedItems
        {
            get => (IEnumerable)this.GetValue(SelectedItemsProperty);
            set => this.SetValue(SelectedItemsProperty, value);
        }

        /// <summary>
        /// Gets or sets the cell defining the selection area. The selection area is defined by the CurrentCell and SelectionCell.
        /// </summary>
        public CellRef SelectionCell
        {
            get => (CellRef)this.GetValue(SelectionCellProperty);
            set => this.SetValue(SelectionCellProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to wrap items in the defined columns.
        /// </summary>
        /// <value><c>true</c> if items should be wrapped; otherwise, <c>false</c> .</value>
        public bool WrapItems
        {
            get => (bool)this.GetValue(WrapItemsProperty);
            set => this.SetValue(WrapItemsProperty, value);
        }

        /// <summary>
        /// Gets the collection view.
        /// </summary>
        /// <value>
        /// The collection view.
        /// </value>
        public ICollectionView CollectionView { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to use rows for the items.
        /// </summary>
        /// <value><c>true</c> if the items are in rows; otherwise, <c>false</c> .</value>
        public bool ItemsInRows => !this.ItemsInColumns;

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        /// <value>The number of columns.</value>
        public int Columns => this.sheetGrid != null ? this.sheetGrid.ColumnDefinitions.Count : 0;

        /// <summary>
        /// Gets the number of rows.</summary>
        /// <value>The number of rows.</value>
        public int Rows => this.sheetGrid != null ? this.sheetGrid.RowDefinitions.Count - 1 : 0;

        /// <summary>
        /// Gets a value indicating whether to use columns for the items.
        /// </summary>
        public bool ItemsInColumns { get; private set; }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        public IDataGridOperator Operator { get; private set; }

        /// <summary>
        /// Gets the row/column definitions.
        /// </summary>
        /// <value>The row/column definitions.</value>
        public Collection<PropertyDefinition> PropertyDefinitions { get; } = new Collection<PropertyDefinition>();

        /// <summary>
        /// Gets a value indicating whether this instance can delete columns.
        /// </summary>
        /// <value><c>true</c> if this instance can delete columns; otherwise, <c>false</c> .</value>
        protected virtual bool CanDeleteColumns => this.Operator?.CanDeleteColumns() ?? false;

        /// <summary>
        /// Gets a value indicating whether this instance can delete rows.
        /// </summary>
        /// <value><c>true</c> if this instance can delete rows; otherwise, <c>false</c> .</value>
        protected virtual bool CanDeleteRows => this.Operator?.CanDeleteRows() ?? false;

        /// <summary>
        /// Gets a value indicating whether this instance can insert columns.
        /// </summary>
        /// <value><c>true</c> if this instance can insert columns; otherwise, <c>false</c> .</value>
        protected virtual bool CanInsertColumns => this.Operator?.CanInsertColumns() ?? false;

        /// <summary>
        /// Gets a value indicating whether this instance can insert rows.
        /// </summary>
        /// <value><c>true</c> if this instance can insert rows; otherwise, <c>false</c> .</value>
        protected virtual bool CanInsertRows => this.Operator?.CanInsertRows() ?? false;

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see
        /// cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> .
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.sheetScrollViewer = this.Template.FindName(PartSheetScrollViewer, this) as ScrollViewer;
            this.sheetGrid = this.Template.FindName(PartSheetGrid, this) as Grid;
            this.columnScrollViewer = this.Template.FindName(PartColumnScrollViewer, this) as ScrollViewer;
            this.columnGrid = this.Template.FindName(PartColumnGrid, this) as Grid;
            this.rowScrollViewer = this.Template.FindName(PartRowScrollViewer, this) as ScrollViewer;
            this.rowGrid = this.Template.FindName(PartRowGrid, this) as Grid;
            this.rowSelectionBackground = this.Template.FindName(PartRowSelectionBackground, this) as Border;
            this.columnSelectionBackground = this.Template.FindName(PartColumnSelectionBackground, this) as Border;
            this.selectionBackground = this.Template.FindName(PartSelectionBackground, this) as Border;
            this.currentBackground = this.Template.FindName(PartCurrentBackground, this) as Border;
            this.selection = this.Template.FindName(PartSelection, this) as Border;
            this.autoFillSelection = this.Template.FindName(PartAutoFillSelection, this) as Border;
            this.autoFillBox = this.Template.FindName(PartAutoFillBox, this) as Border;
            this.topLeft = this.Template.FindName(PartTopLeft, this) as Border;

            if (this.sheetScrollViewer == null)
            {
                throw new Exception(PartSheetScrollViewer + " not found.");
            }

            if (this.rowScrollViewer == null)
            {
                throw new Exception(PartRowScrollViewer + " not found.");
            }

            if (this.columnScrollViewer == null)
            {
                throw new Exception(PartColumnScrollViewer + " not found.");
            }

            if (this.topLeft == null)
            {
                throw new Exception(PartTopLeft + " not found.");
            }

            if (this.autoFillBox == null)
            {
                throw new Exception(PartAutoFillBox + " not found.");
            }

            if (this.columnGrid == null)
            {
                throw new Exception(PartColumnGrid + " not found.");
            }

            if (this.rowGrid == null)
            {
                throw new Exception(PartRowGrid + " not found.");
            }

            if (this.sheetGrid == null)
            {
                throw new Exception(PartSheetGrid + " not found.");
            }

            this.sheetScrollViewer.ScrollChanged += this.ScrollViewerScrollChanged;
            this.rowScrollViewer.ScrollChanged += this.RowScrollViewerScrollChanged;
            this.columnScrollViewer.ScrollChanged += this.ColumnScrollViewerScrollChanged;

            this.topLeft.MouseDown += this.TopLeftMouseDown;
            this.autoFillBox.MouseLeftButtonDown += this.AutoFillBoxMouseDown;
            this.columnGrid.MouseDown += this.ColumnGridMouseDown;
            this.columnGrid.MouseMove += this.ColumnGridMouseMove;
            this.columnGrid.MouseUp += this.ColumnGridMouseUp;
            this.rowGrid.MouseDown += this.RowGridMouseDown;
            this.rowGrid.MouseMove += this.RowGridMouseMove;
            this.rowGrid.MouseUp += this.RowGridMouseUp;
            this.sheetScrollViewer.SizeChanged += (s, e) => this.UpdateGridSize();
            this.sheetGrid.MouseDown += this.SheetGridMouseDown;

            this.sheetScrollViewer.Loaded += (s, e) => this.UpdateGridSize();

            this.autoFiller = new AutoFiller(this.GetCellValue, this.TrySetCellValue);

            this.autoFillToolTip = new ToolTip
            {
                Placement = PlacementMode.Bottom,
                PlacementTarget = this.autoFillSelection
            };

            this.UpdateGridContent();
            this.SelectedCellsChanged();

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (s, e) => this.Copy()));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, (s, e) => this.Cut()));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (s, e) => this.Paste()));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, (s, e) => this.Clear(), (s, e) => e.CanExecute = this.CanClear));
        }

        /// <summary>
        /// Receives events from the centralized event manager.
        /// </summary>
        /// <param name="managerType">The type of the <see cref="T:System.Windows.WeakEventManager" /> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns>
        /// true if the listener handled the event. It is considered an error by the <see cref="T:System.Windows.WeakEventManager" /> handling in WPF to register a listener for an event that the listener does not handle. Regardless, the method should return false if it receives an event that it does not recognize or handle.
        /// </returns>
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(CollectionChangedEventManager) && sender == this.subscribedCollection)
            {
                this.OnItemsCollectionChanged(e as NotifyCollectionChangedEventArgs);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Pastes the content from the clipboard to the selected cells.
        /// </summary>
        public void Paste()
        {
            this.PasteOverride();
        }

        /// <summary>
        /// Gets the column/row definition for the specified cell.
        /// </summary>
        /// <param name="cell">The cell reference.</param>
        /// <returns>
        /// The column/row definition.
        /// </returns>
        internal PropertyDefinition GetPropertyDefinition(CellRef cell)
        {
            var index = this.ItemsInRows ? cell.Column : cell.Row;

            if (index < this.PropertyDefinitions.Count)
            {
                return this.PropertyDefinitions[index];
            }

            return null;
        }

        /// <summary>
        /// Implements the paste operation.
        /// </summary>
        protected virtual void PasteOverride()
        {
            var values = this.GetClipboardData();

            if (values == null)
            {
                return;
            }

            var range = this.SetValues(values, this.GetSelectionRange());

            this.SelectionCell = range.BottomRight;
            this.CurrentCell = range.TopLeft;
            this.ScrollIntoView(this.CurrentCell);
        }

        /// <summary>
        /// Sets the values in the specified range.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="range">The range.</param>
        /// <returns>The range that was actually set.</returns>
        protected virtual CellRange SetValues(object[,] values, CellRange range)
        {
            var rows = values.GetUpperBound(0) + 1;
            var columns = values.GetUpperBound(1) + 1;

            var bottomRight = new CellRef(Math.Max(range.BottomRow, range.TopRow + rows - 1), Math.Max(range.BottomRight.Column, range.LeftColumn + columns - 1));
            var outputRange = new CellRange(range.TopLeft, bottomRight);

            this.suspendCollectionChangedNotifications = true;

            for (var i = range.TopRow; i <= outputRange.BottomRow; i++)
            {
                if (i >= this.Rows)
                {
                    if (!this.CanInsertRows)
                    {
                        break;
                    }

                    this.Operator.InsertRows(i, 1);
                }

                for (var j = range.LeftColumn; j <= outputRange.RightColumn; j++)
                {
                    if (j >= this.Columns)
                    {
                        if (!this.CanInsertColumns)
                        {
                            break;
                        }

                        this.Operator.InsertColumns(i, 1);
                    }

                    var value = values[(i - outputRange.TopRow) % rows, (j - outputRange.LeftColumn) % columns];
                    this.TrySetCellValue(new CellRef(i, j), value);
                }
            }

            // Update the collection view in case items has been added
            this.UpdateCollectionView();
            this.UpdateGridContent();
            this.suspendCollectionChangedNotifications = false;

            return outputRange;
        }

        /// <summary>
        /// Gets the clipboard data.
        /// </summary>
        /// <returns>A two-dimensional array of values.</returns>
        protected virtual object[,] GetClipboardData()
        {
            object[,] values = null;
            var dataObject = Clipboard.GetDataObject();
            if (dataObject != null)
            {
                var data = dataObject.GetData(typeof(DataGrid));
                values = data as object[,];
            }

            if (values == null && Clipboard.ContainsText())
            {
                var text = Clipboard.GetText().Trim();
                values = this.TextToArray(text);
            }

            return values;
        }

        /// <summary>
        /// Scroll the specified cell into view.
        /// </summary>
        /// <param name="cellRef">The cell reference.</param>
        protected void ScrollIntoView(CellRef cellRef)
        {
            this.sheetGrid.UpdateLayout();

            var pos0 = this.GetPosition(cellRef);
            var pos1 = this.GetPosition(new CellRef(cellRef.Row + 1, cellRef.Column + 1));

            if (pos0.X < this.sheetScrollViewer.HorizontalOffset)
            {
                this.sheetScrollViewer.ScrollToHorizontalOffset(pos0.X);
            }

            if (pos1.X > this.sheetScrollViewer.HorizontalOffset + this.sheetScrollViewer.ViewportWidth)
            {
                var newOffset = Math.Max(pos1.X - this.sheetScrollViewer.ViewportWidth, 0);
                this.sheetScrollViewer.ScrollToHorizontalOffset(newOffset);
            }

            if (pos0.Y < this.sheetScrollViewer.VerticalOffset)
            {
                this.sheetScrollViewer.ScrollToVerticalOffset(pos0.Y);
            }

            if (pos1.Y > this.sheetScrollViewer.VerticalOffset + this.sheetScrollViewer.ViewportHeight)
            {
                var newOffset = Math.Max(pos1.Y - this.sheetScrollViewer.ViewportHeight, 0);
                this.sheetScrollViewer.ScrollToVerticalOffset(newOffset);
            }
        }

        /// <summary>
        /// Implements the copy operation.
        /// </summary>
        protected virtual void CopyOverride()
        {
            this.Copy("\t");
        }

        /// <summary>
        /// Implements the cut operation.
        /// </summary>
        protected virtual void CutOverride()
        {
            this.Copy();
            this.Clear();
        }

        /// <summary>
        /// Gets the selection cell range.
        /// </summary>
        /// <returns>The cell range.</returns>
        protected CellRange GetSelectionRange()
        {
            return new CellRange(this.CurrentCell, this.SelectionCell);
        }

        /// <summary>
        /// Copies the selected cells to the clipboard.
        /// </summary>
        /// <param name="separator">The separator.</param>
        protected void Copy(string separator)
        {
            var range = this.GetSelectionRange();
            var valueArray = this.GetCellValues(range);
            var stringArray = this.GetCellStrings(range, valueArray);
            var text = this.ConvertToCsv(stringArray, separator, true);

            var dataObject = new DataObject();
            dataObject.SetText(text);

            try
            {
                dataObject.SetData(typeof(DataGrid), valueArray);
            }
            catch (Exception e)
            {
                // Could not set data on clipboard (e.g., non-serializable values)
                Debug.WriteLine(e);
            }

            Clipboard.SetDataObject(dataObject);
        }

        /// <summary>
        /// Gets the formatted string value for the specified cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The cell string.
        /// </returns>
        protected virtual string FormatCellString(CellRef cell, object value)
        {
            var formatString = this.GetFormatString(cell);
            return FormatValue(value, formatString);
        }

        /// <summary>
        /// Exports the grid to comma separated values.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="includeHeader">Include a header if set to <c>true</c>.</param>
        /// <returns>
        /// The comma separated values string.
        /// </returns>
        protected string ToCsv(CellRange range, string separator = ";", bool includeHeader = true)
        {
            var sb = new StringBuilder();

            if (includeHeader)
            {
                for (var j = range.LeftColumn; j <= range.RightColumn; j++)
                {
                    var h = this.GetColumnHeader(j).ToString();
                    h = CsvEncodeString(h);
                    if (sb.Length > 0)
                    {
                        sb.Append(separator);
                    }

                    sb.Append(h);
                }

                sb.AppendLine();
            }

            var strings = this.GetCellStrings(range);
            var csv = this.ConvertToCsv(strings, ";", true);
            sb.Append(csv);

            return sb.ToString();
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.ImeProcessed)
            {
                if (this.currentEditControl is TextBox textEditor && !textEditor.IsFocused)
                {
                    this.ShowTextBoxEditControl();
                }
            }
        }

        /// <summary>
        /// Handles mouse left button down events.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.Focus();
            base.OnMouseLeftButtonDown(e);

            var pos = e.GetPosition(this.sheetGrid);
            var cellRef = this.GetCell(pos);

            if (cellRef.Column == -1 || cellRef.Row == -1)
            {
                return;
            }

            if (cellRef.Row >= this.Rows && (!this.CanInsertRows || !this.IsEasyInsertByMouseEnabled))
            {
                return;
            }

            if (cellRef.Column > this.Columns && (!this.CanInsertColumns || !this.IsEasyInsertByMouseEnabled))
            {
                return;
            }

            if (this.autoFillSelection.Visibility == Visibility.Visible)
            {
                this.AutoFillCell = cellRef;
                Mouse.OverrideCursor = this.autoFillBox.Cursor;
                this.autoFillToolTip.IsOpen = true;
            }
            else
            {
                if (!this.HandleAutoInsert(cellRef))
                {
                    var shift = (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None;
                    if (!shift)
                    {
                        this.CurrentCell = cellRef;
                    }

                    this.SelectionCell = cellRef;
                    this.ScrollIntoView(cellRef);
                }

                Mouse.OverrideCursor = this.sheetGrid.Cursor;
            }

            this.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// Handles mouse left button up events.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this.OnMouseUp(e);

            this.ReleaseMouseCapture();
            Mouse.OverrideCursor = null;

            if (this.autoFillSelection.Visibility == Visibility.Visible)
            {
                this.autoFiller.AutoFill(this.CurrentCell, this.SelectionCell, this.AutoFillCell);

                this.autoFillSelection.Visibility = Visibility.Hidden;
                this.autoFillToolTip.IsOpen = false;
            }
        }

        /// <summary>
        /// Handles mouse move events.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!this.IsMouseCaptured)
            {
                return;
            }

            var isInAutoFillMode = this.autoFillSelection.Visibility == Visibility.Visible;

            var pos = e.GetPosition(this.sheetGrid);
            var cellRef = this.GetCell(pos, isInAutoFillMode, this.CurrentCell);

            if (cellRef.Column == -1 || cellRef.Row == -1)
            {
                return;
            }

            if (cellRef.Row >= this.Rows && (!this.CanInsertRows || !this.IsEasyInsertByMouseEnabled))
            {
                return;
            }

            if (cellRef.Column > this.Columns && (!this.CanInsertColumns || !this.IsEasyInsertByMouseEnabled))
            {
                return;
            }

            if (isInAutoFillMode)
            {
                this.AutoFillCell = cellRef;
                if (this.autoFiller.TryExtrapolate(
                    cellRef,
                    this.CurrentCell,
                    this.SelectionCell,
                    this.AutoFillCell,
                    out var result))
                {
                    var formatString = this.GetFormatString(cellRef);
                    this.autoFillToolTip.Content = FormatValue(result, formatString);
                }

                this.autoFillToolTip.Placement = PlacementMode.Relative;
                var p = e.GetPosition(this.autoFillSelection);
                this.autoFillToolTip.HorizontalOffset = p.X + 8;
                this.autoFillToolTip.VerticalOffset = p.Y + 8;
            }
            else
            {
                this.SelectionCell = cellRef;
            }

            this.ScrollIntoView(cellRef);
        }

        /// <summary>
        /// Handles mouse wheel preview events.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            var control = Keyboard.IsKeyDown(Key.LeftCtrl);
            if (control)
            {
                var s = 1 + (e.Delta * 0.0004);
                var tg = new TransformGroup();
                if (this.LayoutTransform != null)
                {
                    tg.Children.Add(this.LayoutTransform);
                }

                tg.Children.Add(new ScaleTransform(s, s));
                this.LayoutTransform = tg;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles text input events.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);

            // do not allow special characters (including backspace, tab, enter)
            // it is particularly bad to add backspace characters to the cell, since this may not be XML serialized...
            if (e.Text.Length == 0 || e.Text[0] < 32)
            {
                return;
            }

            if (this.currentEditControl == null)
            {
                this.ShowEditControl();
            }

            if (this.currentEditControl is TextBox textEditor && textEditor.IsEnabled)
            {
                this.ShowTextBoxEditControl();
                this.Dispatcher.BeginInvoke(
                    new Action(
                    () =>
                    {
                        // make sure this code is executed after the textbox has been loaded (and bindings updated)
                        textEditor.Text = e.Text;
                        textEditor.CaretIndex = textEditor.Text.Length;
                    }),
                    DispatcherPriority.Loaded);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles KeyDown events on the grid.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            var control = (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None;
            var shift = (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None;
            var alt = (Keyboard.Modifiers & ModifierKeys.Alt) != ModifierKeys.None;

            var row = shift ? this.SelectionCell.Row : this.CurrentCell.Row;
            var column = shift ? this.SelectionCell.Column : this.CurrentCell.Column;

            switch (e.Key)
            {
                case Key.Enter:
                    if (this.IsMoveAfterEnterEnabled)
                    {
                        if (this.InputDirection == InputDirection.Vertical)
                        {
                            this.ChangeCurrentCell(shift ? -1 : 1, 0);
                        }
                        else
                        {
                            this.ChangeCurrentCell(0, shift ? -1 : 1);
                        }
                    }

                    e.Handled = true;
                    return;

                case Key.Up:
                    if (row > 0)
                    {
                        row--;
                    }

                    if (this.endPressed)
                    {
                        row = this.FindNextRow(row, column, -1);
                    }

                    if (control)
                    {
                        row = 0;
                    }

                    break;
                case Key.Down:
                    if (row < this.Rows - 1 || (this.CanInsertRows && this.IsEasyInsertByKeyboardEnabled))
                    {
                        row++;
                    }

                    if (this.endPressed)
                    {
                        row = this.FindNextRow(row, column, 1);
                    }

                    if (control)
                    {
                        row = this.Rows - 1;
                    }

                    break;
                case Key.Left:
                    if (column > 0)
                    {
                        column--;
                    }

                    if (this.endPressed)
                    {
                        column = this.FindNextColumn(row, column, -1);
                    }

                    if (control)
                    {
                        column = 0;
                    }

                    break;
                case Key.Right:
                    if (column < this.Columns - 1 || (this.CanInsertColumns && this.IsEasyInsertByKeyboardEnabled))
                    {
                        column++;
                    }

                    if (this.endPressed)
                    {
                        column = this.FindNextColumn(row, column, 1);
                    }

                    if (control)
                    {
                        column = this.Columns - 1;
                    }

                    break;
                case Key.End:

                    // Flag that the next key should be handled differently
                    this.endPressed = true;
                    e.Handled = true;
                    return;
                case Key.Home:
                    column = 0;
                    row = 0;
                    break;
                case Key.Back:
                case Key.Delete:
                    if (this.CanClear)
                    {
                        this.Clear();
                        e.Handled = true;
                    }

                    return;
                case Key.F2:
                    if (this.ShowTextBoxEditControl())
                    {
                        e.Handled = true;
                    }

                    return;
                case Key.F4:
                    if (this.OpenComboBoxControl())
                    {
                        e.Handled = true;
                    }

                    return;
                case Key.Space:
                    if (this.ToggleCheck())
                    {
                        e.Handled = true;
                    }

                    return;
                case Key.A:
                    if (control)
                    {
                        this.SelectAll();
                        e.Handled = true;
                    }

                    return;
                case Key.C:
                    if (control && alt)
                    {
                        Clipboard.SetText(this.ToCsv(this.GetSelectionRange()));
                        e.Handled = true;
                    }

                    return;
                default:
                    return;
            }

            if (e.Key != Key.End)
            {
                // Turn of special handling after the first key after End was pressed.
                this.endPressed = false;
            }

            var cell = new CellRef(row, column);
            if (!this.HandleAutoInsert(cell))
            {
                this.SelectionCell = cell;

                if (!shift)
                {
                    this.CurrentCell = cell;
                }

                this.ScrollIntoView(cell);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Creates the operator for the current items source.
        /// </summary>
        /// <returns>The operator.</returns>
        protected virtual IDataGridOperator CreateOperator()
        {
            var list = this.ItemsSource;
            if (list == null)
            {
                return null;
            }

            if (TypeHelper.IsIListIList(list))
            {
                return new ListListOperator(this);
            }

            if (this.WrapItems)
            {
                return new WrapItemsOperator(this);
            }

            return new ListOperator(this);
        }

        /// <summary>
        /// Splits a string separated by \n and \t into an array.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        /// An 2-dimensional array of strings.
        /// </returns>
        protected virtual object[,] TextToArray(string text)
        {
            var rows = 0;
            var columns = 0;
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                rows++;
                var fields = line.Split('\t');
                if (fields.Length > columns)
                {
                    columns = fields.Length;
                }
            }

            if (rows == 0 || columns == 0)
            {
                return null;
            }

            var result = new object[rows, columns];
            var row = 0;
            foreach (var line in lines)
            {
                var fields = line.Split('\t');

                var column = 0;
                foreach (var field in fields)
                {
                    result[row, column] = field.Trim(" \r\n\t".ToCharArray());
                    column++;
                }

                row++;
            }

            return result;
        }

        /// <summary>
        /// Implements the clear operation.
        /// </summary>
        protected virtual void ClearOverride()
        {
            foreach (var cell in this.SelectedCells)
            {
                var defaultValue = this.GetDefaultValue(cell);
                this.TrySetCellValue(cell, defaultValue);
            }
        }

        /// <summary>
        /// Gets the default value for the specified cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>The default value.</returns>
        protected virtual object GetDefaultValue(CellRef cell)
        {
            var value = this.GetCellValue(cell);
            if (value == null)
            {
                return null;
            }

            var type = this.Operator.GetPropertyType(cell);
            var isNullable = Nullable.GetUnderlyingType(type) != null;
            if (type.IsValueType && !isNullable)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch
                {
                    // could not call default ctor?
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the cell values of the specified cell range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns>
        /// An array of cell values.
        /// </returns>
        protected object[,] GetCellValues(CellRange range)
        {
            var result = new object[range.Rows, range.Columns];
            for (var i = 0; i < range.Rows; i++)
            {
                for (var j = 0; j < range.Columns; j++)
                {
                    result[i, j] = this.GetCellValue(new CellRef(range.TopRow + i, range.LeftColumn + j));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the string values of the specified cell range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="values">The values (optional).</param>
        /// <returns>
        /// An array of cell strings.
        /// </returns>
        protected string[,] GetCellStrings(CellRange range, object[,] values = null)
        {
            var result = new string[range.Rows, range.Columns];
            for (var i = 0; i < range.Rows; i++)
            {
                for (var j = 0; j < range.Columns; j++)
                {
                    var cell = new CellRef(range.TopRow + i, range.LeftColumn + j);
                    var value = values != null ? values[i, j] : this.GetCellValue(cell);
                    result[i, j] = this.FormatCellString(cell, value);
                }
            }

            return result;
        }

        /// <summary>
        /// Tries to set the value in the specified cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// True if the value was set.
        /// </returns>
        protected bool TrySetCellValue(CellRef cell, object value)
        {
            var cellWasSet = this.Operator.TrySetCellValue(cell, value);
            if (cellWasSet && !(this.ItemsSource is INotifyCollectionChanged))
            {
                this.UpdateCellContent(cell);
            }

            return cellWasSet;
        }



        /// <summary>
        /// Clamps a value between a minimum and maximum limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>
        /// The clamped value.
        /// </returns>
        private static int Clamp(int value, int min, int max)
        {
            var result = value;
            if (result > max)
            {
                result = max;
            }

            if (result < min)
            {
                result = min;
            }

            return result;
        }

        /// <summary>
        /// Encodes the specified string for use in a comma separated value file.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>
        /// The encoded string.
        /// </returns>
        private static string CsvEncodeString(string input)
        {
            if (input == null)
            {
                return null;
            }

            input = input.Replace("\"", "\"\"");
            if (input.Contains(";") || input.Contains("\""))
            {
                input = "\"" + input + "\"";
            }

            return input;
        }

        /// <summary>
        /// Formats the specified value with the specified format string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="formatString">The format string.</param>
        /// <returns>
        /// The format value.
        /// </returns>
        private static string FormatValue(object value, string formatString)
        {
            if (string.IsNullOrEmpty(formatString))
            {
                return value?.ToString();
            }

            if (!formatString.Contains("{0"))
            {
                formatString = "{0:" + formatString + "}";
            }

            return string.Format(formatString, value);
        }

        /// <summary>
        /// The set element position.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="cell">The cell.</param>
        private static void SetElementPosition(UIElement element, CellRef cell)
        {
            Grid.SetColumn(element, cell.Column);
            Grid.SetRow(element, cell.Row);
        }

        /// <summary>
        /// Coerces the current cell.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject" />.</param>
        /// <param name="basevalue">The base value.</param>
        /// <returns>
        /// The coerced current cell.
        /// </returns>
        private static object CoerceCurrentCell(DependencyObject d, object basevalue)
        {
            var cr = (CellRef)basevalue;
            var row = cr.Row;
            var column = cr.Column;
            var sg = (DataGrid)d;
            if (sg.AutoInsert)
            {
                row = Clamp(row, 0, sg.Rows - 1 + (sg.CanInsertRows ? 1 : 0));
                column = Clamp(column, 0, sg.Columns - 1 + (sg.CanInsertColumns ? 1 : 0));
            }
            else
            {
                row = Clamp(row, 0, sg.Rows - 1);
                column = Clamp(column, 0, sg.Columns - 1);
            }

            return new CellRef(row, column);
        }

        /// <summary>
        /// Coerces the selection cell.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="basevalue">The base value.</param>
        /// <returns>
        /// The coerced selection cell.
        /// </returns>
        private static object CoerceSelectionCell(DependencyObject sender, object basevalue)
        {
            var cr = (CellRef)basevalue;
            var row = cr.Row;
            var column = cr.Column;
            var sg = (DataGrid)sender;
            row = Clamp(row, 0, sg.Rows - 1);
            column = Clamp(column, 0, sg.Columns - 1);
            return new CellRef(row, column);
        }

        /// <summary>
        /// Gets the cell reference for the specified position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="isInAutoFillMode">if set to <c>true</c> [is in auto fill mode].</param>
        /// <param name="relativeTo">The relative to.</param>
        /// <returns>
        /// The cell reference.
        /// </returns>
        private CellRef GetCell(Point position, bool isInAutoFillMode = false, CellRef relativeTo = default(CellRef))
        {
            var w = 0d;
            var column = -1;
            var row = -1;
            for (var j = 0; j < this.sheetGrid.ColumnDefinitions.Count; j++)
            {
                var aw0 = j - 1 >= 0 ? this.sheetGrid.ColumnDefinitions[j - 1].ActualWidth : 0;
                var aw1 = this.sheetGrid.ColumnDefinitions[j].ActualWidth;
                var aw2 = j + 1 < this.sheetGrid.ColumnDefinitions.Count
                                 ? this.sheetGrid.ColumnDefinitions[j + 1].ActualWidth
                                 : 0;
                if (isInAutoFillMode)
                {
                    if (relativeTo.Column <= j)
                    {
                        aw0 = 0;
                        aw2 *= 0.5;
                    }
                    else
                    {
                        aw0 *= 0.5;
                        aw2 = 0;
                    }
                }
                else
                {
                    aw0 = 0;
                    aw2 = 0;
                }

                if (position.X > w - aw0 && position.X < w + aw1 + aw2)
                {
                    column = j;
                    break;
                }

                w += aw1;
            }

            if (w > 0 && column == -1)
            {
                column = this.sheetGrid.ColumnDefinitions.Count - 1;
            }

            var h = 0d;
            for (var i = 0; i < this.sheetGrid.RowDefinitions.Count; i++)
            {
                var ah = this.sheetGrid.RowDefinitions[i].ActualHeight;
                if (position.Y < h + ah)
                {
                    row = i;
                    break;
                }

                h += ah;
            }

            if (h > 0 && row == -1)
            {
                row = this.sheetGrid.RowDefinitions.Count - 1;
            }

            if (column == -1 || row == -1)
            {
                return new CellRef(-1, -1);
            }

            return new CellRef(row, column);
        }

        /// <summary>
        /// Gets the element at the specified cell.
        /// </summary>
        /// <param name="cellRef">The cell reference.</param>
        /// <returns>
        /// The element, or <c>null</c> if the cell was not found.
        /// </returns>
        private FrameworkElement GetCellElement(CellRef cellRef)
        {
            if (this.cellMap.TryGetValue(cellRef.GetHashCode(), out var e))
            {
                // check if the element is wrapped in a border container
                if (e is Border border)
                {
                    return (FrameworkElement)border.Child;
                }

                return e;
            }

            return null;
        }

        /// <summary>
        /// Gets the cell value from the Content property for the specified cell.
        /// </summary>
        /// <param name="cell">The cell reference.</param>
        /// <returns>
        /// The cell value.
        /// </returns>
        private object GetCellValue(CellRef cell)
        {
            return this.Operator.GetCellValue(cell);
        }

        /// <summary>
        /// Gets the position of the specified cell.
        /// </summary>
        /// <param name="cellRef">The cell reference.</param>
        /// <returns>
        /// The upper-left position of the cell.
        /// </returns>
        private Point GetPosition(CellRef cellRef)
        {
            var x = 0d;
            var y = 0d;
            for (var j = 0; j < cellRef.Column && j < this.sheetGrid.ColumnDefinitions.Count; j++)
            {
                x += this.sheetGrid.ColumnDefinitions[j].ActualWidth;
            }

            for (var i = 0; i < cellRef.Row && i < this.sheetGrid.RowDefinitions.Count; i++)
            {
                y += this.sheetGrid.RowDefinitions[i].ActualHeight;
            }

            return new Point(x, y);
        }

#if FEATURE_VIRTUALIZATION
        /// <summary>
        /// Gets the visible cells.
        /// </summary>
        /// <param name="topLeftCell">The top left cell.</param>
        /// <param name="bottomRightCell">The bottom right cell.</param>
        private void GetVisibleCells(out CellRef topLeftCell, out CellRef bottomRightCell)
        {
            var left = this.sheetScrollViewer.HorizontalOffset;
            var right = left + this.sheetScrollViewer.ActualWidth;
            var top = this.sheetScrollViewer.VerticalOffset;
            var bottom = top + this.sheetScrollViewer.ActualHeight;

            topLeftCell = this.GetCell(new Point(left, top));
            bottomRightCell = this.GetCell(new Point(right, bottom));
        }
#endif

        /// <summary>
        /// Removes the current editor control.
        /// </summary>
        private void RemoveEditControl()
        {
            if (this.currentEditControl != null/* && this.currentEditControl.Visibility == Visibility.Visible*/)
            {
                var textEditor = this.currentEditControl as TextBox;
                if (textEditor != null)
                {
                    textEditor.PreviewKeyDown -= this.TextEditorPreviewKeyDown;
                }

                this.sheetGrid.Children.Remove(this.currentEditControl);
                this.currentEditControl = null;
                this.Focus();
            }
        }

        /// <summary>
        /// Inserts an item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="updateGrid">Determines whether the grid should be updated.</param>
        /// <returns>
        /// The actual index of the inserted item, <c>-1</c> if no item was inserted.
        /// </returns>
        private int InsertItem(int index, bool updateGrid = true)
        {
            var actualIndex = this.Operator.InsertItem(index);
            if (actualIndex != -1)
            {
                if (updateGrid)
                {
                    this.UpdateGridContent();
                }
            }

            this.RefreshIfRequired();

            return actualIndex;
        }

        /// <summary>
        /// Changes the current cell with the specified delta.
        /// </summary>
        /// <param name="deltaRows">The change in rows.</param>
        /// <param name="deltaColumns">The change in columns.</param>
        protected void ChangeCurrentCell(int deltaRows, int deltaColumns)
        {
            var row = this.CurrentCell.Row;
            var column = this.CurrentCell.Column;
            row += deltaRows;
            column += deltaColumns;
            if (row < 0)
            {
                row = this.Rows - 1;
                column--;
            }

            if (row >= this.Rows && (!this.CanInsertRows || !this.IsEasyInsertByKeyboardEnabled))
            {
                column++;
                row = 0;
                if (column >= this.Columns)
                {
                    column = 0;
                }
            }

            if (column < 0)
            {
                column = 0;
                row--;
                if (row < 0)
                {
                    row = this.Rows - 1;
                }
            }

            if (column >= this.Columns && (!this.CanInsertColumns || !this.IsEasyInsertByKeyboardEnabled))
            {
                column = 0;
                row++;
                if (row >= this.Rows && (!this.CanInsertRows || !this.IsEasyInsertByKeyboardEnabled))
                {
                    row = 0;
                }
            }

            var cell = new CellRef(row, column);
            if (!this.HandleAutoInsert(cell))
            {
                this.SelectionCell = cell;
                this.CurrentCell = cell;
                this.ScrollIntoView(cell);
            }
        }

        /// <summary>
        /// Shows the text box editor.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the text editor was shown, <c>false</c> otherwise.
        /// </returns>
        private bool ShowTextBoxEditControl()
        {
            if (this.currentEditControl == null)
            {
                this.ShowEditControl();
            }

            var textEditor = this.currentEditControl as TextBox;
            if (textEditor != null)
            {
                textEditor.Visibility = Visibility.Visible;
                textEditor.Focus();
                textEditor.CaretIndex = textEditor.Text.Length;
                textEditor.SelectAll();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Opens the combo box control.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the combo box was shown, <c>false</c> otherwise.
        /// </returns>
        private bool OpenComboBoxControl()
        {
            var comboBox = this.currentEditControl as ComboBox;
            if (comboBox != null)
            {
                comboBox.IsDropDownOpen = true;
                comboBox.Focus();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates the display control for the specified cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>
        /// The display control.
        /// </returns>
        private FrameworkElement CreateDisplayControl(CellRef cell)
        {
            var d = this.Operator.CreateCellDescriptor(cell);
            var cd = this.CellDefinitionFactory.CreateCellDefinition(d);
            var element = this.ControlFactory.CreateDisplayControl(cd);
            if (element == null)
            {
#if DEBUG
                throw new InvalidOperationException("Display control not implemented for " + cd);
#else
                return null;
#endif
            }

            element.DataContext = cd.BindingSource;
            element.SourceUpdated += (s, e) => this.CurrentCellSourceUpdated(cell);

            return element;
        }

        /// <summary>
        /// Creates the edit control for the specified cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>
        /// The edit control.
        /// </returns>
        private FrameworkElement CreateEditControl(CellRef cell)
        {
            var d = this.Operator.CreateCellDescriptor(cell);
            var cd = this.CellDefinitionFactory.CreateCellDefinition(d);
            var element = this.ControlFactory.CreateEditControl(cd);
            if (element == null)
            {
                return null;
            }

            element.DataContext = cd.BindingSource;
            element.SourceUpdated += (s, e) => this.CurrentCellSourceUpdated(cell);

            return element;
        }

        /// <summary>
        /// Shows the edit control for the current cell.
        /// </summary>
        private void ShowEditControl()
        {
            this.RemoveEditControl();
            var cell = this.CurrentCell;

            if (cell.Row >= this.Rows || cell.Column >= this.Columns)
            {
                return;
            }

            if (this.currentEditControl != null)
            {
                throw new InvalidOperationException();
            }

            this.editingCells = this.SelectedCells.ToList();

            this.currentEditControl = this.CreateEditControl(cell);
            if (this.currentEditControl == null)
            {
                return;
            }

            if (this.currentEditControl is TextBox textEditor)
            {
                this.currentEditControl.Visibility = Visibility.Hidden;
                textEditor.PreviewKeyDown += this.TextEditorPreviewKeyDown;
            }

            Grid.SetColumn(this.currentEditControl, this.CurrentCell.Column);
            Grid.SetRow(this.currentEditControl, this.CurrentCell.Row);

            this.sheetGrid.Children.Add(this.currentEditControl);

            if (this.currentEditControl.Visibility == Visibility.Visible)
            {
                this.currentEditControl.Focus();
            }
        }

        /// <summary>
        /// Updates the content of the specified cell.
        /// </summary>
        /// <param name="cellRef">The cell reference.</param>
        private void UpdateCellContent(CellRef cellRef)
        {
            var c = this.GetCellElement(cellRef);
            if (c != null)
            {
                this.sheetGrid.Children.Remove(c);
                this.cellInsertionIndex--;
                this.cellMap.Remove(cellRef.GetHashCode());
            }

            this.InsertDisplayControl(cellRef);
        }

        /// <summary>
        /// Handles changes in the current cell.
        /// </summary>
        /// <param name="changedCell">The cell that was changed.</param>
        private void CurrentCellSourceUpdated(CellRef changedCell)
        {
            if (this.editingCells == null)
            {
                return;
            }

            // The source of the binding for the current cell was updated
            // (e.g. check box (display control) was changed or a combo box (edit control) was changed
            var value = this.GetCellValue(changedCell);

            var selectedCells = this.editingCells.ToArray();
            if (!selectedCells.Contains(changedCell))
            {
                // do not set other cells when changed cell is outside selection
                return;
            }

            // Set the same value in all selected cells.
            foreach (var cell in selectedCells)
            {
                if (this.MultiChangeInChangedColumnOnly && cell.Column != changedCell.Column)
                {
                    // do not change value in other columns when this property is set to true
                    continue;
                }

                if (cell.Equals(changedCell))
                {
                    // the current cell should already be set
                    continue;
                }

                this.TrySetCellValue(cell, value);
            }
        }

        /// <summary>
        /// Inserts the display control for the specified cell.
        /// </summary>
        /// <param name="cellRef">The cell reference.</param>
        private void InsertDisplayControl(CellRef cellRef)
        {
            var e = this.CreateDisplayControl(cellRef);
            if (e == null)
            {
                return;
            }

            SetElementPosition(e, cellRef);

            if (this.cellInsertionIndex > this.sheetGrid.Children.Count)
            {
                throw new InvalidOperationException("Error in DataGrid");
            }

            this.sheetGrid.Children.Insert(this.cellInsertionIndex, e);
            this.cellInsertionIndex++;
            this.cellMap.Add(cellRef.GetHashCode(), e);
        }

        /// <summary>
        /// Handles mouse left button down events on the add item cell.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void AddItemCellMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            var actualIndex = this.InsertItem(-1);
            this.CollectionView?.Refresh();
            if (actualIndex != -1)
            {
                var viewIndex = this.Operator.GetCollectionViewIndex(actualIndex);

                var cell = this.ItemsInRows
                               ? new CellRef(viewIndex, 0)
                               : new CellRef(0, viewIndex);
                this.SelectionCell = cell;
                this.CurrentCell = cell;
                this.ScrollIntoView(cell);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles mouse down events on the auto fill box.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void AutoFillBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Show the auto-fill selection border
            this.autoFillSelection.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles mouse down events on the column grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ColumnGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            var column = this.GetCell(e.GetPosition(this.columnGrid)).Column;
            if (column >= 0)
            {
                var shift = (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None;
                this.SelectionCell = new CellRef(this.Rows - 1, column);
                this.CurrentCell = shift ? new CellRef(0, this.CurrentCell.Column) : new CellRef(0, column);
                this.ScrollIntoView(this.SelectionCell);
            }

            // LMB toggles the sort
            if (e.ChangedButton == MouseButton.Left && this.CanSort())
            {
                this.ToggleSort();
            }

            this.columnGrid.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// Handles mouse up events on the column grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ColumnGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.columnGrid.ReleaseMouseCapture();
        }

        /// <summary>
        /// Handles mouse move events on the column grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ColumnGridMouseMove(object sender, MouseEventArgs e)
        {
            if (!this.columnGrid.IsMouseCaptured)
            {
                return;
            }

            var column = this.GetCell(e.GetPosition(this.columnGrid)).Column;
            if (column >= 0)
            {
                this.SelectionCell = new CellRef(this.Rows - 1, column);

                // e.Handled = true;
            }
        }

        /// <summary>
        /// Handles changes in the column scroll viewer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ColumnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.sheetScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
        }

        /// <summary>
        /// Handles the column splitter change completed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="dragCompletedEventArgs">The drag completed event args.</param>
        private void ColumnSplitterChangeCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
        {
            var gs = (GridSplitter)sender;
            var tt = gs.ToolTip as ToolTip;
            if (tt != null)
            {
                tt.IsOpen = false;
                gs.ToolTip = null;
            }
        }

        /// <summary>
        /// Handles the column splitter change delta event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ColumnSplitterChangeDelta(object sender, DragDeltaEventArgs e)
        {
            var gs = (GridSplitter)sender;
            var tt = gs.ToolTip as ToolTip;

            if (tt == null)
            {
                tt = new ToolTip();
                gs.ToolTip = tt;
                tt.IsOpen = true;
            }

            var column = Grid.GetColumn(gs);
            var width = this.columnGrid.ColumnDefinitions[column].ActualWidth;
            tt.Content = $"Width: {width:0.#}"; // device-independent units

            tt.PlacementTarget = this.columnGrid;
            tt.Placement = PlacementMode.Relative;
            var p = Mouse.GetPosition(this.columnGrid);
            tt.HorizontalOffset = p.X;
            tt.VerticalOffset = gs.ActualHeight + 4;
        }

        /// <summary>
        /// The column splitter change started.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="dragStartedEventArgs">The drag started event args.</param>
        private void ColumnSplitterChangeStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
        {
            this.ColumnSplitterChangeDelta(sender, null);
        }

        /// <summary>
        /// The column splitter double click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ColumnSplitterDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                this.AutoSizeAllColumns();
            }

            var column = Grid.GetColumn((GridSplitter)sender);
            this.AutoSizeColumn(column);
        }

        /// <summary>
        /// Clears the content in the selected cells.
        /// </summary>
        private void Clear()
        {
            this.ClearOverride();
        }

        /// <summary>
        /// Deletes the selected columns.
        /// </summary>
        private void DeleteColumns()
        {
            var from = Math.Min(this.CurrentCell.Column, this.SelectionCell.Column);
            var to = Math.Max(this.CurrentCell.Column, this.SelectionCell.Column);
            this.Operator.DeleteColumns(from, to - from + 1);
            this.RefreshIfRequired();

            var maxColumn = this.Columns > 0 ? this.Columns - 1 : 0;
            if (this.CurrentCell.Column > maxColumn)
            {
                this.CurrentCell = new CellRef(maxColumn, this.CurrentCell.Column);
            }

            if (this.SelectionCell.Column > maxColumn)
            {
                this.SelectionCell = new CellRef(maxColumn, this.SelectionCell.Column);
            }

            this.ScrollIntoView(this.CurrentCell);
        }

        /// <summary>
        /// Deletes the selected rows.
        /// </summary>
        private void DeleteRows()
        {
            var from = Math.Min(this.CurrentCell.Row, this.SelectionCell.Row);
            var to = Math.Max(this.CurrentCell.Row, this.SelectionCell.Row);
            this.Operator.DeleteRows(from, to - from + 1);
            this.RefreshIfRequired();

            var maxRow = this.Rows > 0 ? this.Rows - 1 : 0;
            if (this.CurrentCell.Row > maxRow)
            {
                this.CurrentCell = new CellRef(maxRow, this.CurrentCell.Column);
            }

            if (this.SelectionCell.Row > maxRow)
            {
                this.SelectionCell = new CellRef(maxRow, this.SelectionCell.Column);
            }

            this.ScrollIntoView(this.CurrentCell);
        }

        /// <summary>
        /// Clears the sort descriptions.
        /// </summary>
        private void ClearSort()
        {
            this.sortDescriptions.Clear();
            this.UpdateCollectionView();
        }

        /// <summary>
        /// Sorts the selected columns/rows in the specified direction.
        /// </summary>
        /// <param name="direction">The sort direction.</param>
        /// <param name="append">Append the sort description if set to <c>true</c>.</param>
        private void Sort(ListSortDirection direction, bool append = false)
        {
            var index = this.ItemsInRows ? this.CurrentCell.Column : this.CurrentCell.Row;
            var propertyName = this.PropertyDefinitions[index].PropertyName;

            if (!append)
            {
                this.sortDescriptions.Clear();
            }

            try
            {
                this.sortDescriptions.Add(new SortDescription(propertyName, direction));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            this.UpdateCollectionView();
        }

        /// <summary>
        /// Updates the sort descriptions of the collection view and the visual markers.
        /// </summary>
        private void UpdateCollectionView()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                Debug.WriteLine("Updating collection view on non-dispatcher thread.");
            }

            var cv = this.CollectionView;
            if (cv == null)
            {
                return;
            }

            if (cv is ListCollectionView lcv)
            {
                lcv.CustomSort = this.CustomSort;
            }

            var sortDescriptionCollection = cv.SortDescriptions;
            if (this.CustomSort is ISortDescriptionComparer sdc)
            {
                sortDescriptionCollection = sdc.SortDescriptions;
                cv.SortDescriptions.Clear();
            }

            sortDescriptionCollection.Clear();
            foreach (var sd in this.sortDescriptions)
            {
                sortDescriptionCollection.Add(sd);
            }

            cv.Refresh();
            this.UpdateSortDescriptionMarkers();
        }

        /// <summary>
        /// Determines whether the current column/row can be sorted.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can sort; otherwise, <c>false</c>.
        /// </returns>
        private bool CanSort()
        {
            var index = this.ItemsInRows ? this.CurrentCell.Column : this.CurrentCell.Row;
            return this.CollectionView != null && this.Operator.CanSort(index);
        }

        /// <summary>
        /// Toggles the sort direction for the current column/row.
        /// </summary>
        /// <param name="append">Append the sort description if set to <c>true</c>.</param>
        private void ToggleSort(bool append = false)
        {
            var propertyDefinition = this.GetPropertyDefinition(this.CurrentCell);
            if (!propertyDefinition.CanSort)
            {
                return;
            }

            var propertyName = propertyDefinition.PropertyName;
            //var descriptor = this.Operator.GetPropertyDescriptor(propertyDefinition);
            //var isComparable = descriptor != null && typeof(IComparable).IsAssignableFrom(descriptor.PropertyType);
            //if (!isComparable)
            //{
            //    return;
            //}

            SortDescription? current = null;
            if (this.sortDescriptions.Any(s => s.PropertyName == propertyName))
            {
                current = this.sortDescriptions.First(s => s.PropertyName == propertyName);
            }

            if (!append)
            {
                this.sortDescriptions.Clear();
            }

            try
            {
                if (!current.HasValue)
                {
                    this.sortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Ascending));
                }
                else
                {
                    if (current.Value.Direction == ListSortDirection.Ascending)
                    {
                        this.sortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Descending));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            this.UpdateCollectionView();
        }

        /// <summary>
        /// Enumerate the items in the specified cell range. This is used to update the SelectedItems property.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns>
        /// The items enumeration.
        /// </returns>
        private IEnumerable<object> EnumerateItems(CellRange range)
        {
            var op = this.Operator;
            if (op == null)
            {
                yield break;
            }

            var min = this.ItemsInRows ? range.TopRow : range.LeftColumn;
            var max = this.ItemsInRows ? range.BottomRow : range.RightColumn;
            for (var index = min; index <= max; index++)
            {
                var cell = this.ItemsInRows ? new CellRef(index, range.LeftColumn) : new CellRef(range.TopRow, index);
                var item = op.GetItem(cell);
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Finds the next column that contains an empty cell.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="deltaColumn">The delta column.</param>
        /// <returns>
        /// The new column.
        /// </returns>
        private int FindNextColumn(int row, int column, int deltaColumn)
        {
            while (column >= 0 && column < this.Columns - 1)
            {
                var v = this.GetCellValue(new CellRef(row, column));
                if (string.IsNullOrEmpty(v?.ToString()))
                {
                    break;
                }

                column += deltaColumn;
            }

            return column;
        }

        /// <summary>
        /// Finds the next row that contains an empty cell.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="deltaRow">The delta row.</param>
        /// <returns>
        /// The new row.
        /// </returns>
        private int FindNextRow(int row, int column, int deltaRow)
        {
            while (row >= 0 && row < this.Rows)
            {
                var v = this.GetCellValue(new CellRef(row, column));
                if (string.IsNullOrEmpty(v?.ToString()))
                {
                    break;
                }

                row += deltaRow;
            }

            if (row >= this.Rows && this.Rows > 0)
            {
                row = this.Rows - 1;
            }

            return row;
        }

        /// <summary>
        /// Gets the column element for the specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>
        /// The column element.
        /// </returns>
        private FrameworkElement GetColumnElement(int column)
        {
            if (this.columnHeaderMap.TryGetValue(column, out var headerElement))
            {
                return headerElement;
            }

            throw new InvalidOperationException("Invalid header column: " + column);
        }

        /// <summary>
        /// Gets the row element for the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// The row element.
        /// </returns>
        private FrameworkElement GetRowElement(int row)
        {
            if (this.rowHeaderMap.TryGetValue(row, out var headerElement))
            {
                return headerElement;
            }

            throw new InvalidOperationException("Invalid header row: " + row);
        }

        /// <summary>
        /// Gets the column header for the specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>
        /// The column header.
        /// </returns>
        private object GetColumnHeader(int column)
        {
            if (this.ItemsInRows)
            {
                if (column < this.PropertyDefinitions.Count)
                {
                    return this.PropertyDefinitions[column].Header ?? CellRef.ToColumnName(column);
                }
            }

            return CellRef.ToColumnName(column);
        }

        /// <summary>
        /// Gets the column width for the specified column.
        /// </summary>
        /// <param name="i">The column index.</param>
        /// <returns>
        /// The column width.
        /// </returns>
        private GridLength GetColumnWidth(int i)
        {
            if (i < this.ColumnDefinitions.Count)
            {
                var cd = this.ColumnDefinitions[i] as ColumnDefinition;
                if (cd != null)
                {
                    if (cd.Width.Value < 0)
                    {
                        return this.DefaultColumnWidth;
                    }

                    return cd.Width;
                }
            }

            return this.DefaultColumnWidth;
        }

        /// <summary>
        /// Gets the format string for the specified cell.
        /// </summary>
        /// <param name="cell">The cell reference.</param>
        /// <returns>
        /// The format string.
        /// </returns>
        private string GetFormatString(CellRef cell)
        {
            var pd = this.GetPropertyDefinition(cell);
            return pd?.FormatString;
        }

        /// <summary>
        /// Gets the row header.
        /// </summary>
        /// <param name="j">The j.</param>
        /// <returns>
        /// The get row header.
        /// </returns>
        private object GetRowHeader(int j)
        {
            if (this.ItemsInColumns)
            {
                return j < this.RowDefinitions.Count ? this.RowDefinitions[j].Header : CellRef.ToRowName(j);
            }

            return CellRef.ToRowName(j);
        }

        /// <summary>
        /// Gets the row height for the specified row.
        /// </summary>
        /// <param name="i">The row index.</param>
        /// <returns>
        /// The row height.
        /// </returns>
        private GridLength GetRowHeight(int i)
        {
            if (i < this.RowDefinitions.Count)
            {
                var cd = this.RowDefinitions[i] as RowDefinition;
                if (cd != null)
                {
                    if (cd.Height.Value < 0)
                    {
                        return this.DefaultRowHeight;
                    }

                    return cd.Height;
                }
            }

            return this.DefaultRowHeight;
        }

        /// <summary>
        /// Inserts columns at the selected column.
        /// </summary>
        private void InsertColumns()
        {
            var from = Math.Min(this.CurrentCell.Column, this.SelectionCell.Column);
            var to = Math.Max(this.CurrentCell.Column, this.SelectionCell.Column);

            this.Operator.InsertColumns(from, to - from + 1);
            this.RefreshIfRequired();
        }

        /// <summary>
        /// Inserts rows at the selection.
        /// </summary>
        private void InsertRows()
        {
            var from = Math.Min(this.CurrentCell.Row, this.SelectionCell.Row);
            var to = Math.Max(this.CurrentCell.Row, this.SelectionCell.Row);
            this.Operator.InsertRows(from, to - from + 1);
            this.RefreshIfRequired();
        }

        /// <summary>
        /// Refreshes the collection view and updates the grid content, if the ItemsSource is not implementing INotifyCollectionChanged.
        /// </summary>
        private void RefreshIfRequired()
        {
            if (!(this.ItemsSource is INotifyCollectionChanged))
            {
                this.CollectionView?.Refresh();
            }
        }

        /// <summary>
        /// Handles changes to the items collection.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        private void OnItemsCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (this.suspendCollectionChangedNotifications)
            {
                return;
            }

            this.Dispatcher.Invoke(this.UpdateGridContent);
        }

        /// <summary>
        /// Handles mouse down events on the row grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void RowGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();

            var row = this.GetCell(e.GetPosition(this.rowGrid)).Row;
            var isRightButton = e.ChangedButton == MouseButton.Right;
            var selectionRange = this.GetSelectionRange();
            if (isRightButton && selectionRange.TopRow <= row && selectionRange.BottomRow >= row)
            {
                // do nothing, just show the context menu
                return;
            }

            if (row >= 0)
            {
                var shift = (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None;
                this.SelectionCell = new CellRef(row, this.Columns - 1);
                this.CurrentCell = shift ? new CellRef(this.CurrentCell.Row, 0) : new CellRef(row, 0);
                this.ScrollIntoView(this.SelectionCell);
            }

            this.rowGrid.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// Handles mouse up events on the row grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void RowGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.rowGrid.ReleaseMouseCapture();
        }

        /// <summary>
        /// Handles mouse move events on the row grid.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void RowGridMouseMove(object sender, MouseEventArgs e)
        {
            if (!this.rowGrid.IsMouseCaptured)
            {
                return;
            }

            var row = this.GetCell(e.GetPosition(this.rowGrid)).Row;
            if (row >= 0)
            {
                this.SelectionCell = new CellRef(row, this.Columns - 1);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles changes in the row scroll viewer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void RowScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.sheetScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        }

        /// <summary>
        /// Handles the row splitter change completed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="dragCompletedEventArgs">The drag completed event args.</param>
        private void RowSplitterChangeCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
        {
            var gs = (GridSplitter)sender;
            var tt = gs.ToolTip as ToolTip;
            if (tt != null)
            {
                tt.IsOpen = false;
                gs.ToolTip = null;
            }
        }

        /// <summary>
        /// Handles the row splitter change delta event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void RowSplitterChangeDelta(object sender, DragDeltaEventArgs e)
        {
            var gs = (GridSplitter)sender;
            var tt = gs.ToolTip as ToolTip;

            if (tt == null)
            {
                tt = new ToolTip();
                gs.ToolTip = tt;
                tt.IsOpen = true;
            }

            var row = Grid.GetRow(gs);
            var height = this.rowGrid.RowDefinitions[row].ActualHeight;
            tt.Content = $"Height: {height:0.#}"; // device-independent units

            tt.PlacementTarget = this.rowGrid;
            tt.Placement = PlacementMode.Relative;
            var p = Mouse.GetPosition(this.rowGrid);
            tt.HorizontalOffset = gs.ActualWidth + 4;
            tt.VerticalOffset = p.Y;
        }

        /// <summary>
        /// The row splitter change started.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="dragStartedEventArgs">The drag started event args.</param>
        private void RowSplitterChangeStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
        {
            this.RowSplitterChangeDelta(sender, null);
        }

        /// <summary>
        /// The row splitter double click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void RowSplitterDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                this.AutoSizeAllRows();
            }

            var row = Grid.GetRow((GridSplitter)sender);
            this.AutoSizeRow(row);
        }

        /// <summary>
        /// Handles scroll changes in the scroll viewers (both horizontal and vertical).
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.columnScrollViewer.ScrollToHorizontalOffset(this.sheetScrollViewer.HorizontalOffset);
            this.rowScrollViewer.ScrollToVerticalOffset(this.sheetScrollViewer.VerticalOffset);
        }

        /// <summary>
        /// Selects all cells.
        /// </summary>
        private void SelectAll()
        {
            this.SelectionCell = new CellRef(this.Rows - 1, this.Columns - 1);
            this.CurrentCell = new CellRef(0, 0);
            this.ScrollIntoView(this.CurrentCell);
        }

        /// <summary>
        /// Sets the boolean value in the selected cells.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// True if cells were modified.
        /// </returns>
        private bool SetCheckInSelectedCells(bool value)
        {
            var modified = false;
            foreach (var cell in this.SelectedCells)
            {
                var currentValue = this.GetCellValue(cell);
                if (currentValue is bool)
                {
                    if (this.TrySetCellValue(cell, value))
                    {
                        modified = true;
                    }
                }
            }

            return modified;
        }

        /// <summary>
        /// Handles mouse down events on the grid sheet.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SheetGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.ShowTextBoxEditControl();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles key down events in the TextBox editor.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TextEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textEditor = sender as TextBox;
            if (textEditor == null)
            {
                return;
            }

            var isEverythingSelected = textEditor.SelectionLength == textEditor.Text.Length && textEditor.SelectionLength > 0;

            switch (e.Key)
            {
                case Key.Left:
                    if (textEditor.CaretIndex == 0 && !isEverythingSelected)
                    {
                        this.RemoveEditControl();
                        this.OnKeyDown(e);
                        e.Handled = true;
                    }

                    break;
                case Key.Right:
                    if (textEditor.CaretIndex == textEditor.Text.Length && !isEverythingSelected)
                    {
                        this.RemoveEditControl();
                        this.OnKeyDown(e);
                        e.Handled = true;
                    }

                    break;
                case Key.Down:
                case Key.Up:
                    this.RemoveEditControl();
                    this.OnKeyDown(e);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    this.RemoveEditControl();
                    this.OnKeyDown(e);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    BindingOperations.ClearBinding(this.currentEditControl, TextBox.TextProperty);
                    this.RemoveEditControl();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Converts the specified array to a csv string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="encode">Determines whether to encode the elements.</param>
        /// <returns>
        /// The to string.
        /// </returns>
        private string ConvertToCsv(string[,] input, string separator, bool encode = false)
        {
            var m = input.GetLength(0);
            var n = input.GetLength(1);

            var sb = new StringBuilder();

            for (var i = 0; i < m; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine();
                }

                for (var j = 0; j < n; j++)
                {
                    var cell = input[i, j];
                    if (encode)
                    {
                        cell = CsvEncodeString(cell);
                    }

                    if (j > 0)
                    {
                        sb.Append(separator);
                    }

                    if (cell != null)
                    {
                        sb.Append(cell);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Toggles the check in the current cell.
        /// </summary>
        /// <returns>
        /// True if the cell was modified.
        /// </returns>
        private bool ToggleCheck()
        {
            var element = this.GetCellElement(this.CurrentCell);
            if (element == null || !element.IsEnabled)
            {
                return false;
            }

            var value = true;
            var cellValue = this.GetCellValue(this.CurrentCell);
            if (cellValue is bool)
            {
                value = (bool)cellValue;
                value = !value;
            }

            return this.SetCheckInSelectedCells(value);
        }

        /// <summary>
        /// Handles mouse down events on the top/left selection control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TopLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            this.SelectAll();
            e.Handled = true;
        }

        /// <summary>
        /// Sets the width of the specified column.
        /// </summary>
        /// <param name="column">The column to change.</param>
        /// <param name="newWidth">The new width.</param>
        private void SetColumnWidth(int column, GridLength newWidth)
        {
            this.sheetGrid.ColumnDefinitions[column].Width = newWidth;
        }

        /// <summary>
        /// Handles automatic insert of items.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns><c>true</c> if an item was inserted; otherwise <c>false</c>.</returns>
        private bool HandleAutoInsert(CellRef cell)
        {
            if (!this.AutoInsert || !this.CanInsert)
            {
                return false;
            }

            if (cell.Row >= this.Rows || cell.Column >= this.Columns)
            {
                var actualCell = cell;
                if (cell.Row >= this.Rows)
                {
                    var actualIndex = this.Rows;
                    this.Operator.InsertRows(actualIndex, 1);
                    this.CollectionView?.Refresh();
                    actualIndex = this.Operator.GetCollectionViewIndex(actualIndex);
                    actualCell = new CellRef(actualIndex, cell.Column);
                }

                if (cell.Column >= this.Columns)
                {
                    var actualIndex = this.Columns;
                    this.Operator.InsertColumns(actualIndex, 1);
                    this.CollectionView?.Refresh();
                    actualIndex = this.Operator.GetCollectionViewIndex(actualIndex);
                    actualCell = new CellRef(cell.Row, actualIndex);
                }

                this.SelectionCell = actualCell;
                this.CurrentCell = actualCell;
                this.ScrollIntoView(actualCell);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles change in current cell.
        /// </summary>
        private void CurrentCellChanged()
        {
            this.SelectedCellsChanged();
        }

        /// <summary>
        /// Handles change in selected cells.
        /// </summary>
        private void SelectedCellsChanged()
        {
            if (this.selection == null)
            {
                return;
            }

            var row = Math.Min(this.CurrentCell.Row, this.SelectionCell.Row);
            var column = Math.Min(this.CurrentCell.Column, this.SelectionCell.Column);
            var rowspan = Math.Abs(this.CurrentCell.Row - this.SelectionCell.Row) + 1;
            var columnspan = Math.Abs(this.CurrentCell.Column - this.SelectionCell.Column) + 1;

            Grid.SetRow(this.selection, row);
            Grid.SetColumn(this.selection, column);
            Grid.SetColumnSpan(this.selection, columnspan);
            Grid.SetRowSpan(this.selection, rowspan);

            Grid.SetRow(this.selectionBackground, row);
            Grid.SetColumn(this.selectionBackground, column);
            Grid.SetColumnSpan(this.selectionBackground, columnspan);
            Grid.SetRowSpan(this.selectionBackground, rowspan);

            Grid.SetColumn(this.columnSelectionBackground, column);
            Grid.SetColumnSpan(this.columnSelectionBackground, columnspan);
            Grid.SetRow(this.rowSelectionBackground, row);
            Grid.SetRowSpan(this.rowSelectionBackground, rowspan);

            Grid.SetRow(this.currentBackground, this.CurrentCell.Row);
            Grid.SetColumn(this.currentBackground, this.CurrentCell.Column);

            this.UpdateSelectionVisibility();

            Grid.SetColumn(this.autoFillBox, column + columnspan - 1);
            Grid.SetRow(this.autoFillBox, row + rowspan - 1);

            var r = Math.Min(this.CurrentCell.Row, this.AutoFillCell.Row);
            var c = Math.Min(this.CurrentCell.Column, this.AutoFillCell.Column);
            var rs = Math.Abs(this.CurrentCell.Row - this.AutoFillCell.Row) + 1;
            var cs = Math.Abs(this.CurrentCell.Column - this.AutoFillCell.Column) + 1;

            Grid.SetColumn(this.autoFillSelection, c);
            Grid.SetRow(this.autoFillSelection, r);
            Grid.SetColumnSpan(this.autoFillSelection, cs);
            Grid.SetRowSpan(this.autoFillSelection, rs);

            this.SelectedItems = this.EnumerateItems(this.GetSelectionRange()).ToArray();

            this.ShowEditControl();
        }

        /// <summary>
        /// Updates the selection visibility.
        /// </summary>
        private void UpdateSelectionVisibility()
        {
            var isEnabled = this.IsEnabled;
            if (this.currentBackground != null)
            {
                this.currentBackground.Visibility =
                    isEnabled && this.CurrentCell.Row < this.Rows && this.CurrentCell.Column < this.Columns
                        ? Visibility.Visible
                        : Visibility.Hidden;
            }

            if (this.autoFillBox != null)
            {
                this.autoFillBox.Visibility =
                    isEnabled && this.IsAutoFillEnabled && this.CurrentCell.Row < this.Rows
                    && this.CurrentCell.Column < this.Columns
                        ? Visibility.Visible
                        : Visibility.Hidden;
            }

            if (this.selectionBackground != null)
            {
                this.selectionBackground.Visibility =
                    isEnabled && this.CurrentCell.Row < this.Rows && this.CurrentCell.Column < this.Columns
                        ? Visibility.Visible
                        : Visibility.Hidden;
            }

            if (this.columnSelectionBackground != null)
            {
                this.columnSelectionBackground.Visibility = isEnabled && this.CurrentCell.Column < this.Columns
                                                                ? Visibility.Visible
                                                                : Visibility.Hidden;
            }

            if (this.rowSelectionBackground != null)
            {
                this.rowSelectionBackground.Visibility =
                    isEnabled && this.CurrentCell.Row < this.Rows ? Visibility.Visible : Visibility.Hidden;
            }

            if (this.selection != null)
            {
                this.selection.Visibility =
                    isEnabled && this.CurrentCell.Row < this.Rows && this.CurrentCell.Column < this.Columns
                        ? Visibility.Visible
                        : Visibility.Hidden;
            }

            if (this.topLeft != null && this.rowSelectionBackground != null && this.rowGrid != null)
            {
                var rowspan = Math.Abs(this.CurrentCell.Row - this.SelectionCell.Row) + 1;
                var columnspan = Math.Abs(this.CurrentCell.Column - this.SelectionCell.Column) + 1;
                var allSelected = rowspan == this.Rows && columnspan == this.Columns;
                this.topLeft.Background = isEnabled && allSelected
                                              ? this.rowSelectionBackground.Background
                                              : this.rowGrid.Background;
            }
        }

        /// <summary>
        /// Handles changes in the <see cref="P:SelectionCell" /> property.
        /// </summary>
        private void SelectionCellChanged()
        {
            this.SelectedCellsChanged();
            this.ScrollIntoView(this.SelectionCell);
        }

        /// <summary>
        /// Clears the content of the control.
        /// </summary>
        private void ClearContent()
        {
            this.rowGrid.RowDefinitions.Clear();
            this.rowGrid.ColumnDefinitions.Clear();
            this.rowGrid.Children.Clear();
            this.columnGrid.RowDefinitions.Clear();
            this.columnGrid.ColumnDefinitions.Clear();
            this.columnGrid.Children.Clear();
            this.columnHeaderMap.Clear();
            this.sheetGrid.RowDefinitions.Clear();
            this.sheetGrid.ColumnDefinitions.Clear();
            this.sheetGrid.Children.Clear();
            this.cellInsertionIndex = 0;
            this.cellMap.Clear();
        }

        /// <summary>
        /// Updates the cells of the grid.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <param name="columns">The number of columns.</param>
        private void UpdateCells(int rows, int columns)
        {
            // set the context menu
            this.sheetGrid.ContextMenu = this.SheetContextMenu;

            this.sheetGrid.Children.Add(this.selectionBackground);
            this.sheetGrid.Children.Add(this.currentBackground);

            // Add row lines to the sheet
            for (var i = 1; i <= rows; i++)
            {
                var border = new Border
                {
                    BorderBrush = this.GridLineBrush,
                    BorderThickness = new Thickness(0, 1, 0, 0)
                };

                if (i < rows && this.AlternatingRowsBackground != null && i % 2 == 1)
                {
                    border.Background = this.AlternatingRowsBackground;
                }

                Grid.SetColumn(border, 0);
                if (columns > 0)
                {
                    Grid.SetColumnSpan(border, columns);
                }

                Grid.SetRow(border, i);
                this.sheetGrid.Children.Add(border);
            }

            if (rows > 0)
            {
                // Add column lines to the sheet
                for (var i = 0; i < columns; i++)
                {
                    if (i == 0 && columns > 1)
                    {
                        continue;
                    }

                    var border = new Border
                    {
                        BorderBrush = this.GridLineBrush,
                        BorderThickness = new Thickness(i > 0 ? 1 : 0, 0, i == columns - 1 ? 1 : 0, 0)
                    };

                    Grid.SetRow(border, 0);
                    Grid.SetRowSpan(border, rows);
                    Grid.SetColumn(border, i);
                    this.sheetGrid.Children.Add(border);
                }
            }

            this.cellInsertionIndex = this.sheetGrid.Children.Count;

            // Add all cells to the sheet
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    this.InsertDisplayControl(new CellRef(i, j));
                }
            }

            this.sheetGrid.Children.Add(this.selection);
            this.sheetGrid.Children.Add(this.autoFillBox);
            this.sheetGrid.Children.Add(this.autoFillSelection);
        }

        /// <summary>
        /// Updates the column widths and row heights.
        /// </summary>
        private void UpdateGridSize()
        {
            if (!this.IsLoaded)
            {
                return;
            }

            this.sheetGrid.UpdateLayout();
            this.columnGrid.UpdateLayout();
            this.rowGrid.UpdateLayout();

            for (var i = 0; i < this.Rows; i++)
            {
                if (this.DefaultRowHeight == GridLength.Auto || this.GetRowHeight(i) == GridLength.Auto)
                {
                    this.AutoSizeRow(i);
                }
            }

            var starsToDistribute = 0d;
            var usedWidth = 0d;
            for (var i = 0; i < this.Columns; i++)
            {
                var columnWidth = this.GetColumnWidth(i);
                if (columnWidth == GridLength.Auto)
                {
                    usedWidth += this.AutoSizeColumn(i);
                }
                else if (columnWidth.IsAbsolute)
                {
                    usedWidth += columnWidth.Value;
                }
                else if (columnWidth.IsStar)
                {
                    starsToDistribute += columnWidth.Value;
                }
            }

            var availableWidth = this.sheetScrollViewer.ViewportWidth;

            var widthPerStar = Math.Max((availableWidth - usedWidth) / starsToDistribute, 0);
            for (var i = 0; i < this.Columns; i++)
            {
                var columnWidth = this.GetColumnWidth(i);
                if (columnWidth.IsStar)
                {
                    this.SetColumnWidth(i, new GridLength(widthPerStar * columnWidth.Value));
                }
            }

            this.sheetGrid.UpdateLayout();

            for (var j = 0; j < this.sheetGrid.ColumnDefinitions.Count; j++)
            {
                this.columnGrid.ColumnDefinitions[j].Width =
                    new GridLength(this.sheetGrid.ColumnDefinitions[j].ActualWidth);
            }
        }

        /// <summary>
        /// Updates the sort description markers.
        /// </summary>
        private void UpdateSortDescriptionMarkers()
        {
            foreach (var sdm in this.sortDescriptionMarkers)
            {
                this.columnGrid.Children.Remove(sdm);
            }

            this.sortDescriptionMarkers.Clear();

            if (!this.ItemsInRows)
            {
                return;
            }

            foreach (var sd in this.sortDescriptions)
            {
                var index = -1;
                for (var i = 0; i < this.PropertyDefinitions.Count; i++)
                {
                    if (this.PropertyDefinitions[i].PropertyName == sd.PropertyName)
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    continue;
                }

                var tb = new TextBlock
                {
                    Text = sd.Direction == ListSortDirection.Ascending ? "▼" : "▲",
                    Foreground = Brushes.DarkGray,
                    Margin = new Thickness(0, 0, 4, 0),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(tb, index);
                this.columnGrid.Children.Add(tb);
                this.sortDescriptionMarkers.Add(tb);
            }
        }

        /// <summary>
        /// Updates the specified columns.
        /// </summary>
        /// <param name="columns">The column index.</param>
        private void UpdateColumns(int columns)
        {
            for (var i = 0; i < columns; i++)
            {
                var w = this.GetColumnWidth(i);
                var cd1 = new System.Windows.Controls.ColumnDefinition { Width = w };
                this.sheetGrid.ColumnDefinitions.Add(cd1);

                // bind the width of the header column to the width of the sheet column
                var cd2 = new System.Windows.Controls.ColumnDefinition();
                this.columnGrid.ColumnDefinitions.Add(cd2);
                var binding = new Binding("Width") { Source = cd1, Mode = BindingMode.TwoWay };
                cd2.SetBinding(System.Windows.Controls.ColumnDefinition.WidthProperty, binding);
            }

            // Add one empty column covering the vertical scrollbar
            var scrollBarColumnDefinition = new System.Windows.Controls.ColumnDefinition();
            this.columnGrid.ColumnDefinitions.Add(scrollBarColumnDefinition);

            // bind the width of the column definition to the visibility of the main horizontal scroll bar 
            scrollBarColumnDefinition.SetBinding(
                System.Windows.Controls.ColumnDefinition.WidthProperty,
                new Binding(nameof(ScrollViewer.ComputedVerticalScrollBarVisibility))
                {
                    Source = this.sheetScrollViewer,
                    Converter = VerticalScrollBarVisibilityConverter,
                    ConverterParameter = SystemParameters.VerticalScrollBarWidth
                });

            // set the context menu
            this.columnGrid.ContextMenu = this.ColumnsContextMenu;

            this.columnGrid.Children.Add(this.columnSelectionBackground);
            for (var j = 0; j < columns; j++)
            {
                var header = this.GetColumnHeader(j);
                var cellref = new CellRef(this.ItemsInRows ? -1 : j, this.ItemsInRows ? j : -1);
                var pd = this.GetPropertyDefinition(cellref);

                var border = new Border
                {
                    BorderBrush = this.HeaderBorderBrush,
                    BorderThickness = new Thickness(0, 1, 1, 1),
                    Margin = new Thickness(0, 0, j < columns - 1 ? -1 : 0, 0)
                };
                Grid.SetColumn(border, j);
                this.columnGrid.Children.Add(border);

                var cell = header as FrameworkElement
                           ?? new TextBlock
                           {
                               Text = header?.ToString() ?? "-",
                               VerticalAlignment = VerticalAlignment.Center,
                               HorizontalAlignment = this.ItemsInRows ? pd.HorizontalAlignment : System.Windows.HorizontalAlignment.Center,
                               Padding = new Thickness(4, 2, 4, 2)
                           };

                if (pd?.Tooltip != null)
                {
                    ToolTipService.SetToolTip(cell, pd.Tooltip);
                }

                if (this.ColumnHeadersSource != null && this.ItemsInRows)
                {
                    cell.DataContext = this.ColumnHeadersSource;
                    cell.SetBinding(TextBlock.TextProperty, new Binding($"[{j}]") { StringFormat = this.ColumnHeadersFormatString });
                }

                Grid.SetColumn(cell, j);
                this.columnGrid.Children.Add(cell);
                this.columnHeaderMap[j] = cell;
            }

            for (var j = 0; j < columns; j++)
            {
                if (this.CanResizeColumns)
                {
                    var splitter = new GridSplitter
                    {
                        ResizeDirection = GridResizeDirection.Columns,
                        Background = Brushes.Transparent,
                        Width = 5,
                        RenderTransform = new TranslateTransform(3, 0),
                        Focusable = false,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right
                    };
                    splitter.MouseDoubleClick += this.ColumnSplitterDoubleClick;
                    splitter.DragStarted += this.ColumnSplitterChangeStarted;
                    splitter.DragDelta += this.ColumnSplitterChangeDelta;
                    splitter.DragCompleted += this.ColumnSplitterChangeCompleted;
                    Grid.SetColumn(splitter, j);
                    this.columnGrid.Children.Add(splitter);
                }
            }
        }

        /// <summary>
        /// Handles changes in the <see cref="ItemsSource" /> property.
        /// </summary>
        private void ItemsSourceChanged()
        {
            if (this.synchronizedCollection != null)
            {
                BindingOperations.DisableCollectionSynchronization(this.synchronizedCollection);
                this.synchronizedCollection = null;
            }

            if (this.subscribedCollection != null)
            {
                CollectionChangedEventManager.RemoveListener(this.subscribedCollection, this);

                this.subscribedCollection = null;
            }

            if (this.ItemsSource != null)
            {
                this.synchronizedCollection = this.ItemsSource;

                // Enables the collection to be accessed across multiple threads
                BindingOperations.EnableCollectionSynchronization(this.synchronizedCollection, this);

                var source = new CollectionViewSource { Source = this.ItemsSource };
                this.CollectionView = source.View;
                this.UpdateCollectionView();
            }
            else
            {
                this.CollectionView = null;
            }

            this.UpdateGridContent();

            if (this.CollectionView != null)
            {
                CollectionChangedEventManager.AddListener(this.CollectionView, this);
                this.subscribedCollection = this.CollectionView;
            }
        }

        /// <summary>
        /// Updates all the UIElements of the grid (both cells, headers, row and column lines).
        /// </summary>
        private void UpdateGridContent()
        {
            if (this.sheetGrid == null)
            {
                // return if the template has not yet been applied
                return;
            }

            this.ClearContent();

            if (this.ItemsSource == null)
            {
                return;
            }

            this.Operator = this.CreateOperator();

            if (this.AutoGenerateColumns && this.ColumnDefinitions.Count == 0)
            {
                this.Operator.AutoGenerateColumns();
            }

            this.Operator.UpdatePropertyDefinitions();

            // Determine if columns or rows are defined
            this.ItemsInColumns = this.PropertyDefinitions.FirstOrDefault(pd => pd is RowDefinition) != null;

            var rows = this.Operator.GetRowCount();
            var columns = this.Operator.GetColumnCount();

            var visibility = rows >= 0 ? Visibility.Visible : Visibility.Hidden;

            // Hide the row/column headers if the content is empty
            this.rowScrollViewer.Visibility =
                this.columnScrollViewer.Visibility = this.sheetScrollViewer.Visibility = this.topLeft.Visibility = visibility;

            if (rows < 0)
            {
                return;
            }

            this.UpdateRows(rows);
            this.UpdateColumns(columns);
            this.UpdateCells(rows, columns);
            this.UpdateSortDescriptionMarkers();

            this.UpdateSelectionVisibility();
            this.ShowEditControl();

            // Update column width when all the controls are loaded.
            this.Dispatcher.BeginInvoke(new Action(this.UpdateGridSize), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Updates the rows.
        /// </summary>
        /// <param name="rows">The number rows.</param>
        private void UpdateRows(int rows)
        {
            this.rowGrid.Children.Add(this.rowSelectionBackground);

            for (var i = 0; i < rows; i++)
            {
                var sheetDefinition = new System.Windows.Controls.RowDefinition { Height = this.GetRowHeight(i) };
                this.sheetGrid.RowDefinitions.Add(sheetDefinition);

                var rowDefinition = new System.Windows.Controls.RowDefinition();
                rowDefinition.SetBinding(System.Windows.Controls.RowDefinition.HeightProperty, new Binding { Source = sheetDefinition, Path = new PropertyPath("Height"), Mode = BindingMode.TwoWay });
                this.rowGrid.RowDefinitions.Add(rowDefinition);
            }

            for (var i = 0; i < rows; i++)
            {
                var header = this.GetRowHeader(i);

                var border = new Border
                {
                    BorderBrush = this.HeaderBorderBrush,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Margin = new Thickness(0, 0, 0, -1)
                };

                Grid.SetRow(border, i);
                this.rowGrid.Children.Add(border);

                var cell = header as FrameworkElement
                           ??
                           new TextBlock
                           {
                               Text = header?.ToString() ?? "-",
                               VerticalAlignment = VerticalAlignment.Center,
                               HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                               Padding = new Thickness(4, 2, 4, 2)
                           };

                if (this.ItemHeaderPropertyPath != null && this.ItemsInRows)
                {
                    cell.DataContext = this.Operator.GetItem(new CellRef(i, -1));
                    cell.SetBinding(TextBlock.TextProperty, new Binding(this.ItemHeaderPropertyPath));
                }

                if (this.RowHeadersSource != null && this.ItemsInRows)
                {
                    cell.DataContext = this.RowHeadersSource;
                    cell.SetBinding(
                        TextBlock.TextProperty,
                        new Binding($"[{i}]") { StringFormat = this.RowHeadersFormatString });
                }

                Grid.SetRow(cell, i);
                this.rowGrid.Children.Add(cell);
                this.rowHeaderMap[i] = cell;
            }

            // Add "Insert" row header
            this.AddInserterRow(rows);

            // set the context menu
            this.rowGrid.ContextMenu = this.RowsContextMenu;

            // add a row definition to cover a possible scrollbar
            var scrollBarRowDefinition = new System.Windows.Controls.RowDefinition();
            this.rowGrid.RowDefinitions.Add(scrollBarRowDefinition);

            // bind the height of the row definition to the visibility of the main horizontal scroll bar 
            scrollBarRowDefinition.SetBinding(
                System.Windows.Controls.RowDefinition.HeightProperty,
                new Binding(nameof(ScrollViewer.ComputedHorizontalScrollBarVisibility))
                {
                    Source = this.sheetScrollViewer,
                    Converter = HorizontalScrollBarVisibilityConverter,
                    ConverterParameter = SystemParameters.HorizontalScrollBarHeight
                });

            for (var j = 0; j < rows; j++)
            {
                if (this.CanResizeRows)
                {
                    var splitter = new GridSplitter
                    {
                        ResizeDirection = GridResizeDirection.Rows,
                        Background = Brushes.Transparent,
                        Height = 5,
                        RenderTransform = new TranslateTransform(0, 3),
                        Focusable = false,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
                    };
                    splitter.MouseDoubleClick += this.RowSplitterDoubleClick;
                    splitter.DragStarted += this.RowSplitterChangeStarted;
                    splitter.DragDelta += this.RowSplitterChangeDelta;
                    splitter.DragCompleted += this.RowSplitterChangeCompleted;
                    Grid.SetRow(splitter, j);
                    this.rowGrid.Children.Add(splitter);
                }
            }
        }

        /// <summary>
        /// Adds an "insert" row.
        /// </summary>
        /// <param name="rows">The number of rows.</param>
        /// <remarks>This row is below/to the right of the data rows/columns.</remarks>
        private void AddInserterRow(int rows)
        {
            if (this.CanInsertRows && this.AddItemHeader != null)
            {
                this.sheetGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = this.DefaultRowHeight });
                this.rowGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = this.DefaultRowHeight });

                var cell = new TextBlock
                {
                    Text = this.AddItemHeader,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
                var border = new Border
                {
                    Background = Brushes.Transparent,
                    BorderBrush = this.HeaderBorderBrush,
                    BorderThickness = new Thickness(1, 0, 1, 1),
                    Margin = new Thickness(0, 0, 0, 0)
                };

                border.MouseLeftButtonDown += this.AddItemCellMouseLeftButtonDown;
                Grid.SetRow(border, rows);

                cell.Padding = new Thickness(4, 2, 4, 2);
                Grid.SetRow(cell, rows);
                this.rowGrid.Children.Add(cell);
                this.rowGrid.Children.Add(border);
            }
            else
            {
                this.sheetGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
                this.rowGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
            }
        }

        /// <summary>
        /// Handles changes to the <see cref="P:IsEnabled" /> property.
        /// </summary>
        /// <param name="dependencyPropertyChangedEventArgs">The <see cref="System.Windows.DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private void HandleIsEnabledChanged(DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            this.UpdateSelectionVisibility();
        }

        /// <summary>
        /// Resizes all columns automatically.
        /// </summary>
        private void AutoSizeAllColumns()
        {
            this.sheetGrid.UpdateLayout();
            this.columnGrid.UpdateLayout();
            for (var i = 0; i < this.Columns; i++)
            {
                this.AutoSizeColumn(i);
            }
        }

        /// <summary>
        /// Resizes all rows automatically.
        /// </summary>
        private void AutoSizeAllRows()
        {
            this.sheetGrid.UpdateLayout();
            this.rowGrid.UpdateLayout();
            for (var i = 0; i < this.Rows; i++)
            {
                this.AutoSizeRow(i);
            }
        }

        /// <summary>
        /// Auto-sizes the specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The calculated width of the column.</returns>
        private double AutoSizeColumn(int column)
        {
            if (column < 0 || column >= this.Columns)
            {
                throw new ArgumentException("Invalid column");
            }

            // Initialize with the width of the header element
            var headerElement = this.GetColumnElement(column);
            var maximumWidth = headerElement.ActualWidth + headerElement.Margin.Left + headerElement.Margin.Right;

            // Compare with the widths of the cell elements
            for (var i = 0; i < this.sheetGrid.RowDefinitions.Count; i++)
            {
                var c = this.GetCellElement(new CellRef(i, column));

                if (c != null)
                {
                    maximumWidth = Math.Max(maximumWidth, c.ActualWidth + c.Margin.Left + c.Margin.Right);
                }
            }

            var newWidth = maximumWidth;
            this.SetColumnWidth(column, new GridLength(newWidth));
            return newWidth;
        }

        /// <summary>
        /// Auto-sizes the specified row.
        /// </summary>
        /// <param name="row">The row.</param>
        private void AutoSizeRow(int row)
        {
            // Initialize with the height of the header element
            var headerElement = this.GetRowElement(row);
            var maximumHeight = headerElement.ActualHeight + headerElement.Margin.Top + headerElement.Margin.Bottom;

            // Compare with the heights of the cell elements
            for (var i = 0; i < this.sheetGrid.ColumnDefinitions.Count; i++)
            {
                var c = this.GetCellElement(new CellRef(row, i));
                if (c != null)
                {
                    maximumHeight = Math.Max(maximumHeight, c.ActualHeight + c.Margin.Top + c.Margin.Bottom);
                }
            }

            this.sheetGrid.RowDefinitions[row].Height = new GridLength((int)maximumHeight + 2);
        }

        /// <summary>
        /// Copies the selected cells to the clipboard, tab-separated.
        /// </summary>
        private void Copy()
        {
            this.CopyOverride();
        }

        /// <summary>
        /// Cuts the selected items.
        /// </summary>
        private void Cut()
        {
            this.CutOverride();
        }
    }
}