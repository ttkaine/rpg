using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Warhammer.Core.Entities
{
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public partial class Session
    {
        public bool CreateWithPreviousCharacterList { get; set; }

        public override double BaseScore
        {
            get
            {
                return 1.0;
            }     
        }

        public override double AgeInDays
        {
            get
            {
                if (DateTime.HasValue)
                {
                    TimeSpan span = System.DateTime.Now - DateTime.Value;
                    double days = span.TotalDays;
                    return days;
                }
                else
                {
                    return 1000;
                }
            }
        }

        public IEnumerable<Person> People
        {
            get { return Related.OfType<Person>(); }
        }

        public IEnumerable<Person> Npcs
        {
            get { return People.Where(p => !p.PlayerId.HasValue); }
        }

        public IEnumerable<Person> PlayerCharacters
        {
            get { return People.Where(p => p.PlayerId.HasValue); }
        }

	    public DateTime LastPostTime
	    {
		    get
		    {
			    Post lastPost = Posts.OrderByDescending(p => p.DatePosted).FirstOrDefault();
			    return lastPost != null ? lastPost.DatePosted : System.DateTime.MinValue;
		    }
	    }
    }
}
