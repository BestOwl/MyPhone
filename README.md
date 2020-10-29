# My Phone Assistant

**My Phone** is just a simplified copy cat of *Your Phone* developed by Microsoft, but supports iOS, Windows Phone, and any other mobile devices come with Bluetooth HFP (hands-free profile) feature.

![Screenshot: Main UI](https://github.com/BestOwl/MyPhone/raw/master/docs/Screenshot_1.png)

![Screenshot: Incoming Call](https://github.com/BestOwl/MyPhone/raw/master/docs/Screenshot_2.png)

![Screenshot: Call UI](https://github.com/BestOwl/MyPhone/raw/master/docs/Screenshot_3.png)



## Features

- [x] Calling
- [ ] SMS messaging
- [ ] Phonebook access
- [ ] Notifications 

#### Features compared with *Your Phone*

| Feature                  | Your Phone    | My Phone                                                     |
| ------------------------ | ------------- | ------------------------------------------------------------ |
| Calling                  | ✔             | ✔                                                            |
| SMS messaging            | ✔             | ✔<sup>1</sup>                                                |
| Phonebook access         | ✔             | ✔<sup>1</sup>                                                |
| Notifications            | ✔             | ✔<sup>1</sup>                                                |
| Share photos and files   | ✔             | ❌                                                            |
| Use mobile apps from PC  | ✔<sup>2</sup> | ❌                                                            |
| Supported mobile devices | ⚠Android Only | ✔Android, iOS, Windows Phone, and all Bluetooth-HFP-enabled devices |

- `1`: will be implemented in the future

- `2`: only available for selected Android devices 



Keep in mind that **My Phone** was built only because Microsoft *Your Phone* does not support iOS, Windows Phone and other devices. If you are an Android user, you'd better use *Your Phone*.



## Implementation 

### Project structure

| Project                    | Type                             | Description                                                  |
| -------------------------- | -------------------------------- | ------------------------------------------------------------ |
| *MyPhone.TrayApp.XamlHost* | C++ UWP Project                  | XAML island host                                             |
| *MyPhone.TrayApp*          | C++ Win32 Project                | Main app logic, system tray UI and UWP "broker". Enable the UWP app to call Win32 APIs and keeping the app running in the background. Use XAML island to make the system tray UI has the same look and feel as native UWP system apps. This project handles main app logic including Bluetooth HFP stuff. |
| *MyPhone*                  | C# UWP Project                   | Main app UI                                                  |
| *PackageProject*           | Desktop Bridge Packaging Project | Generate MSIX app package                                    |

### Bluetooth HFP

This app use Windows Runtime APIs: `Windows.ApplicationModel.Calls` , `Windows.Devices.Bluetooth` and `Windows.Devices.Enumeration` to make the HFP works.

1. Use ``Windows.Devices.Enumeration` API to enumerate available `PhoneLineTransportDevice` (paired Bluetooth devices that support HFP),
2. Call `PhoneLineTransportDevice.RegisterApp` to register the device for HFP
3. Then the system will handle the rest (establishing HFP Service Level Connection and etc.) for you.

Note that *Your Phone* use the same APIs mentioned above.

### Self-implemented HFP stack?

Self-implementing a HFP stack is a little bit problematic because the only way to establish a SCO audio connection and transfer audio is to write a custom **kernel-mode** driver and let the end-user to install it. That's quite inconvenient for end-users to use this app, and a **kernel-mode** driver has to be signed by a trusted code signing certificate before the end-user can install it.

Actually, I tried to self-implement HFP before. I managed to establish the HFP Service Level Connection (SLC) and successfully make the phone call via SLC, but failed to establish SCO audio connection and transfer any audio due to the reason above. If you're interested in how to establish HFP Service Level Connection, you may find the demo project `MyPhone.CLI` in `legacy` branch.  



## System requirement

- Windows 10 version 1903 (build 10.0.18362) or above

  

## Contributing

### Requirements

- Windows 10 version 1903 or above.
- Visual Studio 2019 with .NET, UWP and C++ workload

### Build

1. Clone repo
   `git clone --recursive https://github.com/BestOwl/MyPhone.git`
2. Open `MyPhone.sln` with Visual Studio
3. Set `PackageProject` as startup project
4. Set build architecture to ARM, x86 or x64. (AnyCPU is not supported)
5. Build and run 



Feel free to contribute some code :)