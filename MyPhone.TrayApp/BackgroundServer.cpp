#include "pch.h"
#include "BackgroundServer.h"
#include <sstream>

AppServiceConnection _Bridge = nullptr;

void BackgroundServer::InitBridge()
{
	AppServiceConnection connection;
	connection.AppServiceName(L"GoodTimeStudio.MyPhoneBridge");
	connection.PackageFamilyName(Package::Current().Id().FamilyName());
	connection.RequestReceived(_Bridge_OnRequest);
	_Bridge = connection;
}

concurrency::task<bool> BackgroundServer::OpenBridgeAsync()
{
	try
	{
		AppServiceConnectionStatus status = co_await _Bridge.OpenAsync();
		if (status != AppServiceConnectionStatus::Success)
		{
			std::wostringstream  s;
			s << L"Fail to open AppServiceConnection \n Error code: " << (int)status;
			MessageBox(NULL, s.str().c_str(), L"Error", NULL);
			co_return false;
		}
		co_return true;
	}
	catch (hresult_error const& ex)
	{
		MessageBox(NULL, ex.message().c_str(), L"Unexpected error occured", NULL);
	}
	co_return false;
}

void BackgroundServer::_Bridge_OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
{

}

IAsyncAction BackgroundServer::PushCommandAsync(hstring command)
{
	ValueSet set;
	set.Insert(L"request", box_value(command));
	try
	{
		co_await _Bridge.SendMessageAsync(set);
	}
	catch (hresult_error const& ex)
	{
		MessageBox(NULL, ex.message().c_str(), L"Unexpected error occured", NULL);
	}
}
