// Copy from https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/host-custom-control-with-xaml-islands-cpp#create-a-uwp-app-project
#include "pch.h"
#include "App.h"

using namespace winrt;
using namespace Windows::UI::Xaml;
namespace winrt::MyPhone_TrayApp_XamlHost::implementation
{
    App::App()
    {
        Initialize();
        AddRef();
        m_inner.as<::IUnknown>()->Release();
    }
    App::~App()
    {
        Close();
    }
}