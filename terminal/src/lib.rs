#[cfg_attr(windows, path = "windows/mod.rs")]
mod windows;
pub use crate::windows::terminal::WindowsTerminal;

use std::sync::mpsc::{Receiver, Sender};
use std::sync::Arc;

pub trait Terminal {
    fn run(&mut self, command: &str) -> u32;
    fn attach_stdin(&self, rx: Receiver<(Arc<[u8; 1024]>, usize)>);
    fn attach_stdout(&self, tx: Sender<(Arc<[u8; 1024]>, usize)>);
}

#[cfg(test)]
mod tests {
    use crate::{Terminal, WindowsTerminal};
    use std::sync::mpsc::channel;
    use std::sync::Arc;
    use std::thread;
    use std::time::Duration;

    #[test]
    fn test_terminal_stdin_stdout() {
        let mut t = WindowsTerminal::new(None);
        let (stdin_tx, stdin_rx) = channel::<(Arc<[u8; 1024]>, usize)>();
        let (stdout_tx, stdout_rx) = channel::<(Arc<[u8; 1024]>, usize)>();

        t.attach_stdin(stdin_rx);
        t.attach_stdout(stdout_tx);

        thread::spawn(move || {
            t.run("powershell.exe");
        });

        let cmd = "echo a\n";
        let mut input = [0; 1024];
        input[..7].copy_from_slice(&cmd.as_bytes());
        stdin_tx.send((Arc::from(input), cmd.len())).unwrap();

        let output = stdout_rx.recv().unwrap();
        assert_eq!(
            std::str::from_utf8(&output.0[..output.1]).unwrap(),
            "a\n",
            "should echo `a`"
        );
    }
}
