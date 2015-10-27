Posh-UC
-------

Grab the files from the stable release folder

Setup
-----

Run `Import-Module Posh-UC.dll` to load the module.

Examples
--------

Get all Posh-UC commands:

    Get-Command -Module Posh-UC

Connect to a CUCM server using an account with the AXL API role:

    Connect-UcServer "CUCMServer01" "username" "password"

Execute SQL command

    Invoke-UcSqlCommand "select * from device where name = 'jabberjrs'"