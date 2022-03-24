using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Controls
{
    // https://stackoverflow.com/questions/1175618/how-to-bind-selectionstart-property-of-text-box
    public class SelectionBindingTextBox : TextBox
    {
        public static readonly DependencyProperty BindableSelectionStartProperty = DependencyProperty.Register(
            "BindableSelectionStart",
            typeof(int),
            typeof(SelectionBindingTextBox),
            new PropertyMetadata(0, OnBindableSelectionStartChanged));

        public static readonly DependencyProperty BindableSelectionLengthProperty =
            DependencyProperty.Register(
            "BindableSelectionLength",
            typeof(int),
            typeof(SelectionBindingTextBox),
            new PropertyMetadata(0, OnBindableSelectionLengthChanged));

        private bool selectionStartChangeFromUI;
        private bool selectionLengthChangeFromUI;

        public SelectionBindingTextBox() : base()
        {
            SelectionChanged += SelectionBindingTextBox_SelectionChanged;
        }

        private void SelectionBindingTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (BindableSelectionStart != SelectionStart)
            {
                selectionStartChangeFromUI = true;
                BindableSelectionStart = SelectionStart;
            }

            if (BindableSelectionLength != SelectionLength)
            {
                selectionLengthChangeFromUI = true;
                BindableSelectionLength = SelectionLength;
            }
        }

        public int BindableSelectionStart
        {
            get => (int) GetValue(BindableSelectionStartProperty);
            set => SetValue(BindableSelectionStartProperty, value);
        }

        public int BindableSelectionLength
        {
            get => (int) GetValue(BindableSelectionLengthProperty);
            set => SetValue(BindableSelectionLengthProperty, value);
        }

        private static void OnBindableSelectionStartChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            SelectionBindingTextBox textBox = (SelectionBindingTextBox)dependencyObject;

            if (!textBox.selectionStartChangeFromUI)
            {
                int newValue = (int)args.NewValue;
                textBox.SelectionStart = newValue;
            }
            else
            {
                textBox.selectionStartChangeFromUI = false;
            }
        }

        private static void OnBindableSelectionLengthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            SelectionBindingTextBox textBox = (SelectionBindingTextBox)dependencyObject;

            if (!textBox.selectionLengthChangeFromUI)
            {
                int newValue = (int)args.NewValue;
                textBox.SelectionLength = newValue;
            }
            else
            {
                textBox.selectionLengthChangeFromUI = false;
            }
        }

    }
}
