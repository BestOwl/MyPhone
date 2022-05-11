using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.RootPages.Main
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<NavigationMenu> navigationMenus;

        [ObservableProperty]
        private ObservableCollection<NavigationMenu> navigationFooterMenus;

        public MainPageViewModel()
        {
            navigationMenus = new ObservableCollection<NavigationMenu>()
            {
                new NavigationMenu(name:"Call", glyphIcon: "\uE717"),
                new NavigationMenu(name:"Message", glyphIcon: "\uE8BD"),
                new NavigationMenu(name:"Debug", glyphIcon: "\uEBE8"),
            };

            navigationFooterMenus = new ObservableCollection<NavigationMenu>()
            {
                new NavigationMenu(name: "Settings", glyphIcon: "\uE713")
            };
        }
    }

    public class NavigationMenu
    {
        public string Name { get; set; }

        public string GlyphIcon { get; set; }

        public NavigationMenu(string name, string glyphIcon)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            GlyphIcon = glyphIcon ?? throw new ArgumentNullException(nameof(glyphIcon));
        }
    }
}
