#pragma once

using namespace winrt;
using namespace winrt::Windows::Foundation;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::AppService;
using namespace Windows::Foundation::Collections;

class BackgroundServer
{
public:
	static void InitBridge();
	static concurrency::task<bool> OpenBridgeAsync();
	static void _Bridge_OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args);
	static IAsyncAction PushCommandAsync(hstring command);
};

