using System;
using System.Globalization;
using Esprima.Ast;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;

namespace JScriptTest
{

    // Zdroj a nuget Tu:
    // https://github.com/sebastienros/jint

    
    //https://channel9.msdn.com/Shows/Code-Conversations/Sebastien-Ros-on-jint-a-Javascript-Interpreter-for-NET


    public class ExposedFunctionality
    {
        public string Name { get; set; } = "A Test value";

        public void Dump(object o)
        {
            Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
            Console.WriteLine();
        }

        public string Format(string format, object[] args)
        {
            return string.Format(format, args);
        }

    }

    class Program
    {


        private static void Sample1()
        {
            var e = new Engine(cfg => cfg.AllowClr(typeof(System.IO.File).Assembly, typeof(System.Console).Assembly, typeof(Newtonsoft.Json.JsonConvert).Assembly));
            
            
            e.SetValue("log", new Action<object>(Console.WriteLine));
            
            e.SetValue("util", new ExposedFunctionality());

            // CLR accessible
            e.Execute(@"

                // test object in global scope
                var testObj = {a:10, b:'test', c: {name:'pepa', lastName:'nos'}};

                // .net namespace import
                var json = importNamespace('Newtonsoft.Json');

                // Can access .net functionality from js
                var dump = function(input) {
                    return json.JsonConvert.SerializeObject(input);
                };

            ");

            Console.WriteLine("Type exit to quit");
            Console.WriteLine("Use util.log() function to print output to console");
            Console.WriteLine("Use util.dump() function to dump object to console in JSON format");

            Console.WriteLine("All .NET types are exposed eg. ");

            Console.Write("> ");
            string input;

            while (!((input = Console.ReadLine()).Equals("exit", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var completionValue = e.Execute(input).GetCompletionValue();
                    Console.WriteLine(completionValue.ToString());
                    if (completionValue.IsObject())
                    {
                        var obj = completionValue.ToObject();
                        foreach (var mem in obj.GetType().GetMembers())
                        {
                            Console.WriteLine($"member: Type: {mem.MemberType}, Name: {mem.Name}, ");
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("An error occured");
                    Console.WriteLine(exception.ToString());
                }
                Console.Write("> ");
            }

            // Can call js func from .net
            e.Invoke("dump", new {a = 10});
        }

        static void Main(string[] args)
        {
            Sample1();
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
