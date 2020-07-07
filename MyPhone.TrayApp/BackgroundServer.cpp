#include "pch.h"
#include "BackgroundServer.h"

AppServiceConnection _Bridge = nullptr;

void InitBridge()
{
	/*AppServiceConnection connection;
	connection.AppServiceName(L"MyPhoneBridge");
	connection.PackageFamilyName(Package::Current().Id().FamilyName());
	connection.RequestReceived(_Bridge_OnRequest);
	_Bridge = connection;*/
}

void _Bridge_OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
{

}

