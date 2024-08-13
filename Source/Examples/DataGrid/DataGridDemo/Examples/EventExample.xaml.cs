// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventExample.xaml.cs" company="PropertyTools">
//   Copyright (c) 2014 PropertyTools contributors
// </copyright>
// <summary>
//   Interaction logic for EventExample.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using PropertyTools;
using PropertyTools.Wpf;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DataGridDemo
{
    /// <summary>
    /// Interaction logic for EventExample.
    /// </summary>
    public partial class EventExample
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventExample" /> class.
        /// </summary>
        public EventExample()
        {
            this.InitializeComponent();
        }
    }

    public class EventExampleViewModel
    {
        static EventExampleViewModel()
        {
            StaticItemsSource = new ObservableCollection<EventExampleObject>();
            CreateObjects(StaticItemsSource);
        }

        public EventExampleViewModel()
        {
        }

        public static ObservableCollection<EventExampleObject> StaticItemsSource { get; }

        public ObservableCollection<EventExampleObject> ItemsSource => StaticItemsSource;
        public CellDefinitionFactory CellDefinitionFactory { get; } = new EventExampleCellDefinitionFactory();
        public DataGridControlFactory ControlFactory { get; } = new EventExampleControlFactory();

        private static void CreateObjects(ICollection<EventExampleObject> list, int n = 10)
        {
            for (int i = 0; i < n; i++)
            {
                list.Add(EventExampleObject.CreateRandom(GridEventHandler));
            }
        }

        private static void GridEventHandler(object sender, GridEventArgs e) => throw new NotImplementedException();
    }

    public class EventExampleCellDefinitionFactory : CellDefinitionFactory
    {
        protected override CellDefinition CreateCellDefinitionOverride(CellDescriptor d)
        {
            if (d.PropertyType == typeof(GridEvent)) { return new EventCellDefinition(); }
            return base.CreateCellDefinitionOverride(d);
        }
    }

    public class EventCellDefinition : CellDefinition
    {
    }

    public class EventExampleControlFactory : DataGridControlFactory
    {
        protected override FrameworkElement CreateDisplayControlOverride(CellDefinition d)
        {
            if (d is EventCellDefinition)
            {
                var tb = this.CreateTextBlockControl(d) as TextBlock;
                tb.Cursor = System.Windows.Input.Cursors.Hand;
                tb.TextDecorations = System.Windows.TextDecorations.Underline;
                tb.MouseDown += (s, e) => { MessageBox.Show("you clicked on details for item " + d.BindingSource); e.Handled = true; };
                return tb;
            }
            return base.CreateDisplayControlOverride(d);
        }
    }

    public class GridEvent
    {
        public string Title { get; set; }
        public EventHandler<GridEventArgs> Handler { get; set; }
        public override string ToString() => this.Title;
    }

    public class GridEventArgs : EventArgs
    {
        public object Context { get; set; }
    }

    public class EventExampleObject : Observable
    {
        private GridEvent details;
        private string name;

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.SetValue(ref this.name, value, nameof(Name));
            }
        }

        public GridEvent Details
        {
            get
            {
                return this.details;
            }
        }

        public EventExampleObject()
        {
        }

        private static readonly Random r = new Random(0);

        public static EventExampleObject CreateRandom(EventHandler<GridEventArgs> handler)
        {
            return new EventExampleObject
            {
                Name = "Random " + r.Next(10000),
                details = new GridEvent { Title = "See details", Handler = handler }
            };
        }

        public override string ToString() => this.Name;
    }
}