using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HTMLify
{
    class Program
    {
        delegate void InputHandler(string[] args);
        Dictionary<Command, InputHandler> commands = new Dictionary<Command, InputHandler>();

        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            new Program(args);
        }
        public Program(string[] args)
        {
            commands.Add(new Command("-path")
                .addAlias("-p"), Handlers.ReadPath);
            commands.Add(new Command("-output")
                .addAliases(new string[] { "-o", "-out" }), Handlers.OutPath);
            commands.Add(new Command("-configuration"), Handlers.ConPath);
            List<int> starts = new List<int>();
            List<Command> cmds = new List<Command>();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                foreach (Command c in commands.Keys)
                {
                    if (c.matches(arg))
                    {
                        starts.Add(i);
                        cmds.Add(c);
                    }
                }
            }
            for (int h = 0; h < (starts.Count-1); h++)
            {
                List<string> arglist = new List<string>();
                for(int j = starts[h]; j < starts[h+1]; j++)
                {
                    arglist.Add(args[j]);
                }
                commands[cmds[h]](arglist.ToArray());
            }

            List<string> argglist = new List<string>();
            for (int i = starts[starts.Count - 1]; i < args.Length; i++)
            {
                argglist.Add(args[i]);
            }
            commands[cmds[starts.Count - 1]](argglist.ToArray());
            Handlers.Execute();
        }
        class Handlers
        {
            static string originalPath;
            static string outputPath;
            static string configPath;
            public static void ReadPath(string[] args)
            {
                Console.WriteLine(args[0]+":"+args[1]);
                originalPath = args[1];
            }
            public static void OutPath(string[] args)
            {

                Console.WriteLine(args[0] + ":" + args[1]);
                outputPath = args[1];
            }
            public static void ConPath(string[] args)
            {

            }
            public static void Execute()
            {
                string txt = File.ReadAllText(originalPath);
                Console.WriteLine(txt);
                File.WriteAllText(outputPath, txt);
                Console.ReadKey();
            }
        }
        class Command
        {
            string name;
            List<string> aliases = new List<string>();

            public Command(string nm)
            {
                name = nm;
            }
            public Command addAliases(params string[] aliases)
            {
                foreach (string alias in aliases)
                {
                    this.aliases.Add(alias);
                }
                return this;
            }
            public Command addAlias(string alias)
            {
                aliases.Add(alias);
                return this;
            }
            public bool matches(string inp)
            {
                if (inp.StartsWith(name)) return true;
                foreach (string alias in aliases)
                {
                    if (inp.StartsWith(alias)) return true;
                }
                return false;
            }
        }
    }
}
