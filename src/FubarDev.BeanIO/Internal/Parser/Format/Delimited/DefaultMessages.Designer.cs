﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BeanIO.Internal.Parser.Format.Delimited {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///    Eine stark typisierte Ressourcenklasse zum Suchen nach lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder-Klasse
    // mithilfe eines Tools wie ResGen oder Visual Studio automatisch generiert.
    // Bearbeiten Sie zum Hinzufügen oder Entfernen eines Members die .ResX-Datei, und führen Sie dann ResGen
    // mit der Option "/str" erneut aus, oder erstellen Sie das VS-Projekt neu.
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class DefaultMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal DefaultMessages() {
        }
        
        /// <summary>
        ///    Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BeanIO.Internal.Parser.Format.Delimited.DefaultMessages", typeof(DefaultMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///    Überschreibt die Eigenschaft 'CurrentUICulture' des aktuellen Threads für alle
        ///    Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Invalid padded field length, expected {4} characters' ähnelt.
        /// </summary>
        public static string fielderror_length {
            get {
                return ResourceManager.GetString("fielderror.length", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'literal value &apos;{4}&apos;' ähnelt.
        /// </summary>
        public static string fielderror_literal {
            get {
                return ResourceManager.GetString("fielderror.literal", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Maximum field length is {5} characters' ähnelt.
        /// </summary>
        public static string fielderror_maxLength {
            get {
                return ResourceManager.GetString("fielderror.maxLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Expected maximum {5} occurrences' ähnelt.
        /// </summary>
        public static string fielderror_maxOccurs {
            get {
                return ResourceManager.GetString("fielderror.maxOccurs", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Minimum field length is {4} characters' ähnelt.
        /// </summary>
        public static string fielderror_minLength {
            get {
                return ResourceManager.GetString("fielderror.minLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Expected minimum {4} occurrences' ähnelt.
        /// </summary>
        public static string fielderror_minOccurs {
            get {
                return ResourceManager.GetString("fielderror.minOccurs", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Unmatched regular expression &apos;{4}&apos;' ähnelt.
        /// </summary>
        public static string fielderror_regex {
            get {
                return ResourceManager.GetString("fielderror.regex", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Required field not set' ähnelt.
        /// </summary>
        public static string fielderror_required {
            get {
                return ResourceManager.GetString("fielderror.required", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Type conversion error: {4}' ähnelt.
        /// </summary>
        public static string fielderror_type {
            get {
                return ResourceManager.GetString("fielderror.type", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Malformed record at line {0}: {3}' ähnelt.
        /// </summary>
        public static string recorderror_malformed {
            get {
                return ResourceManager.GetString("recorderror.malformed", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Too many fields, expected {4} maximum' ähnelt.
        /// </summary>
        public static string recorderror_maxLength {
            get {
                return ResourceManager.GetString("recorderror.maxLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Too few fields, expected {3} minimum' ähnelt.
        /// </summary>
        public static string recorderror_minLength {
            get {
                return ResourceManager.GetString("recorderror.minLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Unexpected {1} record at line {0}' ähnelt.
        /// </summary>
        public static string recorderror_unexpected {
            get {
                return ResourceManager.GetString("recorderror.unexpected", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Unidentified record at line {0}' ähnelt.
        /// </summary>
        public static string recorderror_unidentified {
            get {
                return ResourceManager.GetString("recorderror.unidentified", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Expected record/group {1} at line {0}' ähnelt.
        /// </summary>
        public static string recorderror_unsatisfied {
            get {
                return ResourceManager.GetString("recorderror.unsatisfied", resourceCulture);
            }
        }
    }
}
