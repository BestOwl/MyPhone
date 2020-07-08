#include "pch.h"
#include "resource.h"
#include "BackgroundServer.h"

using namespace winrt;
using namespace Windows::UI;
using namespace Windows::UI::Composition;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Hosting;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Numerics;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::AppService;

using namespace MyPhone_TrayApp_XamlHost;

LRESULT CALLBACK WindowProc(HWND, UINT, WPARAM, LPARAM);
void LaunchMainApp();

HWND _hWnd;
HINSTANCE _hInstance;
NOTIFYICONDATAW _nid;
NOTIFYICONIDENTIFIER _niid;

constexpr UINT WM_NOTIFYICON = WM_APP + 1;
constexpr UINT WM_OPENBRIDGE = WM_APP + 2;

const wchar_t szWindowClass[] = L"GOODTIMESTUDIO-MYPHONETRAYAPP";
const wchar_t szWindowTitle[] = L"MyPhone.TrayApp";

App hostApp{ nullptr };
DesktopWindowXamlSource _desktopWindowXamlSource{ nullptr };

Canvas _Root = nullptr;
MenuFlyout _ContextMenu = nullptr;
bool _ContextMenuMouseClick;

bool _OpenBridgeConnection;

int CALLBACK wWinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ LPWSTR lpCmdLine, _In_ int nCmdShow)
{
	_OpenBridgeConnection = false;
	Uri uri(lpCmdLine);
	if (uri.Host() == L"open-appservice")
	{
		_OpenBridgeConnection = true;
	}

	// Prevent multiple tray app instance
	HWND preHwnd = FindWindow(szWindowClass, szWindowTitle);
	if (preHwnd != nullptr)
	{
		if (_OpenBridgeConnection)
		{
			PostMessage(preHwnd, WM_OPENBRIDGE, 0, 0);
		}

		return 0;
	}

#pragma region XAML Island Init

	// The call to winrt::init_apartment initializes COM; by default, in a multithreaded apartment.
	winrt::init_apartment(apartment_type::single_threaded);
	hostApp = App{};
	_desktopWindowXamlSource = DesktopWindowXamlSource{};

#pragma endregion XAML Island Init

	_hInstance = hInstance;

	
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
		szWindowTitle,
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

#pragma region XAML Island 
	if (_desktopWindowXamlSource != nullptr)
	{
		auto interop = _desktopWindowXamlSource.as<IDesktopWindowXamlSourceNative>();
		check_hresult(interop->AttachToWindow(_hWnd));
		HWND hWndXamlIsland = nullptr;
		interop->get_WindowHandle(&hWndXamlIsland);
		RECT windowRect;
		::GetWindowRect(_hWnd, &windowRect);
		// Update the xaml island window size because initially is 0,0
		::SetWindowPos(hWndXamlIsland, NULL, 0, 0, windowRect.right - windowRect.left, windowRect.bottom - windowRect.top, SWP_SHOWWINDOW);
		
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
				auto async{ BackgroundServer::PushCommandAsync(L"exit") };
				PostMessage(_hWnd, WM_CLOSE, 0, 0);
			});

		MenuFlyoutItem openAppItem;
		openAppItem.Text(L"Open App");
		openAppItem.Click([](auto sender, auto args)
			{
				LaunchMainApp();
			});

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

		_desktopWindowXamlSource.Content(xamlContainer);
		_Root = xamlContainer;
	}
#pragma endregion Xaml Island

	ShowWindow(_hWnd, nCmdShow);
	UpdateWindow(_hWnd);

	// Init NotifyIcon
	_nid.cbSize = sizeof(_nid);
	_nid.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP | NIF_SHOWTIP;
	_nid.uCallbackMessage = WM_NOTIFYICON;
	_nid.uVersion = NOTIFYICON_VERSION_4;
	_nid.hIcon = windowClass.hIconSm;
	_nid.hWnd = _hWnd;
	_nid.uID = 0;
	//_nid.guidItem = _ContextMenuGuid;
	wcscpy_s(_nid.szTip, L"My Phone");

	_niid.cbSize = sizeof(_niid);
	_niid.hWnd = _hWnd;
	_niid.uID = 0;

	if (!Shell_NotifyIcon(NIM_ADD, &_nid))
	{
		MessageBox(NULL, L"Call to Shell_NotifyIcon NIM_ADD failed!", L"Error", NULL);
		return 0;
	}
	// Set the behaviour version, opt-in WM_CONTEXTMENU message
	if (!Shell_NotifyIcon(NIM_SETVERSION, &_nid))
	{
		MessageBox(NULL, L"Call to Shell_NotifyIcon NIM_SETVERSION failed!", L"Error", NULL);
		return 0;
	}

	BackgroundServer::InitBridge();
	if (_OpenBridgeConnection)
	{
		auto async {BackgroundServer::OpenBridgeAsync()};
	}

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
		break;
	case WM_OPENBRIDGE:
	{
		auto async{ BackgroundServer::OpenBridgeAsync() };
		break;
	}
	default:
		return DefWindowProc(hWnd, messageCode, wParam, lParam);
		break;
	}

	return 0;
}

void LaunchMainApp()
{
	Windows::System::Launcher::LaunchUriAsync(Uri(L"goodtimestudio.myphone-launch://"));
}