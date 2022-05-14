# My Phone

***My Phone*** is a desktop application that allows you to link your phone (supports Android, iOS, and Windows Phone) to your Windows 10/11 PC via Bluetooth without having to install a companion app on your phone. 

[![WinUI 3 MSIX app](https://github.com/BestOwl/MyPhone/actions/workflows/main.yml/badge.svg)](https://github.com/BestOwl/MyPhone/actions/workflows/main.yml)

![Screenshot: Main Page](https://user-images.githubusercontent.com/8947026/168443434-8b001b6b-1428-4b08-8ac5-3751f650059c.png)

![Screenshot: Calling UI](https://user-images.githubusercontent.com/8947026/167856802-42f7ebc3-1ff9-4f62-a1b9-70edfde65b1f.png)

## Features

- [x] Calling
- [ ] SMS messaging
- [ ] Phonebook access
- [ ] Notifications 

### Features comparison with *Phone Link (a.k.a. Your Phone)*

| Feature                  | Phone Link    | My Phone                                                     |
| ------------------------ | ------------- | ------------------------------------------------------------ |
| Calling                  | ✔             | ✔                                                            |
| SMS messaging            | ✔             | ✔<sup>1</sup>                                                |
| Phonebook access         | ✔             | ✔<sup>1</sup>                                                |
| App Notifications        | ✔             | ✔<sup>2</sup>                                                |
| Share photos and files   | ✔             | ❌                                                            |
| Use mobile apps from PC  | ✔<sup>3</sup> | ❌                                                            |
| Supported mobile devices | ⚠Android Only | ✔Android, iOS, Windows Phone, and all Bluetooth-HFP-enabled devices |

- `1`: will be implemented in v1.0 this summer
- `2`: will be implemented in the future
- `3`: only available for some selected Android devices 

## Implementation 
### Hands-Free Profile (HFP)
This app use Windows Runtime APIs: `Windows.ApplicationModel.Calls` , `Windows.Devices.Bluetooth` and `Windows.Devices.Enumeration` to make the HFP works.

1. Use ``Windows.Devices.Enumeration` API to enumerate available `PhoneLineTransportDevice` (i.e. paired Bluetooth devices that support HFP),
2. Call `PhoneLineTransportDevice.RegisterApp` to register the device for HFP
3. Call `PhoneLineTransportDevice.Connect` to connect to the deivice (establishes HFP Service Level Connection).
4. Now you can receive and answer phone calls on your PC.
5. To make phone call, use `Windows.Devices.Enumeration` API to enumerate available `PhoneLine`, then call `PhoneLine.Dial`

Note that Microsoft's *Phone Link* use the same APIs mentioned above.

#### The HFP stack
The above APIs have very limited functionality. They can only be only used for call control. If you need to handle raw audio stream or something else, you need to write a custom **user-mode** profile driver ([more information here](https://github.com/BestOwl/MyPhone/issues/1)).

Actually, I did try to implement HFP stack before but eventually gave up because the MSIX package does not support installing drivers. I managed to establish the HFP Service Level Connection (SLC) and successfully make the phone call via SLC, but failed to establish SCO audio connection and transfer any audio. If you're interested in how to establish HFP Service Level Connection, you may find the [source code here](MyPhone.Demo/HFP.cs).   

## System requirement

- Windows 10 Version 1903 (build 10.0.18362) and later


## Contributing

### Requirements

- Windows 10 Version 1903 and later.
- Visual Studio 2022 with .NET and UWP workload (.NET 6 SDK and Windows SDK 10.0.19041.0)
- [Single-project MSIX Packaging Tools extension](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/single-project-msix?tabs=csharp#install-the-single-project-msix-packaging-tools)

### Build

1. Clone repo
   `git clone https://github.com/BestOwl/MyPhone.git`
2. Open `MyPhone.sln` with Visual Studio
3. Set `MyPhone` as startup project
4. Run `MyPhone (Package)`
