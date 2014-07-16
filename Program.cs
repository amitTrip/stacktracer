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
    class Program
    {
        static void Main(string[] args)
        {
            int pid = Process.GetProcessesByName("w3wp")[0].Id;
            Console.WriteLine("ProcessId" + pid);
            using (DataTarget dataTarget = DataTarget.AttachToProcess(pid, 5000))
            {
                string dacLocation = dataTarget.ClrVersions[0].TryGetDacLocation();
                ClrRuntime runtime = dataTarget.CreateRuntime(dacLocation);
                
                ClrThread Thread =  runtime.Threads[1];
                // ...
            Console.WriteLine("====================================================================================");
            Console.WriteLine("There are  {0}  threads in the {1} process", runtime.Threads.Count, Process.GetProcessById(pid).ProcessName);
            Console.WriteLine("====================================================================================");
            Console.WriteLine();
            Console.WriteLine("Hit Enter to get the stack traces");
            Console.Read();

            foreach (ClrThread crlThreadObj in runtime.Threads)
            {
                Console.WriteLine("There are  {0}  itmes in the stack for the thread with thread ID {1}", crlThreadObj.StackTrace.Count, crlThreadObj.OSThreadId);
                IList<ClrStackFrame> Stackframe = crlThreadObj.StackTrace;             
                foreach (ClrStackFrame stackFrame in Stackframe)
                {
                    Console.WriteLine("stack trace for thread- " + stackFrame.StackPointer + " -Stack String - " + stackFrame.DisplayString);
                }
            }
                Console.WriteLine("Hit Enter to dereliaze the object");
                Console.Read();
                // serelizing the result for the runtime to look into the full object
                XmlSerializer serializer = new XmlSerializer(typeof(ClrThread));
                using (TextWriter writer = new StreamWriter(@"D:\Xml.xml"))
                {
                    serializer.Serialize(writer, Thread);
                                 
                }
                  Console.WriteLine("serelization done " );
 
                Console.Read();
                 
                // Itearing the heap for process -- need to find its imprtance

                //ClrHeap heap = runtime.GetHeap();
                //var stats = from o in heap.EnumerateObjects()
                //            let t = heap.GetObjectType(o)
                //            group o by t into g
                //            let size = g.Sum(o => (uint)g.Key.GetSize(o))
                //            orderby size
                //            select new
                //            {
                //                Name = g.Key.Name,
                //                Size = size,
                //                Count = g.Count()
                //            };

                //foreach (var item in stats)
                //    Console.WriteLine("{0,12:n0} {1,12:n0} {2}", item.Size, item.Count, item.Name);
                //Console.Read();
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
