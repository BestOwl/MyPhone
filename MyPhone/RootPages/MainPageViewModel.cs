using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;

namespace GoodTimeStudio.MyPhone.RootPages
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<NavigationMenu> navigationMenus;

        [ObservableProperty]
        private ObservableCollection<NavigationMenu> navigationFooterMenus;

        public MainPageViewModel()
        {
            ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
            navigationMenus = new ObservableCollection<NavigationMenu>()
            {
                new NavigationMenu(name:resourceLoader.GetString("Item_Call"), glyphIcon: "\uE717", id: "call"),
                new NavigationMenu(name:resourceLoader.GetString("Item_Message"), glyphIcon: "\uE8BD", id: "message"),
                new NavigationMenu(name:resourceLoader.GetString("Item_Contacts"), glyphIcon: "\uE77B", id : "contacts"),
                new NavigationMenu(name:resourceLoader.GetString("Item_Debug"), glyphIcon: "\uEBE8", id : "debug"),
            };

            navigationFooterMenus = new ObservableCollection<NavigationMenu>()
            {
                new NavigationMenu(name: resourceLoader.GetString("Item_Settings"), glyphIcon: "\uE713", id : "settings")
            };
        }
    }

    public class NavigationMenu
    {
        public string Name { get; set; }

        public string GlyphIcon { get; set; }

        public string Id { get; set; }

        public NavigationMenu(string name, string glyphIcon, string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            GlyphIcon = glyphIcon ?? throw new ArgumentNullException(nameof(glyphIcon));
        }
    }
}
