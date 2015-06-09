﻿using System.Security.Cryptography.X509Certificates;

using BeanIO.Annotation;

namespace BeanIO.Parser.Annotation
{
    public interface IAnnotatedUserInterface
    {
        [Field(At = 3, MinOccurs = 2, RegEx = "((left)|(right))")]
        string[] Hands { get; }
    }
}