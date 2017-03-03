using NUnit.Framework;
using Warhammer.Core.Entities;

namespace Warhammer.Tests.Smoke.UnitTests
{
    [TestFixture]
    [Category("Unit")]
    public class MoneyTests
    {
        [Test]
        public void Spend240Removes1Crown()
        {


            Person person = new Person
            {
                Crowns = 2,
                Shillings = 2,
                Pennies = 2
            };

            int amountToSpend = 240;
            int original = person.TotalPennies;

            person.DeductMoney(amountToSpend);

            Assert.AreEqual(1, person.Crowns, "One Crown Left");
            Assert.AreEqual(2, person.Shillings, "Two Shillings Left");
            Assert.AreEqual(2, person.Pennies, "Two Pennies Left");

            Assert.AreEqual(original - amountToSpend, person.TotalPennies, "Net effect shouold be the right amount taken off");
        }

        [Test]
        public void Spend241Removes1CrownAnd1Penny()
        {
            Person person = new Person
            {
                Crowns = 2,
                Shillings = 2,
                Pennies = 2
            };

            int amountToSpend = 241;
            int original = person.TotalPennies;

            person.DeductMoney(amountToSpend);

            Assert.AreEqual(1, person.Crowns, "One Crown Left");
            Assert.AreEqual(2, person.Shillings, "Two Shillings Left");
            Assert.AreEqual(1, person.Pennies, "One Pennies Left");


            Assert.AreEqual(original - amountToSpend, person.TotalPennies, "Net effect shouold be the right amount taken off");
        }



        [Test]
        public void Spend120Removes1CrownAndAdds10Shillings()
        {
            Person person = new Person
            {
                Crowns = 2,
                Shillings = 2,
                Pennies = 2
            };

            int amountToSpend = 120;
            int original = person.TotalPennies;

            person.DeductMoney(amountToSpend);

            Assert.AreEqual(1, person.Crowns, "One Crown Left");
            Assert.AreEqual(12, person.Shillings, "Twelve Shillings Left");
            Assert.AreEqual(2, person.Pennies, "two Pennies Left");

            Assert.AreEqual(original - amountToSpend, person.TotalPennies, "Net effect shouold be the right amount taken off");
        }

        [Test]
        public void Spend200Removes1CrownAndAdds3ShillingsAnd4Pence()
        {
            Person person = new Person
            {
                Crowns = 2,
                Shillings = 2,
                Pennies = 2
            };

            int amountToSpend = 200;
            int original = person.TotalPennies;

            person.DeductMoney(amountToSpend);


            Assert.AreEqual(original - amountToSpend, person.TotalPennies, "Net effect shouold be the right amount taken off");

            Assert.AreEqual(1, person.Crowns, "One Crown Left");
            Assert.AreEqual(5, person.Shillings, "Five Shillings Left");
            Assert.AreEqual(6, person.Pennies, "six Pennies Left");
        }


        [Test]
        public void Spend1Removes1CrownAndAdds19ShillingsAnd11Pence()
        {
            Person person = new Person
            {
                Crowns = 100,
                Shillings = 0,
                Pennies = 0
            };

            int amountToSpend = 1;
            int original = person.TotalPennies;

            person.DeductMoney(amountToSpend);


            Assert.AreEqual(original - amountToSpend, person.TotalPennies, "Net effect shouold be the right amount taken off");

            Assert.AreEqual(99, person.Crowns, "99 Crowns Left");
            Assert.AreEqual(19, person.Shillings, "19 Shillings Left");
            Assert.AreEqual(11, person.Pennies, "11 Pennies Left");
        }


        [Test]
        public void Spend241Removes2CrownAndAdds19ShillingsAnd11Pence()
        {
            Person person = new Person
            {
                Crowns = 100,
                Shillings = 0,
                Pennies = 0
            };

            int amountToSpend = 241;
            int original = person.TotalPennies;

            person.DeductMoney(amountToSpend);


            Assert.AreEqual(original - amountToSpend, person.TotalPennies, "Net effect shouold be the right amount taken off");

            Assert.AreEqual(98, person.Crowns, "89 Crowns Left");
            Assert.AreEqual(19, person.Shillings, "19 Shillings Left");
            Assert.AreEqual(11, person.Pennies, "11 Pennies Left");
        }


        [Test]
        public void Spend242Removes2CrownAndAdds19ShillingsAnd11PenceIfOnly1Pence()
        {
            Person person = new Person
            {
                Crowns = 0,
                Shillings = 0,
                Pennies = 1
            };

            int amountToSpend = 242;
            int original = person.TotalPennies;

            person.DeductMoney(amountToSpend);


            Assert.AreEqual(original - amountToSpend, person.TotalPennies, "Net effect shouold be the right amount taken off");

            Assert.AreEqual(-1, person.Crowns, "-1 Crowns Left");
            Assert.AreEqual(0, person.Shillings, "no Shillings Left");
            Assert.AreEqual(-1, person.Pennies, "-1 Pennies Left");
        }

        [Test]
        public void AddMoneyAddsCrowns()
        {
            Person person = new Person
            {
                Crowns = 0,
                Shillings = 0,
                Pennies = 1
            };


            int amountToAdd = 240;
            int original = person.TotalPennies;

            person.AddMoney(amountToAdd);

            Assert.AreEqual(original + amountToAdd, person.TotalPennies, "Net effect shouold be the right amount added");

            Assert.AreEqual(1, person.Crowns, "1 Crowns Left");
            Assert.AreEqual(0, person.Shillings, "no Shillings Left");
            Assert.AreEqual(1, person.Pennies, "1 Pennies Left");
        }


        [Test]
        public void AddMoneyAddsCrownsShillingsAndPence()
        {
            Person person = new Person
            {
                Crowns = 0,
                Shillings = 0,
                Pennies = 0
            };


            int amountToAdd = 253;
            int original = person.TotalPennies;

            person.AddMoney(amountToAdd);

            Assert.AreEqual(original + amountToAdd, person.TotalPennies, "Net effect shouold be the right amount added");

            Assert.AreEqual(1, person.Crowns, "1 Crowns Left");
            Assert.AreEqual(1, person.Shillings, "1 Shillings Left");
            Assert.AreEqual(1, person.Pennies, "1 Pennies Left");
        }
    }
}