// thanks http://blogs.msdn.com/b/kaelr/archive/2006/03/28/indentedtextwriter.aspx

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace CoreTechs.Logging
{
    /// <summary>
    ///     Provides a text writer that can indent new lines by a tab string token.
    /// </summary>
    /// <remarks>
    ///     This class provides more functionality and fixes a bug in <see cref="System.CodeDom.Compiler.IndentedTextWriter" />
    ///     .
    /// </remarks>
    [PermissionSet(SecurityAction.LinkDemand), PermissionSet(SecurityAction.InheritanceDemand)]
    public class IndentedTextWriter : System.CodeDom.Compiler.IndentedTextWriter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref='IndentedTextWriter' /> class using a new string writer and the default
        ///     tab string.
        /// </summary>
        public IndentedTextWriter()
            : base(new StringWriter((IFormatProvider) null))
        {
            FixIndentedTextWriterBug();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref='IndentedTextWriter' /> class using a new string writer and the
        ///     specified tab string.
        /// </summary>
        /// <param name="tabString">The tab string to use for indentation.</param>
        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters")]
        public IndentedTextWriter(string tabString)
            : base(new StringWriter((IFormatProvider) null), tabString)
        {
            FixIndentedTextWriterBug();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref='IndentedTextWriter' /> class using the specified text writer and
        ///     default tab string.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> to use for output.</param>
        public IndentedTextWriter(TextWriter writer)
            : base(writer)
        {
            FixIndentedTextWriterBug();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref='IndentedTextWriter' /> class using the specified text writer and tab
        ///     string.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter" /> to use for output.</param>
        /// <param name="tabString">The tab string to use for indentation.</param>
        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters")]
        public IndentedTextWriter(TextWriter writer, string tabString)
            : base(writer, tabString)
        {
            FixIndentedTextWriterBug();
        }

        /// <summary>
        ///     Returns the inner writer's ToString(), which in the case of a StringWriter is the buffer itself.
        /// </summary>
        /// <returns>InnerWriter's ToString().</returns>
        public override string ToString()
        {
            return InnerWriter != null ? InnerWriter.ToString() : base.ToString();
        }

        /// <summary>
        ///     The <see cref="System.CodeDom.Compiler.IndentedTextWriter" /> class has a bug in its constructor that
        ///     sets the private field <c>tabsPending</c> to <c>false</c> instead of <c>true</c>, which prevents the first line
        ///     written from being indented.
        ///     This function fixes that bug via reflection.
        /// </summary>
        private void FixIndentedTextWriterBug()
        {
            typeof (System.CodeDom.Compiler.IndentedTextWriter).GetField("tabsPending",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, true);
        }

        [StringFormatMethod("format")]
        public void WriteLines([NotNull] string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            
            var lines = string.Format(format, args ?? new object[0])
                .Split(new[] {Environment.NewLine, "\n"}, StringSplitOptions.None);

            foreach (var line in lines)
                WriteLine(line);
        }
    }
}