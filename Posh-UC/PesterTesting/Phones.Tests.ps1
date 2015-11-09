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

Describe "Add-UcPhone" {
	Context "All of it" {
		It "create a jabber phone for a known user" {
			Add-UcPhone -TemplateDevice "CSFUSER001" -DeviceName "CSFUSERXX1" -DirectoryNumber "1111" -Description "User 01 Phone" -PhoneDisplay "User 01" -DirectoryUri "user01@domain.com" -Username "user01"
		}		
	}
}

Describe "Remove-UcPhone" {
	Context "Previously created phone" {
		It "removes a phone" {
			Remove-UcPhone "CSFUSERXX1"
		}
	}
}