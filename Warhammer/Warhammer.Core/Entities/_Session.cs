﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Warhammer.Core.Entities
{
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public partial class Session
    {
        public override double ActivityBonus
        {
            get
            {
                return MonthsAgo < 1 ? 1 : 1 / MonthsAgo;
            }
        }

        private double MonthsAgo
        {

            get
            {
                if (DateTime.HasValue)
                {
                    return System.DateTime.Now.Subtract(this.DateTime.Value).Days/(365.25/12);
                }
                else
                {
                    return 0;
                }
            }
        }

        public override double BaseScore
        {
            get
            {
                return 1.0;
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
    }
}
