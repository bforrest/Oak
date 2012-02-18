using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class Book : DynamicModel
    {
        public Book()
            : this(new { })
        {

        }

        public Book(object dto)
            : base(dto)
        {
        }
        
        public IEnumerable<dynamic> Validates()
        {
            yield return new Presence("Title");

            yield return new Presence("Body");
        }
    }
}