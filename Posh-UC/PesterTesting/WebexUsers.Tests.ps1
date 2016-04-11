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
#        User Commands via Webex
#
################################################

Describe "Get-WebexUser" {
	Context "Username" {
		It "get a regular user" {
			$pass = ConvertTo-SecureString $env:webexPassword -AsPlainText -Force
			$creds = New-Object system.management.automation.PSCredential($env:webexUser, $pass)
			Connect-WebexServer $env:webexServer $creds -SiteId $env:webexId -Email $env:webexEmail -PartnerId $env:webexPartner -DoNotVerify | Should Be $true
			Get-WebexUser -Active | Should Not BeNullOrEmpty
		}		
	}
}