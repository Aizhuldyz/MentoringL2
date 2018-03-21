using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HomeTask2.E3SClient.Entities;
using HomeTask2.E3SClient;
using System.Configuration;
using System.Linq;

namespace HomeTask2
{
	[TestClass]
	public class E3SProviderTests
	{
		[TestMethod]
		public void WithoutProvider()
		{
			var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"] , ConfigurationManager.AppSettings["password"]);
			var res = client.SearchFTS<EmployeeEntity>("workstation:(EPRUIZHW0249)", 0, 1);

			foreach (var emp in res)
			{
				Console.WriteLine("{0} {1}", emp.nativename, emp.shortStartWorkDate);
			}
		}

		[TestMethod]
		public void WithoutProviderNonGeneric()
		{
			var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
			var res = client.SearchFTS(typeof(EmployeeEntity), "workstation:(EPRUIZHW0249)", 0, 10);

			foreach (var emp in res.OfType<EmployeeEntity>())
			{
				Console.WriteLine("{0} {1}", emp.nativename, emp.shortStartWorkDate);
			}
		}


		[TestMethod]
		public void WithProviderMemberLeft()
		{
			var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

			foreach (var emp in employees.Where(e => e.workstation == "EPRUIZHW0249"))
			{
				Console.WriteLine("{0} {1}", emp.nativename, emp.shortStartWorkDate);
			}
        }

        [TestMethod]
        public void WithProviderMemberRight()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => "EPRUIZHW0249" == e.workstation))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.shortStartWorkDate);
            }
        }

        [TestMethod]
        public void WithProviderMemberWithContains()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.manager.Contains("Grafkin")))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.shortStartWorkDate);
            }
        }

        [TestMethod]
        public void WithProviderMemberWithStartsWith()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.unit.StartsWith("Victor")))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.shortStartWorkDate);
            }
        }

	    [TestMethod]
	    public void WithProviderQueryWithAnd()
	    {
	        var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

	        foreach (var emp in employees.Where(e => e.unit.StartsWith("Victor") && e.nativename.Contains("Айжулдыз")))
	        {
	            Console.WriteLine("{0} {1}", emp.nativename, emp.shortStartWorkDate);
	        }
        }
	}
}
