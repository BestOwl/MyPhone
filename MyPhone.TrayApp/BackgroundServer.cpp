#include "pch.h"
#include "BackgroundServer.h"

static DeviceInformation DeviceInfo{nullptr};
static PhoneLineTransportDevice CallDevice{nullptr};
static BluetoothDevice BthDevice{nullptr};

static DeviceState _State = DeviceState::Disconnected;

ThreadPoolTimer _Timer{ nullptr };
int _count = 0;

void DebugWriteLine(hstring msg)
{
    OutputDebugStringW((msg + L"\n").c_str());
}

//15s/ -> 1min; 30s/ -> 6min; 1min/ -> 15min; 3min/ -> 30min; 10min/ 1hour 
void OnTimer(ThreadPoolTimer timer)
{
    if (CallDevice == nullptr)
    {
        timer.Cancel();
        return;
    }
    if (BackgroundServer::Reconnect().get())
    {
        _count = 0;
        timer.Cancel();
        _State == DeviceState::Connected;
        return;
    }

    if (_count == 3)
    {
        timer.Cancel();
        _Timer = ThreadPoolTimer::CreatePeriodicTimer(OnTimer, std::chrono::seconds(30)); 
    }
    else if (_count == 13)
    {
        timer.Cancel();
        _Timer = ThreadPoolTimer::CreatePeriodicTimer(OnTimer, std::chrono::minutes(1));
    }
    else if (_count == 22)
    {
        timer.Cancel();
        _Timer = ThreadPoolTimer::CreatePeriodicTimer(OnTimer, std::chrono::minutes(3));
    }
    else if (_count == 37)
    {
        timer.Cancel();
        _Timer = ThreadPoolTimer::CreatePeriodicTimer(OnTimer, std::chrono::minutes(10));
    }
    else if (_count == 40)
    {
        timer.Cancel();
    }

    _count++;
}

void Bth_ConnectionStatusChanged(BluetoothDevice sender, IInspectable args)
{
    if (sender.ConnectionStatus() == BluetoothConnectionStatus::Connected)
    {
        DebugWriteLine(L"Bth_ConnectionStatusChanged: Connected");
    }
    else
    {
        DebugWriteLine(L"Bth_ConnectionStatusChanged: Disconnected");
        _State = DeviceState::Registered;
        
        _Timer = ThreadPoolTimer::CreatePeriodicTimer(OnTimer, std::chrono::seconds(15));
    }
}

void InitWatcher()
{
    BthDevice.ConnectionStatusChanged(Bth_ConnectionStatusChanged);
}

IAsyncAction BackgroundServer::Init()
{
    auto settings = ApplicationData::Current().LocalSettings().Values();
    if (settings.HasKey(L"deviceId"))
    {
        hstring id = unbox_value_or(settings.Lookup(L"deviceId"), L"");
        if (!id.empty())
        {
            DeviceInfo = co_await DeviceInformation::CreateFromIdAsync(id);
            co_await Reconnect();
            InitWatcher();
        }
    }
}

task<bool> BackgroundServer::Reconnect()
{
    if (DeviceInfo != nullptr)
    {
        if (CallDevice == nullptr)
        {
            CallDevice = PhoneLineTransportDevice::FromId(DeviceInfo.Id());
            if (CallDevice == nullptr)
            {
                co_return false;
            }

            DeviceAccessStatus status = co_await CallDevice.RequestAccessAsync();
            if (status != DeviceAccessStatus::Allowed)
            {
                co_return false;
            }
        }

        if (BthDevice == nullptr)
        {
            BthDevice = co_await BluetoothDevice::FromIdAsync(DeviceInfo.Id());
        }
        
        if (!CallDevice.IsRegistered())
        {
            CallDevice.RegisterApp();
        }

        bool success = co_await CallDevice.ConnectAsync();
        if (success)
        {
            _State = DeviceState::Connected;
        }
        else
        {
            _State = DeviceState::Registered;
        }
        co_return success;
    }
}

/// <summary>
/// Connect to a new phone device
/// </summary>
/// <param name="deviceInfo"></param>
/// <returns></returns>
task<bool> BackgroundServer::ConnectTo(DeviceInformation deviceInfo)
{
    if (DeviceInfo != deviceInfo)
    {
        DeviceInfo = nullptr;
        BthDevice = nullptr;
    }

    CallDevice = PhoneLineTransportDevice::FromId(deviceInfo.Id());
    if (CallDevice == nullptr)
    {
        co_return false;
    }

    DeviceAccessStatus status = co_await CallDevice.RequestAccessAsync();
    if (status != DeviceAccessStatus::Allowed)
    {
        co_return false;
    }

    BthDevice = co_await BluetoothDevice::FromIdAsync(deviceInfo.Id());
    DeviceInfo = deviceInfo;
    if (!CallDevice.IsRegistered())
    {
        CallDevice.RegisterApp();
    }
    _State = DeviceState::Registered;

    InitWatcher();

    bool success = co_await CallDevice.ConnectAsync();
    if (success)
    {
        _State = DeviceState::Connected;
    }

    co_return true;
}

enum DeviceState BackgroundServer::CurrentState()
{
    return _State;
}
