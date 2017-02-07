# Prerequisites:
# 1. Make sure VM agent is installed on the VM
# 2. Enable port 80 or 443 
# 3. Install PowerShellGet on the VM
#	 go to https://github.com/PowerShell/PowerShellGet#get-powershellget-module-for-powershell-versions-30-and-40 
#	 and download and install MSI 
# 4. Enable "Active Scripting" so that you can login to Azure
#	 Go IE -> Internet Options -> Security -> Custom Level -> Scroll to Scripting and Enable "Active Scripting"

param(
    [String] $CSName = "<cloud service name>",
    [String] $VMName = "<virtual machine name>"
) 

Install-Module Azure

Add-AzureAccount

$vm = Get-AzureVM -ServiceName $CSName -Name $VMName
$vm.VM.ProvisionGuestAgent

$Agent = Get-AzureVMAvailableExtension -Publisher Symantec -ExtensionName SymantecEndpointProtection

Set-AzureVMExtension -Publisher Symantec –Version $Agent.Version -ExtensionName SymantecEndpointProtection -VM $vm | Update-AzureVM

Restart-Computer

# Final manual step:
# On the taskbar find Symantec client and review the status, most likely it's not working
# you will need press Fix-All and after few minutes Symantec will ask you to reboot your VM again
