using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oak;

namespace BorrowedGames.SchemaGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Seed seed = new Seed(new ConnectionProfile { ConnectionString = args[0] });

            seed.PurgeDb();
        }
    }
}
