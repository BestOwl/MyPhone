#pragma once

using namespace winrt;
using namespace winrt::Windows::Foundation;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::AppService;
using namespace Windows::Foundation::Collections;
using namespace Windows::Devices::Enumeration;
using namespace Windows::Devices::Bluetooth;
using namespace Windows::ApplicationModel::Calls;
using namespace Windows::Storage;
using namespace Windows::System::Threading;

using namespace concurrency;

class BackgroundServer
{
public:
	static task<bool> ConnectTo(DeviceInformation deviceInfo);
	static task<bool> Reconnect();
	static IAsyncAction Init();
	static enum DeviceState CurrentState();
};

enum DeviceState
{
	Disconnected,
	Registered,
	Connected
};

