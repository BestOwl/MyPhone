[Windows.Devices.Enumeration.DeviceInformation, Windows.Devices.Enumeration, ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Enumeration.DeviceInformationCollection, Windows.Devices.Enumeration, ContentType=WindowsRuntime] | Out-Null
[Windows.Devices.Bluetooth.BluetoothDevice, Windows.Devices.Bluetooth, ContentType=WindowsRuntime] | Out-Null

Add-Type -AssemblyName System.Runtime.WindowsRuntime
$asTaskGeneric = ([System.WindowsRuntimeSystemExtensions].GetMethods() | ? { $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1' })[0]
Function Await($WinRtTask, $ResultType) {
    $asTask = $asTaskGeneric.MakeGenericMethod($ResultType)
    $netTask = $asTask.Invoke($null, @($WinRtTask))
    $netTask.Wait(-1) | Out-Null
    $netTask.Result
}
Function AwaitAction($WinRtAction) {
    $asTask = ([System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object { $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and !$_.IsGenericMethod })[0]
    $netTask = $asTask.Invoke($null, @($WinRtAction))
    $netTask.Wait(-1) | Out-Null
}

$deviceSelector = [Windows.Devices.Bluetooth.BluetoothDevice]::GetDeviceSelectorFromPairingState($true)
$devices = Await ([Windows.Devices.Enumeration.DeviceInformation]::FindAllAsync($deviceSelector)) ([Windows.Devices.Enumeration.DeviceInformationCollection])
Write-Host "Paired Bluetooth devices:"
for ($i = 0; $i -lt $devices.Count; $i++) {
    $devName = ($devices[$i]).Name
    Write-Host "#$i | $devName";
}
Write-Host
$index = (Read-Host "Please enter the device index") -as [int]
$deviceId = ($devices[$index]).Id
$deviceName = ($devices[$index]).Name
Set-Content -Path "./devicetest.local.json" -Value ('{"deviceId":"' + $deviceId + '"}')
Write-Host "Selected $deviceName"