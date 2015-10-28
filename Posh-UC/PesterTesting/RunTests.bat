echo "Setting up environment"
set ucBuild=Debug
call "%VS140COMNTOOLS%\..\..\VC\vcvarsall.bat" x86_amd64

echo "Building PoSH-Sodium"
MSBuild ..\PoSH-UC.sln /p:Configuration=Debug /p:Platform="Any CPU"

echo "Storing connection details"
set ucServer=10.10.20.1
set ucUsername=administrator
set ucPassword=ciscopsdt

echo "Running tests"
cmd /c ..\..\Pester\bin\pester.bat

pause