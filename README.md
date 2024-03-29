# PowerSession

> Record a Session in PowerShell.

**‼️  DEPRECATED Checkout the brand new Rust implementation: https://github.com/Watfaq/PowerSession-rs ‼️**

PowerShell version of [asciinema](https://github.com/asciinema/asciinema) based on [Windows Pseudo Console(ConPTY)](https://devblogs.microsoft.com/commandline/windows-command-line-introducing-the-windows-pseudo-console-conpty/)

## Checkout A Demo

[![asciicast](https://asciinema.org/a/272866.svg)](https://asciinema.org/a/272866)

## Compatibilities

* The output is comptible with asciinema v2 standard and can be played by `ascinnema`.
* The `auth` and `upload` functionalities are agains `asciinema.org`.

## Installation

### using dotnet tool

```
> dotnet tool install --global PowerSession
```

### Using [Scoop](https://scoop.sh)

```
> scoop install PowerSession
```

### Manual Download

Download `PowerSession.exe` at Release Page https://github.com/ibigbug/PowerSession/releases


## Usage

### Record

```PowerShell
$ PowerSession.Cli.exe rec a.txt
```

### Play

```PowerShell
$ PowerSession.Cli.exe play a.txt
```

### Auth

```PowerShell
$ PowerSession.Cli.exe auth
```

### Upload

```PowerShell
$ PowerSession.Cli.exe upload a.txt
```

### Get Help

```PowerShell
$ PowerSession.exe rec -h

rec:
  Record and save a session

Usage:
  PowerSession rec [options] <file>

Arguments:
  <file>    The filename to save the record

Options:
  -c, --command <command>    The command to record, default to be powershell.exe
```

## Supporters
- [GitBook](https://www.gitbook.com/) Community License
