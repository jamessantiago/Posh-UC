0.1.2
-----
Released 12-Nov-15

New
===
 - Get-UcCmDevice
 - Get-UcCtiDevice
 - Added RisClient from updated UC.NET library for above two commands

Changed
=======
 - Changed Connect-UcServer command to request a credential object instead of username and password plaintext

0.1.1
-----
Released 12-Nov-15

New
===

- Add-UcPhone
- Remove-UcPhone
- New version of UC.NET

Fixes
=====

- Refactoring of cmdlet classes

0.1
---
Released 28-Oct-15

New
===

 - Connect-UcServer
 - Disconnect-UcServer
 - Invoke-UcSqlCmd
 - Get-UcPhone
 - Get-UcUser

Fixes
=====

 - Corrected formatting of SQl output
 - Connection doesn't reoccure now when rerunning Connect-UcServer, use -Force to reconnect