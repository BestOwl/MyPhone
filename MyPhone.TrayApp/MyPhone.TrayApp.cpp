#include "resource.h"

#include <windows.h>
#include <windowsx.h>
#include <stdlib.h>
#include <string.h>

#include <winrt/Windows.system.h>
#include <winrt/windows.ui.xaml.hosting.h>
#include <windows.ui.xaml.hosting.desktopwindowxamlsource.h>
#include <winrt/Windows.UI.Xaml.Controls.h>
#include <winrt/Windows.UI.Xaml.Controls.Primitives.h>
#include <winrt/Windows.ui.xaml.media.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt\impl\Windows.UI.Xaml.Markup.0.h>

using namespace winrt;
using namespace Windows::UI;
using namespace Windows::UI::Composition;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Hosting;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Numerics;

LRESULT CALLBACK WindowProc(HWND, UINT, WPARAM, LPARAM);

HWND _hWnd;
HWND _childhWnd;
HINSTANCE _hInstance;

Canvas _Root = nullptr;
MenuFlyout _ContextMenu = nullptr;
// {E7F3B498-BDDC-4343-AF8E-D8AB405C1232}
static const GUID _ContextMenuGuid =
{ 0xe7f3b498, 0xbddc, 0x4343, { 0xaf, 0x8e, 0xd8, 0xab, 0x40, 0x5c, 0x12, 0x32 } };
bool _ContextMenuMouseClick;


NOTIFYICONDATAW _nid;
NOTIFYICONIDENTIFIER _niid;
constexpr UINT WM_NOTIFYICON = WM_APP + 1;

int CALLBACK WinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ LPSTR lpCmdLine, _In_ int nCmdShow)
{
	_hInstance = hInstance;

	// The main window class name.
	const wchar_t szWindowClass[] = L"MYPHONETRAYAPP";
	WNDCLASSEX windowClass = { };

	windowClass.cbSize = sizeof(WNDCLASSEX);
	windowClass.lpfnWndProc = WindowProc;
	windowClass.hInstance = hInstance;
	windowClass.lpszClassName = szWindowClass;

	windowClass.hIconSm = LoadIcon(windowClass.hInstance, MAKEINTRESOURCEW(IDI_APPICON));

	if (RegisterClassEx(&windowClass) == NULL)
	{
		MessageBox(NULL, L"Windows registration failed!", L"Error", NULL);
		return 0;
	}

	_hWnd = CreateWindowEx(
		WS_EX_NOACTIVATE | WS_EX_LAYERED | WS_EX_TOPMOST,
		szWindowClass,
		L"MyPhone.TrayApp",
		WS_POPUP,
		0, 0, 0, 0,
		NULL,
		NULL,
		hInstance,
		NULL
	);
	if (_hWnd == NULL)
	{
		MessageBox(NULL, L"Call to CreateWindow failed!", L"Error", NULL);
		return 0;
	}

	// Init NotifyIcon
	_nid.cbSize = sizeof(_nid);
	_nid.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP | NIF_SHOWTIP | NIF_GUID;
	_nid.uCallbackMessage = WM_NOTIFYICON;
	_nid.uVersion = NOTIFYICON_VERSION_4;
	_nid.hIcon = windowClass.hIconSm;
	_nid.hWnd = _hWnd;
	_nid.guidItem = _ContextMenuGuid;
	wcscpy_s(_nid.szTip, L"My Phone");

	_niid.cbSize = sizeof(_niid);
	_niid.guidItem = _ContextMenuGuid;

	Shell_NotifyIcon(NIM_ADD, &_nid);
	// Set the behaviour version, opt-in WM_CONTEXTMENU message
	Shell_NotifyIcon(NIM_SETVERSION, &_nid); 


#pragma region XAMLIsland
	//https://docs.microsoft.com/en-us/windows/apps/desktop/modernize/host-standard-control-with-xaml-islands-cpp

	// The call to winrt::init_apartment initializes COM; by default, in a multithreaded apartment.
	winrt::init_apartment(apartment_type::single_threaded);

	// Initialize the Xaml Framework's corewindow for current thread
	WindowsXamlManager winxamlmanager = WindowsXamlManager::InitializeForCurrentThread();

	DesktopWindowXamlSource desktopSource;
	auto interop = desktopSource.as<IDesktopWindowXamlSourceNative>();
	check_hresult(interop->AttachToWindow(_hWnd));

	// This Hwnd will be the window handler for the Xaml Island: A child window that contains Xaml.  
	HWND hWndXamlIsland = nullptr;
	interop->get_WindowHandle(&hWndXamlIsland);
	// Update the xaml island window size becuase initially is 0,0
	SetWindowPos(hWndXamlIsland, 0, 0, 0, 1, 1, SWP_SHOWWINDOW);

	//Creating the Xaml content
	Canvas xamlContainer;
	xamlContainer.Background(Windows::UI::Xaml::Media::SolidColorBrush{ Windows::UI::Colors::LightGray() });

	FontIcon closeIcon;
	closeIcon.Glyph(L"\xE8BB");

	MenuFlyoutItem exitItem;
	exitItem.Text(L"Exit");
	exitItem.Icon(closeIcon);
	exitItem.Click([](auto sender, auto args)
	{
		PostMessage(_hWnd, WM_CLOSE, 0, 0);
	});

	MenuFlyoutItem openAppItem;
	openAppItem.Text(L"Open App");

	MenuFlyout menu;
	menu.Items().Append(openAppItem);
	menu.Items().Append(exitItem);
	menu.Opened([](auto sender, auto args) 
	{
		int i = _ContextMenu.Items().Size();
		if (i > 0)
		{
			if (_ContextMenuMouseClick)
			{
				_ContextMenu.Items().GetAt(i - 1).Focus(FocusState::Pointer);
			}
		}
		_ContextMenuMouseClick = false;
	});
	menu.Closed([](auto sender, auto args)
	{
		ShowWindow(_hWnd, SW_HIDE);
	});
	_ContextMenu = menu;

	desktopSource.Content(xamlContainer);
	_Root = xamlContainer;

#pragma endregion XAMLIsland

	ShowWindow(_hWnd, nCmdShow);
	UpdateWindow(_hWnd);

	//Message loop:
	MSG msg = { };
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return 0;
}

LRESULT CALLBACK WindowProc(HWND hWnd, UINT messageCode, WPARAM wParam, LPARAM lParam)
{
	switch (messageCode)
	{
	case WM_DESTROY:
		Shell_NotifyIcon(NIM_DELETE, &_nid);
		PostQuitMessage(0);
		break;
	case WM_NOTIFYICON:
		switch (LOWORD(lParam))
		{
		case WM_RBUTTONUP:
			_ContextMenuMouseClick = true;
			break;
		case WM_CONTEXTMENU:
			SetForegroundWindow(hWnd);

			RECT rect;
			Shell_NotifyIconGetRect(&_niid, &rect);
			
			Point point
			{
				static_cast<float>(rect.left),
				static_cast<float>(rect.top)
			};

			_ContextMenu.ShowAt(_Root, point);
			break;
		default:
			break;
		}
	default:
		return DefWindowProc(hWnd, messageCode, wParam, lParam);
		break;
	}

	return 0;
}