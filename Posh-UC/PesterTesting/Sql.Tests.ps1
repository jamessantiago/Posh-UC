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
#        SQL Commands via AXL
#
################################################

Describe "Invoke-UcSqlCmd" {
	Context "Sql Command" {
		It "grabs a count of devices" {
			Connect-UcServer $env:ucServer $env:ucUsername $env:ucPassword | Should Be $true
			Invoke-UcSqlCmd "select count(*) from device" | Should Not BeNullOrEmpty
		}		
	}
}