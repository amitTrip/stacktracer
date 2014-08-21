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
using System.Reflection;

namespace StackTracer
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
        enum ParseState
        {
            Unknown, Samples, Interval,Help,Predelay
        }

        static void Usage()
        {
            Console.WriteLine("Usage: Stack Tracer : ProcessName|PID [options] ");
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("  /D     Initial delay to start the trace collection by the application (Default:0)");
            Console.WriteLine("  /S     Indicates how many samples of the stacks you have to take (default:10)");
            Console.WriteLine("  /I     The interval between stack samples in milliseconds (default:1000)");
            Console.WriteLine("  /?     Help Menu");
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine("Example: stacktracer w3wp /d 10 /s 60 /i 500");
            Console.WriteLine("Wait for 10 seconsts to Take 60 samples one sample in every 500 milliseconds");

        }
       static void Main(string[] args)
        {
            StringBuilder errorString = new StringBuilder();
            var state = ParseState.Unknown;
           try
            {
                // Global variable declaration

                int pid = -1;
                string processName = "w3wp";
                int Pid = -1;
                int delay = 500;
                int stackTraceCount = 5;
                string stacktraceLocation = null;
                int pdelay = 0;
              
              
                
               // Getting the parameters inatilized 
                #region Region for setting the console parameters switches   
               if (args.ToList<string>().Count != 0)
                {
                foreach (var arg in args.Skip(1))
                {
                    switch (state)
                    {
                        case ParseState.Unknown:
                            if (arg.ToLower() == "/s")
                            {
                                state = ParseState.Samples;
                            }
                            else if (arg.ToLower() == "/i")
                            {
                                state = ParseState.Interval;
                            }
                            else if (arg.ToLower() == "/d")
                            {
                                state = ParseState.Predelay;
                            }
                            else
                            {
                                Usage();
                                state = ParseState.Help;
                                return;
                            }
                            break;
                        case ParseState.Samples:
                            if (!int.TryParse(arg, out stackTraceCount))
                            {
                                Usage();
                                state = ParseState.Help;
                                return;
                            }
                            state = ParseState.Unknown;
                            break;
                        case ParseState.Interval:
                            if (!int.TryParse(arg, out delay))
                            {
                                Usage();
                                state = ParseState.Help;
                                return;
                            }
                            state = ParseState.Unknown;
                            break;
                            case ParseState.Predelay:
                            if (!int.TryParse(arg, out pdelay))
                            {
                                Usage();
                                state = ParseState.Help;
                                return;
                            }
                            state = ParseState.Unknown;
                            break;
                        default:
                            state = ParseState.Help;
                            break;
                    }
                }                          
                try
                {
                   Pid = int.Parse(args[0]);
                }
                catch
                {
                   

                    if (args[0] != null && args[0].Length != 0)
                        if (args[0].ToLower() == "/?")
                        {
                            state = ParseState.Help;
                           
                        }
                        else
                        {
                            processName = args[0];
                        }
                    else
                    {
                        processName = "w3wp";
                        errorString.AppendLine("The switch for process is not provided using [w3wp] as default process name");                    
                    }
                }                             
                }
                else
               {
                   Usage();
                   state = ParseState.Help;
                                
                    
                }
                #endregion
               if (state != ParseState.Help )
               {
                   errorString.AppendLine("the pid is" + processName + " and the duration is " + delay + " stacktrace count is " + stackTraceCount + " and stack trace location is" + stacktraceLocation);
                   Console.WriteLine("StackTrace Collection Begin...");
                   System.Threading.Thread.Sleep(pdelay);

                   if (processName == "")
                       pid = Pid;
                   pid = Process.GetProcessesByName(processName)[0].Id;
                   errorString.AppendLine("the selected pid for the process" + processName + " is" + pid);
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
                           errorString.AppendLine("=============================================================================================================");
                           errorString.AppendLine("There are" + runtime.Threads.Count + "threads in the" + Process.GetProcessById(pid).ProcessName + " process");
                           errorString.AppendLine("=============================================================================================================");
                           errorString.AppendLine();
                           foreach (ClrThread crlThreadObj in runtime.Threads)
                           {
                               Thread stackTracerThreadObj = new Thread();
                               List<StackFrame> tracerStackThread = new List<StackFrame>();
                               IList<ClrStackFrame> Stackframe = crlThreadObj.StackTrace;
                               stackTracerThreadObj.oSID = crlThreadObj.OSThreadId;
                               stackTracerThreadObj.managedThreadId = crlThreadObj.ManagedThreadId;
                               errorString.AppendLine("There are " + crlThreadObj.StackTrace.Count + "  itmes in the stack for the thread ");
                               foreach (ClrStackFrame stackFrame in Stackframe)
                               {
                                   stackTracerThreadObj.sampleCaptureTime = DateTime.UtcNow;
                                   string tempClrMethod = "NULL";
                                   if (stackFrame.Method != null)
                                       tempClrMethod = stackFrame.Method.GetFullSignature(); // We need to create a dicitonary 
                                   tracerStackThread.Add(new StackFrame(stackFrame.DisplayString, stackFrame.InstructionPointer, tempClrMethod, stackFrame.StackPointer));
                                   errorString.AppendLine("stack trace for thread- " + stackFrame.StackPointer + " -Stack String - " + stackFrame.DisplayString);
                               }
                               stackTracerThreadObj.stackTrace = tracerStackThread;
                               stackTracerProcessobj.processThreadCollection.Add(stackTracerThreadObj);
                           }
                       }
                       stackSampleCollection.Add(stackTracerProcessobj);
                       System.Threading.Thread.Sleep(delay);
                   }
                   stackTracer.sampleCollection = stackSampleCollection;
                   Type testype = stackTracer.GetType();
                   objectSeralizer(stacktraceLocation, testype, stackTracer);
                   errorString.AppendLine();
               }
             
            }
            catch (Exception ex)
            {

                if (ex != null && ex.StackTrace != null)
                {
                    errorString.AppendLine("Unhandled Exception Occured " + ex.StackTrace.ToString());
                    if (state != ParseState.Help)
                        Console.WriteLine("Unhandled Exception Occured " + ex.StackTrace.ToString());
                }
             }
           finally
            {
                if(errorString.Length !=0)
                {
                    System.IO.File.WriteAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),  "Error.txt"), errorString.ToString());
                   // Console.ReadLine();
                }
                
            }
        }        
       public static void objectSeralizer(String filePath, Type OjectType, object Object)
        {
             //Sample to get the file from the resource. 
             //Check if stacktrace.xsl already exixt in filepath location
             string tempfilepath = (System.Reflection.Assembly.GetExecutingAssembly().Location).Replace(Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location),"")+ "stacktrace.xsl";      

            if (!File.Exists(tempfilepath))
            {
               Assembly _assembly = Assembly.GetExecutingAssembly();
               Stream _textStreamReader = _assembly.GetManifestResourceStream("StackTracer.bin.Debug.stacktrace.xsl");
               using (Stream s = File.Create(tempfilepath))
               {
                  _textStreamReader.CopyTo(s);
               }
               
            }
           
           //  Code to seralize the oject.
            string  stacktraceLocation = filePath;
             Type ClassToSerelaize = OjectType;
                if (string.IsNullOrEmpty(stacktraceLocation))
                    stacktraceLocation = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), DateTime.Now.Ticks + ".xml");          
                    Console.WriteLine("Generating the StackTrace report");
                
           //Serelizing the result for the runtime to look into the full object
                XmlSerializer serializer = new XmlSerializer(ClassToSerelaize);
                using (XmlWriter writer = XmlWriter.Create(stacktraceLocation))
                            {
                                writer.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"stacktrace.xsl\"");
                                serializer.Serialize(writer, Object);
                            }            
                                Console.WriteLine("StackTace Report Generated");
                               // Console.Read();          
        }              
    }
}

