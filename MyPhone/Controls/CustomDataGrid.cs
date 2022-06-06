using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Controls
{
    /// <summary>
    /// Custom DataGrid that fixed horizontal scrollbar overlap problem
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/answers/questions/79438/uwp-datagrid-horizontal-scrollbar-overlaps-datagri.html"/>
    public class CustomDataGrid : DataGrid
    {
        public CustomDataGrid() : base()
        {
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var rowPresenter = GetTemplateChild("RowsPresenter");
            if (rowPresenter != null)
            {
                Grid.SetRowSpan((Microsoft.UI.Xaml.FrameworkElement)rowPresenter, 1);
            }
        }
    }
}
