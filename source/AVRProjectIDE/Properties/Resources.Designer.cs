﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AVRProjectIDE.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AVRProjectIDE.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://frank.circleofcurrent.com/usnoobie/.
        /// </summary>
        internal static string AdURL {
            get {
                return ResourceManager.GetString("AdURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #include &lt;WProgram.h&gt;
        ///
        ///int main(void)
        ///{
        ///	init();
        ///
        ///	setup();
        ///    
        ///	for (;;)
        ///		loop();
        ///        
        ///	return 0;
        ///}
        ///
        ///.
        /// </summary>
        internal static string arduinomain {
            get {
                return ResourceManager.GetString("arduinomain", resourceCulture);
            }
        }
        
        internal static byte[] arduinomakefile {
            get {
                object obj = ResourceManager.GetObject("arduinomakefile", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;AutoComplete&gt;
        ///  &lt;C&gt;
        ///    &lt;Group type=&quot;statements&quot;&gt;
        ///      if else do while for switch case default break goto continue return sizeof main
        ///    &lt;/Group&gt;
        ///    &lt;Group type=&quot;types&quot;&gt;
        ///      void bool boolean byte char short int long word dword float double bool bit bitfield byte uchar ushort uint ulong uword struct union enum
        ///      int8_t int16_t int32_t int64_t uint8_t uint16_t uint32_t uint64_t
        ///    &lt;/Group&gt;
        ///    &lt;Group type=&quot;modifiers&quot;&gt;
        ///      signed unsigned static [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string autocomplete {
            get {
                return ResourceManager.GetString("autocomplete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;ExternalTools&gt;
        ///  &lt;Tool text=&quot;Edit Project File in Notepad&quot; cmd=&quot;notepad&quot; dir=&quot;&quot; args=&quot;%PROJNAME%.avrproj&quot; /&gt;
        ///&lt;/ExternalTools&gt;
        ///.
        /// </summary>
        internal static string ext_tools {
            get {
                return ResourceManager.GetString("ext_tools", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://frank.circleofcurrent.com/.
        /// </summary>
        internal static string FranksSiteURL {
            get {
                return ResourceManager.GetString("FranksSiteURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;HelpLinks&gt;
        ///  &lt;Link Text=&quot;AVR-Libc Reference&quot; URL=&quot;http://www.nongnu.org/avr-libc/user-manual/&quot;&gt;
        ///    &lt;Link Text=&quot;Library Reference&quot; URL=&quot;http://www.nongnu.org/avr-libc/user-manual/modules.html&quot; /&gt;
        ///    &lt;Link Text=&quot;Interrupts&quot; URL=&quot;http://www.nongnu.org/avr-libc/user-manual/group__avr__interrupts.html&quot; /&gt;
        ///    &lt;Link Text=&quot;FAQ&quot; URL=&quot;http://www.nongnu.org/avr-libc/user-manual/FAQ.html&quot; /&gt;
        ///  &lt;/Link&gt;
        ///  &lt;Link Text=&quot;Baud Rate Chart&quot; URL=&quot;http://www.wormfood.net/avrbaudc [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string helplinks {
            get {
                return ResourceManager.GetString("helplinks", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://code.google.com/p/avr-project-ide/wiki/Help.
        /// </summary>
        internal static string HelpURL {
            get {
                return ResourceManager.GetString("HelpURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;us-ascii&quot;?&gt;
        ///&lt;InterruptVectors&gt;
        ///	&lt;ListOfVectors&gt;
        ///		&lt;Vector Text=&quot;ADC Conversion Complete&quot;&gt;
        ///			&lt;NewName&gt;ADC_vect&lt;/NewName&gt;
        ///			&lt;OldName&gt;SIG_ADC&lt;/OldName&gt;
        ///			&lt;Desc&gt;ADC Conversion Complete&lt;/Desc&gt;
        ///		&lt;/Vector&gt;
        ///		&lt;Vector Text=&quot;Analog Comparator 0&quot;&gt;
        ///			&lt;NewName&gt;ANALOG_COMP_0_vect&lt;/NewName&gt;
        ///			&lt;OldName&gt;SIG_COMPARATOR0&lt;/OldName&gt;
        ///			&lt;Desc&gt;Analog Comparator 0&lt;/Desc&gt;
        ///		&lt;/Vector&gt;
        ///		&lt;Vector Text=&quot;Analog Comparator 1&quot;&gt;
        ///			&lt;NewName&gt;ANALOG_COMP_1_vect&lt;/NewName&gt;
        ///			&lt;OldName&gt;SIG_COMP [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string interruptvectors {
            get {
                return ResourceManager.GetString("interruptvectors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 0.
        /// </summary>
        internal static string PanelWorkspaceVersion {
            get {
                return ResourceManager.GetString("PanelWorkspaceVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;Styles&gt;
        ///  &lt;Style Name=&quot;ASM&quot;						/&gt;
        ///  &lt;Style Name=&quot;ATTRIBUTE&quot;					ForeColor=&quot;Red&quot; /&gt;
        ///  &lt;Style Name=&quot;BACKTICKS&quot;					ForeColor=&quot;Red&quot; /&gt;
        ///  &lt;Style Name=&quot;BINARY&quot;					ForeColor=&quot;Orange&quot; /&gt;
        ///  &lt;Style Name=&quot;BINNUMBER&quot;					ForeColor=&quot;Orange&quot; /&gt;
        ///  &lt;Style Name=&quot;BRACELIGHT&quot;				ForeColor=&quot;Red&quot; Bold=&quot;True&quot;/&gt;
        ///  &lt;Style Name=&quot;BLOCK_COMMENT&quot;				ForeColor=&quot;Green&quot; /&gt;
        ///  &lt;Style Name=&quot;CHAR&quot;						ForeColor=&quot;Red&quot; /&gt;
        ///  &lt;Style Name=&quot;CHARACTER&quot;					ForeColor=&quot;Red&quot; /&gt;
        ///  &lt;Style Name=&quot;CLA [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string scintconfig {
            get {
                return ResourceManager.GetString("scintconfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;ProjTemplates&gt;
        ///
        ///  &lt;template name=&quot;Blank Template&quot;&gt;
        ///
        ///  &lt;/template&gt;
        ///
        ///  &lt;template name=&quot;Pin Definitions and Macros&quot;&gt;
        ///    &lt;CreateFile name=&quot;pindefs.h&quot;&gt;
        ///      &lt;Template&gt;defaultheader.txt&lt;/Template&gt;
        ///    &lt;/CreateFile&gt;
        ///    &lt;CreateFile name=&quot;macros.h&quot;&gt;
        ///      &lt;Template&gt;defaultheader.txt&lt;/Template&gt;
        ///    &lt;/CreateFile&gt;
        ///  &lt;/template&gt;
        ///
        ///  &lt;template name=&quot;Maximum Optimization&quot;&gt;
        ///    &lt;Optimization&gt;-Os&lt;/Optimization&gt;
        ///    &lt;PackStructs&gt;true&lt;/PackStructs&gt;
        ///    &lt;ShortEnums&gt; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string templates {
            get {
                return ResourceManager.GetString("templates", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://code.google.com/p/avr-project-ide/wiki/UsbInfoPanel.
        /// </summary>
        internal static string UsbInfoPanelWikiURL {
            get {
                return ResourceManager.GetString("UsbInfoPanelWikiURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://code.google.com/p/avr-project-ide/.
        /// </summary>
        internal static string WebsiteURL {
            get {
                return ResourceManager.GetString("WebsiteURL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://code.google.com/p/avr-project-ide/wiki/SystemRequirements.
        /// </summary>
        internal static string WinAVRURL {
            get {
                return ResourceManager.GetString("WinAVRURL", resourceCulture);
            }
        }
    }
}
