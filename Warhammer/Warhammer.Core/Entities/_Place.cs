using System.CodeDom.Compiler;

namespace Warhammer.Core.Entities
{
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public partial class Place
    {
        public string Breadcrumb
        {
            get
            {
                if (Parent == null)
                {
                    return ShortName;
                }
                else
                {
                    return Parent.Breadcrumb + " > " + ShortName;
                }
            }
        }
    }
}
