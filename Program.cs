using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace ProcessInfomer
{ 
    public class StackTracer
    {
        public StackTracer()
        {

        }
        public string processName { get; set; }
        public int processID { get; set; }
        public List<StackSample> sampleCollection { get; set; }
    }    
    public class StackSample
    {
        public StackSample()
        {

        }
        public int sampleCounter { get; set; }
        public DateTime samplingTime { get; set; }
        public int threadCount { get; set; }
        public List<Thread> processThreadCollection { get; set; }
    }   
    public class Thread
    {
         public Thread()
        {
        }
        public Thread(DateTime stackCaptureTime, List<StackFrame> stackTrace)
        {
            this.sampleCaptureTime = stackCaptureTime;
            this.stackTrace = stackTrace;
        }
        public DateTime sampleCaptureTime { get; set; }
        public int managedThreadId { get; set; }
        public uint oSID { get; set; }
        public List<StackFrame> stackTrace { get; set; }     
    }
    public class StackFrame
    {    
        public StackFrame()
        {

        }
        public StackFrame(string stackTraceString, ulong instructionPointer,string clrMethodString,ulong StackPointer )
        {
           this.stackTraceString =   stackTraceString;
           this.instructionPointer =  instructionPointer;
           this.clrMethodString = clrMethodString;
           this.stackPointer= StackPointer;
         }
        public string stackTraceString { get; set; }
        public ulong instructionPointer { get; set; }        
        // get this from clrmethod - GetFullSignature()
        public string clrMethodString { get; set; }
        public ulong stackPointer { get; set; }          
    }    
    class Program
    { 
       static void Main(string[] args)
        {
            try
            {
                int pid = -1;
                string processName = "";
                int Pid = -1;
                int delay = 500;
                int stackTraceCount = 5;
                string stacktraceLocation = null;
                // Getting the parameters inatilized 
                #region Region for setting the console parameters switches

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

                #endregion
                Console.WriteLine("the pid is {0} and the duration is {1}  stacktrace count is  {2} and stack trace location is {3}", processName, delay, stackTraceCount, stacktraceLocation);
                if (processName == "")
                    pid = Pid;
                pid = Process.GetProcessesByName(processName)[0].Id;
                Console.WriteLine("the selected pid for the process {0} is  {1}", processName, pid);
                StackTracer stackTracer = new StackTracer();
                List<StackSample> stackSampleCollection = new List<StackSample>();
                stackTracer.processName = Process.GetProcessById(pid).ProcessName;
                stackTracer.processID = pid;
                for (int i = 0; i < stackTraceCount; i++)
                {
                    StackSample stackTracerProcessobj = new StackSample();
                    stackTracerProcessobj.processThreadCollection = new List<Thread>();
                    stackTracerProcessobj.sampleCounter = i;
                    stackTracerProcessobj.samplingTime = DateTime.UtcNow;
                    using (DataTarget dataTarget = DataTarget.AttachToProcess(pid, 5000, AttachFlag.Invasive))
                    {
                        string dacLocation = dataTarget.ClrVersions[0].TryGetDacLocation();
                        ClrRuntime runtime = dataTarget.CreateRuntime(dacLocation);
                        stackTracerProcessobj.threadCount = runtime.Threads.Count;
                        Console.WriteLine("=============================================================================================================");
                        Console.WriteLine("There are  {0}  threads in the {1} process", runtime.Threads.Count, Process.GetProcessById(pid).ProcessName);
                        Console.WriteLine("=============================================================================================================");
                        Console.WriteLine();
                        foreach (ClrThread crlThreadObj in runtime.Threads)
                        {
                            Thread stackTracerThreadObj = new Thread();
                            List<StackFrame> tracerStackThread = new List<StackFrame>();
                            IList<ClrStackFrame> Stackframe = crlThreadObj.StackTrace;
                            IEnumerable<ClrRoot> test = crlThreadObj.EnumerateStackObjects();
                            stackTracerThreadObj.oSID = crlThreadObj.OSThreadId;
                            stackTracerThreadObj.managedThreadId = crlThreadObj.ManagedThreadId;
                            Console.WriteLine("There are  {0}  itmes in the stack for the thread ", crlThreadObj.StackTrace.Count);
                            foreach (ClrStackFrame stackFrame in Stackframe)
                            {
                                stackTracerThreadObj.sampleCaptureTime = DateTime.UtcNow;
                                string tempClrMethod = "NULL";
                                if (stackFrame.Method != null)
                                    tempClrMethod = stackFrame.Method.GetFullSignature();
                                tracerStackThread.Add(new StackFrame(stackFrame.DisplayString, stackFrame.InstructionPointer, tempClrMethod, stackFrame.StackPointer));
                                Console.WriteLine("stack trace for thread- " + stackFrame.StackPointer + " -Stack String - " + stackFrame.DisplayString);
                            }
                            stackTracerThreadObj.stackTrace = tracerStackThread;
                            stackTracerProcessobj.processThreadCollection.Add(stackTracerThreadObj);
                        }
                    }                   
                    stackSampleCollection.Add(stackTracerProcessobj);
                    System.Threading.Thread.Sleep(delay);
                }
                stackTracer.sampleCollection = stackSampleCollection;                
                //if (string.IsNullOrEmpty(stacktraceLocation))
                //    stacktraceLocation = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), DateTime.Now.Ticks + ".xml");

                //Console.WriteLine("Deseraliazing the object");

                ////Serelizing the result for the runtime to look into the full object
                //XmlSerializer serializer = new XmlSerializer(typeof(StackTracer));
                //using (TextWriter writer = new StreamWriter(stacktraceLocation))
                //{
                //    serializer.Serialize(writer, stackTracer);
                //}
                //Console.WriteLine("Serialization Complete ");
                Type testype = stackTracer.GetType();
                objectSeralizer(stacktraceLocation, testype, stackTracer);
                Console.Read();
            }
            catch (Exception ex)
            {
                try {
                    Console.WriteLine("Unhandled Exception Occured {0}", ex.StackTrace.ToString());
                    Console.WriteLine("Inner Exception Occured {0}", ex.InnerException.StackTrace.ToString());
                   }
                catch 
                {
                }
               Console.Read();
            }          
        }

       public static void objectSeralizer(String filePath, Type OjectType, object Object)
        {
            string  stacktraceLocation = filePath;
                Type ClassToSerelaize = OjectType;
                if (string.IsNullOrEmpty(stacktraceLocation))
                stacktraceLocation = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), DateTime.Now.Ticks + ".xml");          
                Console.WriteLine("Deseraliazing the object");
                //Serelizing the result for the runtime to look into the full object
                XmlSerializer serializer = new XmlSerializer(ClassToSerelaize);
                using (XmlWriter writer = XmlWriter.Create(stacktraceLocation))
                            {
                                writer.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"stacktrace.xsl\"");
                            serializer.Serialize(writer, Object);
                            }            
                            Console.WriteLine("Serialization Complete ");

                           
        }              
    }
}

// Comments - Can store another resouces in the resources based on the bitness of the file.
// 