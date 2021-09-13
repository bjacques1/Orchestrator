Before using the activities in the PowerShell integration pack, PowerShellInvoke as a Windows Service needs to be installed.

Suppose -

* PowerShell script needs to be run with user "scxsvc" in "REDMOND" domain.
* The password for above account is "mypassword".
* The machine is installed with an amd64 Windows.

Run the following command in Administrator elevated Command Prompt to install the service:

  %windir%\Microsoft.NET\Framework\v2.0.50727\InstallUtil.exe /username=REDMOND\scxsvc /password=mypassword "%CommonProgramFiles(x86)%\Microsoft System Center 2012\Orchestrator\Extensions\Support\Integration Toolkit\c881d8b5-df21-4a77-ba47-d26355c5f626\PowerShellInvoke-x86.exe"

  %windir%\Microsoft.NET\Framework64\v2.0.50727\InstallUtil.exe /username=REDMOND\scxsvc /password=mypassword "%CommonProgramFiles(x86)%\Microsoft System Center 2012\Orchestrator\Extensions\Support\Integration Toolkit\c881d8b5-df21-4a77-ba47-d26355c5f626\PowerShellInvoke-x64.exe"

If you have other users for running the PowerShell using this IP, add them as well.  After installing Windows Services, start them in services.msc.  Later they will be started automatically when OS boots up.

To uninstall the service, run above commands with the option /u before /username=REDMOND\scxsvc.
