using System;
using System.Diagnostics;
using System.ComponentModel;

Process process = new Process();
process.StartInfo.FileName = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
process.StartInfo.Arguments = "https://www.youtube.com/watch?v=dQw4w9WgXcQ" + " --new-window";
process.Start();