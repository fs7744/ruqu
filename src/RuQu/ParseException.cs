﻿using System;

namespace RuQu
{
    public class ParseException : Exception
    {
        public ParseException(string? message) : base(message)
        {
        }
    }
}