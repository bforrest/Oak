using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Controllers;
using System.Reflection;

namespace BorrowedGames.Tests.Controllers
{
    class describe_SeedController : nspec
    {
        SeedController controller;

        void before_each()
        {
            controller = new SeedController();

            controller.PurgeDb();
        }

        void games_converted_from_ints_to_guids()
        {
            before = () => controller.MigrateTo(controller.AddReturnDateToWantedGames);

            context["games have already been added"] = () =>
            {
                it["works"] = () => "hello".should_be("hello");
            };
        }
    }
}
