//
//  AtributteReader.cs
//
//  Author:
//       Andrew Senko <andrewsen98@gmail.com>
//
//  Copyright (c) 2015 Andrew Senko
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Compiler
{
    public struct AttributeData
    {
        internal DataTypes Type;
        internal object Value;
        internal bool IsOptional;
        internal string Key;
    }

    class AttributeDataList : List<AttributeData>
    {
        public bool HasKey(string key) {
            var keys = from a in this
                       where a.Key == key
                       select a;
            return keys.Any();
        }

        public AttributeData GetData(string key) {
            var keys = (from a in this
                        where a.Key == key
                        select a).ToList();
            if (!keys.Any())
                throw new IndexOutOfRangeException("No key " + key + " assigned");
            return keys.First();
        }

        public AttributeData this[string key] {
            get {
                return GetData(key);
            }
        }
    }

    public struct AttributeObject
    {
        internal string Name;
        internal bool Binded;
        internal bool Compilable;
        internal AttributeDataList Data;

        public AttributeObject(string name, bool compilable) {
            Name = name;
            Binded = false;
            Compilable = compilable;
            Data = new AttributeDataList();
        }
    }

    public class AttributeList : List<AttributeObject>
    {
        public bool HasAttribute(string name) {
            var keys = from a in this
                       where a.Name == name
                       select a;
            return keys.Any();
        }

        public AttributeObject GetAttrubute(string name) {
            var keys = (from a in this
                        where a.Name == name
                        select a).ToList();
            if (!keys.Any())
                throw new IndexOutOfRangeException("No attribute " + name + " found");
            return keys.First();
        }

        public AttributeObject this[string name] {
            get {
                return GetAttrubute(name);
            }
        }
    }

    public class AttributeReader
    {
        private TokenStream ts;
        private AttributeObject aobj = new AttributeObject();

        internal AttributeReader(TokenStream ts) {
            this.ts = ts;
        }

        public AttributeObject Read() {
            aobj = read();
            return aobj;
        }

        private AttributeObject read() {
            aobj.Binded = false;
            aobj.Data = new AttributeDataList();

            aobj.Name = ts.Next();
            if (!ts.Current.IsIdentifier())
                InfoProvider.AddError("Wrong name `" + aobj.Name + "`", ExceptionType.AttributeException, ts.SourcePosition);
            //throw new AttributeException(aobj.Name, "Wrong name");
            if (ts.IsNext(";")) // Like "RuntimeInternal;"
                return aobj;
            else if (ts.Is(":")) // Like "Debug:Log"
            {
                do {
                    aobj.Name += ":" + ts.Next();
                    if (!ts.Current.IsIdentifier())
                        InfoProvider.AddError("Wrong name `" + aobj.Name + "`", ExceptionType.AttributeException, ts.SourcePosition);
                } while (ts.IsNext(":"));
                //_ts.PushBack();
            }
            if (!ts.Is("(")) {
                aobj.Binded = true;
                ts.PushBack();
                return aobj;
            }

            if (!ts.IsNext("(")) {
                if (ts.IsNext(";"))
                    return aobj;
                aobj.Binded = true;
                ts.PushBack();
                return aobj;
            }
            //_ts.Next();
            while (true) {
                AttributeData ad = new AttributeData();
                ad.IsOptional = false;

                var type = ts.Current.ConstType;
                var val = ts.ToString();

                if (ts.Current.IsIdentifier()) //For smth like @Attr(<Key>=<Value>)
                {
                    if (ts.IsNext("=")) {
                        ad.IsOptional = true;
                        ad.Key = val;
                        val = ts.Next();
                        type = ts.Current.ConstType;
                    }
                    else //For @Attr(<Value>)
                        ts.PushBack();
                }

                switch (type) {
                    case ConstantType.Bool:
                        ad.Value = bool.Parse(val);
                        ad.Type = DataTypes.Bool;
                        break;
                    case ConstantType.Double:
                        ad.Value = ts.Current.GetDouble();
                        ad.Type = DataTypes.Double;
                        break;
                    case ConstantType.I32:
                        ad.Value = ts.Current.GetI32();
                        ad.Type = DataTypes.I32;
                        break;
                    case ConstantType.String:
                        var str = ts.Current.Unquoted;
                        ad.Value = str.Remove(str.LastIndexOf('"')).Replace("\\\"", "\"");
                        ad.Type = DataTypes.String;
                        break;
                    default:
                        InfoProvider.AddError("Unsupported data type: " + type.ToString().Replace("DataTypes.", ""),
                            ExceptionType.AttributeException, ts.SourcePosition);
                        break;
                }

                aobj.Data.Add(ad);

                if (ts.IsNext(")"))
                    break;
                else if (!ts.Is(","))
                    InfoProvider.AddError("Unexpected character " + ts, ExceptionType.AttributeException, ts.SourcePosition);
                ts.Next();

            }
            if (ts.IsNext(";"))
                return aobj;
            aobj.Binded = true;
            ts.PushBack();
            return aobj;
        }
    }
}

