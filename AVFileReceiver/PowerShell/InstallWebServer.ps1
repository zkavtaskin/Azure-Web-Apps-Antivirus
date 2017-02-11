# Run this in PowerShell ISE (Admin mode)

Configuration AVFileReceiver 
{ 
  param(
        [string[]]
		$ComputerName="localhost"
    )

  Node $ComputerName 
  { 
    WindowsFeature IIS 
    { 
      Ensure = “Present” 
      Name = “Web-Server” 
    }
    WindowsFeature ASPNET
    { 
      Ensure = “Present” 
      Name = “Web-Asp-Net45” 
    } 
    File TempDir {
        Type = 'Directory'
        DestinationPath = 'D:\Temp'
        Ensure = "Present"
    }
	WindowsFeature IISManagementTools { 
		Name = "Web-Mgmt-Tools" 
		Ensure = "Present" 
		IncludeAllSubFeature = $True 
		DependsOn = "[WindowsFeature]IIS"
	} 
  } 
}

AVFileReceiver

Enable-PSRemoting -SkipNetworkProfileCheck

Start-DscConfiguration -Path .\AVFileReceiver –ComputerName "localhost" -Wait -Force -Verbose

# Final manual step:
# In IIS change DefaultAppPool identity to LocalSystem 
# if you don't do this you will not be able to invoke Symantec command line interface 
