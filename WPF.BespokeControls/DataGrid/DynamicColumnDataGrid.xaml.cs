using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.BespokeControls.Converter;

namespace WPF.BespokeControls.DataGrid
{
    /// <summary>
    /// Interaction logic for DynamicColumnDataGrid.xaml
    /// </summary>
    public partial class DynamicColumnDataGrid : System.Windows.Controls.DataGrid
    {
        public ObservableCollection<string> ColumnHeaders
        {
            get { return GetValue(ColumnHeadersProperty) as ObservableCollection<string>; }
            set { SetValue(ColumnHeadersProperty, value); }
        }

        public static readonly DependencyProperty ColumnHeadersProperty = 
            DependencyProperty.Register("ColumnHeaders"
                , typeof(ObservableCollection<string>)
                , typeof(DynamicColumnDataGrid)
                , new PropertyMetadata(new PropertyChangedCallback(OnColumnsChanged)));

        static void OnColumnsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            DynamicColumnDataGrid dataGrid = dependencyObject as DynamicColumnDataGrid;
            dataGrid.UpdateColumns();
        }

        public void UpdateColumns()
        {
            Columns.Clear();
        
            //Add Manufactures Columns
            foreach (string value in ColumnHeaders)
            {
                var column = new DataGridTextColumn(){
                    Header=value,
                    Binding=new Binding("Fields")
                    {
                        ConverterParameter = value,
                        Converter = new FieldConverter()}
                    };

                Columns.Add(column);
            }
        }
    }
}
