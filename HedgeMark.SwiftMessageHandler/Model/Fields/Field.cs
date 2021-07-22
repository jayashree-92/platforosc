﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HedgeMark.SwiftMessageHandler.Utils;

namespace HedgeMark.SwiftMessageHandler.Model.Fields
{
    public class Field
    {
        private string value;

        private static readonly List<char> InvalidFieldValues = new List<char>() { ':', '{', '}' };

        /// <summary>
        /// https://www2.swift.com/knowledgecentre/publications/usgi/1.0?topic=con_31519.htm
        /// </summary>
        protected static readonly string RegexNotAllowedSwiftXCharacterSet = @"[^a-zA-Z\d\s\/\-\?\:\(\)\.\,\'\+]";

        public Field(string name)
        {
            Name = name;
            Value = string.Empty;
            Components = new List<string>();
        }

        public string Name { get; set; }
        public string Label { get { return FieldDirectory.Labels.ContainsKey(Name) ? FieldDirectory.Labels[Name] : string.Format("Field {0}", Name); } }

        public string Value
        {
            get { return value; }
            protected set
            {
                if (value.Any(s => InvalidFieldValues.Contains(s)))
                    throw new InvalidDataException(string.Format("{0}: The field value cannot contain characters like '{{','}}' or ':' in {1}", Name, value));

                this.value = value;
            }
        }

        public string GetFieldAndValue
        {
            get
            {
                var val = GetValue();
                return string.IsNullOrWhiteSpace(val) ? string.Empty : string.Format("{0}:{1}", Name, val);
            }
        }

        public virtual string GetValue()
        {
            return Value;
        }

        public virtual List<string> Components { get; set; }
        public virtual string GetComponentValue(string component)
        {
            return component.GetComponentValue(value);
        }

        public virtual void SetValue(string newValue)
        {
            Value = newValue;
        }

        public Field setValue(string newValue)
        {
            Value = newValue;
            return this;
        }

        public static string RemoveInvalidXCharacterSet(string inputLine)
        {
            if (string.IsNullOrWhiteSpace(inputLine))
                return inputLine;

            inputLine = Regex.Replace(inputLine, RegexNotAllowedSwiftXCharacterSet, string.Empty);
            inputLine = Regex.Replace(inputLine, @"\t|\n|\r", string.Empty);
            return inputLine;
        }
    }
}
