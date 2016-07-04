using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Mvc.Controllers
{
    public class EnumDefinition
    {
        public string Area { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class TimiController : BaseController
    {
        // GET: Timi
        public TimiController(IAuthenticatedDataProvider data) : base(data)
        {
        }



        public ActionResult Index()
        {
            List<EnumDefinition> definitions = new List<EnumDefinition>();
            string assemblyName = "Warhammer.Core";

            Assembly assembly = System.Reflection.Assembly.Load(assemblyName);

            var things = assembly.GetTypes();

            string @namespace = "Warhammer.Core.Entities";

            var allEnums = from t in assembly.GetTypes() 
                    where t.IsEnum && t.Namespace == @namespace
                    select t;

            foreach (Type type in allEnums)
            {
                List<string> values = Enum.GetNames(type).ToList();
                foreach (string name in values)
                {
                  object o = Enum.Parse(type, name);
                  int value = Convert.ToInt32(o);
                    definitions.Add(new EnumDefinition
                    {
                        Area = type.Name,
                        Name = name,
                        Value = value  
                    });
                }
            }

            List<string> names = allEnums.Select(p => p.Name).ToList();


            List<Page> pages = DataProvider.RecentPages().ToList();
            return null;
        }
    }
}