﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BorrowedGames.Models;
using Oak;

namespace BorrowedGames.Controllers
{
    public class GamesController : BaseController
    {
        public dynamic Preferred()
        {
            return Json(LinksForPreferredGames(User().PreferredGames()));
        }

        public dynamic Wanted()
        {
            return Json(LinksForWantedGames(User().WantedGames()));
        }

        public dynamic Requested()
        {
            return Json((User().RequestedGames() as IEnumerable<dynamic>).ToList());
        }

        public dynamic NotInterested()
        {
            return Json(LinksForNotInterestedGames(User().NotInterestedGames()));
        }

        [HttpPost]
        public void NotInterested(int gameId)
        {
            User().MarkGameNotInterested(gameId);
        }

        [HttpPost]
        public void WantGame(int gameId, int followingId)
        {
            User().WantGame(gameId, followingId);
        }

        [HttpPost]
        public void DeleteWant(int gameId, int followingId)
        {
            User().DeleteWantedGame(gameId, followingId);
        }

        [HttpPost]
        public void UndoNotInterested(int gameId)
        {
            User().UndoNotInterestedGame(gameId);
        }

        [HttpPost]
        public void GameGiven(int gameId, int toUserId)
        {
            User().GameGiven(gameId, toUserId);
        }

        public IEnumerable<dynamic> LinksForPreferredGames(IEnumerable<dynamic> games)
        {
            games.ForEach(s =>
            {
                s.NotInterested = Url.RouteUrl(new
                {
                    controller = "Games",
                    action = "NotInterested",
                    gameId = s.Id
                });

                s.WantGame = Url.RouteUrl(new
                {
                    controller = "Games",
                    action = "WantGame",
                    gameId = s.Id,
                    followingId = s.Owner.Id
                });
            });

            return games;
        }

        public IEnumerable<dynamic> LinksForWantedGames(IEnumerable<dynamic> games)
        {
            games.ForEach(s => 
            {
                s.DeleteWant = Url.RouteUrl(new
                {
                    controller = "Games",
                    action = "DeleteWant",
                    gameId = s.Id,
                    followingId = s.Owner.Id
                });
            });

            return games;
        }

        public IEnumerable<dynamic> LinksForNotInterestedGames(IEnumerable<dynamic> games)
        {
            games.ForEach(s => 
            {
                s.UndoNotInterested = Url.RouteUrl(new
                {
                    controller = "Games",
                    action = "UndoNotInterested",
                    gameId = s.GameId
                });
            });

            return games;
        }
    }
}
