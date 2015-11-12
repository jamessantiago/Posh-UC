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

Describe "Get-UcCmDevice" {
	Context "DeviceName" {
		It "get a cm device" {
			$pass = ConvertTo-SecureString $env:ucPassword -AsPlainText -Force
			$creds = New-Object system.management.automation.PSCredential($env:ucUsername, $pass)
			Connect-UcServer $env:ucServer $creds -DoNotVerify | Should Be $true
			Get-UcCmDevice SIPTrunkToCUP | Should Not BeNullOrEmpty
		}
	}
	Context "no parameter" {
		It "get all cm devices" {
			$(Get-UcCmDevice).TotalDevicesFound -gt 0 | Should Be $true
		}		
	}
}

Describe "Get-UcCtiItem" {
	Context "DeviceName" {
		It "get a cti device" {
			$(Get-UcCtiItem -DeviceName CSFUSER001).TotalItemsFound -gt 0 | Should Be $true
		}
	}
	Context "DirectoryNumber" {
		It "get a cti device by number" {
			$(Get-UcCtiItem -DirectoryNumber 1001).TotalItemsFound -gt 0 | Should Be $true
		}
	}
	Context "no parameter" {
		It "get all cti devices" {
			$(Get-UcCtiItem).TotalItemsFound -gt 0 | Should Be $true
		}
	}
}