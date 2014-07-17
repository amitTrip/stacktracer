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
        public List<StackTracerThread> processThreadlist { get; set; }

    }
    public class StackTracerThread
    {
         public StackTracerThread()
        {

        }
        public StackTracerThread(DateTime stackCaptureTime, List<StackTraceNode> stackTrace)
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
            int pid = Process.GetProcessesByName("w3wp")[0].Id;
            // Custom Process Object
            

            ProcessThreadCollection stackTracerProcessobj = new ProcessThreadCollection();
            stackTracerProcessobj.processThreadlist = new List<StackTracerThread>();


            for (int i = 0; i < 3; i++)
            {

                Console.WriteLine("ProcessId" + pid);
                using (DataTarget dataTarget = DataTarget.AttachToProcess(pid, 5000))
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
                        StackTracerThread stackTracerThreadObj = new StackTracerThread();

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


                    Console.WriteLine("Hit Enter to dereliaze the object");
                   // Console.Read();
                    // serelizing the result for the runtime to look into the full object
                    XmlSerializer serializer = new XmlSerializer(typeof(ProcessThreadCollection));
                    using (TextWriter writer = new StreamWriter(@"D:\Stack" + DateTime.Now.Ticks + ".xml"))
                    {
                        serializer.Serialize(writer, stackTracerProcessobj);

                    }
                    Console.WriteLine("serelization done ");

                  //  Console.Read();

                     //Itearing the heap for process -- need to find its imprtance

                    ClrHeap heap = runtime.GetHeap();
                    var stats = from o in heap.EnumerateObjects()
                                let t = heap.GetObjectType(o)
                                group o by t into g
                                let size = g.Sum(o => (uint)g.Key.GetSize(o))
                                orderby size
                                select new
                                {
                                    Name = g.Key.Name,
                                    Size = size,
                                    Count = g.Count()
                                };

                    foreach (var item in stats)
                        Console.WriteLine("{0,12:n0} {1,12:n0} {2}", item.Size, item.Count, item.Name);
                  //  Console.Read();

                }
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
