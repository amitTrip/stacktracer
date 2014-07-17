using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime;
using System.Xml.Serialization;
using System.IO;

namespace ProcessInfomer
{
 
    public class ProcessThreadCollection
    {
        public ProcessThreadCollection()
        {

        }
        public string processName { get; set; }
        public int threadCount { get; set; }
        
        public List<StackTrace> processThreadlist { get; set; }

    }
    
    public class StackTrace
    {
         public StackTrace()
        {

        }
        public StackTrace(DateTime stackCaptureTime, List<StackTraceNode> stackTrace)
        {

            this.stackCaptureTime = stackCaptureTime;
            this.stackTrace = stackTrace;
        }
        public DateTime stackCaptureTime { get; set; }
        public List<StackTraceNode> stackTrace { get; set; }
     
    }

    public class StackTraceNode
    {    
        public StackTraceNode()
        {

        }
        public StackTraceNode(string stackTraceString, ulong instructionPointer,string clrMethodString,ulong StackPointer )
        {
           this.stackTraceString =   stackTraceString;
           this.instructionPointer =  instructionPointer;
           this.clrMethodString = clrMethodString;
           this.StackPointer= StackPointer;
         }
        public string stackTraceString { get; set; }
        public ulong instructionPointer { get; set; }        
        // get this from clrmethod - GetFullSignature()
        public string clrMethodString { get; set; }
        public ulong StackPointer { get; set; }      
    }
   
    
    class Program
    {
        static void Main(string[] args)
        {
            int pid = -1;
            // Custom Process Object
            string processName = "";
            int Pid = -1;
            int delay = 500;
            int stackTraceCount = 5;
            string stacktraceLocation = null;
                   
            // Getting the parameters inatilized 
            try
            {

                 Pid = int.Parse(args[0]);
            }
            catch
            {
                if (args[0] != null && args[0].Length != 0)
                    processName = args[0];
                else
                {
                    processName = "w3wp";

                    Console.WriteLine("The switch for process is not provided using [w3wp] as default process name");
                }
            }

            try
            {

                delay = int.Parse(args[1]);
            }
            catch
            {
                Console.WriteLine("The switch for delay to capture stack trace is not provided using 500MS  as default");
            }

            try
            {

                stackTraceCount = int.Parse(args[2]);
            }
            catch
            {
                Console.WriteLine("The switch for stacktrace counts is not provided using 4 traces as default");
            }
            try
            {
                if (args[3] != null && args[3].Length != 0)
                 stacktraceLocation = args[3];
            }
            catch
            {
                Console.WriteLine("The switch for stacktrace location is not provided using {0} as default location", stacktraceLocation);
            }

            Console.WriteLine("the pid is {0} and the duration is {1}  stacktrace count is  {2} and stack trace location is {3}", processName, delay, stackTraceCount,stacktraceLocation);
           
            if ( processName == "" )
            pid = Pid;
            pid = Process.GetProcessesByName(processName)[0].Id;

            Console.WriteLine("the selected pid for the process {0} is  {1}", processName,pid);

            
            ProcessThreadCollection stackTracerProcessobj = new ProcessThreadCollection();
            stackTracerProcessobj.processThreadlist = new List<StackTrace>();


            for (int i = 0; i < stackTraceCount; i++)
            {
                
                Console.WriteLine("ProcessId" + pid);
                using (DataTarget dataTarget = DataTarget.AttachToProcess(pid, 5000,AttachFlag.Invasive))
                {
                    

                    string dacLocation = dataTarget.ClrVersions[0].TryGetDacLocation();
                    ClrRuntime runtime = dataTarget.CreateRuntime(dacLocation);

                    ClrThread Thread = runtime.Threads[1];
                    // ...

                    stackTracerProcessobj.processName = Process.GetProcessById(pid).ProcessName;
                    stackTracerProcessobj.threadCount = runtime.Threads.Count;


                    Console.WriteLine("====================================================================================");
                    Console.WriteLine("There are  {0}  threads in the {1} process", runtime.Threads.Count, Process.GetProcessById(pid).ProcessName);
                    Console.WriteLine("====================================================================================");
                    Console.WriteLine();
                    Console.WriteLine("Hit Enter to get the stack traces");
                   // Console.Read();



                    
                   foreach (ClrThread crlThreadObj in runtime.Threads)
                    {
                        StackTrace stackTracerThreadObj = new StackTrace();

                        List<StackTraceNode> tracerStackThread = new List<StackTraceNode>();

                        Console.WriteLine("There are  {0}  itmes in the stack for the thread with thread ID {1}", crlThreadObj.StackTrace.Count, crlThreadObj.OSThreadId);
                        IList<ClrStackFrame> Stackframe = crlThreadObj.StackTrace;
                        IEnumerable<ClrRoot> test = crlThreadObj.EnumerateStackObjects();
                        foreach (ClrStackFrame stackFrame in Stackframe)
                        {
                            stackTracerThreadObj.stackCaptureTime = DateTime.UtcNow;
                            string tempClrMethod = "NULL";
                            if (stackFrame.Method != null)
                                tempClrMethod = stackFrame.Method.GetFullSignature();
                            tracerStackThread.Add(new StackTraceNode(stackFrame.DisplayString, stackFrame.InstructionPointer, tempClrMethod, stackFrame.StackPointer));

                            Console.WriteLine("stack trace for thread- " + stackFrame.StackPointer + " -Stack String - " + stackFrame.DisplayString);
                        }
                        stackTracerThreadObj.stackTrace = tracerStackThread;

                        stackTracerProcessobj.processThreadlist.Add(stackTracerThreadObj);

                    }
                    if(string.IsNullOrEmpty(stacktraceLocation))
                       stacktraceLocation= Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),  DateTime.Now.Ticks + ".xml");
                    
                    Console.WriteLine("Hit Enter to dereliaze the object");
                   // Console.Read();
                    // serelizing the result for the runtime to look into the full object
                    XmlSerializer serializer = new XmlSerializer(typeof(ProcessThreadCollection));
                    using (TextWriter writer = new StreamWriter(stacktraceLocation))
                    {
                        serializer.Serialize(writer, stackTracerProcessobj);
                    }
                    Console.WriteLine("serelization done ");

                  

                }

                System.Threading.Thread.Sleep(delay);
            }
            Console.Read();
        }

        public void GetProcessList()
        {
            Process[] processlist = Process.GetProcesses();

            foreach (Process theprocess in processlist)
            {
                Console.WriteLine("Process-{0}|PID- {1}|Thread count-{2}|Memory{3}", theprocess.ProcessName, theprocess.Id, theprocess.Threads.Count, theprocess.VirtualMemorySize);
                foreach (ProcessThread thread in theprocess.Threads)
                {
                    Console.WriteLine("Thread name " + thread.Id + " Thread" + thread.Container);
                }
                Console.Read();

            }
            Console.ReadLine();
        }
        }
}
