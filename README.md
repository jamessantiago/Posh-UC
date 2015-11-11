Posh-UC
-------

Grab the files from the stable release folder

Setup
-----

Run `Import-Module Posh-UC.dll` to load the module or save the stable releases folder as "Posh-UC" under the one of the powershell modules directories such as `My Documents\WindowsPowershell\Modules` the load the module using `Import-Module Posh-UC`.

Examples
--------

Get all Posh-UC commands:

    Get-Command -Module Posh-UC

Connect to a CUCM server using an account with the AXL API role:

    Connect-UcServer "CUCMServer01" "username" "password"

Execute SQL command

    Invoke-UcSqlCmd "select * from device where name = 'jabberjrs'"