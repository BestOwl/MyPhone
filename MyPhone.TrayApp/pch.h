#pragma once

#include "targetver.h"

#include <windows.h>
#include <windowsx.h>
#include <stdlib.h>
#include <string.h>
#include <pplawait.h>

#include <winrt/Windows.System.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.ApplicationModel.h>
#include <winrt/Windows.ApplicationModel.AppService.h>

#include <winrt/Windows.UI.Core.h>
#include <winrt/Windows.UI.Xaml.Controls.h>
#include <winrt/Windows.UI.Xaml.Controls.Primitives.h>
#include <winrt/Windows.UI.Xaml.Media.h>
#include <winrt/Windows.UI.Xaml.Markup.h>
#include <winrt/Windows.UI.Xaml.Hosting.h>
#include <windows.ui.xaml.hosting.desktopwindowxamlsource.h>

#include <winrt/MyPhone_TrayApp_XamlHost.h>

// C++/WinRT
// Fixes warning C4002: too many arguments for function-like macro invocation 'GetCurrentTime'
#undef GetCurrentTime