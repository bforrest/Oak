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
        Games games;

        Library library;

        NotInterestedGames notInterestedGames;

        WantedGames wantedGames;

        object gameId, userId, userId2;

        Schema schema;

        Seed seed;

        void before_each()
        {
            seed = new Seed();

            schema = new Schema(seed);

            games = new Games();

            library = new Library();

            notInterestedGames = new NotInterestedGames();

            wantedGames = new WantedGames();

            seed.PurgeDb();
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
                schema.MigrateUpTo(schema.GameIdsFromIntToGuids);

                SetupTwoUsers();
            };

            act = () => schema.ExecuteNonQuery(schema.GameIdsFromIntToGuids);

            context["game has already been added"] = () =>
            {
                before = () => gameId = new { Name = "Gears of War" }.InsertInto("Games");

                it["game's integer based id is converted to guid"] = () =>
                {
                    var game = Game();

                    (game.Id.GetType() as Type).should_be(typeof(Guid));

                    (game.Name as string).should_be("Gears of War");
                };

                it["old game id is removed"] = () =>
                    ((bool)Game().RespondsTo("IdOld")).should_be_false();

                context["user has game in library"] = () =>
                {
                    before = () => new { userId, gameId }.InsertInto("Library");

                    it["the game in the user's library is also updated"] = () =>
                        (Library().GameId as object).should_be(Game().Id as object);

                    it["old game id is removed"] = () =>
                        ((bool)Library().RespondsTo("GameIdOld")).should_be_false();
                };

                context["user has game he is not interested in"] = () =>
                {
                    before = () => new { userId, gameId }.InsertInto("NotInterestedGames");

                    it["the game in the user's not interested list is also updated"] = () =>
                        (NotInterestedGame().GameId as object).should_be(Game().Id as object);

                    it["old game id is removed"] = () =>
                        ((bool)NotInterestedGame().RespondsTo("GameIdOld")).should_be_false();
                };

                context["user wants game that another use has"] = () =>
                {
                    before = () => new { UserId = userId, GameId = gameId, FromUserId = userId2 }.InsertInto("WantedGames");

                    it["the game in the wanted queue is also updated"] = () =>
                        (WantedGame().GameId as object).should_be(Game().Id as object);

                    it["old game id is removed"] = () =>
                        ((bool)WantedGame().RespondsTo("GameIdOld")).should_be_false();
                };
            };
        }

        dynamic Game()
        {
            return games.All().First();
        }

        dynamic NotInterestedGame()
        {
            return notInterestedGames.All().First();
        }

        dynamic WantedGame()
        {
            return wantedGames.All().First();
        }

        dynamic Library()
        {
            return library.All().First();
        }
    }
}
