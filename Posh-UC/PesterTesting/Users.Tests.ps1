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
#        User Commands via AXL
#
################################################

Describe "Get-UcUser" {
	Context "Username" {
		It "get a regular user" {
			Connect-UcServer $env:ucServer $env:ucUsername $env:ucPassword | Should Be $true
			Get-UcUser user01 | Should Not BeNullOrEmpty
		}		
	}
}

Describe "Set-UcUser" {
	Context "Username and associated devices" {
		It "remove all devices from a known user" {
			Set-UcUser "user01" -associateddevices @("") | select -ExpandProperty associateddevices | Should be $null
		}
		It "associate a known device with a known user" {
			Set-UcUser "user01" -associateddevices @("CSFuser001") | select -ExpandProperty associateddevices | Should be "CSFUSER001"
		}
	} 
}