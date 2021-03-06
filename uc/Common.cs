﻿using System;
using System.Collections.Generic;

namespace Translator
{

    /*
     * Compiler 2.0
     * sizeof(char) == sizeof(short) == sizeof(int16) == 2
     *
     * Valid digits:
     *  123
     *  123.456
     *  123u
     *  123ul
     *  123f
     *  123.0f
     *  0xCAFE
     *  0127
     *  0b011100110
     * Types:
     *  int
     *  string[]
     *  MyClass
     *  MyClass[][]
     *
     *
     *
     *
     *
    */

    public delegate void ErrorLimitReachedDelegate();


    interface IDirective
    {
    }

    class DirectiveList : List<IDirective>
    {
    }

    class Metadata : IDirective
    {
        public string Key;
        public DataTypes Type;
        public object Value;
        public bool Compilable = true;
    }

    class MetadataList : List<Metadata>
    {
    }

    class RuntimeMetadata : Metadata
    {
        public string Prefix;
    }

    public class CompilerConfig
    {
        public bool AllowBuiltins = false;
        public bool SaveAsm = false;
        public bool WriteIntermediateInfo = false;
        public bool PureBuild = false;
        public bool OnlyProduceAsmSource = false;
        public bool RunAssembler = false;   
        public bool DebugBuild = false;
        public string OutInfoFile = null;
        public string OutBinaryFile = null;
        public BinaryType BinaryType = BinaryType.Executable;
        public List<string> Sources = new List<string>();
        public List<string> Defines = new List<string>();
    }

    public static class InfoProvider
    {
        public static event ErrorLimitReachedDelegate ErrorLimitReached;

        public static List<Info> InfoList = new List<Info>();
        public static int ErrorLimit = 10;

        private static int errorCount = 0;

        public static void AddError(string what, ExceptionType ex, SourcePosition where)
        {                
            Add(InfoType.Error, what, ex, where);
            errorCount++;
            if(errorCount >  ErrorLimit)
                ErrorLimitReached?.Invoke();            
        }

        public static void AddWarning(string what, ExceptionType ex, SourcePosition where)
        {
            Add(InfoType.Warning, what, ex, where);
        }

        public static void AddInfo(string what, ExceptionType ex, SourcePosition where)
        {
            Add(InfoType.Info, what, ex, where);
        }

        public static void Add(InfoType type, string what, ExceptionType ex, SourcePosition where)
        {
            InfoList.Add(new Info(type, what, ex, where));
        }

        public static void Print()
        {
            foreach (var i in InfoList)
            {
                var color = Console.ForegroundColor;
                switch (i.Type)
                {
                    case InfoType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case InfoType.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case InfoType.Info:
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                }
                Console.Write(i.Type);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(" at " + i.Where);
                Console.ForegroundColor = color;
                Console.WriteLine("\t`{0}`:{1}", i.ExceptionType, i.What);
            }
        }
    }

    public class Info
    {
        string what;
        InfoType type;
        ExceptionType ex;
        SourcePosition where;

        public Info(InfoType type, string what, SourcePosition where)
        {
            this.type = type;
            this.what = what;
            this.ex = ExceptionType.None;
            this.where = where;
        }

        public Info(InfoType type, string what)
        {
            this.type = type;
            this.what = what;
            this.ex = ExceptionType.None;
            this.where = null;
        }

        public Info(InfoType type, string what, ExceptionType ex, SourcePosition where)
        {
            this.type = type;
            this.what = what;
            this.ex = ex;
            this.where = where;
        }

        public InfoType Type => type;
        public ExceptionType ExceptionType => ex;
        public SourcePosition Where => where;
        public string What => what;

        /*public override string ToString()
        {
            return string.Format("{0} at ({1}:{2}:{3}): {4}\n\t`{5}`: {6}", 
                type.ToString(), where.File, where.LineNum, where.TokenPos, where.Line, ex.ToString(), what);
        }*/
    }
}

