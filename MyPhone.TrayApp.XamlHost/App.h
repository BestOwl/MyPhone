#pragma once
// Copy from https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/host-custom-control-with-xaml-islands-cpp#create-a-uwp-app-project
#include "App.g.h"
#include "App.base.h"
namespace winrt::MyPhone_TrayApp_XamlHost::implementation
{
    class App : public AppT2<App>
    {
    public:
        App();
        ~App();
    };
}
namespace winrt::MyPhone_TrayApp_XamlHost::factory_implementation
{
    class App : public AppT<App, implementation::App>
    {
    };
}