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

        Library library;

        NotInterestedGames notInterestedGames;

        WantedGames wantedGames;

        object gameId, userId, userId2;

        void before_each()
        {
            controller = new SeedController();

            games = new Games();

            library = new Library();

            notInterestedGames = new NotInterestedGames();

            wantedGames = new WantedGames();

            controller.PurgeDb();
        }

        void SetupTwoUsers()
        {
            userId = new { Email = "user@example.com", Password = "password", Handle = "@user" }.InsertInto("Users");

            userId2 = new { Email = "user2@example.com", Password = "password", Handle = "@user2" }.InsertInto("Users");
        }

        void games_converted_from_ints_to_guids()
        {
            before = () =>
            {
                controller.MigrateUpTo(controller.GameIdsFromIntToGuids);

                SetupTwoUsers();
            };

            act = () => controller.ExecuteNonQuery(controller.GameIdsFromIntToGuids);

            context["game has already been added"] = () =>
            {
                before = () => gameId = new { Name = "Gears of War" }.InsertInto("Games");

                it["game's integer based id is converted to guid"] = () =>
                {
                    var game = FirstGame();

                    (game.Id.GetType() as Type).should_be(typeof(Guid));

                    (game.Name as string).should_be("Gears of War");
                };

                context["user has game in library"] = () =>
                {
                    before = () => new { userId, gameId }.InsertInto("Library");

                    it["the game in the user's library is also updated"] = () =>
                        (library.All().First().GameId as object).should_be(FirstGame().Id as object);
                };

                context["user has game he is not interested in"] = () =>
                {
                    before = () => new { userId, gameId }.InsertInto("NotInterestedGames");

                    it["the game in the user's not interested list is also updated"] = () =>
                        (notInterestedGames.All().First().GameId as object).should_be(FirstGame().Id as object);
                };

                context["user wants game that another use has"] = () =>
                {
                    before = () => new { UserId = userId, GameId = gameId, FromUserId = userId2 }.InsertInto("WantedGames");

                    it["the game in the wanted queue is also updated"] = () =>
                        (wantedGames.All().First().GameId as object).should_be(FirstGame().Id as object);
                };
            };
        }

        dynamic FirstGame()
        {
            return games.All().First();
        }
    }
}
