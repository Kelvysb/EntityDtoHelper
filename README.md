# EntityHelper

Tool for extract Entity and Dto's from database tables.

# Install:

```
dotnet tool install --global EntityDtoHelper
```

  * Requires .Net core 3.0 : https://dotnet.microsoft.com/download/dotnet-core/3.0
  * It may be nescessary to add the 'dotnet tools' folder to the path environment variable in windows.
  * in windows the dotnet tools folder it's '%USERPROFILE%\.dotnet\tools'


# Usage:

To start execute 'EntityDtoHelper' on powershell, cmd or bash.

Set a connection with -a <Connection name> or --add <Connection name> before executing -run <Connection name>,
to see all stored connections use -l or --list

If the path was not set by -p <Path>, it will use the current path, to check the current path use -dir or -d

To run just type -r <Connection name> or --run <Connection name> and follow the instructions.

To exit the program type -e or --exit at the main menu or ctrl+c at any time.

Commands:

```
Run execution wizard:
 --run or -run <connection name>

Set Target Path:
 --path or -p <connection name>

Add Connection:
 --add or -a <connection name>

List saved connections:
 --list or -l

Remove saved connections:
 --rem or -rem <connection name>

 Get current dir:
     --dir or -d

 Program Version:
     --version or -v
 Help:
     --help or -h
 Exit:
     --exit or -e
```