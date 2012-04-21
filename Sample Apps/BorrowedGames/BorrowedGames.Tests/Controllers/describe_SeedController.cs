using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Controllers;
using System.Reflection;
using Oak;
using BorrowedGames.Models;

namespace BorrowedGames.Tests.Controllers
{
    class describe_SeedController : nspec
    {
        SeedController controller;

        Games games;

        object gameId;

        void before_each()
        {
            controller = new SeedController();

            games = new Games();

            controller.PurgeDb();
        }

        void games_converted_from_ints_to_guids()
        {
            before = () => controller.MigrateUpTo(controller.GameIdsFromIntToGuids);

            act = () => controller.Seed.ExecuteNonQuery(controller.GameIdsFromIntToGuids());

            context["games have already been added"] = () =>
            {
                before = () => gameId = new { Name = "Gears of War" }.InsertInto("Games");

                it["game's integer based id is converted to guid"] = () =>
                {
                    var game = games.All().First();

                    (game.Name as string).should_be("Gears of War");

                    (game.Id5.GetType() as Type).should_be(typeof(Guid));
                };
            };
        }
    }
}
