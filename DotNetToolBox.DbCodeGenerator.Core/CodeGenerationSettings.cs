using System;

namespace DotNetToolBox.DbCodeGenerator.Core
{
    public class CodeGenerationSettings
    {
        public Int32 CSharpFilesCodePage { get; set; }
        public string CSharpIndentType { get; set; }
        public Int32 CSharpIndentSize { get; set; }
        public string SqlIndentType { get; set; }
        public Int32 SqlIndentSize { get; set; }
    }
}
