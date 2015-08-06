using System;
using System.CodeDom.Compiler;
using System.Linq;

namespace Warhammer.Core.Entities
{
       [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public partial class SessionLog
    {
           public override double BaseScore
           {
               get
               {
                   if (string.IsNullOrWhiteSpace(Description))
                   {
                       return -1;
                   }
                   string theContent = Description.Trim();
                   while (theContent.Contains("  "))
                   {
                       theContent = theContent.Replace("  ", " ");
                   }
                   int words = theContent.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Count();
                   double baseScore = words / 1000.0;
                   return baseScore;
               }
           }
    }
}
