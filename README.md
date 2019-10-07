# PowerSession

> Record a Session in PowerShell.

PowerShell version(PoC still) of [asciinema](https://github.com/asciinema/asciinema) using [Windows Pseudo Console(ConPTY)](https://devblogs.microsoft.com/commandline/windows-command-line-introducing-the-windows-pseudo-console-conpty/)

Only the `record` feature is implemented now. The output is comptible with asciinema v2 standard and can be played by `ascinnema`


## Play around

1. Build the Solution
2. Run `PowerSession.Cli.exe` under `.\PowerSession\PowerSession.Cli\bin\Debug\netcoreapp3.0`

    ```PowerShell
    $ PowerSession.Cli.exe rec a.txt
    ```

3. Play

    ```bash
    $ PowerSession.Cli.exe play a.txt
    ```
    
## Checkout `asciinema` playing PowerSession record output: 

[![asciicast](https://asciinema.org/a/272577.svg)](https://asciinema.org/a/272577)

## Roadmap

- [ ] Implement other necessary features
- [ ] ...
