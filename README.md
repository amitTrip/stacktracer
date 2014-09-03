#StackTracer

<img src="http://debugging.io/images/stack.ico"
 alt="stacktracer logo" title="stacktracer" align="right" />

 A tool that captures and analyzes stack trace samples captured at fixed intervals from any .net CLR process.

The Stacktracer is a console application attaches using the debugengine Interface and works with any .net process; no access to application source code is necessary,
no library modifications are needed, and there is no run-time instrumentation of CLR code. Configuration
options given at start of command line to specify the interval for stack trace and number of samples.Current implementation include output generation in xml/xslt for viewing the most recent stack traces. The performance impact of the stacktracer is minimal: less than a 8% increase in total elapsed time for a
set of standard benchmarks. Future plans include GUI, better filtering capabilities in analysis.	


##Highlights

>*	800 kb single exe file with only dependency of .net framework 4.0 client profile.
>*	Captures stack trace of any .net process Windows forms ,WPF, asp.net you name it. 
>*	No need of any symbols.
>*	Supports .net framework 2.0 to 4.5
>*	Can target 32 bit and 64 bit process.
>*	View traces in your favourite browser IE. (inspired from IIS FREB)
>*	Intuitive timeline view to filter out unwanted stacks/threads.
>*	Very easy to troubleshoot Hang or High CPU issues on customer machines.
>*	Can troubleshoot hangs less than 5 seconds easily.	


##Limitations

>*	Needs .NET framework 4.0
>*	Will not show any native stack information.
>*	That means if the hang is due to GC, this may not help you.	

##Downloads
<table>
  <tr>
    <th>ID</th><th>Category</th><th>Bitness</th>
  </tr>
  <tr>
    <td>1</td><td>Azure Websites</td><td><a href="https://onedrive.live.com/download?resid=ADDED4FD84D96960%21249">x86 </a> | <a href="https://onedrive.live.com/download?resid=ADDED4FD84D96960%21252"> x64 </a></td>
  </tr>
  <tr>
    <td>2</td><td>.NET Process</td><td><a href="https://onedrive.live.com/download?resid=ADDED4FD84D96960%21251">x86 </a> |<a href="https://onedrive.live.com/download?resid=ADDED4FD84D96960%21250"> x64 </a></td>
  </tr>
</table>
---------------------------------------------------------------------------------------------------------------

<br/>

	
