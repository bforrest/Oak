using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class Person : DynamicModel
    {
        public Person()
            : this(new { })
        {
        }

        public Person(object dto)
            : base(dto)
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Confirmation("Email");
        }
    }
}
