using System;
using System.CodeDom.Compiler;
using System.Linq;

namespace Warhammer.Core.Entities
{
       [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public partial class SessionLog
    {
           public new double BaseScore
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
                   double baseScore = words / 2000.0;

                   if (baseScore > 1)
                   {
                       baseScore = 1;
                   }                  
                  
                   return baseScore + 1;
               }
           }
    }
}
