// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tlangbc
{
    public enum Argtype
    {
        STATIC,
        VARIABLE
    }
    public class Command
    {
        public String id;
        public Byte byteid;
        public int argsLength;

        public Command(String id, Byte byteid, int argslength)
        {
            this.id = id;
            this.byteid = byteid;
            this.argsLength = argslength;
        }
    }

    public class Argument
    {
        public Argtype type;
        public int value;

        public Argument(Argtype type, int value)
        {
            this.type = type;
            this.value = value;
        }
    }
    
    static class Program
    {

        public static List<Command> Commands = new List<Command>();

        public static void initCommands()
        {
            Commands.Add(new Command("INC", 0x10, 2));
            Commands.Add(new Command("DEC", 0x11, 2));
            Commands.Add(new Command("PRINTA", 0x12, 1));
            Commands.Add(new Command("PRINTB", 0x13, 1));
            Commands.Add(new Command("GOTO", 0x14, 1));
            Commands.Add(new Command("TEST", 0x15, 2));
        }

        public static Command CommandfromID(String ID)
        {
            foreach (var command in Commands)
            {
                if (command.id == ID)
                    return command;
            }

            return null;
        }

        public static void WriteCompiledCommandToFile(Command command, Argument[] args, String filePath)
        {
            List<Byte> appendData = new List<byte>();
            appendData.Add(command.byteid);
            foreach (var arg in args)
            {
                Byte type = 0x45;
                if (arg.type == Argtype.VARIABLE)
                    type = 0x46;
                appendData.Add(type);
                appendData.Add((byte)arg.value);
            }
            File.WriteAllBytes(filePath, File.ReadAllBytes(filePath).Concat(appendData).ToArray());
        }
        
        static void Main(String[] args)
        {
            initCommands();
            String inPath = args[0];
            String outPath = args[1];
            File.WriteAllBytes(outPath, new Byte[0]);
            String[] fileLines = File.ReadAllLines(inPath);
            for (int line = 0; line < fileLines.Length; line++)
            {
                if(fileLines[line].StartsWith("//") || string.IsNullOrWhiteSpace(fileLines[line]))
                    continue;

                String fileLine = fileLines[line];
                
                //Gotta parse me some args!

                Command command = CommandfromID(fileLines[line].Split(' ')[0].Trim());

                String[] strArgs = fileLines[line].Split(' ').Skip(1).ToArray();
                List<Argument> arguments = new List<Argument>();

                foreach (var arg in strArgs)
                {
                    Argtype type = (arg.StartsWith('$')) ? Argtype.VARIABLE : Argtype.STATIC;
                    int value = (type == Argtype.STATIC)
                        ? Int32.Parse(arg)
                        : Int32.Parse(arg.Substring(1));
                    arguments.Add(new Argument(type, value));
                }

                WriteCompiledCommandToFile(command, arguments.ToArray(), outPath);
            }
        }
    }
}