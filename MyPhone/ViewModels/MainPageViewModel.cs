using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.ViewModels
{
    public class MainPageViewModel : BindableBase
    {
        private ObservableCollection<NavigationMenu> _NavigationMenus;
        public ObservableCollection<NavigationMenu> NavigationMenus
        {
            get => _NavigationMenus;
            set => SetProperty(ref _NavigationMenus, value);
        }

        public MainPageViewModel()
        {
            NavigationMenus = new ObservableCollection<NavigationMenu>()
            {
                new NavigationMenu(name:"Call", glyphIcon: "\uE717"),
                new NavigationMenu(name:"Message", glyphIcon: "\uE8BD"),
                new NavigationMenu(name:"Debug", glyphIcon: "\uEBE8"),
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
