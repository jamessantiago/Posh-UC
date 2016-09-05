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
#        Imp Commands
#
################################################

Describe "Get-ImpUser" {
	Context "Username" {
		It "get an imp user" {
			$pass = ConvertTo-SecureString $env:ucPassword -AsPlainText -Force
			$creds = New-Object system.management.automation.PSCredential($env:ucUsername, $pass)
			Connect-ImpServer $env:impServer $creds -DoNotVerify | Should Be $true
			Get-ImpUser user01 | Should Not BeNullOrEmpty
		}		
	}
}
