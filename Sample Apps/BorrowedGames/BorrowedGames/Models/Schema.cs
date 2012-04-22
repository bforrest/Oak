using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;

namespace BorrowedGames.Models
{
    public class Schema
    {
        public Seed Seed { get; set; }

        public Schema(Seed seed)
        {
            Seed = seed;
        }

        public IEnumerable<Func<dynamic>> Scripts()
        {
            yield return CreateUsers;

            yield return CreateGames;

            yield return CreateLibrary;

            yield return CreateFriends;

            yield return CreateNotInterestedGames;

            yield return CreateWantedGames;

            yield return AddConsoleToGames;

            yield return AddReturnDateToWantedGames;

            yield return GameIdsFromIntToGuids;
        }

        public Func<dynamic> Current()
        {
            return Scripts().Last();
        }

        public string CreateUsers()
        {
            return Seed.CreateTable("Users", new dynamic[] 
            { 
                Id(),
                new { Email = "nvarchar(1000)" },
                new { Password = "nvarchar(100)" },
                new { Handle = "nvarchar(1000)" }
            });
        }

        public string CreateGames()
        {
            return Seed.CreateTable("Games", new dynamic[] {
                Id(),
                new { Name = "nvarchar(1000)" }
            });
        }

        public string CreateLibrary()
        {
            return Seed.CreateTable("Library", new dynamic[] {
                Id(),
                UserId(),
                GameId(),
            });
        }

        public string CreateFriends()
        {
            return Seed.CreateTable("FriendAssociations", new dynamic[] {
                Id(),
                UserId(),
                new { IsFollowing = "int", ForeignKey = "Users(Id)" }
            });
        }

        public string CreateNotInterestedGames()
        {
            return Seed.CreateTable("NotInterestedGames", new dynamic[] { 
                Id(),
                UserId(),
                GameId(),
            });
        }

        public string CreateWantedGames()
        {
            return Seed.CreateTable("WantedGames", new dynamic[] {
                Id(),
                UserId(),
                GameId(),
                new { FromUserId = "int", ForeignKey = "Users(Id)" },
            });
        }

        public IEnumerable<string> GameIdsFromIntToGuids()
        {
            yield return Seed.RenameColumn("Games", "Id", "IdOld");

            yield return Seed.AddColumns("Games", new dynamic[] 
            { 
                new { Id = "uniqueidentifier", Default = "newid()", Nullable = false }
            });

            yield return Seed.RenameColumn("Library", "GameId", "GameIdOld");

            yield return Seed.AddColumns("Library", new dynamic[] 
            { 
                new { GameId = "uniqueidentifier" }
            });

            yield return @"
                update Library
                set GameId = G.Id
                from Library L
                inner join Games G
                on L.GameIdOld = G.IdOld
            ";

            yield return Seed.RenameColumn("NotInterestedGames", "GameId", "GameIdOld");

            yield return Seed.AddColumns("NotInterestedGames", new dynamic[] 
            { 
                new { GameId = "uniqueidentifier" }
            });

            yield return @"
                update NotInterestedGames
                set GameId = G.Id
                from NotInterestedGames N
                inner join Games G
                on N.GameIdOld = G.IdOld
            ";

            yield return Seed.RenameColumn("WantedGames", "GameId", "GameIdOld");

            yield return Seed.AddColumns("WantedGames", new dynamic[] 
            { 
                new { GameId = "uniqueidentifier" }
            });

            yield return @"
                update WantedGames
                set GameId = G.Id
                from WantedGames W
                inner join Games G
                on W.GameIdOld = G.IdOld
            ";

            yield return Seed.DropConstraint("WantedGames", "GameIdOld");

            yield return Seed.DropColumn("WantedGames", "GameIdOld");

            yield return Seed.DropConstraint("NotInterestedGames", "GameIdOld");

            yield return Seed.DropColumn("NotInterestedGames", "GameIdOld");

            yield return Seed.DropConstraint("Library", "GameIdOld");

            yield return Seed.DropColumn("Library", "GameIdOld");

            yield return Seed.DropConstraint("Games", "IdOld");

            yield return Seed.DropColumn("Games", "IdOld");
        }

        public string AddConsoleToGames()
        {
            return Seed.AddColumns("Games", new dynamic[] 
            { 
                new { Console = "nvarchar(255)" }
            });
        }

        public string AddReturnDateToWantedGames()
        {
            return Seed.AddColumns("WantedGames", new dynamic[] 
            { 
                new { ReturnDate = "datetime" }
            });
        }

        private object Id()
        {
            return new { Id = "int", Identity = true, PrimaryKey = true };
        }

        private object UserId()
        {
            return new { UserId = "int", ForeignKey = "Users(Id)" };
        }

        private object GameId()
        {
            return new { GameId = "int", ForeignKey = "Games(Id)" };
        }

        public void MigrateUpTo(Func<dynamic> method)
        {
            Seed.ExecuteUpTo(Scripts(), method);
        }

        public void ExecuteNonQuery(Func<dynamic> script)
        {
            Seed.ExecuteNonQuery(script());
        }
    }
}