extern crate terminal;

use serde::Serialize;
use std::sync::mpsc::channel;
use std::time::SystemTime;
use std::{
    collections::HashMap,
    env,
    fs::File,
    io::{Read, Write},
    thread,
};
use terminal::{Terminal, WindowsTerminal};

#[derive(Serialize)]
struct RecordHeader {
    version: u8,
    width: i16,
    height: i16,
    timestamp: u64,
    #[serde(rename = "env")]
    environment: HashMap<&'static str, String>,
}

pub struct Record {
    output_writer: Box<dyn Write + Send + Sync>,
    env: HashMap<&'static str, String>,
    command: String,
    terminal: WindowsTerminal,
}

impl Record {
    pub fn new(
        filename: String,
        mut env: Option<HashMap<&'static str, String>>,
        command: String,
    ) -> Self {
        let cwd = std::env::current_dir()
            .expect("failed to get cwd")
            .to_str()
            .unwrap_or("C:\\")
            .to_string();
        Record {
            output_writer: Box::new(File::create(filename).expect("Can't create file")),
            env: env.get_or_insert(HashMap::new()).clone(), // this clone() looks wrong??
            command,
            terminal: WindowsTerminal::new(cwd),
        }
    }
    pub fn execute(&mut self) {
        self.env.insert("POWERSESSION_RECORDING", "1".to_owned());
        self.env.insert("SHELL", "powershell.exe".to_owned());
        let term: String = env::var("TERMINAL_EMULATOR").unwrap_or("UnKnown".to_string());
        self.env.insert("TERM", term);

        self.record();
    }

    fn record(&mut self) {
        let header = RecordHeader {
            version: 2,
            width: self.terminal.width,
            height: self.terminal.height,
            timestamp: SystemTime::now()
                .duration_since(SystemTime::UNIX_EPOCH)
                .expect("check your machine time")
                .as_secs(),
            environment: self.env.clone(),
        };
        let _ = self
            .output_writer
            .write(serde_json::to_string(&header).unwrap().as_bytes());

        let (stdin_tx, stdin_rx) = channel::<u8>();
        let (stdout_tx, stdout_rx) = channel::<u8>();

        thread::spawn(move || loop {
            let stdin = std::io::stdin();
            let mut handle = stdin.lock();
            let mut buf = [0; 1];
            let rv = handle.read(&mut buf);
            match rv {
                Ok(n) if n > 0 => {
                    stdin_tx.send(buf[0]).unwrap();
                }
                _ => {
                    println!("pty stdin closed");
                    break;
                }
            }
        });

        thread::spawn(move || loop {
            let mut stdout = std::io::stdout();

            let rv = stdout_rx.recv();
            match rv {
                Ok(byte) => {
                    let _ = stdout.write(&[byte]);
                }
                Err(err) => {
                    println!("{}", err.to_string());
                    break;
                }
            }
        });
        self.terminal.attach_stdin(stdin_rx);
        self.terminal.attach_stdout(stdout_tx);
        self.terminal.run(&self.command);
    }
}
