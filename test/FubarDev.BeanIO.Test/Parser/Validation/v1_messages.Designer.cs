﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BeanIO.Parser.Validation {
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
    public class v1_messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal v1_messages() {
        }
        
        /// <summary>
        ///    Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BeanIO.Parser.Validation.v1_messages", typeof(v1_messages).Assembly);
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
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Invalid {2} at line {0} on {1}, expected &apos;{4}&apos;' ähnelt.
        /// </summary>
        public static string fielderror_literal_field_literal {
            get {
                return ResourceManager.GetString("fielderror.literal.field.literal", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'maxLength({5}) at line {0}' ähnelt.
        /// </summary>
        public static string fielderror_maxLength {
            get {
                return ResourceManager.GetString("fielderror.maxLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'minLength({4}) at line {0}' ähnelt.
        /// </summary>
        public static string fielderror_minLength {
            get {
                return ResourceManager.GetString("fielderror.minLength", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'regex(&apos;{4}&apos;) at line {0}' ähnelt.
        /// </summary>
        public static string fielderror_regex {
            get {
                return ResourceManager.GetString("fielderror.regex", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'required at line {0}' ähnelt.
        /// </summary>
        public static string fielderror_required {
            get {
                return ResourceManager.GetString("fielderror.required", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'type at line {0}' ähnelt.
        /// </summary>
        public static string fielderror_typeHandler_type {
            get {
                return ResourceManager.GetString("fielderror.typeHandler.type", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Literal Record' ähnelt.
        /// </summary>
        public static string label_literal {
            get {
                return ResourceManager.GetString("label.literal", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Sucht nach einer lokalisierten Zeichenfolge, die 'Literal Field' ähnelt.
        /// </summary>
        public static string label_literal_field {
            get {
                return ResourceManager.GetString("label.literal.field", resourceCulture);
            }
        }
    }
}
