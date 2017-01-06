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
                .addAliases( "-o", "-out" ), Handlers.OutPath);
            commands.Add(new Command("-configuration")
                .addAliases("-config","-c"), Handlers.ConPath);
            commands.Add(new Command("-mode")
                .addAliases("-m", "-,"), Handlers.Mode); ;
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

            static string mode;
            public static void Mode(string[] args)
            {
                mode = "html";
            }
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
                Console.WriteLine(args[0] + ":" + args[1]);
                configPath = args[1];
            }
            static Dictionary<string, string> Template;
            public static void Execute()
            {
                //string instance of the text to be htmlified
                string txt = File.ReadAllText(originalPath);
                //array of lines in the config file
                string[] configText = File.ReadAllLines(configPath);
                //will hold the location of the template
                int temLoc = -1;
                //contains all k/v pairs within the template,wheter we support them or not.
                Template = new Dictionary<string, string>();
                foreach(string line in configText)
                {
                    Console.WriteLine("Reading Line");
                    //# demarcates comments
                    if (line.StartsWith("#")) continue;
                    //split into key/value pairs
                    string[] kp = line.Split('=');
                    //set the te,plate start location
                    if (line.Contains("&template")&& temLoc==-1)
                    {
                        temLoc = Array.IndexOf(configText, line);
                        break;
                    }
                    Console.WriteLine(kp.Length + ":" + line);
                        // k/v pair not present as in the &template tag
                        if (kp.Length < 2) continue;
                        
                    // split by the # char
                    string[] kpc = kp[1].Split('#');
                    // use only first value, cuts out all comments.
                    kp[1] = kpc[0];
                    Console.Write("k/v:");
                    Console.WriteLine("k:" + kp[0] + ",v:" + kp[1]);
                    Template.Add(kp[0], kp[1]);
                    
                }
                StringBuilder b = new StringBuilder();
                for (int i = temLoc+1; i < configText.Length; i++)
                {
                    string line = configText[i];
                    b.AppendLine(line);
                }

                ReadAccess r = ReadText(txt);
                string template = b.ToString();
                StringBuilder builder = new StringBuilder(template);
                Console.WriteLine(Template["template_title"]);
                builder.Replace(Template["template_title"], r.title);
                builder.Replace(Template["template_css_title"], Template["css_class_title"]);
                foreach (string paragraph in r.Paragraphs)
                {
                    if (paragraph != "\uE000")
                    {
                        builder.Replace(Template["template_body"], paragraph + "</"
                            + Template["template_body_tag"] + ">" + "\r\n<" + Template["template_body_tag"]
                            + " class='" + Template["template_css_body"] + "'>\r\n" + Template["template_body"]);
                    }
                    else
                    {
                        builder.Replace(Template["template_body"], "<br>\r\n<" + Template["template_body_tag"]
                            + " class='" + Template["template_css_body"] + "'>\r\n" + Template["template_body"]);
                    }
                }
                builder.Replace(Template["template_body"], "");
                builder.Replace(Template["template_css_body"], Template["css_class_body"]);
                File.WriteAllText(outputPath, builder.ToString());
                Console.WriteLine("Done.");
                Console.ReadKey();
            }
            static ReadAccess ReadText(string text)
            {
                ReadAccess r = new ReadAccess();
                r.Paragraphs = new List<string>();
                foreach (string line in text.Split('\n'))
                {
                    if (!r.titleDefined && !line.IsWhitespace())
                    {
                        r.title = line;
                        r.titleDefined = true;
                    } else if (!line.IsWhitespace()) {
                        r.Paragraphs.Add(line);
                             }
                    else
                    {
                        r.Paragraphs.Add("\uE000");
                    }
                }
                return r;
            }
            
        }
        
        struct ReadAccess
        {
            public List<string> Paragraphs;
            public string title;
            public bool titleDefined;
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
    public static class Extensions
    {
        public static bool IsWhitespace(this string text)
        {
            foreach (char c in text.ToCharArray())
            {
                if (!Char.IsWhiteSpace(c)) return false;
            }
            return true;
        }
    }
}
