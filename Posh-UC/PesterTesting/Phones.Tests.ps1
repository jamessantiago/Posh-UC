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
#        Phone Commands via AXL
#
################################################

Describe "Get-UcPhone" {
	Context "DeviceName" {
		It "get a device by name" {
			Connect-UcServer $env:ucServer $env:ucUsername $env:ucPassword | Should Be $true
			Get-UcPhone csfuser001 | Should Not BeNullOrEmpty
		}		
	}
}