using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class User : DynamicModel
    {
        Games games;

        Library library;

        Users users;

        Friends friends;

        public User() : this(new { }) { }

        public User(object dto)
            : base(dto)
        {
            games = new Games();

            library = new Library();

            users = new Users();

            friends = new Friends();
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasManyThrough(games, library);

            yield return new HasManyThrough(users, friends, named: "Friends")
            {
                ForeignKey = "IsFollowing"
            };

            yield return new HasMany(library);
        }
    }
}
