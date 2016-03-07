if (-not (Get-Module Posh-UC))
{
	if (($env:ucBuild) -and $env:ucBuild -eq "Release")
	{
		Import-Module "$pwd\..\Stable Release\Posh-UC.dll"
	}
	else {
		Import-Module "$pwd\..\Posh-UC\bin\Debug\Posh-UC.dll"
	}
}

################################################
#
#        User Commands via PerfMon
#
################################################

Describe "Get-UcPerfMonCounters" {
	Context "Hostname" {
		It "list counters" {
			$pass = ConvertTo-SecureString $env:ucPassword -AsPlainText -Force
			$creds = New-Object system.management.automation.PSCredential($env:ucUsername, $pass)
			Connect-UcServer $env:ucServer $creds -DoNotVerify | Should Be $true
			Get-UcPerfMonCounters "10.10.20.17" | Should Not BeNullOrEmpty
		}
	}
}

Describe "Get-UcPerfMonCounterData" {
	Context "DeviceName" {
		It "get a cti device" {
			write-host $(Get-UcPerfMonCounterData "10.10.20.17" "Cisco XCP TC")
			Get-UcPerfMonCounterData "10.10.20.17" "Cisco XCP TC" | Should Not BeNullOrEmpty
		}
	}
}