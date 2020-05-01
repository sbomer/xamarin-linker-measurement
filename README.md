# Measuring linker output in Xamarin

## Prerequisites
- VS for mac

## Steps
1. Clone https://github.com/xamarin/app-contacts
2. Open `src/MyContacts.sln` in VS for mac
3. Change configuration to `Release | iPhone`

   ![VS for mac configurations](./config-release.png)

   You can also try `Release | iPhoneSimulator` if you want to run the app on a simulator, but don't use this for the size measurement.
4. Set `<MtouchArch>` to `x86_64` in `MyContacts.iOS.csproj`

   Or set this via the project settings.
5. Enable linking. Set `<MtouchLink>SdkOnly</MtouchLink>` or `<MtouchLink>Full</MtouchLink>`, or set this via the project settings.
6. (Optional) In Xamarin, linking happens inside of mtouch. Unfortunately this doesn't expose an option to set the linker verbosity, so linker warnings will not show up in the output. To fix this, patch mtouch.exe:
   ```
   sudo dotnet run -p PatchMtouch/PatchMtouch.csproj
   ```
   This is fragile as it depends on the location of mtouch.exe and its implementation.
   After you are finished measuring, be sure to restore mtouch.exe from the backup.
7. Build MyContacts
8. Collect the build output, noting any reflection warnings if mtouch was patched.
9. Measure the size of the output. I used the included `MeasureSize`. From the cloned `app-contacts` repo:

   ```
   app-contacts $ dotnet run -p ../MeasureSize/MeasureSize.csproj src/MyContacts.iOS/bin/iPhoneSimulator/Release
   ```

## My results

### app-contacts

I collected some numbers by ignoring the AcquainiOS.app directory on simulator builds (see [MeasureSize](MeasureSize/Program.cs)), but this needs more work before I trust the numbers.

- [Release | iPhone, no linking](results/app-contacts-nolink.txt)
- [Release | iPhoneSimulator, no linking](results/app-contacts-nolink-simulator.txt)
- [Release | iPhoneSimulator, link sdk](results/app-contacts-linksdk-simulator.txt)

  [linker output](results/app-contacts-linksdk-simulator-out.txt)


## Measuring the package archive

I attempted to measure the actual package archive (right-click project -> "Archive for Publishing") instead of the output directory, but ran into too many roadblocks. Even to create the archive without running it, one needs:

- An [Apple ID](https://appleid.apple.com/account#!&page=create) registered as an Apple Develper
  - [Linked](https://docs.microsoft.com/en-us/xamarin/cross-platform/macios/apple-account-management?tabs=macos) to VS for mac
- A signing certificate for [device provisioning](https://docs.microsoft.com/en-us/xamarin/ios/get-started/installation/device-provisioning/). Either:
  - A paid [enrollment](https://developer.apple.com/programs/enroll/) in the Apple Developer Program, or
  - A free [provisioning profile](https://docs.microsoft.com/en-us/xamarin/ios/get-started/installation/device-provisioning/free-provisioning?tabs=macos) to use with manual provisioning in VS for mac
    - which requires requesting a certificate from Xcode for the specific app bundle identifier
      - ...which requires a physical iOS device