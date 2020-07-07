#pragma once

using namespace winrt;
using namespace Windows::ApplicationModel;
using namespace Windows::ApplicationModel::AppService;

void InitBridge();
void _Bridge_OnRequest(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args);
