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